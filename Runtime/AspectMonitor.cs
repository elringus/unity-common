using System;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Allows monitoring screen aspect ratio and receive events when it changes.
    /// </summary>
    public class AspectMonitor
    {
        /// <summary>
        /// Invoked when screen aspect ratio is changed.
        /// </summary>
        public event Action<AspectMonitor> OnChanged;
        
        /// <summary>
        /// Last updated screen aspect ratio.
        /// </summary>
        public float CurrentAspect { get; private set; }
        /// <summary>
        /// Aspect ratio as it was on the update before the last one.
        /// </summary>
        public float PreviousAspect { get; private set; }
        
        private readonly Timer timer;

        public AspectMonitor ()
        {
            timer = new Timer(onLoop: Update);
            Update();
        }

        /// <summary>
        /// Starts monitoring screen aspect ratio.
        /// </summary>
        /// <param name="updateDelay">How frequently update the values, in seconds.</param>
        /// <param name="target">When provided and becomes invalid, monitoring will automatically stop.</param>
        public void Start (float updateDelay = .5f, UnityEngine.Object target = default, 
            in AsyncToken asyncToken = default)
        { 
            timer.Run(updateDelay, true, true, asyncToken, target);
        } 
        
        /// <summary>
        /// Stop the monitoring.
        /// </summary>
        public void Stop () => timer.Stop();

        private void Update ()
        {
            CurrentAspect = (float)Screen.width / Screen.height;
            
            if (Mathf.Approximately(CurrentAspect, PreviousAspect)) return;
            
            OnChanged?.Invoke(this);
            PreviousAspect = CurrentAspect;
        }
    }
}
