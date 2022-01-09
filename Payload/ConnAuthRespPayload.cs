using Bytebuf;
using DotNetty.Transport.Channels;

namespace FastLivePushClient.Payload
{
    internal class ConnAuthRespPayload : AbstractPayload
    {
        internal static readonly EnumMessage.PayloadCode Code = EnumMessage.PayloadCode.ConnAuthRespCode;

        internal uint StatusCode { get; set; }
        internal string Message { get; set; }
        internal ulong ServerTime { get; set; }
        internal ushort SpeedLimit { get; set; }
        
        internal override EnumMessage.PayloadCode GetPayloadCode()
        {
            return Code;
        }

        internal override void Pack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
            throw new System.NotImplementedException();
        }

        internal override void Unpack(IChannelHandlerContext ctx, FastBytebuf buf)
        {
            StatusCode = buf.ReadUInt();
            Message = buf.ReadeStringWithUIntLength();
            ServerTime = buf.ReadULong();
            var speed = buf.ReadUShort();
            if (speed == 0)
            {
                SpeedLimit = 1000 / 1;
            }else if (speed > 1000)
            {
                SpeedLimit = 1000 / 1000;
            }
            else
            {
                SpeedLimit = (ushort)(1000  /  speed);
            }
        }
    }
}