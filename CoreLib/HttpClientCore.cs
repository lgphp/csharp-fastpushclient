using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using EasyHttp.Http;
using FastLivePushClient.Entity;
using FastLivePushClient.Payload;
using FastLivePushClient.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;

namespace FastLivePushClient.CoreLib
{
    public class FastLiveHttpClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly HttpClient _httpClient;
        private AppInfo _info;

        public FastLiveHttpClient(AppInfo info)
        {
            _info = info;
            _httpClient = new HttpClient();
        }

        public HttpResponse Post(string url, object reqBody)
        {
            var signature = ApiSignUtil.MakeSign(reqBody, KeyUtil.GetApiKey(_info.AppKey));
            _httpClient.Request.AddExtraHeader("API-SIGNATURE", signature);
            _httpClient.Request.AddExtraHeader("APP-ID", _info.AppId);
            _httpClient.Request.Accept = HttpContentTypes.ApplicationJson;
            var httpResponse = _httpClient.Post(url, reqBody, "application/json;charset=UTF-8");
            return httpResponse;
        }

        internal string buildUrl(string baseUri, string apiUri)
        {
            return $"{baseUri}{apiUri}";
        }


        internal PushGateAddress[] GetPushList()
        {
            var _pushListReq = new ReqConnAuthPayload {appID = _info.AppId};
            try
            {
                var _url = buildUrl(CoreConst.APIBASE, CoreConst.PUSHLIST_URL);
                var res = Post(_url, _pushListReq);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var dataJson = JsonConvert.DeserializeObject<JObject>(res.RawText);
                    var statusCode = dataJson["code"].Value<int>();
                    if (statusCode == CoreConst.HTTP_RESPONSE_STATUS_OK)
                    {
                        var data = dataJson["data"].ToArray();
                        var pushlist = new List<PushGateAddress>();
                        foreach (var ipaddr in data)
                        {
                            var pushAddres = new PushGateAddress();
                            var split = ipaddr.ToString().Split(':');
                            pushAddres.Ip = split[0];
                            pushAddres.Port = Convert.ToInt32(split[1]);
                            pushlist.Add(pushAddres);
                        }

                        return pushlist.ToArray();
                    }

                    var message = dataJson["message"].Value<string>();
                    logger.Warn("Request PushGate server failed , code:{} , message:{1}", statusCode, message);
                    throw new InvalidOperationException();
                }

                logger.Warn("Request PushGate server failed , Response.StatusCode Not OK");
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                logger.Warn("Request PushGate server failed : {0}", ex.Message);
                throw;
            }
        }
    }
}