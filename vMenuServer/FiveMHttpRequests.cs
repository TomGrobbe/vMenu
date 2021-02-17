using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace vMenuServer
{
    public class FiveMHttpRequests : BaseScript
    {
        static readonly Dictionary<int, PendingRequest> _pendingRequests = new Dictionary<int, PendingRequest>();
        public FiveMHttpRequests() {
            EventHandlers["__cfx_internal:httpResponse"] += new Action<int, int, string, dynamic>( OnHttpResponse );
        }

        private void OnHttpResponse( int token, int statusCode, string body, dynamic headers ) {
            if( _pendingRequests.TryGetValue( token, out var req ) ) {
                if( statusCode == 200 ) {
                    req.SetResult( body );
                }
                else {
                    req.SetException( new Exception( "Server returned status code: " + statusCode ) );
                }

                _pendingRequests.Remove( token );
            }
        }

        public static async Task<string> DownloadString( string url ) {
            var args = new Dictionary<string, object>() {
                { "url", url }
            };
            var argsJson = JsonConvert.SerializeObject( args );
            var id = API.PerformHttpRequestInternal( argsJson, argsJson.Length );
            var req = _pendingRequests[id] = new PendingRequest( id );
            return await req.Task;
        }
        public static async Task<string> SendWebhook( string url, string username, string color, string title, string desc, string footer ) {
            var args = new Dictionary<string, object>() {
                { "url", url },
                { "method", "POST" },
                { "data", new Dictionary<string, object>()
                    {
                        {"username", username},
                        {"embeds", new Dictionary<string, object>()
                            {
                                {"color", color},
                                {"title", title},
                                {"description", desc},
                                {
                                    "footer", new Dictionary<string, object>()
                                        {
                                            {"text", footer},
                                        }
                                }
                            } 
                        }
                    } 
                },
                { "headers", new Dictionary<string, string> { { "Content-Type", "application/json" } } }
            };
            var argsJson = JsonConvert.SerializeObject( args );
            var id = API.PerformHttpRequestInternal( argsJson, argsJson.Length );
            var req = _pendingRequests[id] = new PendingRequest( id );
            return await req.Task;
        }

        private class PendingRequest : TaskCompletionSource<string>
        {
            public int Token;
            public PendingRequest( int token ) {
                Token = token;
            }
        }
    }
}