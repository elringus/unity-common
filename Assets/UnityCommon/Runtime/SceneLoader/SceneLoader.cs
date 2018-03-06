using System;
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
    /// Whether to wait for the user to invoke <see cref="ActivateLoadedScene"/> before activating loaded scene.
    /// </summary>
    public bool ManualActivation { get; protected set; }
    /// <summary>
    /// Whether the new scene is loaded and ready to be activated.
    /// </summary>
    public bool IsReadyToActivate { get { return newSceneLoadOperation != null && newSceneLoadOperation.progress >= .9f;  } }

    private AsyncOperation newSceneLoadOperation;
    private Timer manualActivationLoopTimer;
    private bool manualActivationInvoked;
    private bool onReadyToActivateInvoked;

    /// <summary>
    /// Creates a new instance of the class.
    /// </summary>
    /// <param name="transitionScenePath">Relative scene file path (e.g: "Assets/Scenes/Scene1.unity").</param>
    /// <param name="unloadScenes">Whether to unload currently loaded scenes.</param>
    /// <param name="manualActivation">Whether to wait for manual scene activation.</param>
    public SceneLoader (string transitionScenePath, bool unloadScenes = true, bool manualActivation = false)
    {
        TransitionScenePath = transitionScenePath;
        UnloadScenes = unloadScenes;
        ManualActivation = manualActivation;

        // Brilliant design decision Unity, bravo!
        // https://docs.unity3d.com/ScriptReference/AsyncOperation-allowSceneActivation.html
        manualActivationLoopTimer = new Timer(.5f, true, true, onLoop: ManualActivationLoop);

        Context.Register(this);
    }

    /// <summary>
    /// Loads scene with the provided path.
    /// </summary>
    /// <param name="sceneToLoadPath">Relative scene file path (e.g: "Assets/Scenes/Scene1.unity").</param>
    public virtual AsyncAction LoadScene (string sceneToLoadPath)
    {
        SceneToLoadPath = sceneToLoadPath;
        manualActivationInvoked = false;
        onReadyToActivateInvoked = false;

        if (ManualActivation) manualActivationLoopTimer.Run();

        return LoadTransitionScene().ThenAsync(UnloadAllScenesExceptTransition).ThenAsync(LoadNewScene).ThenAsync(UnloadTransitionScene);
    }

    /// <summary>
    /// Activates the new scene when using <see cref="ManualActivation"/>.
    /// </summary>
    public virtual void ActivateLoadedScene ()
    {
        manualActivationInvoked = true;
    }

    protected virtual AsyncAction LoadTransitionScene ()
    {
        return SceneManager.LoadSceneAsync(TransitionScenePath, LoadSceneMode.Additive);
    }

    protected virtual AsyncAction UnloadAllScenesExceptTransition ()
    {
        var actions = new AsyncActionSet();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scenePath = SceneManager.GetSceneAt(i).path;
            if (scenePath == TransitionScenePath) continue;
            actions.AddAction(SceneManager.UnloadSceneAsync(scenePath));
        }
        actions.Dispose();
        return actions.Then(InvokeOnScenesUnloaded);
    }

    protected virtual AsyncAction LoadNewScene ()
    {
        newSceneLoadOperation = SceneManager.LoadSceneAsync(SceneToLoadPath, LoadSceneMode.Additive);
        newSceneLoadOperation.allowSceneActivation = !ManualActivation;
        return ((AsyncAction)newSceneLoadOperation).Then(InvokeOnSceneLoaded);
    }

    protected virtual AsyncAction UnloadTransitionScene ()
    {
        return SceneManager.UnloadSceneAsync(TransitionScenePath);
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
