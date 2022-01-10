using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FastLivePushClient.Atomic;
using FastLivePushClient.Container;
using FastLivePushClient.Entity;
using FastLivePushClient.Handler;
using FastLivePushClient.Listener;
using FastLivePushClient.Payload;
using FastLivePushClient.Util;
using NLog;
using NLog.Fluent;

namespace FastLivePushClient.CoreLib
{
    public sealed class PushClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        private SingleThreadEventLoop _heartbeatLoop = new SingleThreadEventLoop();
        private SingleThreadEventLoop _sendLoop = new SingleThreadEventLoop();


        private Action _sendHeartbeatTask;
        internal Action SendHeartbeatTask => _sendHeartbeatTask;

        private ClientListener.ConListener _conlistener;
        private ClientListener.SendListener _sendlistener;
        private ClientListener.NotifyStatusListener _notifyStatusListener;

        private string _clientId;
        private AppInfo _appInfo;

        internal IChannel _ch;


        private bool _isCanSendNotification;

        private ushort _sendSpeed;

        private PushGateAddress[] _pushGateAddress;
        private FastLiveHttpClient _httpClient;

        //与服务器时差
        private ulong timeDiff = 0;


        internal SingleThreadEventLoop HeartbeatLoop => _heartbeatLoop;

        internal SingleThreadEventLoop SendLoop => _sendLoop;


        internal ConcurrentQueue<PushMessagePayload> _sendQueue;

        internal ClientListener.ConListener Conlistener
        {
            get => _conlistener;
            set => _conlistener = value;
        }

        internal ClientListener.SendListener Sendlistener
        {
            get => _sendlistener;
            set => _sendlistener = value;
        }

        internal ClientListener.NotifyStatusListener NotifyStatusListener
        {
            get => _notifyStatusListener;
            set => _notifyStatusListener = value;
        }


        internal ushort SendSpeed
        {
            get => _sendSpeed;
            set => _sendSpeed = value;
        }

        internal bool IsCanSendNotification
        {
            get => _isCanSendNotification;
            set => _isCanSendNotification = value;
        }

        internal ulong TimeDiff
        {
            get => timeDiff;
            set => timeDiff = value;
        }


        public PushClient()
        {
            _sendSpeed = 1;
            ComponentScan.Scan();
            // 设置心跳的任务action
            _sendHeartbeatTask = StartHeartbeatTask;
            _sendQueue = new ConcurrentQueue<PushMessagePayload>();
        }


        public PushClient SetAppInfo(AppInfo appInfo)
        {
            logger.Info("Initial PushClient");
            _appInfo = appInfo;
            _clientId = $"{_appInfo.AppId}-{Guid.NewGuid().ToString()}";
            _httpClient = new FastLiveHttpClient(appInfo);
            return this;
        }

        public AppInfo GetAppInfo()
        {
            return _appInfo;
        }


        public PushClient AddInitailListener(ClientListener.ConListener l)
        {
            _conlistener = l;
            return this;
        }


        public PushClient AddSendListener(ClientListener.SendListener l)
        {
            _sendlistener = l;
            return this;
        }

        public PushClient AddNotifyListener(ClientListener.NotifyStatusListener l)
        {
            _notifyStatusListener = l;
            return this;
        }


        public PushClient BuildClient()
        {
            if (_conlistener == null)
            {
                throw new ArgumentNullException("_conlistener");
            }

            if (_sendlistener == null)
            {
                throw new ArgumentNullException("_sendlistener");
            }

            if (_notifyStatusListener == null)
            {
                throw new ArgumentNullException("_notifyStatusListener");
            }

            logger.Info("Will Get PushGate Server");
            //  获取pushlist
            _pushGateAddress = _httpClient.GetPushList();
            // 连接服务器
            logger.Info("Will Connect to PushGate server");
            ConnectServer();
            return _isCanSendNotification ? this : null;
        }

        internal AtomicBoolean _isTcpConnected = new AtomicBoolean(false);

        private void ConnectServer()
        {
            foreach (var pushGateAddr in _pushGateAddress)
            {
                ConnectServer(pushGateAddr);
                if (_isTcpConnected.Get())
                {
                    break;
                }
            }
        }

        private MultithreadEventLoopGroup _group;
        private Bootstrap _bootstrap;

        private void ConnectServer(PushGateAddress pushGateAddr)
        {
            var serverIp = IPAddress.Parse(pushGateAddr.Ip);
            var serverPort = pushGateAddr.Port;
            _group = new MultithreadEventLoopGroup();
            _bootstrap = new Bootstrap();
            _bootstrap.Group(_group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(5000))
                .Option(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.SoSndbuf, 2 << 14)
                .Option(ChannelOption.SoRcvbuf, 2 << 14)
                .Handler(ChannelInitializer.InitializeChannelHandler(this));
            _bootstrap.RemoteAddress(new IPEndPoint(serverIp, serverPort));
            RunClientAsync().Wait();
        }


        private async Task ConnectToServerAsync()
        {
            try
            {
                var channel = await _bootstrap.ConnectAsync();

                if (channel != null && channel.Active)
                {
                    _isTcpConnected.Set(true);
                    _isReconnecting.Set(false);
                    _reconnectCnt.Set(0);
                    _ch = channel;
                    _conlistener(201, $"Connect Success! Ready for Communication: {channel.RemoteAddress} ");
                    SendConnAuthentication();
                }
                else
                {
                    _isTcpConnected.Set(false);
                    _conlistener(500,
                        $"Connect Failed , Socket Channel has null or inactive ");
                    ReConnectServer();
                }
            }
            catch (ConnectException ex)
            {
                _isTcpConnected.Set(false);
                ReConnectServer();
                _conlistener(503,
                    $"Connect failed , { ex.StackTrace} : Network not available");
            }
        }


