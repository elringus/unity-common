using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a serializable container for a int named item.
    /// </summary>
    [System.Serializable]
    public class NamedInt : SerializableNamed<int>
    {
        [SerializeField] private int value = default;

        public NamedInt (string name, int value)
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
