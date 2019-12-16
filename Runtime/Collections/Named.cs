using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a container for a <see cref="string"/> (name) and a generic value 
    /// with support for Unity serialization (for derived non-generic types).
    /// </summary>
    /// <typeparam name="TValue">Type of the value; should be natively supported by the Unity serialization system.</typeparam> 
    [Serializable]
    public class Named<TValue> : IEquatable<Named<TValue>>
    {
        /// <summary>
        /// Name of the item; underlying serialized type supports null values (via <see cref="NullableString"/>).
        /// </summary>
        public string Name { get => name.HasValue ? name : null; set => name = value; }
        /// <summary>
        /// Value of the item.
        /// </summary>
        public TValue Value { get => value; set => this.value = value; }

        [SerializeField] private NullableString name = default;
        [SerializeField] private TValue value = default;

        public Named () { }

        public Named (string name, TValue value)
        {
            Name = name;
            Value = value;
        }

        public override bool Equals (object obj)
        {
            return Equals(obj as Named<TValue>);
        }

        public bool Equals (Named<TValue> other)
        {
            return other != null &&
                   EqualityComparer<NullableString>.Default.Equals(name, other.name) &&
                   EqualityComparer<TValue>.Default.Equals(value, other.value);
        }

        public override int GetHashCode ()
        {
            var hashCode = 1477024672;
            hashCode = hashCode * -1521134295 + EqualityComparer<NullableString>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<TValue>.Default.GetHashCode(value);
            return hashCode;
        }

        public static bool operator == (Named<TValue> left, Named<TValue> right)
        {
            return EqualityComparer<Named<TValue>>.Default.Equals(left, right);
        }

        public static bool operator != (Named<TValue> left, Named<TValue> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableString"/> value.
    /// </summary>
    [Serializable]
    public class NamedString : Named<NullableString>
    {
        public NamedString () { }
        public NamedString (string name, string value) : base(name, value) { }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableInteger"/> value.
    /// </summary>
    [Serializable]
    public class NamedInteger : Named<NullableInteger>
    {
        public NamedInteger () { }
        public NamedInteger (string name, int? value) : base(name, value) { }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableFloat"/> value.
    /// </summary>
    [Serializable]
    public class NamedFloat : Named<NullableFloat>
    {
        public NamedFloat () { }
        public NamedFloat (string name, float? value) : base(name, value) { }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableBoolean"/> value.
    /// </summary>
    [Serializable]
    public class NamedBoolean : Named<NullableBoolean>
    {
        public NamedBoolean () { }
        public NamedBoolean (string name, bool? value) : base(name, value) { }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableVector3"/> value.
    /// </summary>
    [Serializable]
    public class NamedVector2 : Named<NullableVector2>
    {
        public NamedVector2 () { }
        public NamedVector2 (string name, Vector2? value) : base(name, value) { }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableVector3"/> value.
    /// </summary>
    [Serializable]
    public class NamedVector3 : Named<NullableVector3>
    {
        public NamedVector3 () { }
        public NamedVector3 (string name, Vector3? value) : base(name, value) { }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableVector4"/> value.
    /// </summary>
    [Serializable]
    public class NamedVector4 : Named<NullableVector4>
    {
        public NamedVector4 () { }
        public NamedVector4 (string name, Vector4? value) : base(name, value) { }
    }

    /// <summary>
    /// Represents a serializable <see cref="Named{TValue}"/> with <see cref="NullableQuaternion"/> value.
    /// </summary>
    [Serializable]
    public class NamedQuaternion : Named<NullableQuaternion>
    {
        public NamedQuaternion () { }
        public NamedQuaternion (string name, Quaternion? value) : base(name, value) { }
    }
}
   