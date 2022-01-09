using System;
using System.Linq;
using Bytebuf;
using DotNetty.Buffers;

namespace FastLivePushClient.Util
{
    public class ByteBufUtil
    {
        public static FastBytebuf ConvertFromIByteBuffer(IByteBuffer buffer)
        {
            FastBytebuf buf = FastByteBufUtil.NewBytebufWithDefault();
            int len = buffer.ReadableBytes;
            byte[] ib = new byte[len];
            buffer.ReadBytes(ib);
            buf.WriteBytes(ib);
            return buf;
        }


        public static IByteBuffer ConvertFromFastByteBuf(FastBytebuf buffer)
        {
            var buf = ByteBufferUtil.DefaultAllocator.DirectBuffer(buffer.Capacity());
            buf.WriteBytes(buffer.AvailableBytes());
            buffer.Release();
            return buf;
        }
    }
}