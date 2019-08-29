using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityCommon
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Cached domain exported types from the non-dynamic assemblies.
        /// </summary>
        public static HashSet<Type> ExportedDomainTypes => cachedDomainTypes ?? (cachedDomainTypes = GetExportedDomainTypes());

        private static HashSet<Type> cachedDomainTypes;

        public static bool IsDynamicAssembly (Assembly assembly)
        {
            #if NET_4_6 || NET_STANDARD_2_0
            return assembly.IsDynamic;
            #else
            return assembly is System.Reflection.Emit.AssemblyBuilder;
            #endif
        }

        public static HashSet<Assembly> GetDomainAssemblies (bool excludeDynamic = true)
        {
            var result = new HashSet<Assembly>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            result.UnionWith(excludeDynamic ? assemblies.Where(a => !IsDynamicAssembly(a)) : assemblies);
            return result;
        }

        public static HashSet<Type> GetExportedDomainTypes ()
        {
            var result = new HashSet<Type>();
            result.UnionWith(GetDomainAssemblies().SelectMany(a => a.GetExportedTypes()));
            return result;
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
