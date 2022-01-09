namespace FastLivePushClient.Payload
{
    
    public class ReqConnAuthPayload
    {
        private string _appId;

        public string appID
        {
            get => _appId;
            set => _appId = value;
        }
    }
}