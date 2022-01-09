using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FastLivePushClient.CoreLib;
using FastLivePushClient.Payload;
using NLog;
using NLog.Fluent;

namespace FastLivePushClient.Container
{
    public class ComponentScan
    {
        private static Dictionary<EnumMessage.PayloadCode, ObjDispatchMapping> _instanceContainer =
            new Dictionary<EnumMessage.PayloadCode, ObjDispatchMapping>();
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static string CONNTROLLER_NAMESPACES = "FastLivePushClient.Controller";


        public static Dictionary<EnumMessage.PayloadCode, ObjDispatchMapping> InstanceContainer
        {
            get => _instanceContainer;
            set => _instanceContainer = value;
        }

        public static void Scan()
        {
            var scandClazzes = ComponentScanWithNameSpace(CONNTROLLER_NAMESPACES);
            var clazzes = GetTypesWithAttribute(typeof(MessageController), scandClazzes);
            logger.Info("scanned {0} types", clazzes.Count);
            if (!clazzes.Any()) return;
            foreach (var clazz in clazzes)
            {
                // 取得所有方法
                var methods = clazz.GetMethods().Where(m => m.IsPublic && !m.IsStatic && !m.IsAbstract).ToArray();
                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute(typeof(MessageMapping)) == null) continue;
                    // 放入容器
                    var obj = Activator.CreateInstance(clazz);
                    var mapping = new ObjDispatchMapping(obj, method);
                    var messageMappingAttr = (MessageMapping) method.GetCustomAttribute(typeof(MessageMapping));
                    _instanceContainer.Add(messageMappingAttr.Value, mapping);
                }

                logger.Info("producing {0} keys and {1} values  ", _instanceContainer.Count, _instanceContainer.Count);
            }
        }


        public static HashSet<Type> ComponentScanWithNameSpace(string namespec)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(t => String.Equals(t.Namespace, namespec, StringComparison.Ordinal))
                .ToArray();
            var typeSet = new HashSet<Type>();
            foreach (var type in types)
            {
                typeSet.Add(type);
            }

            return typeSet;
        }

        public static HashSet<Type> GetTypesWithAttribute(Type attributeType, HashSet<Type> clazzes)
        {
            var types = clazzes.Where(t => t.GetCustomAttribute(attributeType) != null).ToArray();
            var typeSet = new HashSet<Type>();
            foreach (var type in types)
            {
                typeSet.Add(type);
            }

            return typeSet;
        }
    }
}