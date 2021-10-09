using System;
using System.Threading;

namespace UnityCommon.Async.Internal
{
    internal class ContinuationQueue
    {
        private const int MaxArrayLength = 0X7FEFFFFF;
        private const int InitialSize = 16;

        private SpinLock gate = new SpinLock();
        private bool dequing = false;

        private int actionListCount = 0;
        private Action[] actionList = new Action[InitialSize];

        private int waitingListCount = 0;
        private Action[] waitingList = new Action[InitialSize];

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

        public void Clear ()
        {
            actionListCount = 0;
            actionList = new Action[InitialSize];

            waitingListCount = 0;
            waitingList = new Action[InitialSize];
        }

        public void Run ()
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

                action();
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
