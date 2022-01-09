using System;
using Bytebuf;
using DotNetty.Transport.Channels;

namespace FastLivePushClient.Payload
{
    internal class ConnAuthPayload : AbstractPayload
    {
        
        internal static readonly EnumMessage.PayloadCode Code = EnumMessage.PayloadCode.ConnAuthCode;
        
        internal string ClientInstanceId { get; set; }
        internal string MerchantId { get; set; }
        internal string AppId { get; set; }
        internal byte[] AuthKey { get; set; }

        internal override EnumMessage.PayloadCode GetPayloadCode()
        {
            return Code;
        }

        internal override void Pack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
            buf.WriteStringWithByteLength(ClientInstanceId);
            buf.WriteStringWithByteLength(MerchantId);
            buf.WriteStringWithByteLength(AppId);
            buf.WriteBytes(AuthKey);
        }

        internal override void Unpack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
           ClientInstanceId = buf.ReadeStringWithByteLength();
           MerchantId = buf.ReadeStringWithByteLength();
           AppId = buf.ReadeStringWithByteLength();
           byte[] ak =  new byte[16];
           buf.ReadBytes(ref ak);
           AuthKey = ak;
        }
    }
}