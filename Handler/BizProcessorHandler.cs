using System;
using System.Net.Sockets;
using DotNetty.Transport.Channels;
using FastLivePushClient.Container;
using FastLivePushClient.CoreLib;
using FastLivePushClient.Payload;
using NLog;

namespace FastLivePushClient.Handler
{
    public class BizProcessorHandler : ChannelHandlerAdapter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private PushClient _client;

        public BizProcessorHandler(PushClient client)
        {
            _client = client;
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            logger.Info("connected to server:{0} ， Channel Status:{1}" , ctx.Channel.RemoteAddress  , ctx.Channel.Active);
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
           logger.Info("disconnected from server : {0}"  , ctx.Channel.RemoteAddress );
           _client._isTcpConnected.Set(false);
           _client.ReConnectServer(); 
        }

        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            _client._isTcpConnected.Set(false);
            _client.ReConnectServer(); 
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            var payload = (AbstractPayload) message;
            DispatchMessage.Invoke(payload.GetPayloadCode() , ctx, payload , _client);
        }


        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
        {
            if (exception is SocketException)
            {
                logger.Warn(" Network subsystem is down ， Reconnect to server");
                _client._isTcpConnected.Set(false); 
                _client.ReConnectServer(); 
            }
            else
            {
                logger.Error(exception);
            }
          
        }
    }
    
    
}