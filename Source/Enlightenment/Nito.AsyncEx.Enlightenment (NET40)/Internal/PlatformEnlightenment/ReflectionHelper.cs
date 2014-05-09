using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    // TOOD: remove unused members
    public static class ReflectionHelper
    {
        public static Type Type(string typeName)
        {
            try
            {
                return System.Type.GetType(typeName, false);
            }
            catch (ArgumentException)
            {
            }
            catch (TargetInvocationException)
            {
            }
            catch (TypeLoadException)
            {
            }
            catch (IOException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            return null;
        }

        public static MemberExpression Property(Type type, string propertyName)
        {
            if (type == null)
                return null;
            try
            {
                var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                if (prop == null)
                    return null;
                return Expression.Property(null, prop);
            }
            catch (ArgumentException)
            {
            }
            catch (AmbiguousMatchException)
            {
            }
            return null;
        }

        public static MemberExpression Property(Expression instance, string propertyName)
        {
            if (instance == null)
                return null;
            try
            {
                return Expression.Property(instance, propertyName);
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        public static T Compile<T>(Expression body, params ParameterExpression[] parameters) where T : class
        {
            if (body == null || parameters.Any(x => x == null))
                return null;
            try
            {
                return Expression.Lambda<T>(body, parameters).Compile();
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        public static MethodCallExpression Call(Type type, string methodName, params Expression[] arguments)
        {
            if (type == null || arguments.Any(x => x == null))
                return null;
            try
            {
                return Expression.Call(type, methodName, null, arguments);
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }

        public static MethodCallExpression Call(Expression instance, string methodName, params Expression[] arguments)
        {
            if (instance == null || arguments.Any(x => x == null))
                return null;
            try
            {
                return Expression.Call(instance, methodName, null, arguments);
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }

        public static MethodCallExpression Call(Expression instance, string methodName, BindingFlags flags, params Expression[] arguments)
        {
            if (instance == null || arguments.Any(x => x == null))
                return null;
            MethodInfo method;
            try
            {
                method = instance.Type.GetMethod(methodName, flags);
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }
            try
            {
                return Expression.Call(instance, method, arguments);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static InvocationExpression Invoke(Expression instance, params Expression[] arguments)
        {
            if (instance == null || arguments.Any(x => x == null))
                return null;
            try
            {
                return Expression.Invoke(instance, arguments);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }

        public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters)
        {
            if (delegateType == null || body == null || parameters.Any(x => x == null))
                return null;
            try
            {
                return Expression.Lambda(delegateType, body, parameters);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }

        public static ConstantExpression Constant(Type type, object value)
        {
            if (type == null)
                return null;
            try
            {
                return Expression.Constant(value, type);
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        public static UnaryExpression Convert(Expression instance, Type type)
        {
            if (instance == null || type == null)
                return null;
            try
            {
                return Expression.Convert(instance, type);
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }

        public static T? EnumValue<T>(string name) where T : struct
        {
            try
            {
                return (T)Enum.Parse(typeof(T), name, true);
            }
            catch (ArgumentException)
            {
            }
            catch (OverflowException)
            {
            }
            return null;
        }

        public static UnaryExpression EnumValue(Type type, string name)
        {
            try
            {
                var value = Enum.Parse(type, name, true);
                return Convert(Constant(type, value), type);
            }
            catch (ArgumentException)
            {
            }
            catch (OverflowException)
            {
            }
            return null;
        }

        public static BinaryExpression Equal(Expression a, Expression b)
        {
            try
            {
                return Expression.Equal(a, b);
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }
    }
}
