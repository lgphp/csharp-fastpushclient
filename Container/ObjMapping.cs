using System.Reflection;
using FastLivePushClient.Payload;

namespace FastLivePushClient.Container
{
    public struct ObjDispatchMapping 
    {
        private object obj;
        private MethodInfo method;

        public ObjDispatchMapping(object obj, MethodInfo method)
        {
            this.obj = obj;
            this.method = method;
        }

        public object Obj
        {
            get => obj;
            set => obj = value;
        }

        public MethodInfo Method
        {
            get => method;
            set => method = value;
        }
    }
}