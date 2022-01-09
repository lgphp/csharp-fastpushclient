using System;
using Bytebuf;
using DotNetty.Transport.Channels;

namespace FastLivePushClient.Payload
{
    internal class HeartBeatPayload : AbstractPayload
    {
        internal static readonly EnumMessage.PayloadCode Code = EnumMessage.PayloadCode.HeartBeatCode;

        
        internal override EnumMessage.PayloadCode GetPayloadCode()
        {
            return Code;
        }
        internal byte Zero { get; set; }
        internal override void Pack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
            buf.WriteByte(this.Zero);    
        }

        internal override void Unpack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
            throw new NotImplementedException();
        }
    }
}