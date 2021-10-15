using System;
using System.Threading;

namespace UnityCommon.Async.Internal
{
    internal sealed class ContinuationQueue
    {
        private const int MaxArrayLength = 0X7FEFFFFF;
        private const int InitialSize = 16;

        private readonly PlayerLoopTiming timing;

        private SpinLock gate = new SpinLock(false);
        private bool dequing = false;

        private int actionListCount = 0;
        private Action[] actionList = new Action[InitialSize];

        private int waitingListCount = 0;
        private Action[] waitingList = new Action[InitialSize];

        public ContinuationQueue (PlayerLoopTiming timing)
        {
            this.timing = timing;
        }

        public void Enqueue (Action continuation)
        {
            bool lockTaken = false;
            try
            {
                gate.Enter(ref lockTaken);

                if (dequing)
                {
                    // Ensure Capacity
                    if (waitingList.Length == waitingListCount)
                    {
                        var newLength = waitingListCount * 2;
                        if ((uint)newLength > MaxArrayLength) newLength = MaxArrayLength;

                        var newArray = new Action[newLength];
                        Array.Copy(waitingList, newArray, waitingListCount);
                        waitingList = newArray;
                    }
                    waitingList[waitingListCount] = continuation;
                    waitingListCount++;
                }
                else
                {
                    // Ensure Capacity
                    if (actionList.Length == actionListCount)
                    {
                        var newLength = actionListCount * 2;
                        if ((uint)newLength > MaxArrayLength) newLength = MaxArrayLength;

                        var newArray = new Action[newLength];
                        Array.Copy(actionList, newArray, actionListCount);
                        actionList = newArray;
                    }
                    actionList[actionListCount] = continuation;
                    actionListCount++;
                }
            }
            finally
            {
                if (lockTaken) gate.Exit(false);
            }
        }

        public int Clear ()
        {
            var rest = actionListCount + waitingListCount;

            actionListCount = 0;
            actionList = new Action[InitialSize];

            waitingListCount = 0;
            waitingList = new Action[InitialSize];

            return rest;
        }

        // delegate entrypoint.
        public void Run ()
        {
            // for debugging, create named stacktrace.
            #if DEBUG
            switch (timing)
            {
                case PlayerLoopTiming.Initialization:
                    Initialization();
                    break;
                case PlayerLoopTiming.LastInitialization:
                    LastInitialization();
                    break;
                case PlayerLoopTiming.EarlyUpdate:
                    EarlyUpdate();
                    break;
                case PlayerLoopTiming.LastEarlyUpdate:
                    LastEarlyUpdate();
                    break;
                case PlayerLoopTiming.FixedUpdate:
                    FixedUpdate();
                    break;
                case PlayerLoopTiming.LastFixedUpdate:
                    LastFixedUpdate();
                    break;
                case PlayerLoopTiming.PreUpdate:
                    PreUpdate();
                    break;
                case PlayerLoopTiming.LastPreUpdate:
                    LastPreUpdate();
                    break;
                case PlayerLoopTiming.Update:
                    Update();
                    break;
                case PlayerLoopTiming.LastUpdate:
                    LastUpdate();
                    break;
                case PlayerLoopTiming.PreLateUpdate:
                    PreLateUpdate();
                    break;
                case PlayerLoopTiming.LastPreLateUpdate:
                    LastPreLateUpdate();
                    break;
                case PlayerLoopTiming.PostLateUpdate:
                    PostLateUpdate();
                    break;
                case PlayerLoopTiming.LastPostLateUpdate:
                    LastPostLateUpdate();
                    break;
                #if UNITY_2020_2_OR_NEWER
                case PlayerLoopTiming.TimeUpdate:
                    TimeUpdate();
                    break;
                case PlayerLoopTiming.LastTimeUpdate:
                    LastTimeUpdate();
                    break;
                #endif
                default:
                    break;
            }
            #else
            RunCore();
            #endif
        }

        private void Initialization () => RunCore();
        private void LastInitialization () => RunCore();
        private void EarlyUpdate () => RunCore();
        private void LastEarlyUpdate () => RunCore();
        private void FixedUpdate () => RunCore();
        private void LastFixedUpdate () => RunCore();
        private void PreUpdate () => RunCore();
        private void LastPreUpdate () => RunCore();
        private void Update () => RunCore();
        private void LastUpdate () => RunCore();
        private void PreLateUpdate () => RunCore();
        private void LastPreLateUpdate () => RunCore();
        private void PostLateUpdate () => RunCore();
        private void LastPostLateUpdate () => RunCore();
        #if UNITY_2020_2_OR_NEWER
        void TimeUpdate() => RunCore();
        void LastTimeUpdate() => RunCore();
        #endif

        [System.Diagnostics.DebuggerHidden]
        private void RunCore ()
        {
            {
                bool lockTaken = false;
                try
                {
                    gate.Enter(ref lockTaken);
                    if (actionListCount == 0) return;
                    dequing = true;
                }
                finally
                {
                    if (lockTaken) gate.Exit(false);
                }
            }

            for (int i = 0; i < actionListCount; i++)
            {
                var action = actionList[i];
                actionList[i] = null;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }

            {
                bool lockTaken = false;
                try
                {
                    gate.Enter(ref lockTaken);
                    dequing = false;

                    var swapTempActionList = actionList;

                    actionListCount = waitingListCount;
                    actionList = waitingList;

                    waitingListCount = 0;
                    waitingList = swapTempActionList;
                }
                finally
                {
                    if (lockTaken) gate.Exit(false);
                }
            }
        }
    }
}
