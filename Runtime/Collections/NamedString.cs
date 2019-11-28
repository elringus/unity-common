using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a serializable container for a string named item.
    /// </summary>
    [System.Serializable]
    public class NamedString : SerializableNamed<string>
    {
        [SerializeField] private string value = default;

        public NamedString (string name, string value)
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
