using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace vMenuServer
{
	// FiveMHttpRequests class is made by Blade: https://gist.github.com/bladecoding/368f6a57108696c350fa7408f387a482
	// Some stuff was added for discord webhook requests
	public class HttpRequests : BaseScript
	{
		static readonly Dictionary<int, PendingRequest> _pendingRequests = new Dictionary<int, PendingRequest>();

		public HttpRequests()
		{
			EventHandlers["__cfx_internal:httpResponse"] += new Action<int, int, string, dynamic>(OnHttpResponse);
		}

		private void OnHttpResponse(int token, int statusCode, string body, dynamic headers)
		{
			if (_pendingRequests.TryGetValue(token, out var req))
			{
				if (statusCode == 200)
				{
					req.SetResult(body);
				}
				else
				{
					req.SetException(new Exception("Server returned status code: " + statusCode));
				}

				_pendingRequests.Remove(token);
			}
		}

		public static async Task<string> DownloadString(string url)
		{
			var args = new Dictionary<string, object>() {
			{ "url", url }
		};
			var argsJson = JsonConvert.SerializeObject(args);
			var id = API.PerformHttpRequestInternal(argsJson, argsJson.Length);
			var req = _pendingRequests[id] = new PendingRequest(id);
			return await req.Task;
		}
		public static async Task<string> UploadString(string url, string body)
		{
			var args = new Dictionary<string, object>() {
			{ "url", url },
			{ "method", "POST" },
			{ "data", body },
			{ "headers", new Dictionary<string, string> { { "Content-Type", "application/json" } } }
		};
			var argsJson = JsonConvert.SerializeObject(args);
			var id = API.PerformHttpRequestInternal(argsJson, argsJson.Length);
			var req = _pendingRequests[id] = new PendingRequest(id);
			return await req.Task;
		}

		private class PendingRequest : TaskCompletionSource<string>
		{
			public int Token;
			public PendingRequest(int token)
			{
				Token = token;
			}
		}

		// Discord stuff.
		public static dynamic DiscordEmbed(string title, string color, string description, string footerText)
		{
			return new
			{
				title = title,
				description = description,
				color = color,
				footer = new
				{
					text = footerText
				},
				thumbnail = new
				{
					url = ""
				}
			};
		}
	
		public static async Task SendWebhook(string webhook, string content, object embed)
		{
			try
			{
				await UploadString(webhook,
					JsonConvert.SerializeObject(new
					{
						username = "vMenu Discord Logs",
						content = content,
						embeds = new[]
						{
							embed
						}
					}
					));
			}
			catch { }
		}

		public static async Task SendWebhook(string webhook, string content, object[] embeds)
		{
			try
			{
				await UploadString(webhook,
					JsonConvert.SerializeObject(new
					{
						content = content,
						embeds = embeds
					}
					));
			}
			catch { }
		}
	}
}
