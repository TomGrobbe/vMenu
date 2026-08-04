using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using Newtonsoft.Json;

using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Free text typed by the player.
/// </summary>
/// <remarks>
/// Registered as raw NUI callbacks: an ordinary one is dispatched as an event whose source is
/// "nui:&lt;resource&gt;", which this runtime parses as a player id and throws on. Three things about
/// raw ones are not negotiable — the reference has to come from the core's own registry or the host
/// answers "Invalid function", only the request may be declared because the second argument is a
/// function reference that will not deserialize, and the page has to post JSON because the body is
/// parsed before anything is dispatched.
/// 
/// Existing bug report on RFC: https://github.com/citizenfx/rfc/discussions/257
/// To be changed whenever that bug is fixed.
/// </remarks>
public static class UserInput
{
    private const string SubmitCallback = "vMenuPromptSubmit";

    private const string CancelCallback = "vMenuPromptCancel";

    private const string ReadyCallback = "vMenuPromptReady";

    private const string CloseMessage = """{"type":"close"}""";

    private const int ReadyTimeoutMs = 3000;

    /// <summary>The page's first callback of a session takes seconds; every one after is immediate.</summary>
    private const int FirstReadyTimeoutMs = 15000;

    private const int ButtonGraceMs = 300;

    private static bool _callbacksRegistered;
    private static bool _handshaken;
    private static TaskCompletionSource<string?>? _pending;
    private static TaskCompletionSource<bool>? _ready;

    /// <returns>What was typed, or <see langword="null"/> if the player cancelled.</returns>
    public static async Task<string?> GetTextAsync(
        MenuText title,
        int maxLength,
        string initialValue = "",
        IReadOnlyList<InputSuggestion>? suggestions = null)
    {
        if (_pending is not null)
        {
            return null;
        }

        EnsureCallbacks();

        var pending = new TaskCompletionSource<string?>();
        var ready = new TaskCompletionSource<bool>();

        _pending = pending;
        _ready = ready;

        var buttonsWereEnabled = !MenuController.DisableMenuButtons;

        MenuController.DisableMenuButtons = true;

        try
        {
            Native.SendNuiMessage(BuildOpenMessage(title.Resolve(Localizer.Current), maxLength, initialValue, suggestions));
            Native.SetNuiFocus(hasFocus: true, hasCursor: true);

            var timeout = _handshaken ? ReadyTimeoutMs : FirstReadyTimeoutMs;

            if (await Task.WhenAny(ready.Task, API.Delay(timeout)) != ready.Task)
            {
                API.Log.Error($"[Input] The prompt did not answer within {timeout}ms. Is ui/index.html part of the resource?");

                return null;
            }

            return await pending.Task;
        }
        finally
        {
            _pending = null;
            _ready = null;

            Native.SetNuiFocus(hasFocus: false, hasCursor: false);
            Native.SendNuiMessage(CloseMessage);

            if (buttonsWereEnabled)
            {
                _ = ReleaseMenuButtonsAsync();
            }
        }
    }

    /// <summary>
    /// The key or click that closed the prompt is still held when focus returns to the game, and
    /// MenuAPI selects on release: without this grace the row that opened the prompt reopens it.
    /// </summary>
    private static async Task ReleaseMenuButtonsAsync()
    {
        await API.Delay(ButtonGraceMs);

        if (_pending is null)
        {
            MenuController.DisableMenuButtons = false;
        }
    }

    private static void EnsureCallbacks()
    {
        if (_callbacksRegistered)
        {
            return;
        }

        _callbacksRegistered = true;

        Register(SubmitCallback, value => _pending?.TrySetResult(value));
        Register(CancelCallback, _ => _pending?.TrySetResult(null));

        Register(ReadyCallback, _ =>
        {
            _handshaken = true;
            _ready?.TrySetResult(true);
        });
    }

    private static void Register(string callback, Action<string> handler)
    {
        // To be fixed when https://github.com/citizenfx/rfc/discussions/257 and https://github.com/citizenfx/rfc/discussions/350 are solved
#pragma warning disable FIVEM001 // The only registry the host invokes from.
        var reference = SharedAPI.GetCore().FuncRefManager.Register(new Action<object>(request => handler(BodyOf(request))));
#pragma warning restore FIVEM001

        Native.RegisterRawNuiCallback(callback, (int)reference);
    }

    private static string BodyOf(object? request) => request switch
    {
        IDictionary<object, object> map when map.TryGetValue("body", out var body) => Text(body),
        IDictionary<string, object> map when map.TryGetValue("body", out var body) => Text(body),
        _ => Unreadable(request),
    };

    /// <summary>The page posts what was typed as a JSON string, so the body arrives quoted and escaped.</summary>
    private static string Text(object? body)
    {
        if (body is not string raw || raw.Length == 0)
        {
            return string.Empty;
        }

        if (ClientJson.TryDeserialize<string>(raw, out var text))
        {
            return text ?? string.Empty;
        }

        API.Log.Error($"[Input] A callback body was not the JSON string the page posts: {raw}");

        return string.Empty;
    }

    private static string Unreadable(object? request)
    {
        API.Log.Error($"[Input] A callback arrived as {request?.GetType().FullName ?? "null"}, which has no body this can read.");

        return string.Empty;
    }

    private static string BuildOpenMessage(string title, int maxLength, string initialValue, IReadOnlyList<InputSuggestion>? suggestions)
    {
        var localizer = Localizer.Current;
        var rows = new SuggestionRow[suggestions?.Count ?? 0];

        for (var index = 0; index < rows.Length; index++)
        {
            var suggestion = suggestions![index];

            rows[index] = new SuggestionRow
            {
                Value = suggestion.Value,
                Label = suggestion.Label,
                Icon = suggestion.Icon ?? string.Empty,
                Detail = suggestion.Detail ?? string.Empty,
            };
        }

        return ClientJson.Serialize(new OpenMessage
        {
            Title = title,
            Value = initialValue,
            MaxLength = maxLength,
            Placeholder = localizer.Get(Loc.Framework.InputPlaceholder),
            Hint = localizer.Get(Loc.Framework.InputHint),
            NoMatches = localizer.Get(Loc.Framework.InputNoMatches),
            Suggestions = rows,
        });
    }

    private sealed class OpenMessage
    {
        public string Type { get; } = "open";

        public required string Title { get; init; }

        public required string Value { get; init; }

        public required int MaxLength { get; init; }

        public required string Placeholder { get; init; }

        public required string Hint { get; init; }

        public required string NoMatches { get; init; }

        public required IReadOnlyList<SuggestionRow> Suggestions { get; init; }
    }

    /// <summary>Single letter keys: a spawner sends thousands of these in one message.</summary>
    private sealed class SuggestionRow
    {
        [JsonProperty("v")]
        public required string Value { get; init; }

        [JsonProperty("l")]
        public required string Label { get; init; }

        [JsonProperty("i")]
        public required string Icon { get; init; }

        [JsonProperty("d")]
        public required string Detail { get; init; }
    }
}
