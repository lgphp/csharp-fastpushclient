using Bytebuf;
using DotNetty.Transport.Channels;

namespace FastLivePushClient.Payload
{
    public abstract  class AbstractPayload

    {

        internal abstract EnumMessage.PayloadCode GetPayloadCode();
        /**
         * 编码
         */
        internal abstract void Pack(IChannelHandlerContext ctx, FastBytebuf buf);
        /**
         * 解码
         */
        internal abstract void Unpack(IChannelHandlerContext ctx, FastBytebuf buf);
        
    }
}