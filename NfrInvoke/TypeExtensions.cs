using System;

namespace NFRInvoke
{
    static class TypeExtensions
    {
        /// <summary>
        /// Emulates default(T) for <paramref name="type"/> Isn't perfect for value types, as it will fail equality tests
        /// </summary>
        /// <returns></returns>
        public static object GetDefault(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>Tests whether <paramref name="value"/> is the default(T) for its type.</summary>
        public static bool IsDefaultValue(this object value)
        {
            return value == null || value.Equals(value.GetType().GetDefault());
        }
    }
}