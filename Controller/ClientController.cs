using System;
using DotNetty.Transport.Channels;
using FastLivePushClient.CoreLib;
using FastLivePushClient.Payload;
using FastLivePushClient.Service;

namespace FastLivePushClient.Controller
{
    [MessageController]
    public class ClientController
    {
        
        [MessageMapping(EnumMessage.PayloadCode.ConnAuthRespCode)]
        public void HandleConnAuthResp(IChannelHandlerContext ctx, AbstractPayload payload , PushClient client)
        {
            new MessageHandle().HandleConnAuthResponse(ctx, (ConnAuthRespPayload)payload, client);
        }
        
        
        [MessageMapping(EnumMessage.PayloadCode.PushMessageAckCode)]
        public void HandlePushMessageACK(IChannelHandlerContext ctx, AbstractPayload payload , PushClient client)
        {
            new MessageHandle().HandlePushMessageACK(ctx, (PushMessageAckPayload)payload, client);
        }
    }
}