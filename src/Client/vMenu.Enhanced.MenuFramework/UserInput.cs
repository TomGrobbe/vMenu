using System.Text.Json.Serialization;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.MenuFramework;

public static class UserInput
{
    private const string SubmitCallback = "vMenuPromptSubmit";

    private const string CancelCallback = "vMenuPromptCancel";

    private const string ReadyCallback = "vMenuPromptReady";

    private const string CloseMessage = """{"type":"close"}""";

    private const int ReadyTimeoutMs = 3000;

    // The page's first callback of a session takes seconds; every one after is immediate.
    private const int FirstReadyTimeoutMs = 15000;

    private const int ButtonGraceMs = 300;

    private static bool _callbacksRegistered;
    private static bool _handshaken;
    private static bool _open;
    private static TaskCompletionSource<string?>? _pending;
    private static TaskCompletionSource<bool>? _ready;

    // What was typed, or null if the player cancelled.
    public static async Task<string?> GetTextAsync(
        MenuText title,
        int maxLength,
        string initialValue = "",
        IReadOnlyList<InputSuggestion>? suggestions = null,
        bool suggestWhenEmpty = false,
        MenuText description = default)
    {
        var answers = await GetTextAsync(
            new InputPrompt(title, maxLength, initialValue, suggestions, suggestWhenEmpty, description));

        return answers?[0];
    }

    // Asks for several things one after another, answering null if the player cancelled any of them.
    // One session rather than repeated GetTextAsync calls: the page is only closed and NUI focus only
    // dropped at the end, so the next prompt does not need a delay in front of it to come up focused.
    public static async Task<string[]?> GetTextAsync(params InputPrompt[] prompts)
    {
        if (prompts.Length == 0 || _open)
        {
            return null;
        }

        EnsureCallbacks();

        _open = true;

        var buttonsWereEnabled = !MenuController.DisableMenuButtons;

        MenuController.DisableMenuButtons = true;

        try
        {
            var answers = new string[prompts.Length];

            for (var index = 0; index < prompts.Length; index++)
            {
                if (await AskAsync(prompts[index]) is not { } answer)
                {
                    return null;
                }

                answers[index] = answer;
            }

            return answers;
        }
        finally
        {
            _open = false;

            Native.SetNuiFocus(false, false);
            Native.SendNuiMessage(CloseMessage);

            if (buttonsWereEnabled)
            {
                _ = ReleaseMenuButtonsAsync();
            }
        }
    }

    private static async Task<string?> AskAsync(InputPrompt prompt)
    {
        var pending = new TaskCompletionSource<string?>();
        var ready = new TaskCompletionSource<bool>();

        _pending = pending;
        _ready = ready;

        try
        {
            Native.SendNuiMessage(BuildOpenMessage(prompt));

            Native.SetNuiFocus(true, true);

            var timeout = _handshaken ? ReadyTimeoutMs : FirstReadyTimeoutMs;

            if (await Task.WhenAny(ready.Task, API.Delay(timeout)) != ready.Task)
            {
                Log.Error($"[Input] The prompt did not answer within {timeout}ms. Is ui/index.html part of the resource?");

                return null;
            }

            return await pending.Task;
        }
        finally
        {
            _pending = null;
            _ready = null;
        }
    }

    // The key or click that closed the prompt is still held when focus returns to the game, and MenuAPI
    // selects on release: without this grace the row that opened the prompt reopens it.
    private static async Task ReleaseMenuButtonsAsync()
    {
        await API.Delay(ButtonGraceMs);

        if (!_open)
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

        NuiCallbacks.Register(SubmitCallback, body => _pending?.TrySetResult(Text(body)));
        NuiCallbacks.Register(CancelCallback, _ => _pending?.TrySetResult(null));

        NuiCallbacks.Register(ReadyCallback, _ =>
        {
            _handshaken = true;
            _ready?.TrySetResult(true);
        });
    }

    // The page posts what was typed as a JSON string, so the body arrives quoted and escaped.
    private static string Text(string raw)
    {
        if (raw.Length == 0)
        {
            return string.Empty;
        }

        if (ClientJson.TryDeserialize<string>(raw, out var text))
        {
            return text ?? string.Empty;
        }

        Log.Error($"[Input] A callback body was not the JSON string the page posts: {raw}");

        return string.Empty;
    }

    private static string BuildOpenMessage(InputPrompt prompt)
    {
        var localizer = Localizer.Current;
        var suggestions = prompt.Suggestions;
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
            Title = prompt.Title.Resolve(localizer),
            Description = prompt.Description.Resolve(localizer),
            Value = prompt.InitialValue,
            MaxLength = prompt.MaxLength,
            Placeholder = localizer.Get(Loc.Framework.InputPlaceholder),
            Hint = localizer.Get(Loc.Framework.InputHint),
            NoMatches = localizer.Get(Loc.Framework.InputNoMatches),
            SuggestWhenEmpty = prompt.SuggestWhenEmpty,
            Suggestions = rows,
        });
    }

    private sealed class OpenMessage
    {
        public string Type { get; } = "open";

        public required string Title { get; init; }

        public required string Description { get; init; }

        public required string Value { get; init; }

        public required int MaxLength { get; init; }

        public required string Placeholder { get; init; }

        public required string Hint { get; init; }

        public required string NoMatches { get; init; }

        public required bool SuggestWhenEmpty { get; init; }

        public required IReadOnlyList<SuggestionRow> Suggestions { get; init; }
    }

    // Single letter keys: a spawner sends thousands of these in one message.
    private sealed class SuggestionRow
    {
        [JsonPropertyName("v")]
        public required string Value { get; init; }

        [JsonPropertyName("l")]
        public required string Label { get; init; }

        [JsonPropertyName("i")]
        public required string Icon { get; init; }

        [JsonPropertyName("d")]
        public required string Detail { get; init; }
    }
}