        private async Task RunClientAsync()
        {
            await ConnectToServerAsync();
        }


        private readonly AtomicInt _reconnectCnt = new AtomicInt(0);
        private readonly AtomicBoolean _isReconnecting = new AtomicBoolean(false);
        private readonly SingleThreadEventLoop _reconnectLoop = new SingleThreadEventLoop();

        internal void ReConnectServer()
        {
            if (_isReconnecting.Get()) return;
            _isReconnecting.Set(true);
            _reconnectLoop.ScheduleAsync(async () =>
            {
                logger.Info($"Reconnecting to server....");
                while ((_reconnectCnt.Get() <= 61) && (_isTcpConnected.Get() == false))
                {
                    try
                    {
                        if (_reconnectCnt.Get() > 60)
                        {
                            _conlistener(504,
                                $"Reconnect Server Failed ..  times : {_reconnectCnt} , Client will shutdown.");
                            ShutdownAllEventLoop();
                            return;
                        }

//                        new Action(async () =>
//                            {
                        await ConnectToServerAsync();
//                            }).BeginInvoke(null, null);
                        _reconnectCnt.Increment();
                        _conlistener(504, $"Reconnect Server ..  times : " + _reconnectCnt.Get());
                        Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 * 4 * (_reconnectCnt.Get() + 1)));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }, TimeSpan.FromSeconds(10));
        }


        internal void ShutdownAllEventLoop()
        {
            _reconnectLoop.AddShutdownHook(() => logger.Info("reconnectLoop has shut down"));
            _reconnectLoop.ShutdownGracefullyAsync(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(5));
            _heartbeatLoop.AddShutdownHook(() => { logger.Info("Heartbeat Task has shut down"); });
            _heartbeatLoop.ShutdownGracefullyAsync(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(5));
            Shutdown();
        }


        public bool IsReady()
        {
            return _isCanSendNotification;
        }
        
        private bool IsWriteble()
        {
            if (_ch == null || !_ch.Active)
            {
                _isTcpConnected.Set(false);
                //启动重新连接
                ReConnectServer();
            }

            return _ch != null && _ch.Active;
        }

        internal void SendConnAuthentication()
        {
            if (!IsWriteble()) return;
            _conlistener(201, "Start to connection auth..");
            var connAuthPayload = new ConnAuthPayload();
            connAuthPayload.AppId = _appInfo.AppId;
            connAuthPayload.MerchantId = _appInfo.MerchantId;
            connAuthPayload.ClientInstanceId = _clientId;
            connAuthPayload.AuthKey = KeyUtil.GetAuthKey(_appInfo.AppKey);
            _ch.WriteAndFlushAsync(connAuthPayload);
        }

        internal void Shutdown()
        {
            _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }


        private readonly AtomicBoolean _heartbeatTaskFlag = new AtomicBoolean(false);

        /**
         * 开启心跳任务
         */
        private void StartHeartbeatTask()
        {
            _heartbeatTaskFlag.Set(true);
            while (_heartbeatTaskFlag.Get())
            {
                if (IsWriteble())
                {
                    var hbp = new HeartBeatPayload();
                    _ch.WriteAndFlushAsync(hbp);
                    Thread.Sleep(15000);
                }
                else
                {
                    StopHeartbeatTask();
                    break;
                }
            }
        }

        /**
         * 停止心跳任务，跳出循环即可
         */
        internal void StopHeartbeatTask()
        {
            if (_heartbeatLoop.IsShutdown) return;
            _heartbeatTaskFlag.Set(false);
            // 不能shut down 这个线程池，因为只有一个线程池（在client实例化的时候）负责处理，除非在认证通过后启动一个新的eventloop
//            _heartbeatLoop.AddShutdownHook(() => { logger.Info("Heartbeat Task has shut down"); });
//            _heartbeatLoop.ShutdownGracefullyAsync(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(5));
        }


        internal void SendMessageQueneTask()
        {
            while (true)
            {
                if (!_sendQueue.TryDequeue(out var payload)) continue;
                _ch.WriteAndFlushAsync(payload);
                _sendlistener(200, $"{payload.MessageId} send OK");
                Thread.Sleep(_sendSpeed);
            }
        }


        public void SendPushNotification(Notification n)
        {
            if (_isCanSendNotification)
            {
                var pushPayload = PushMessagePayload.CreatePushMessageWithNotification(n, _appInfo);
                if (IsWriteble())
                {
                    // 入队
                    if (_sendQueue.ToArray().Length > 1000)
                    {
                        _sendlistener(500, $"{pushPayload.MessageId} : send too quickly , please slowly!");
                    }
                    else
                    {
                        _sendQueue.Enqueue(pushPayload);
                    }
                }
                else
                {
                    _sendlistener(502, $"send failed : {pushPayload.MessageId} , Connection han been closed");
                }
            }
            else
            {
                _sendlistener(503, "Did not send push message: Connection Auth haven't finished ");
            }
        }


        public void SendVoipNotification(Notification n)
        {
            if (_isCanSendNotification)
            {
                var pushPayload = PushMessagePayload.CreateVoipMessageWithNotification(n, _appInfo);
                if (IsWriteble())
                {
                    if (_sendQueue.ToArray().Length > 1000)
                    {
                        _sendlistener(500, "send too quickly , please slowly!");
                    }
                    else
                    {
                        _sendQueue.Enqueue(pushPayload);
                    }
                }
                else
                {
                    _sendlistener(502, $"send failed : {pushPayload.MessageId} , Connection han been closed");
                }
            }
            else
            {
                _sendlistener(503, "Did not send push message: Connection Auth haven't finished ");
            }
        }
    }
}