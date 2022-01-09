using System.Linq;
using Bytebuf;

namespace FastLivePushClient.Util
{
    public class KeyUtil
    {
        private const ushort keyLen = 16;


        public static byte[] GetApiKey(byte[] appKey)
        {
            using (var buf = FastByteBufUtil.NewBytebufWithBytes(appKey))
            {
                var apiKey = new byte[keyLen];
                buf.GetBytes(ref apiKey, 0, 15);
                return apiKey;
            }
        }


        public static byte[] GetAuthKey(byte[] appKey)
        {
            using (var buf = FastByteBufUtil.NewBytebufWithBytes(appKey))
            {
                var authKey = new byte[keyLen];
                buf.Skip(16);
                buf.ReadBytes(ref authKey );
                return authKey;
            }
        }


        public static byte[] GetMsgEnchKey(byte[] appKey)
        {
            using (var buf = FastByteBufUtil.NewBytebufWithBytes(appKey))
            {
                var msgEncKey = new byte[keyLen];
                buf.Skip(32);
                buf.ReadBytes(ref msgEncKey);
                return msgEncKey;
            }
        }

        public static byte[] GetMsgEncIv(byte[] appKey)
        {
            using (var buf = FastByteBufUtil.NewBytebufWithBytes(appKey))
            {
                var msgEncIv = new byte[keyLen];
                buf.Skip(48);
                buf.ReadBytes(ref msgEncIv);
                return msgEncIv;
            }
        }
    }
}