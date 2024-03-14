using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SecurityTesting1.Common.Helpers
{
    public static class ProtocolBuffersHelper
    {
        private static MethodInfo _protoBufNetSerializationMethod;

        static ProtocolBuffersHelper()
        {
            //At this time, the ProtoBuf-net library only has generic methods for serialization.
            //This means we need to know the type at design time. So we are going to do serialization
            //via Reflection. The challenge is that the the Serialize<T> method is overloaded so we
            //need to find it. Putting this in a static constructor so that it is only done once for the
            //life of the application.
            _protoBufNetSerializationMethod = FindProtoBufNetSerializationMethod();
        }

        private static MethodInfo FindProtoBufNetSerializationMethod()
        {
            IEnumerable<MethodInfo> methodInfos = typeof(Serializer).GetMethods().Where(method => method.Name == nameof(Serializer.Serialize) && method.IsGenericMethod);
            foreach (MethodInfo methodInfo in methodInfos)
            {
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length == 2)
                {
                    if (parameterInfos[0].ParameterType.Name == "Stream" && parameterInfos[1].ParameterType.Name == "T")
                    {
                        return methodInfo;
                    }
                }
            }

            throw new Exception($"Cannot find the right '{nameof(Serializer.Serialize)}' method to call in ProtoBuf-net library. The APIs of that library may have changed.");
        }

        public static bool CanSerializeAsProtocolBuffers(Type type)
        {
            if (type.IsGenericType)
            {
                return Serializer.NonGeneric.CanSerialize(type.GetTypeInfo().GenericTypeArguments[0]);
            }

            return Serializer.NonGeneric.CanSerialize(type);
        }

        public static byte[] SerializeViaReflection(object obj)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                MethodInfo generic = _protoBufNetSerializationMethod.MakeGenericMethod(obj.GetType());
                object[] parameters = new object[] { ms, obj };
                generic.Invoke(obj, parameters);
                data = ms.ToArray();
            }

            return data;
        }
    }
}
