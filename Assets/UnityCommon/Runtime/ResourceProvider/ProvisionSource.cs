using System;

namespace UnityCommon
{
    /// <summary>
    /// Represents a <see cref="IResourceProvider"/> associated with a path prefix used to evaluate full path to the provider resources.
    /// </summary>
    public class ProvisionSource
    {
        /// <summary>
        /// Provider associated with the source.
        /// </summary>
        public readonly IResourceProvider Provider;
        /// <summary>
        /// Path prefix to build full paths to the provider resources.
        /// </summary>
        public readonly string PathPrefix;

        public ProvisionSource (IResourceProvider provider, string pathPrefix)
        {
            Provider = provider;
            PathPrefix = pathPrefix;
        }
        
        /// <summary>
        /// Given a local path to the resource, builds full path using predefined <see cref="PathPrefix"/>.
        /// </summary>
        public static string BuildFullPath (string pathPrefix, string localPath)
        {
            if (!string.IsNullOrWhiteSpace(pathPrefix))
            {
                if (!string.IsNullOrWhiteSpace(localPath)) return $"{pathPrefix}/{localPath}";
                else return pathPrefix;
            }
            else return localPath;
        }
        
        /// <summary>
        /// Given a full path to the resource, builds local path using predefined <see cref="PathPrefix"/>.
        /// </summary>
        public static string BuildLocalPath (string pathPrefix, string fullPath)
        {
            if (!string.IsNullOrWhiteSpace(pathPrefix))
            {
                var prefixAndSlash = $"{pathPrefix}/";
                if (!fullPath.Contains(prefixAndSlash))
                    throw new Exception($"Failed to build local path from `{fullPath}`: the provided path doesn't contain `{pathPrefix}` path prefix.");
                return fullPath.GetAfterFirst(prefixAndSlash);
            }
            else return fullPath;
        }

        /// <inheritdoc cref="BuildFullPath(string,string)"/>
        public string BuildFullPath (string localPath) => BuildFullPath(PathPrefix, localPath);
        
        /// <inheritdoc cref="BuildLocalPath(string,string)"/>
        public string BuildLocalPath (string fullPath) => BuildLocalPath(PathPrefix, fullPath);
    }
}
