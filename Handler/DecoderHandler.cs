using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using FastLivePushClient.Payload;
using FastLivePushClient.Util;
using NLog;

namespace FastLivePushClient.Handler
{
    public class DecoderHandler : ByteToMessageDecoder
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static int MIN_PAYLOAD_LEN = 8;
        private static int MAX_PAYLOAD_LEN = 65535;

        // 用于单元测试
        public void Decoder(IByteBuffer input)
        {
            List<object> o = new List<object>();
            Decode(null, input, o);
        }

        
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            var buf = ByteBufUtil.ConvertFromIByteBuffer(input);
            try
            {
                // 读包体长度
                var pktLen = buf.ReadInt();
                if (pktLen < MIN_PAYLOAD_LEN) throw new CodecException("< MIN_PAYLOAD_LEN:" + MIN_PAYLOAD_LEN);
                if (pktLen > MAX_PAYLOAD_LEN) throw new CodecException("> MAX_PAYLOAD_LEN:" + MIN_PAYLOAD_LEN);
                var _ = buf.ReadByte();
                var code = buf.ReadUShort();
                // 根据code获得枚举
                var payloadCodeEnum = EnumMessage.GetPayloadCodeEnum(code);
                // 根据枚举获得类型的classname
                var clazzName = EnumMessage.GetEnumDescription(payloadCodeEnum);
                
                if (clazzName == null) throw new CodecException("PayloadCode not support");
                // 获得类型
                var clazz = Type.GetType(clazzName);
                //实例化类型并调用解码器
                var payloadObj = (AbstractPayload) Activator.CreateInstance(clazz ?? throw new CodecException("decoding failed"));
                payloadObj.Unpack(context, buf);
                output.Add(payloadObj);
            }
            catch (Exception e)
            {
                throw new CodecException("Error decoding:" + e);
            }
            finally
            {
                buf.Skip(buf.ReadableBytes());
                buf.Release();
            }
        }
    }
}