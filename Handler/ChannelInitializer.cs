using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FastLivePushClient.CoreLib;

namespace FastLivePushClient.Handler
{
    public sealed  class ChannelInitializer
    {
        public static IChannelHandler InitializeChannelHandler( PushClient _client)
        {
            return new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(65535, 0,
                    4, 0, 0));
                pipeline.AddLast(new DecoderHandler());
                pipeline.AddLast(new EncoderHandler());
                pipeline.AddLast(new BizProcessorHandler(_client));
            });
        } 
    }
}