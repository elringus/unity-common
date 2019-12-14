using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a <see cref="Nullable"/> with support for Unity serialization (for derived non-generic types).
    /// </summary>
    /// <typeparam name="TValue">Type of the value; should be natively supported by the Unity serialization system.</typeparam>
    [Serializable]
    public class Nullable<TValue> : IEquatable<Nullable<TValue>>
    {
        /// <summary>
        /// Current value when <see cref="HasValue"/>, default otherwise.
        /// </summary>
        public TValue Value { get => GetValue(); set => SetValue(value); }
        /// <summary>
        /// Whether the value is assigned.
        /// </summary>
        public bool HasValue { get => GetHasValue(); set => SetHasValue(value); }

        /// <summary>
        /// Whether <typeparamref name="TValue"/> is a value type.
        /// </summary>
        protected bool IsValueType => typeof(TValue).IsValueType;

        [SerializeField] private TValue value = default;
        [SerializeField] private bool hasValue = default;

        public static implicit operator TValue (Nullable<TValue> nullable)
        {
            return (nullable is null || !nullable.HasValue) ? default(TValue) : nullable.Value;
        }

        public static implicit operator Nullable<TValue> (TValue value)
        {
            return new Nullable<TValue> { Value = value };
        }

        public override bool Equals (object obj)
        {
            return Equals(obj as Nullable<TValue>);
        }

        public bool Equals (Nullable<TValue> other)
        {
            return other != null &&
                   EqualityComparer<TValue>.Default.Equals(value, other.value) &&
                   hasValue == other.hasValue;
        }

        public override int GetHashCode ()
        {
            var hashCode = 1753382938;
            hashCode = hashCode * -1521134295 + EqualityComparer<TValue>.Default.GetHashCode(value);
            hashCode = hashCode * -1521134295 + hasValue.GetHashCode();
            return hashCode;
        }

        public static bool operator == (Nullable<TValue> left, Nullable<TValue> right)
        {
            return EqualityComparer<Nullable<TValue>>.Default.Equals(left, right);
        }

        public static bool operator != (Nullable<TValue> left, Nullable<TValue> right)
        {
            return !(left == right);
        }

        protected virtual TValue GetValue ()
        {
            return HasValue ? value : default;
        }

        protected virtual void SetValue (TValue value)
        {
            this.value = value;

            HasValue = IsValueType || EqualityComparer<TValue>.Default.Equals(value, default);
        }

        protected virtual bool GetHasValue ()
        {
            return hasValue;
        }

        protected virtual void SetHasValue (bool hasValue)
        {
            this.hasValue = hasValue;
        }
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="string"/>.
    /// </summary>
    [Serializable]
    public class NullableString : Nullable<string>
    {
        public static implicit operator NullableString (string value) => new NullableString { Value = value };
        public static implicit operator string (NullableString nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="int"/>.
    /// </summary>
    [Serializable]
    public class NullableInteger : Nullable<int>
    {
        public static implicit operator NullableInteger (int value) => new NullableInteger { Value = value };
        public static implicit operator int? (NullableInteger nullable) => (nullable is null || !nullable.HasValue) ? null : (int?)nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="float"/>.
    /// </summary>
    [Serializable]
    public class NullableFloat : Nullable<float>
    {
        public static implicit operator NullableFloat (float value) => new NullableFloat { Value = value };
        public static implicit operator float? (NullableFloat nullable) => (nullable is null || !nullable.HasValue) ? null : (float?)nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="bool"/>.
    /// </summary>
    [Serializable]
    public class NullableBoolean : Nullable<bool>
    {
        public static implicit operator NullableBoolean (bool value) => new NullableBoolean { Value = value };
        public static implicit operator bool? (NullableBoolean nullable) => (nullable is null || !nullable.HasValue) ? null : (bool?)nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="Vector2"/>.
    /// </summary>
    [Serializable]
    public class NullableVector2 : Nullable<Vector2>
    {
        public static implicit operator NullableVector2 (Vector2 value) => new NullableVector2 { Value = value };
        public static implicit operator Vector2? (NullableVector2 nullable) => (nullable is null || !nullable.HasValue) ? null : (Vector2?)nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="Vector3"/>.
    /// </summary>
    [Serializable]
    public class NullableVector3 : Nullable<Vector3>
    {
        public static implicit operator NullableVector3 (Vector3 value) => new NullableVector3 { Value = value };
        public static implicit operator Vector3? (NullableVector3 nullable) => (nullable is null || !nullable.HasValue) ? null : (Vector3?)nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="Vector4"/>.
    /// </summary>
    [Serializable]
    public class NullableVector4 : Nullable<Vector4>
    {
        public static implicit operator NullableVector4 (Vector4 value) => new NullableVector4 { Value = value };
        public static implicit operator Vector4? (NullableVector4 nullable) => (nullable is null || !nullable.HasValue) ? null : (Vector4?)nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="Quaternion"/>.
    /// </summary>
    [Serializable]
    public class NullableQuaternion : Nullable<Quaternion>
    {
        public static implicit operator NullableQuaternion (Quaternion value) => new NullableQuaternion { Value = value };
        public static implicit operator Quaternion? (NullableQuaternion nullable) => (nullable is null || !nullable.HasValue) ? null : (Quaternion?)nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedString"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedString : Nullable<NamedString>
    {
        public static implicit operator NullableNamedString (NamedString value) => new NullableNamedString { Value = value };
        public static implicit operator NamedString (NullableNamedString nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedInteger"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedInteger : Nullable<NamedInteger>
    {
        public static implicit operator NullableNamedInteger (NamedInteger value) => new NullableNamedInteger { Value = value };
        public static implicit operator NamedInteger (NullableNamedInteger nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedFloat"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedFloat : Nullable<NamedFloat>
    {
        public static implicit operator NullableNamedFloat (NamedFloat value) => new NullableNamedFloat { Value = value };
        public static implicit operator NamedFloat (NullableNamedFloat nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedBoolean"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedBoolean : Nullable<NamedBoolean>
    {
        public static implicit operator NullableNamedBoolean (NamedBoolean value) => new NullableNamedBoolean { Value = value };
        public static implicit operator NamedBoolean (NullableNamedBoolean nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedVector2"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedVector2 : Nullable<NamedVector2>
    {
        public static implicit operator NullableNamedVector2 (NamedVector2 value) => new NullableNamedVector2 { Value = value };
        public static implicit operator NamedVector2 (NullableNamedVector2 nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedVector3"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedVector3 : Nullable<NamedVector3>
    {
        public static implicit operator NullableNamedVector3 (NamedVector3 value) => new NullableNamedVector3 { Value = value };
        public static implicit operator NamedVector3 (NullableNamedVector3 nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedVector4"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedVector4 : Nullable<NamedVector4>
    {
        public static implicit operator NullableNamedVector4 (NamedVector4 value) => new NullableNamedVector4 { Value = value };
        public static implicit operator NamedVector4 (NullableNamedVector4 nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedQuaternion"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedQuaternion : Nullable<NamedQuaternion>
    {
        public static implicit operator NullableNamedQuaternion (NamedQuaternion value) => new NullableNamedQuaternion { Value = value };
        public static implicit operator NamedQuaternion (NullableNamedQuaternion nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }
}
