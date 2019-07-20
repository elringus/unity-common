using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public static class ArrayUtils
    {
        public static Vector2 ToVector2 (float?[] array, Vector2 @default = default)
        {
            return new Vector2(
                array?.ElementAtOrDefault(0) ?? @default.x,
                array?.ElementAtOrDefault(1) ?? @default.y);
        }

        public static Vector3 ToVector3 (float?[] array, Vector3 @default = default)
        {
            return new Vector3(
                array?.ElementAtOrDefault(0) ?? @default.x,
                array?.ElementAtOrDefault(1) ?? @default.y,
                array?.ElementAtOrDefault(2) ?? @default.z);
        }

        public static Vector4 ToVector4 (float?[] array, Vector4 @default = default)
        {
            return new Vector4(
                array?.ElementAtOrDefault(0) ?? @default.x,
                array?.ElementAtOrDefault(1) ?? @default.y,
                array?.ElementAtOrDefault(2) ?? @default.z,
                array?.ElementAtOrDefault(3) ?? @default.w);
        }

        /// <summary>
        /// Appends <paramref name="item"/> to the end of <paramref name="array"/>.
        /// </summary>
        public static void Add<T> (ref T[] array, T item)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = item;
        }

        /// <summary>
        /// Compares two arrays using <see cref="object.Equals(object)"/> on each element.
        /// </summary>
        public static bool ArrayEquals<T> (T[] lhs, T[] rhs)
        {
            if (lhs == null || rhs == null)
                return lhs == rhs;

            if (lhs.Length != rhs.Length)
                return false;

            for (int i = 0; i < lhs.Length; i++)
                if (!lhs[i].Equals(rhs[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Compares two arrays using <see cref="object.ReferenceEquals(object, object)"/> on each element.
        /// </summary>
        public static bool ArrayReferenceEquals<T> (T[] lhs, T[] rhs)
        {
            if (lhs == null || rhs == null)
                return lhs == rhs;

            if (lhs.Length != rhs.Length)
                return false;

            for (int i = 0; i < lhs.Length; i++)
                if (!ReferenceEquals(lhs[i], rhs[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Appends items to the end of array.
        /// </summary>
        public static void AddRange<T> (ref T[] array, T[] items)
        {
            var size = array.Length;
            Array.Resize(ref array, array.Length + items.Length);
            for (int i = 0; i < items.Length; i++)
                array[size + i] = items[i];
        }

        /// <summary>
        /// Inserts <paramref name="item"/> at position <paramref name="index"/>.
        /// </summary>
        public static void Insert<T> (ref T[] array, int index, T item)
        {
            ArrayList a = new ArrayList();
            a.AddRange(array);
            a.Insert(index, item);
            array = a.ToArray(typeof(T)) as T[];
        }

        /// <summary>
        /// Removes <paramref name="item"/> from <paramref name="array"/>.
        /// </summary>
        public static void Remove<T> (ref T[] array, T item)
        {
            var newList = new List<T>(array);
            newList.Remove(item);
            array = newList.ToArray();
        }

        public static List<T> FindAll<T> (T[] array, Predicate<T> match)
        {
            var list = new List<T>(array);
            return list.FindAll(match);
        }

        public static T Find<T> (T[] array, Predicate<T> match)
        {
            var list = new List<T>(array);
            return list.Find(match);
        }

        /// <summary>
        /// Find the index of the first element that satisfies the predicate.
        /// </summary>
        public static int FindIndex<T> (T[] array, Predicate<T> match)
        {
            var list = new List<T>(array);
            return list.FindIndex(match);
        }

        /// <summary>
        /// Index of first element with value <paramref name="value"/>.
        /// </summary>
        public static int IndexOf<T> (T[] array, T value)
        {
            var list = new List<T>(array);
            return list.IndexOf(value);
        }

        /// <summary>
        /// Index of the last element with value <paramref name="value"/>.
        /// </summary>
        public static int LastIndexOf<T> (T[] array, T value)
        {
            var list = new List<T>(array);
            return list.LastIndexOf(value);
        }

        /// <summary>
        /// Remove element at position <paramref name="index"/>.
        /// </summary>
        public static void RemoveAt<T> (ref T[] array, int index)
        {
            var list = new List<T>(array);
            list.RemoveAt(index);
            array = list.ToArray();
        }

        /// <summary>
        /// Determines if the array contains the item.
        /// </summary>
        public static bool Contains<T> (T[] array, T item)
        {
            var list = new List<T>(array);
            return list.Contains(item);
        }

        /// <summary>
        /// Clears and resizes the array to zero length.
        /// </summary>
        public static void ClearAndResize<T> (ref T[] array)
        {
            Array.Clear(array, 0, array.Length);
            Array.Resize(ref array, 0);
        }
    }
}