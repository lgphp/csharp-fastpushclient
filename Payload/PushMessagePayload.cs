using System;
using System.Text;
using Bytebuf;
using DotNetty.Transport.Channels;
using FastLivePushClient.CoreLib;
using FastLivePushClient.Entity;
using FastLivePushClient.Util;
using Newtonsoft.Json;

namespace FastLivePushClient.Payload
{
    public class PushMessagePayload : AbstractPayload
    {
        internal static readonly EnumMessage.PayloadCode Code = EnumMessage.PayloadCode.PushMessageCode;

        internal string messageID;
        internal EnumMessage.NotificationClassifier classifier;
        internal string merchantID;
        internal string appID;
        internal EnumMessage.Priority priority;
        internal string toUid;
        internal MessageBody messageBody;

        internal string MessageId
        {
            get => messageID;
            set => messageID = value;
        }

        internal EnumMessage.NotificationClassifier Classifier
        {
            get => classifier;
            set => classifier = value;
        }

        internal string MerchantId
        {
            get => merchantID;
            set => merchantID = value;
        }

        internal string AppId
        {
            get => appID;
            set => appID = value;
        }

        internal EnumMessage.Priority Priority
        {
            get => priority;
            set => priority = value;
        }

        internal string ToUid
        {
            get => toUid;
            set => toUid = value;
        }

        internal MessageBody MessageBody
        {
            get => messageBody;
            set => messageBody = value;
        }


        internal override EnumMessage.PayloadCode GetPayloadCode()
        {
            return Code;
        }

        internal override void Pack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
            buf.WriteStringWithByteLength(messageID);
            buf.WriteByte((byte) classifier);
            buf.WriteStringWithByteLength(merchantID);
            buf.WriteStringWithByteLength(appID);
            byte encflag = 2;
            var encKey = ctx.Channel.GetAttribute(CoreConst.EncryptionAttributeKey).Get().EncKey;
            var encIv = ctx.Channel.GetAttribute(CoreConst.EncryptionAttributeKey).Get().Iv;
            var messageBodyBytes = EncryptionUtil.AESEncrypt(JsonConvert.SerializeObject(messageBody), encKey, encIv);
            if (messageBodyBytes.Length > CoreConst.MESSAGE_BODY_NEED_COMPRESS_LEN)
            {
                messageBodyBytes = GzipUtil.Gzip(messageBodyBytes);
                encflag = 3;
            }

            // 写加密标志
            buf.WriteByte(encflag);
            // 写优先级
            buf.WriteByte((byte) priority);
            // 写toUserID
            buf.WriteStringWithByteLength(toUid);
            // 写body
            buf.WriteBytes(messageBodyBytes);
        }

        internal override void Unpack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
            throw new System.NotImplementedException();
        }


        private static PushMessagePayload CreatePushPayload(Notification n, AppInfo info)
        {
            return new PushMessagePayload
            {
                messageID = Guid.NewGuid().ToString(),
                appID = info.AppId,
                merchantID = info.MerchantId,
                priority = n._messagePriority,
                toUid = n._toUid,
                messageBody = n._messageBody
            };
        }

        internal static PushMessagePayload CreatePushMessageWithNotification(Notification n, AppInfo info)
        {
            var pmp = CreatePushPayload(n, info);
            pmp.Classifier = EnumMessage.NotificationClassifier.PUSH;
            return pmp;
        }

        internal static PushMessagePayload CreateVoipMessageWithNotification(Notification n, AppInfo info)
        {
            var pmp = CreatePushPayload(n, info);
            pmp.Classifier = EnumMessage.NotificationClassifier.VOIP;
            return pmp;
        }
    }
}