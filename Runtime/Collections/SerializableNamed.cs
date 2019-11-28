using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a serializable container for a generic named item.
    /// </summary>
    /// <typeparam name="TValue">Type of the value</typeparam>
    [System.Serializable]
    public abstract class SerializableNamed<TValue> : Named<TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] string name = default;

        public SerializableNamed (string name, TValue value)
            : base(name, value) { }

        public virtual void OnAfterDeserialize ()
        {
            Name = name;
        }

        public virtual void OnBeforeSerialize ()
        {
            name = Name;
        }
    }
}
