using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nito.AsyncEx.Internal
{
    internal static class ReflectionShim
    {
        public static Assembly GetAssembly(Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static EventInfo GetEvent(Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredEvent(name);
        }

        public static IEnumerable<EventInfo> GetEvents(Type type)
        {
            return type.GetTypeInfo().DeclaredEvents;
        }

        public static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }

        public static bool IsClass(Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsInterface(Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsGenericType(Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static Delegate CreateDelegate(Type type, object target, MethodInfo method)
        {
            return method.CreateDelegate(type, target);
        }
    }
}
