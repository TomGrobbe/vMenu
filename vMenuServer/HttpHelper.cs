using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GHMatti.Http
{
    public struct RequestResponse
    {
        public HttpStatusCode status;
        public WebHeaderCollection headers;
        public string content;
    }

    public struct RequestDataInternal
    {
        public string url;
        public string method;
        public string data;
        public dynamic headers;
    }

    public class RequestInternal : BaseScript
    {
        public Dictionary<int, Dictionary<string, dynamic>> responseDictionary;

        public RequestInternal()
        {
            responseDictionary = new Dictionary<int, Dictionary<string, dynamic>>();
            EventHandlers["__cfx_internal:httpResponse"] += new Action<int, int, string, dynamic>(Response);
            Exports.Add("HttpRequest", new Func<string, string, string, string, Task<Dictionary<string, dynamic>>>(Http));
        }

        public void Response(int token, int status, string text, dynamic header)
        {
            Dictionary<string, dynamic> response = new Dictionary<string, dynamic>();
            response["headers"] = header;
            response["status"] = status;
            response["content"] = text;
            responseDictionary[token] = response;
        }

        public async Task<Dictionary<string, dynamic>> Http(string url, string method, string data, dynamic headers)
        {
            RequestDataInternal requestData = new RequestDataInternal();
            requestData.url = url;
            requestData.method = method;
            requestData.data = data;
            requestData.headers = headers;
            string json = JsonConvert.SerializeObject(requestData);
            int token = API.PerformHttpRequestInternal(json, json.Length);
            while (!responseDictionary.ContainsKey(token))
            {
                await Delay(0);
            }
            Dictionary<string, dynamic> res = responseDictionary[token];
            responseDictionary.Remove(token);
            return res;
        }
    }

    public class Request : BaseScript
    {
        public Request() {}

        public async Task<RequestResponse> Http(string url, string method = "GET", string data = "", Dictionary<string, string> headers = null)
        {
            headers = (headers == null) ? new Dictionary<string, string>() : headers;
            return ParseRequestResponseInternal(
                    await Exports[API.GetCurrentResourceName()].HttpRequest(url, method, data, headers)
                );
        }
        
        private WebHeaderCollection ParseHeadersInternal(dynamic headerDyn)
        {
            WebHeaderCollection headers = new WebHeaderCollection();
            IDictionary<string, object> headerDict = (IDictionary<string, object>) headerDyn;
            foreach(KeyValuePair<string, object> entry in headerDict)
            {
                headers.Add(entry.Key, entry.Value.ToString());
            }
            return headers;
        }

        private HttpStatusCode ParseStatusInternal(int status)
        {
            return (HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), status);
        }

        private RequestResponse ParseRequestResponseInternal(IDictionary<string, dynamic> rr)
        {
            RequestResponse result = new RequestResponse();          
            result.status = ParseStatusInternal(rr["status"]);
            result.headers = ParseHeadersInternal(rr["headers"]);
            result.content = rr["content"];
            return result;
        }

    }

}
