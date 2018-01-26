using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GHMatti.Http
{
    public struct RequestResponse
    {
        public int status;
        public string headers;
        public string content;
    }

    public class RequestInternal : BaseScript
    {
        public Dictionary<int, RequestResponse> responseDictionary;

        public RequestInternal()
        {
            responseDictionary = new Dictionary<int, RequestResponse>();
            EventHandlers["__cfx_internal:httpResponse"] += new Action<int, int, string, object>(Response);
            Exports.Add("HttpRequest", new Func<string, string, string, string, Task<object>>(Http));
        }

        public void Response(int token, int status, string text, object header)
        {
            RequestResponse response = new RequestResponse();
            response.headers = JsonConvert.SerializeObject(header);
            response.status = status;
            response.content = text;
            responseDictionary[token] = response;
        }

        public struct RequestDataInternal
        {
            public string url;
            public string method;
            public string data;
            public Dictionary<string, string> headers;
        }

        public async Task<object> Http(string url, string method, string data, string headers)
        {
            RequestDataInternal requestData = new RequestDataInternal();
            requestData.url = url;
            requestData.method = method;
            requestData.data = data;
            requestData.headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headers);
            string json = JsonConvert.SerializeObject(requestData);
            int token = API.PerformHttpRequestInternal(json, json.Length);
            while (!responseDictionary.ContainsKey(token))
            {
                await Delay(0);
            }
            string res = responseDictionary[token].content;
            //string res = JsonConvert.SerializeObject(responseDictionary[token]);
            responseDictionary.Remove(token);
            
            return res;
        }
    }
}