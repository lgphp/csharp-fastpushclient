namespace FastLivePushClient.Listener
{
    public struct ClientListener
    {
        public delegate void ConListener(int code, string message);

        public delegate void SendListener(int code, string message);
        
        public delegate void NotifyStatusListener(int code, string message);
    }
}