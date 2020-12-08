using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Implementation is able to represent a <see cref="Nullable"/> value.
    /// </summary>
    public interface INullableValue
    {
        /// <summary>
        /// Whether value is assigned.
        /// </summary>
        bool HasValue { get; }
    }

    /// <summary>
    /// Implementation is able to represent a <see cref="Nullable"/> value of type <typeparamref name="TValue"/>.
    /// </summary>
    public interface INullable<TValue> : INullableValue
    {
        /// <summary>
        /// Value of the item.
        /// </summary>
        TValue Value { get; set; }
    }

    /// <summary>
    /// Represents a <see cref="Nullable"/> with support for Unity serialization (for derived non-generic types).
    /// </summary>
    /// <typeparam name="TValue">Type of the value; should be natively supported by the Unity serialization system.</typeparam>
    [Serializable]
    public class Nullable<TValue> : INullable<TValue>
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

        public override string ToString ()
        {
            if (!HasValue) return "null";
            return typeof(TValue) == typeof(bool) ? Value.ToString().ToLowerInvariant() : Value.ToString();
        }

        public static implicit operator TValue (Nullable<TValue> nullable)
        {
            return (nullable is null || !nullable.HasValue) ? default : nullable.Value;
        }

        public static implicit operator Nullable<TValue> (TValue value)
        {
            return new Nullable<TValue> { Value = value };
        }

        protected virtual TValue GetValue ()
        {
            return HasValue ? value : default;
        }

        protected virtual void SetValue (TValue value)
        {
            this.value = value;

            HasValue = IsValueType || !EqualityComparer<TValue>.Default.Equals(value, default);
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
    /// Represents a <see cref="Nullable"/> <see cref="Named{TValue}"/> with support for Unity serialization (for derived non-generic types).
    /// </summary>
    public abstract class NullableNamed<TNamed, TNamedValue> : Nullable<TNamed>
        where TNamed : INamed<TNamedValue>
        where TNamedValue : class
    {
        /// <summary>
        /// Name component of the value or null when value is not assigned.
        /// </summary>
        public string Name => HasValue ? Value.Name : null;
        /// <summary>
        /// Value component of the value or null when value is not assigned.
        /// </summary>
        public TNamedValue NamedValue => HasValue ? Value.Value : null;
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
    public class NullableNamedString : NullableNamed<NamedString, NullableString>
    {
        public static implicit operator NullableNamedString (NamedString value) => new NullableNamedString { Value = value };
        public static implicit operator NamedString (NullableNamedString nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedInteger"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedInteger : NullableNamed<NamedInteger, NullableInteger>
    {
        public static implicit operator NullableNamedInteger (NamedInteger value) => new NullableNamedInteger { Value = value };
        public static implicit operator NamedInteger (NullableNamedInteger nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedFloat"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedFloat : NullableNamed<NamedFloat, NullableFloat>
    {
        public static implicit operator NullableNamedFloat (NamedFloat value) => new NullableNamedFloat { Value = value };
        public static implicit operator NamedFloat (NullableNamedFloat nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedBoolean"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedBoolean : NullableNamed<NamedBoolean, NullableBoolean>
    {
        public static implicit operator NullableNamedBoolean (NamedBoolean value) => new NullableNamedBoolean { Value = value };
        public static implicit operator NamedBoolean (NullableNamedBoolean nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedVector2"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedVector2 : NullableNamed<NamedVector2, NullableVector2>
    {
        public static implicit operator NullableNamedVector2 (NamedVector2 value) => new NullableNamedVector2 { Value = value };
        public static implicit operator NamedVector2 (NullableNamedVector2 nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedVector3"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedVector3 : NullableNamed<NamedVector3, NullableVector3>
    {
        public static implicit operator NullableNamedVector3 (NamedVector3 value) => new NullableNamedVector3 { Value = value };
        public static implicit operator NamedVector3 (NullableNamedVector3 nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedVector4"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedVector4 : NullableNamed<NamedVector4, NullableVector4>
    {
        public static implicit operator NullableNamedVector4 (NamedVector4 value) => new NullableNamedVector4 { Value = value };
        public static implicit operator NamedVector4 (NullableNamedVector4 nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }

    /// <summary>
    /// Represents a serializable <see cref="System.Nullable"/> <see cref="NamedQuaternion"/>.
    /// </summary>
    [Serializable]
    public class NullableNamedQuaternion : NullableNamed<NamedQuaternion, NullableQuaternion>
    {
        public static implicit operator NullableNamedQuaternion (NamedQuaternion value) => new NullableNamedQuaternion { Value = value };
        public static implicit operator NamedQuaternion (NullableNamedQuaternion nullable) => (nullable is null || !nullable.HasValue) ? null : nullable.Value;
    }
}
