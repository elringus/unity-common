using System;
using UnityEngine;

namespace UnityCommon
{
    public class ProxyTrigger : MonoBehaviour
    {
        public event Action<Collider> OnProxyTriggerEnter;
        public event Action<Collider> OnProxyTriggerStay;
        public event Action<Collider> OnProxyTriggerExit;

        private void OnTriggerEnter (Collider other)
        {
            OnProxyTriggerEnter?.Invoke(other);
        }

        private void OnTriggerStay (Collider other)
        {
            OnProxyTriggerStay?.Invoke(other);
        }

        private void OnTriggerExit (Collider other)
        {
            OnProxyTriggerExit?.Invoke(other);
        }
    }
}
