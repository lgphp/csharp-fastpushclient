using System;

namespace FastLivePushClient.Entity
{
    public struct AppInfo
    {
        private string _merchantId;
        private string _appId;
        private byte[] _appKey;
        
        public AppInfo(string merchantId, string appId, string appKey)
        {
            _merchantId = merchantId;
            _appId = appId;
            _appKey =  Convert.FromBase64String(appKey);
        }

        public string MerchantId
        {
            get => _merchantId;
            set => _merchantId = value;
        }

        public string AppId
        {
            get => _appId;
            set => _appId = value;
        }

        public byte[] AppKey
        {
            get => _appKey;
            set => _appKey = value;
        }
    }
}