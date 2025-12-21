namespace Catel.SourceGenerators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An immutable, equatable array. This is equivalent to <see cref="Array"/> but with value equality support.
    /// </summary>
    /// <typeparam name="T">The type of values in the array.</typeparam>
    /// <remarks>
    /// This code comes from https://github.com/andrewlock/blog-examples/blob/master/NetEscapades.EnumGenerators/src/NetEscapades.EnumGenerators/EquatableArray.cs and
    /// is licenses apache 2.0 Which is based on the community toolkit, and that part is MIT licensed.
    /// <para />
    /// This class is slightly modified.
    /// </remarks>
    public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// The underlying <typeparamref name="T"/> array.
        /// </summary>
        private readonly IReadOnlyList<T> _array;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquatableArray{T}"/> struct.
        /// </summary>
        /// <param name="array">The input array to wrap.</param>
        public EquatableArray(IReadOnlyList<T> array)
        {
            _array = array;
        }

        /// <summary>
        /// Gets the length of the array, or 0 if the array is null
        /// </summary>
        public int Count => _array?.Count ?? 0;

        /// <summary>
        /// Checks whether two <see cref="EquatableArray{T}"/> values are the same.
        /// </summary>
        /// <param name="left">The first <see cref="EquatableArray{T}"/> value.</param>
        /// <param name="right">The second <see cref="EquatableArray{T}"/> value.</param>
        /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> are equal.</returns>
        public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks whether two <see cref="EquatableArray{T}"/> values are not the same.
        /// </summary>
        /// <param name="left">The first <see cref="EquatableArray{T}"/> value.</param>
        /// <param name="right">The second <see cref="EquatableArray{T}"/> value.</param>
        /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> are not equal.</returns>
        public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public bool Equals(EquatableArray<T> array)
        {
            return this.SequenceEqual(array);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is EquatableArray<T> array && Equals(array);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (_array is not T[] array)
            {
                return 0;
            }

            HashCode hashCode = default;

            foreach (T item in array)
            {
                hashCode.Add(item);
            }

            return hashCode.ToHashCode();
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();
        }
    }
}
