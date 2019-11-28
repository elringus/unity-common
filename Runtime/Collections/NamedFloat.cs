using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a serializable container for a float named item.
    /// </summary>
    [System.Serializable]
    public class NamedFloat : SerializableNamed<float>
    {
        [SerializeField] private float value = default;

        public NamedFloat (string name, float value)
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
