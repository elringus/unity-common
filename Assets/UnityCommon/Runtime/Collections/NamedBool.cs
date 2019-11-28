using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a serializable container for a bool named item.
    /// </summary>
    [System.Serializable]
    public class NamedBool : SerializableNamed<bool>
    {
        [SerializeField] private bool value = default;

        public NamedBool (string name, bool value)
            : base(name, value) { }

        public override void OnAfterDeserialize ()
        {
            base.OnAfterDeserialize();

            Value = value;
        }

        public override void OnBeforeSerialize ()
        {
            base.OnBeforeSerialize();

            value = Value;
        }
    }
}
