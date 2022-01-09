using System;
using System.ComponentModel;

namespace FastLivePushClient.Payload
{
    public class EnumMessage
    {
        
     
        
        
        public enum Priority : byte
        {
            HIGH = 0,
            MIDDLE = 1,
            LOW = 2,
        }


        public static string GetEnumDescription(Enum value)
        {
           
            return ((DescriptionAttribute) Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()),
                typeof(DescriptionAttribute))).Description;
        }

        public static  PayloadCode GetPayloadCodeEnum(ushort code)
        {
            var pc = Enum.GetName(typeof(PayloadCode), code);
            var r = (PayloadCode)Enum.Parse(typeof(PayloadCode), pc);
            return r;
        }
        
        
        
        public enum PayloadCode : ushort
        {
            [Description("FastLivePushClient.Payload.HeartBeatPayload")] HeartBeatCode = 10000,
            [Description("FastLivePushClient.Payload.ConnAuthPayload")] ConnAuthCode = 20000,
            [Description("FastLivePushClient.Payload.ConnAuthRespPayload")] ConnAuthRespCode = 20001,
            [Description("FastLivePushClient.Payload.PushMessagePayload")] PushMessageCode = 30000,
            [Description("FastLivePushClient.Payload.PushMessageAckPayload")] PushMessageAckCode = 30001,
        }

        public enum SpChannel : byte
        {
            APNS = 10,
            FCM = 11,
            HMS = 12,
            APPLE_PUSHKIT = 60,
            HW_PUSHKIT = 61
        }

        public enum NotificationClassifier : byte
        {
            PUSH = 1,
            SMS = 2,
            EMAIL = 3,
            INNERMESSAGE = 4,
            VOIP = 5
        }
    }
}