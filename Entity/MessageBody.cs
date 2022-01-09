using System.Collections.Generic;
using Newtonsoft.Json;

namespace FastLivePushClient.Entity
{
    public class MessageBody
    {
        private string _title;
        private string _body;
        private Dictionary<string,string> _data = new Dictionary<string,string>();

        public string title
        {
            get => _title;
            set => _title = value;
        }

        public string body
        {
            get => _body;
            set => _body = value;
        }

        public Dictionary<string, string> data
        {
            get => _data;
            set => _data = value;
        }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal MessageBody(string title, string body , Dictionary<string,string> attachData)
        {
            _title = title;
            _body = body;
            _data = attachData;
        }

        public static MessageBody Create(string title, string body , Dictionary<string,string> attachData)
        {
            return new MessageBody(title, body, attachData);
        }
    }
}