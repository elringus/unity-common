using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityCommon
{
    public static class ReflectionUtils
    {
        public static IEnumerable<Type> ExportedDomainTypes { get { return cachedDomainTypes ?? (cachedDomainTypes = GetExportedDomainTypes()); } }

        private static IEnumerable<Type> cachedDomainTypes;

        public static bool IsDynamicAssembly (Assembly assembly)
        {
            #if NET_4_6 || NET_STANDARD_2_0
            return assembly.IsDynamic;
            #else
            return assembly is System.Reflection.Emit.AssemblyBuilder;
            #endif
        }

        public static IEnumerable<Assembly> GetDomainAssemblies (bool excludeDynamic = true)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return excludeDynamic ? assemblies.Where(a => !IsDynamicAssembly(a)) : assemblies;
        }

        public static IEnumerable<Type> GetExportedDomainTypes ()
        {
            return GetDomainAssemblies().SelectMany(a => a.GetExportedTypes());
        }

        /// <summary>
        /// Uses <see cref="Type.GetField(string, BindingFlags)"/>, but also includes private fields from all the base types.
        /// In case multiple fields with equal names exist in different base types, will return only the first most-derived one.
        /// </summary>
        public static FieldInfo GetFieldWithInheritence (this Type type, string fieldName, BindingFlags flags = BindingFlags.Default)
        {
            if (type is null) return null;
            var field = type.GetField(fieldName, flags);
            return field ?? GetFieldWithInheritence(type.BaseType, fieldName, flags);
        }
    }
}
