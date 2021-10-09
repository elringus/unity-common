using System;
using System.Linq;
using System.Threading;
using UnityCommon.Async.Internal;
using UnityEngine;
using UnityEngine.LowLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityCommon.Async
{
    public static class UniTaskLoopRunners
    {
        public struct UniTaskLoopRunnerInitialization { }
        public struct UniTaskLoopRunnerEarlyUpdate { }
        public struct UniTaskLoopRunnerFixedUpdate { }
        public struct UniTaskLoopRunnerPreUpdate { }
        public struct UniTaskLoopRunnerUpdate { }
        public struct UniTaskLoopRunnerPreLateUpdate { }
        public struct UniTaskLoopRunnerPostLateUpdate { }

        // Yield

        public struct UniTaskLoopRunnerYieldInitialization { }
        public struct UniTaskLoopRunnerYieldEarlyUpdate { }
        public struct UniTaskLoopRunnerYieldFixedUpdate { }
        public struct UniTaskLoopRunnerYieldPreUpdate { }
        public struct UniTaskLoopRunnerYieldUpdate { }
        public struct UniTaskLoopRunnerYieldPreLateUpdate { }
        public struct UniTaskLoopRunnerYieldPostLateUpdate { }
    }

    public interface IPlayerLoopItem
    {
        bool MoveNext ();
    }

    public static class PlayerLoopHelper
    {
        public static SynchronizationContext UnitySynchronizationContext => unitySynchronizationContext;
        public static int MainThreadId => mainThreadId;

        private static int mainThreadId;
        private static SynchronizationContext unitySynchronizationContext;
        private static ContinuationQueue[] yielders;
        private static PlayerLoopRunner[] runners;

        private static PlayerLoopSystem[] InsertRunner (PlayerLoopSystem loopSystem, Type loopRunnerYieldType,
            ContinuationQueue cq, Type loopRunnerType, PlayerLoopRunner runner)
        {
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.EnteredEditMode ||
                    state == PlayModeStateChange.EnteredPlayMode) return;

                if (runner != null)
                    runner.Clear();
                if (cq != null)
                    cq.Clear();
            };
            #endif

            var yieldLoop = new PlayerLoopSystem {
                type = loopRunnerYieldType,
                updateDelegate = cq.Run
            };

            var runnerLoop = new PlayerLoopSystem {
                type = loopRunnerType,
                updateDelegate = runner.Run
            };

            var source = loopSystem.subSystemList // Remove items from previous initializations.
                .Where(ls => ls.type != loopRunnerYieldType && ls.type != loopRunnerType).ToArray();
            var dest = new PlayerLoopSystem[source.Length + 2];
            Array.Copy(source, 0, dest, 2, source.Length);
            dest[0] = yieldLoop;
            dest[1] = runnerLoop;
            return dest;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init ()
        {
            // capture default(unity) sync-context.
            unitySynchronizationContext = SynchronizationContext.Current;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;

            #if UNITY_EDITOR
            // When domain reload is disabled, re-initialization is required when entering play mode; 
            // otherwise, pending tasks will leak between play mode sessions.
            var domainReloadDisabled = EditorSettings.enterPlayModeOptionsEnabled &&
                                       EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload);
            if (!domainReloadDisabled && runners != null) return;
            #else
            if (runners != null) return; // already initialized
            #endif

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            Initialize(ref playerLoop);
        }

        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitOnEditor ()
        {
            //Execute the play mode init method
            Init();

            //register an Editor update delegate, used to forcing playerLoop update
            EditorApplication.update += ForceEditorPlayerLoopUpdate;
        }

        private static void ForceEditorPlayerLoopUpdate ()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                EditorApplication.isUpdating)
            {
                // Not in Edit mode, don't interfere
                return;
            }

            //force unity to update PlayerLoop callbacks
            EditorApplication.QueuePlayerLoopUpdate();
        }

        #endif

        public static void Initialize (ref PlayerLoopSystem playerLoop)
        {
            yielders = new ContinuationQueue[7];
            runners = new PlayerLoopRunner[7];

            var copyList = playerLoop.subSystemList.ToArray();

            copyList[0].subSystemList = InsertRunner(copyList[0], typeof(UniTaskLoopRunners.UniTaskLoopRunnerYieldInitialization), yielders[0] = new ContinuationQueue(), typeof(UniTaskLoopRunners.UniTaskLoopRunnerInitialization), runners[0] = new PlayerLoopRunner());
            copyList[1].subSystemList = InsertRunner(copyList[1], typeof(UniTaskLoopRunners.UniTaskLoopRunnerYieldEarlyUpdate), yielders[1] = new ContinuationQueue(), typeof(UniTaskLoopRunners.UniTaskLoopRunnerEarlyUpdate), runners[1] = new PlayerLoopRunner());
            copyList[2].subSystemList = InsertRunner(copyList[2], typeof(UniTaskLoopRunners.UniTaskLoopRunnerYieldFixedUpdate), yielders[2] = new ContinuationQueue(), typeof(UniTaskLoopRunners.UniTaskLoopRunnerFixedUpdate), runners[2] = new PlayerLoopRunner());
            copyList[3].subSystemList = InsertRunner(copyList[3], typeof(UniTaskLoopRunners.UniTaskLoopRunnerYieldPreUpdate), yielders[3] = new ContinuationQueue(), typeof(UniTaskLoopRunners.UniTaskLoopRunnerPreUpdate), runners[3] = new PlayerLoopRunner());
            copyList[4].subSystemList = InsertRunner(copyList[4], typeof(UniTaskLoopRunners.UniTaskLoopRunnerYieldUpdate), yielders[4] = new ContinuationQueue(), typeof(UniTaskLoopRunners.UniTaskLoopRunnerUpdate), runners[4] = new PlayerLoopRunner());
            copyList[5].subSystemList = InsertRunner(copyList[5], typeof(UniTaskLoopRunners.UniTaskLoopRunnerYieldPreLateUpdate), yielders[5] = new ContinuationQueue(), typeof(UniTaskLoopRunners.UniTaskLoopRunnerPreLateUpdate), runners[5] = new PlayerLoopRunner());
            copyList[6].subSystemList = InsertRunner(copyList[6], typeof(UniTaskLoopRunners.UniTaskLoopRunnerYieldPostLateUpdate), yielders[6] = new ContinuationQueue(), typeof(UniTaskLoopRunners.UniTaskLoopRunnerPostLateUpdate), runners[6] = new PlayerLoopRunner());

            playerLoop.subSystemList = copyList;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void AddAction (PlayerLoopTiming timing, IPlayerLoopItem action)
        {
            runners[(int)timing].AddAction(action);
        }

        public static void AddContinuation (PlayerLoopTiming timing, Action continuation)
        {
            yielders[(int)timing].Enqueue(continuation);
        }
    }
}
