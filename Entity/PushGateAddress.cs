namespace FastLivePushClient.Entity
{
    public struct PushGateAddress
    {
        private string _ip;
        private int _port;
        
        
        

        public PushGateAddress(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        internal string Ip
        {
            get => _ip;
            set => _ip = value;
        }

        internal int Port
        {
            get => _port;
            set => _port = value;
        }
    }
}