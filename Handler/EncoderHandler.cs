using System;
using Bytebuf;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using FastLivePushClient.Payload;
using FastLivePushClient.Util;
using NLog;

namespace FastLivePushClient.Handler
{
    public class EncoderHandler : MessageToByteEncoder<AbstractPayload>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // 用于单元测试
        public void Encoder(AbstractPayload message, IByteBuffer buffer)
        {
            Encode(null ,message , buffer );
        }
        protected override void Encode(IChannelHandlerContext ctx, AbstractPayload message, IByteBuffer buffer)
        {
            var buf = FastBytebuf.NewBytebufWithDefault();
            // 写长度
            try
            {
                // 写长度占位
                buf.WriteInt(0);
                // 写版本
                buf.WriteByte(1);
                // 写类型码
                buf.WriteUShort((ushort) message.GetPayloadCode());
                // 根据全限定名获取clazz
                message.Pack(ctx, buf);
                // 写总长度
                buf.PutInt(0, buf.GetWriterIndex() - 4);
                buffer.WriteBytes(buf.AvailableBytes());
            }
            catch (Exception e)
            {
                logger.Warn(e, "encoder exception");
            }
            finally
            {
                buf.Release();
            }
        }
    }
}