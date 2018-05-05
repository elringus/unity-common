using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Allows asynchronously loading scenes and masking the process with a transition scene.
/// </summary>
public class SceneLoader 
{
    /// <summary>
    /// Event invoked when previously loaded scenes are unloaded.
    /// </summary>
    public event Action OnScenesUnloaded;
    /// <summary>
    /// Event invoked when the new scene is ready to be activated.
    /// </summary>
    public event Action OnReadyToActivate;
    /// <summary>
    /// Event invoked when the new scene is loaded.
    /// </summary>
    public event Action OnSceneLoaded;

    /// <summary>
    /// The transition scene will be asynchronously loaded before any load/unload operations
    /// and unloaded after activating the new scene to mask the scene transition process. 
    /// </summary>
    public string TransitionScenePath { get; protected set; }
    /// <summary>
    /// The new scene to load.
    /// </summary>
    public string SceneToLoadPath { get; protected set; }
    /// <summary>
    /// Whether to unload all loaded scenes before loading the new one.
    /// </summary>
    public bool UnloadScenes { get; protected set; }
    /// <summary>
    /// Whether the new scene is loaded and ready to be activated.
    /// </summary>
    public List<string> UnloadedScenesPath { get; protected set; }
    /// <summary>
    /// Whether to wait for the user to invoke <see cref="ActivateLoadedScene"/> before activating loaded scene.
    /// </summary>
    public bool ManualActivation { get; protected set; }
    /// <summary>
    /// Whether the new scene is loaded and ready to be activated.
    /// </summary>
    public bool IsReadyToActivate { get { return newSceneLoadOperation != null && newSceneLoadOperation.progress >= .9f;  } }
    /// <summary>
    /// Whether the loading operation is currently in progress.
    /// </summary>
    public bool IsLoading { get; protected set; }

    private AsyncOperation newSceneLoadOperation;
    private Timer manualActivationLoopTimer;
    private bool manualActivationInvoked;
    private bool onReadyToActivateInvoked;

    public SceneLoader ()
    {
        UnloadedScenesPath = new List<string>();
    }

    /// <summary>
    /// Loads scene with the provided path.
    /// </summary>
    /// <param name="sceneToLoadPath">Relative scene file path (e.g: "Assets/Scenes/Scene1.unity").</param>
    /// <param name="transitionScenePath">Relative scene file path (e.g: "Assets/Scenes/Scene1.unity").</param>
    /// <param name="unloadScenes">Whether to unload currently loaded scenes.</param>
    /// <param name="manualActivation">Whether to wait for manual scene activation.</param>
    public virtual async Task LoadSceneAsync (string sceneToLoadPath, string transitionScenePath, bool unloadScenes = true, bool manualActivation = false)
    {
        if (IsLoading)
        {
            Debug.LogError(string.Format("Can't load scene '{0}': another scene ('{1}') is being loaded.", sceneToLoadPath, SceneToLoadPath));
            return;
        }

        IsLoading = true;
        TransitionScenePath = transitionScenePath;
        UnloadScenes = unloadScenes;
        ManualActivation = manualActivation;
        SceneToLoadPath = sceneToLoadPath;
        manualActivationInvoked = false;
        onReadyToActivateInvoked = false;

        if (ManualActivation)
        {
            // Brilliant design decision Unity, bravo!
            // https://docs.unity3d.com/ScriptReference/AsyncOperation-allowSceneActivation.html
            manualActivationLoopTimer = new Timer(.5f, true, true, onLoop: ManualActivationLoop);
            manualActivationLoopTimer.Run();
        }

        await LoadTransitionSceneAsync();
        await UnloadAllScenesExceptTransitionAsync();
        await LoadNewSceneAsync();
        await UnloadTransitionSceneAsync();

        FinishLoading();
    }

    /// <summary>
    /// Activates the new scene when using <see cref="ManualActivation"/>.
    /// </summary>
    public virtual void ActivateLoadedScene ()
    {
        manualActivationInvoked = true;
    }

    protected virtual async Task LoadTransitionSceneAsync () => await SceneManager.LoadSceneAsync(TransitionScenePath, LoadSceneMode.Additive);

    protected virtual async Task UnloadAllScenesExceptTransitionAsync ()
    {
        UnloadedScenesPath.Clear();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scenePath = SceneManager.GetSceneAt(i).path;
            if (scenePath == TransitionScenePath) continue;
            UnloadedScenesPath.Add(scenePath);
            await SceneManager.UnloadSceneAsync(scenePath);
        }
        InvokeOnScenesUnloaded();
    }

    protected virtual async Task LoadNewSceneAsync ()
    {
        newSceneLoadOperation = SceneManager.LoadSceneAsync(SceneToLoadPath, LoadSceneMode.Additive);
        newSceneLoadOperation.allowSceneActivation = !ManualActivation;
        await newSceneLoadOperation;
    }

    protected virtual async Task UnloadTransitionSceneAsync () => await SceneManager.UnloadSceneAsync(TransitionScenePath);

    protected virtual void FinishLoading ()
    {
        IsLoading = false;
        InvokeOnSceneLoaded();
    }

    protected void InvokeOnScenesUnloaded ()
    {
        if (OnScenesUnloaded != null) OnScenesUnloaded.Invoke();
    }

    protected void InvokeOnReadyToActivate ()
    {
        if (OnReadyToActivate != null) OnReadyToActivate.Invoke();
    }

    protected void InvokeOnSceneLoaded ()
    {
        if (OnSceneLoaded != null) OnSceneLoaded.Invoke();
    }

    private void ManualActivationLoop ()
    {
        if (IsReadyToActivate && !onReadyToActivateInvoked)
        {
            onReadyToActivateInvoked = true;
            InvokeOnReadyToActivate();
        }

        if (IsReadyToActivate && manualActivationInvoked)
        {
            manualActivationLoopTimer.Stop();
            newSceneLoadOperation.allowSceneActivation = true;
        }
    }
}
