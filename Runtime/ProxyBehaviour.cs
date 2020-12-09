using System;
using UnityEngine;

namespace UnityCommon
{
    public class ProxyBehaviour : MonoBehaviour
    {
        public event Action OnBehaviourAwake;
        public event Action OnBehaviourEnable;
        public event Action OnBehaviourStart;
        public event Action OnBehaviourUpdate;
        public event Action OnBehaviourDisable;
        public event Action OnBehaviourDestroy;

        private void Awake ()
        {
            OnBehaviourAwake?.Invoke();
        }

        private void OnEnable ()
        {
            OnBehaviourEnable?.Invoke();
        }

        private void Start ()
        {
            OnBehaviourStart?.Invoke();
        }

        private void Update ()
        {
            OnBehaviourUpdate?.Invoke();
        }

        private void OnDisable ()
        {
            OnBehaviourDisable?.Invoke();
        }

        private void OnDestroy ()
        {
            OnBehaviourDestroy?.Invoke();
        }
    }
}
