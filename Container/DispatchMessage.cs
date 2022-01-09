using DotNetty.Transport.Channels;
using FastLivePushClient.CoreLib;
using FastLivePushClient.Payload;

namespace FastLivePushClient.Container
{
    public class DispatchMessage
    {
        public static void Invoke(EnumMessage.PayloadCode payloadCode, IChannelHandlerContext ctx, AbstractPayload payload , PushClient client)
        {
            if (!ComponentScan.InstanceContainer.ContainsKey(payloadCode)) return;
            var objDispatchMapping = ComponentScan.InstanceContainer[payloadCode];
            objDispatchMapping.Method.Invoke(objDispatchMapping.Obj , new object[] {ctx, payload , client});
        }
    }
}