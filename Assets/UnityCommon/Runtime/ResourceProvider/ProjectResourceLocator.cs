using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

namespace UnityCommon
{
    public class ProjectResourceLocator<TResource> : LocateResourcesRunner<TResource>
        where TResource : UnityEngine.Object
    {
        private readonly IReadOnlyDictionary<string, Type> projectResources;

        public ProjectResourceLocator (IResourceProvider provider, string resourcesPath,
            IReadOnlyDictionary<string, Type> projectResources) : base(provider, resourcesPath ?? string.Empty)
        {
            this.projectResources = projectResources;
        }

        public override UniTask RunAsync ()
        {
            var locatedResourcePaths = LocateProjectResources(Path, projectResources);
            SetResult(locatedResourcePaths);
            return UniTask.CompletedTask;
        }

        public static IReadOnlyCollection<string> LocateProjectResources (string resourcesPath, IReadOnlyDictionary<string, Type> projectResources)
        {
            return projectResources.Keys.LocateResourcePathsAtFolder(resourcesPath).ToArray();
        }
    }
}
