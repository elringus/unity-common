
namespace UnityCommon
{
    /// <summary>
    /// Represents a container for a generic named item.
    /// </summary>
    /// <typeparam name="TValue">Type of the value</typeparam>
    [System.Serializable]
    public class Named<TValue> : Pair<string, TValue>
    {
        public string Name { get => Item1; set => Item1 = value; }
        public TValue Value { get => Item2; set => Item2 = value; }

        public Named (string name, TValue value)
            : base(name, value) { }
    }
}
