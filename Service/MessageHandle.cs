using System;
using DotNetty.Transport.Channels;
using FastLivePushClient.CoreLib;
using FastLivePushClient.Payload;
using FastLivePushClient.Util;
using NLog;

namespace FastLivePushClient.Service
{
    internal sealed class MessageHandle
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal void HandleConnAuthResponse(IChannelHandlerContext ctx, ConnAuthRespPayload payload, PushClient c)
        {

            if (CoreConst.HTTP_RESPONSE_STATUS_OK == payload.StatusCode)
            {
                var stime = payload.ServerTime;
                var ctime = DateTime.Now.Millisecond;
                c.TimeDiff = stime - (ulong) ctime;
                c.IsCanSendNotification = true;
                // 设置发送速度
                c.SendSpeed = payload.SpeedLimit;
                // 设置channel上下文,其实对于客户端来说没必要这样做
                EncryptionAttribute ea = new EncryptionAttribute();
                ea.EncKey = KeyUtil.GetMsgEnchKey(c.GetAppInfo().AppKey);
                ea.Iv = KeyUtil.GetMsgEncIv(c.GetAppInfo().AppKey);
                ctx.Channel.GetAttribute(CoreConst.EncryptionAttributeKey).Set(ea);
                
                logger.Info("Connect Authentication Success , Send Speed Limited: {0} /sec", 1000/c.SendSpeed);
                // 发送心跳
                c.HeartbeatLoop.ScheduleAsync(c.SendHeartbeatTask, TimeSpan.FromMilliseconds(15000));
                // 启动发送任务
                c.SendLoop.ScheduleAsync(c.SendMessageQueneTask, TimeSpan.FromMilliseconds(1000));
                // 设置成功回调
                c.Conlistener(200, "SDK Initial OK , Ready for Send PushMessage");
            }
            else
            {
                // 鉴权失败
                c.IsCanSendNotification = false;
                logger.Warn("Authentication of connection failed. client will shut down...");
                c.Conlistener( 502,  $"{payload.StatusCode} {payload.Message}" );
                c.ShutdownAllEventLoop();
            }
        }

        internal void HandlePushMessageACK(IChannelHandlerContext ctx, PushMessageAckPayload payload, PushClient c)
        {
            c.NotifyStatusListener((int)payload.StatusCode,$"投递结果:{payload.MessageId} | {payload.UserId} | {payload.StatusMessage}");
        }
        
        
    }
}