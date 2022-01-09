using DotNetty.Common.Utilities;

namespace FastLivePushClient.CoreLib
{
    public class CoreConst
    {
        public   static readonly  int  HTTP_RESPONSE_STATUS_OK = 0;
        public   static readonly  int  CLIENT_RESPONSE_STATUS_OK = 200;
        
        internal const string APIBASE = "http://77.242.242.209:8080";
        internal const string PUSHLIST_URL = "/biz/push/list";
        
        internal static readonly uint MESSAGE_BODY_NEED_COMPRESS_LEN = 1024;
        
        internal static  AttributeKey<EncryptionAttribute>  EncryptionAttributeKey =  AttributeKey<EncryptionAttribute>.NewInstance("encryption");
        
    }


    internal class EncryptionAttribute
    {
        private byte[] _encKey;
        private byte[] _iv;

        internal byte[] EncKey
        {
            get => _encKey;
            set => _encKey = value;
        }

        internal byte[] Iv
        {
            get => _iv;
            set => _iv = value;
        }
    }
}