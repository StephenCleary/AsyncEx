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
            return type.Assembly;
        }

        public static EventInfo GetEvent(Type type, string name)
        {
            return type.GetEvent(name);
        }

        public static IEnumerable<EventInfo> GetEvents(Type type)
        {
            return type.GetEvents();
        }

        public static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetMethod(name);
        }

        public static bool IsClass(Type type)
        {
            return type.IsClass;
        }

        public static bool IsInterface(Type type)
        {
            return type.IsInterface;
        }

        public static bool IsGenericType(Type type)
        {
            return type.IsGenericType;
        }

        public static Delegate CreateDelegate(Type type, object target, MethodInfo method)
        {
            return Delegate.CreateDelegate(type, target, method);
        }
    }
}
