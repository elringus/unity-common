using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The object of class will be auto-registered in Context on scene load (before Awake calls).
/// Only works for MonoBehaviours already placed on scene.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterInContext : Attribute { }

/// <summary>
/// When resolving in context, if not registered, an instance of component object will 
/// be auto-spawned (attached to a new empty gameobject) and registered. Only works for MonoBehaviours.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SpawnOnContextResolve : Attribute
{
    public readonly HideFlags HideFlags;

    public SpawnOnContextResolve (HideFlags hideFlags = HideFlags.None)
    {
        HideFlags = hideFlags;
    }
}

/// <summary>
/// Keeps weak references to registered objects.
/// </summary>
public class Context : MonoBehaviour
{
    const float GC_INTERVAL = 60;

    public static bool IsInitialized { get { return instance != null && instance; } }

    private static Context instance;
    private Dictionary<Type, List<WeakReference>> references;

    private void Start ()
    {
        StartCoroutine(RemoveDeadReferences());
    }

    private void OnDestroy ()
    {
        if (references != null)
            references.Clear();
        instance = null;
    }

    public static T Resolve<T> (Predicate<T> predicate = null, bool strictType = true) where T : class
    {
        if (!AssertUsage()) return null;

        T result = null;
        var resolvingType = typeof(T);
        if (resolvingType.IsInterface || resolvingType.IsAbstract)
            strictType = false;

        var refsOfType = GetReferencesOfType(resolvingType, strictType);
        if (refsOfType != null && refsOfType.Count > 0)
        {
            var weakRef = refsOfType.FirstOrDefault(r => IsWeakRefValid(r) &&
                (predicate == null || predicate(r.Target as T)));
            if (weakRef != null) result = weakRef.Target as T;
        }

        if (result == null && ShouldAutoSpawn(resolvingType))
            return SpawnAndRegister(resolvingType) as T;

        return result;
    }

    public static List<T> ResolveAll<T> (Predicate<T> predicate = null, bool strictType = true) where T : class
    {
        if (!AssertUsage()) return null;

        var resolvingType = typeof(T);
        if (resolvingType.IsInterface || resolvingType.IsAbstract)
            strictType = false;

        var refsOfType = GetReferencesOfType(resolvingType, strictType);
        if (refsOfType == null || refsOfType.Count == 0)
            return new List<T>();

        return refsOfType
            .Where(r => IsWeakRefValid(r) && (predicate == null || predicate(r.Target as T)))
            .Select(r => r.Target)
            .Cast<T>()
            .ToList();
    }

    public static bool IsRegistered (object obj, bool strictType = true)
    {
        if (!AssertUsage()) return false;

        var refsOfType = GetReferencesOfType(obj.GetType(), strictType);
        if (refsOfType == null || refsOfType.Count == 0)
            return false;

        return refsOfType.Exists(r => IsWeakRefValid(r) && r.Target == obj);
    }

    public static void Register (object obj)
    {
        if (!AssertUsage()) return;

        if (obj == null)
        {
            Debug.LogWarning("Attempted to register a null object to Context.");
            return;
        }

        if (IsRegistered(obj))
        {
            Debug.LogWarning("Attempted to re-register the same object to Context.");
            return;
        }

        var objType = obj.GetType();
        var reference = new WeakReference(obj);

        if (instance.references.ContainsKey(objType))
            instance.references[objType].Add(reference);
        else instance.references.Add(objType, new List<WeakReference>() { reference } );
    }

    [ContextMenu("Log Reference Count")]
    public void LogReferenceCount ()
    {
        var total = references.Values.Select(refList => refList.Count).Sum();
        var valid = references.Values.Select(refList => refList.Count(r => IsWeakRefValid(r))).Sum();
        Debug.Log(string.Format("Context: {0} total and {1} valid references", total, valid));
    }

    private static List<WeakReference> GetReferencesOfType (Type type, bool strictType = true)
    {
        if (strictType)
        {
            List<WeakReference> result;
            instance.references.TryGetValue(type, out result);
            return result;
        }
        else return instance.references
                .Where(kv => type.IsAssignableFrom(kv.Key))
                .SelectMany(kv => kv.Value)
                .ToList();
    }

    private static void Initialize ()
    {
        var gameobject = new GameObject("Context");
        gameobject.hideFlags = HideFlags.HideInHierarchy;
        instance = gameobject.AddComponent<Context>();
        instance.references = new Dictionary<Type, List<WeakReference>>();
        RegisterSceneObjects();
    }

    private static void RegisterSceneObjects ()
    {
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb.GetType().IsDefined(typeof(RegisterInContext), true))
                Register(mb);
        }
    }

    private static object SpawnAndRegister (Type type, HideFlags hideFlags = HideFlags.None)
    {
        if (!type.IsSubclassOf(typeof(Component))) return null;

        if (type.IsDefined(typeof(SpawnOnContextResolve), true))
        {
            var attrs = (SpawnOnContextResolve[])type
                .GetCustomAttributes(typeof(SpawnOnContextResolve), true);
            if (attrs.Length > 0)
            {
                var attr = attrs[0];
                hideFlags = attr.HideFlags;
            }
        }

        var containerObject = new GameObject(type.Name);
        containerObject.hideFlags = hideFlags;
        var component = containerObject.AddComponent(type);
        Register(component);
        return component;
    }

    private static bool ShouldAutoSpawn (Type type)
    {
        return type.IsSubclassOf(typeof(Component)) && 
            type.IsDefined(typeof(SpawnOnContextResolve), true);
    }

    private static bool AssertUsage ()
    {
        if (!Application.isPlaying)
        {
            //Debug.LogError("Context is only avialable in play mode.");
            return false;
        }

        if (!IsInitialized) Initialize();

        return true;
    }

    private static bool IsWeakRefValid (WeakReference weakRef)
    {
        if (weakRef == null) return false;
        var targetCopy = weakRef.Target; // To prevent race conditions.
        if (targetCopy == null) return false;

        // Check Unity objects internal (C++) state.
        if (targetCopy is UnityEngine.Object)
        {
            var unityObject = targetCopy as UnityEngine.Object;
            if (unityObject == null || !unityObject) return false;
        }

        return true;
    }

    private IEnumerator RemoveDeadReferences ()
    {
        while (true)
        {
            references.Values.ToList()
                .ForEach(refList => refList
                .RemoveAll(r => !r.IsAlive));

            yield return new WaitForSeconds(GC_INTERVAL);
        }
    }
}
