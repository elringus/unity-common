using System.Globalization;

namespace UnityCommon
{
    public static class ParseUtils
    {
        /// <summary>
        /// Invokes a <see cref="int.TryParse(string, NumberStyles, System.IFormatProvider, out int)"/> on the provided string,
        /// using <see cref="CultureInfo.InvariantCulture"/> and <see cref="NumberStyles.Integer"/>.
        /// </summary>
        /// <returns>Whether parsing succeeded.</returns>
        public static bool TryInvariantInt (string str, out int result)
        {
            return int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Invokes a <see cref="float.TryParse(string, NumberStyles, System.IFormatProvider, out float)"/> on the provided string,
        /// using <see cref="CultureInfo.InvariantCulture"/> and <see cref="NumberStyles.Float"/>.
        /// </summary>
        /// <returns>Whether parsing succeeded.</returns>
        public static bool TryInvariantFloat (string str, out float result)
        {
            return float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Invokes a <see cref="int.TryParse(string, NumberStyles, System.IFormatProvider, out int)"/> on the provided string,
        /// using <see cref="CultureInfo.InvariantCulture"/> and <see cref="NumberStyles.Integer"/>.
        /// </summary>
        /// <returns>Parsed value when parsing succeeded or null.</returns>
        public static int? AsInvariantInt (this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var succeeded = TryInvariantInt(str, out var result);
            return succeeded ? (int?)result : null;
        }

        /// <summary>
        /// Invokes a <see cref="float.TryParse(string, NumberStyles, System.IFormatProvider, out float)"/> on the provided string,
        /// using <see cref="CultureInfo.InvariantCulture"/> and <see cref="NumberStyles.Float"/>.
        /// </summary>
        /// <returns>Parsed value when parsing succeeded or null.</returns>
        public static float? AsInvariantFloat (this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var succeeded = TryInvariantFloat(str, out var result);
            return succeeded ? (float?)result : null;
        }
    }
}
