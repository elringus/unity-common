using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public class LoadLoggerScrollRect : ScriptableUIComponent<ScrollRect>
    {
        [SerializeField] private Text loggerText = null;

        private SceneLoader sceneLoader;
        private IResourceProvider resourceProvider;

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(loggerText);

            sceneLoader = Context.Resolve<SceneLoader>(assertResult: true);
            resourceProvider = Context.Resolve<IResourceProvider>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            sceneLoader.OnScenesUnloaded += LogUnloadedScenes;
            sceneLoader.OnReadyToActivate += LogActivationReady;
            sceneLoader.OnSceneLoaded += LogLoadFinished;

            if (resourceProvider != null)
                resourceProvider.OnMessage += LogResourceProviderMessage;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            sceneLoader.OnScenesUnloaded -= LogUnloadedScenes;
            sceneLoader.OnReadyToActivate -= LogActivationReady;
            sceneLoader.OnSceneLoaded -= LogLoadFinished;

            if (resourceProvider != null)
                resourceProvider.OnMessage -= LogResourceProviderMessage;
        }

        protected override void Start ()
        {
            base.Start();

            loggerText.text = string.Empty;

            Log(string.Format("Preparing to load scene <i>{0}</i>...", sceneLoader.SceneToLoadPath));
            LogCurrentMemoryUsage();
        }

        public void Log (string message)
        {
            loggerText.text += message;
            loggerText.text += Environment.NewLine;
            UIComponent.verticalNormalizedPosition = 0;
        }

        private void LogResourceProviderMessage (string message)
        {
            Log(string.Format("{0}", message));
        }

        private void LogUnloadedScenes ()
        {
            var paths = string.Join(", ", sceneLoader.UnloadedScenesPath.ToArray());
            Log(string.Format("Finished unloading scenes: <i>{0}</i>", paths));
            LogCurrentMemoryUsage();
        }

        private void LogActivationReady ()
        {
            Log(string.Format("Scene <i>{0}</i> is loaded and waiting to be activated.", sceneLoader.SceneToLoadPath));
            LogCurrentMemoryUsage();
        }

        private void LogLoadFinished ()
        {
            Log(string.Format("Scene <i>{0}</i> was loaded and activated.", sceneLoader.SceneToLoadPath));
            LogCurrentMemoryUsage();
        }

        private void LogCurrentMemoryUsage ()
        {
            Log(string.Concat("<b>Total memory used: ", Mathf.CeilToInt(GC.GetTotalMemory(true) * .000001f), "Mb</b>"));
        }
    }
}
