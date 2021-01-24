using System;

namespace UnityCommon
{
    public readonly struct CachedResourceLocation : IEquatable<CachedResourceLocation>
    {
        public readonly string Path;
        public readonly Type Type;

        public CachedResourceLocation (string path, Type type)
        {
            Path = path;
            Type = type;
        }

        public CachedResourceLocation (string path, string typeName)
            : this(path, Type.GetType(typeName)) { }

        public static bool operator == (CachedResourceLocation left, CachedResourceLocation right) => left.Equals(right);
        public static bool operator != (CachedResourceLocation left, CachedResourceLocation right) => !left.Equals(right);
        public bool Equals (CachedResourceLocation other) => Path == other.Path && Type == other.Type;
        public override bool Equals (object obj) => obj is CachedResourceLocation other && Equals(other);

        public override int GetHashCode ()
        {
            unchecked
            {
                return (Path.GetHashCode() * 397) ^ Type.GetHashCode();
            }
        }
    }
}
