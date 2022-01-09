using FastLivePushClient.Payload;

namespace FastLivePushClient.Entity
{
    public class Notification
    {
        internal  string _toUid;
        internal  EnumMessage.Priority _messagePriority;
        internal MessageBody _messageBody;

        internal Notification(string uid, EnumMessage.Priority messagePriority, MessageBody messageBody)
        {
            _toUid = uid;
            _messagePriority = messagePriority;
            _messageBody = messageBody;
        }

        public static Notification Create(string uid, EnumMessage.Priority priority, MessageBody messageBody)
        {
            return  new Notification(uid, priority, messageBody);
        }
        
        
        
        
    }
}