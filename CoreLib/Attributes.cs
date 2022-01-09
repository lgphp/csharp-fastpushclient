using System;
using FastLivePushClient.Payload;

namespace FastLivePushClient.CoreLib
{
    [AttributeUsage(AttributeTargets.Class,
        AllowMultiple = true)]
    public class MessageController : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = true)]
    public class MessageMapping : Attribute
    {
        private EnumMessage.PayloadCode value;

        public MessageMapping(EnumMessage.PayloadCode value)
        {
            this.value = value;
        }

        public EnumMessage.PayloadCode Value
        {
            get => value;
            set => this.value = value;
        }
    }
}