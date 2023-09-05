// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Copy/pasted from https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/Text/ValueStringBuilder.cs
// + re-structured to fit project rules
// + formatting rules applied

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace Xtensive.Core
{
  internal ref struct ValueStringBuilder
  {
    private char[]? arrayToReturnToPool;
    private Span<char> chars;
    private int position;

    public ref char this[int index] => ref chars[index];

    public int Length
    {
      readonly get => position;
      set => position = value;
    }

    public int Capacity => chars.Length;


    /// <summary>
    /// Returns the underlying storage of the builder.
    /// </summary>
    public Span<char> RawChars => chars;

    /// <summary>
    /// Ensures that capacity is greater or equal to <paramref name="capacity"/>.
    /// Grows inner collection if required.
    /// </summary>
    /// <param name="capacity">Required capacity</param>
    public void EnsureCapacity(int capacity)
    {
      // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
      if ((uint) capacity > (uint) chars.Length) {
        Grow(capacity - position);
      }
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length"/>
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)"
    /// </summary>
    public ref char GetPinnableReference() => ref MemoryMarshal.GetReference(chars);

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ref char GetPinnableReference(bool terminate)
    {
      if (terminate) {
        EnsureCapacity(Length + 1);
        chars[Length] = '\0';
      }

      return ref MemoryMarshal.GetReference(chars);
    }

    public void Insert(int index, char value, int count)
    {
      if (position > chars.Length - count) {
        Grow(count);
      }

      var remaining = position - index;
      chars.Slice(index, remaining).CopyTo(chars.Slice(index + count));
      chars.Slice(index, count).Fill(value);
      position += count;
    }

    /// <summary>
    /// Inserts non-nul string by given index.
    /// </summary>
    /// <param name="index">Index to insert.</param>
    /// <param name="s">Non-null string.</param>
    public void Insert(int index, string? s)
    {
      if (s == null) {
        return;
      }

      var count = s.Length;
      if (position > (chars.Length - count)) {
        Grow(count);
      }

      var remaining = position - index;
      chars.Slice(index, remaining).CopyTo(chars.Slice(index + count));
#if NET6_0_OR_GREATE
      s.CopyTo(chars.Slice(index));
#else
      s.AsSpan().CopyTo(chars.Slice(index));
#endif
      position += count;
    }

    /// <summary>
    /// Appends char.
    /// </summary>
    /// <param name="c">Char to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
      var origPos = position;
      var origChars = chars;
      if ((uint) origPos < (uint) origChars.Length) {
        origChars[origPos] = c;
        position = origPos + 1;
      }
      else {
        GrowAndAppend(c);
      }
    }

    /// <summary>
    /// Appends non-null string.
    /// </summary>
    /// <param name="s">String to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s)
    {
      if (s == null) {
        return;
      }

      var origPos = position;
      if (s.Length == 1 && (uint) origPos < (uint) chars.Length) {
        // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        chars[origPos] = s[0];
        position = origPos + 1;
      }
      else {
        if (origPos > chars.Length - s.Length) {
          Grow(s.Length);
        }

#if NET6_0_OR_GREATER
        s.CopyTo(chars.Slice(origPos));
#else
      s.AsSpan().CopyTo(chars.Slice(origPos));
#endif
        position += s.Length;
      }
    }

    public void Append(char c, int count)
    {
      if (position > chars.Length - count) {
        Grow(count);
      }

      var dst = chars.Slice(position, count);
      for (var i = 0; i < dst.Length; i++) {
        dst[i] = c;
      }

      position += count;
    }

    public void Append(ReadOnlySpan<char> value)
    {
      var pos = position;
      if (pos > chars.Length - value.Length) {
        Grow(value.Length);
      }

      value.CopyTo(chars.Slice(position));
      position += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
      var origPos = position;
      if (origPos > chars.Length - length) {
        Grow(length);
      }

      position = origPos + length;
      return chars.Slice(origPos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
      Grow(1);
      Append(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="position"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
      const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

      ArgumentValidator.EnsureArgumentIsGreaterThan(additionalCapacityBeyondPos, 0, nameof(additionalCapacityBeyondPos));

      // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
      // to double the size if possible, bounding the doubling to not go beyond the max array length.
      var newCapacity = (int) Math.Max(
        (uint) (position + additionalCapacityBeyondPos),
        Math.Min((uint) chars.Length * 2, ArrayMaxLength));

      // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
      // This could also go negative if the actual required length wraps around.
      var poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

      chars.Slice(0, position).CopyTo(poolArray);

      var toReturn = arrayToReturnToPool;
      chars = arrayToReturnToPool = poolArray;
      if (toReturn != null) {
        ArrayPool<char>.Shared.Return(toReturn);
      }
    }


    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ReadOnlySpan<char> AsSpan(bool terminate)
    {
      if (terminate) {
        EnsureCapacity(Length + 1);
        chars[Length] = '\0';
      }

      return chars.Slice(0, position);
    }

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <returns>Read-only span wrapper of the content.</returns>
    public ReadOnlySpan<char> AsSpan() => chars.Slice(0, position);

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="start">Start position.</param>
    /// <returns>Read-only span wrapper of the content with reqested portion of data.</returns>
    public ReadOnlySpan<char> AsSpan(int start) => chars.Slice(start, position - start);

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="start">Start position.</param>
    /// <param name="length">Length.</param>
    /// <returns>Read-only span wrapper of the content with reqested portion of data.</returns>
    public ReadOnlySpan<char> AsSpan(int start, int length) => chars.Slice(start, length);

    /// <summary>
    /// Tries to copy rawchars to given <paramref name="destination"/>.
    /// If try is failed it disposes resources.
    /// </summary>
    /// <param name="destination">The span to copy to.</param>
    /// <param name="charsWritten">Count of copied chars.</param>
    /// <returns><see langword="true"/> if copy was sucessful, othrewise <see langword ="false"/></returns>
    public bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
      if (chars.Slice(0, position).TryCopyTo(destination)) {
        charsWritten = position;
        Dispose();
        return true;
      }
      else {
        charsWritten = 0;
        Dispose();
        return false;
      }
    }

    /// <summary>
    /// Transforms result to string and disposes resources.
    /// </summary>
    /// <returns>result string</returns>
    public override string ToString()
    {
      var s = chars.Slice(0, position).ToString();
      Dispose();
      return s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
      var toReturn = arrayToReturnToPool;
      this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
      if (toReturn != null) {
        ArrayPool<char>.Shared.Return(toReturn);
      }
    }

    public ValueStringBuilder(Span<char> initialBuffer)
    {
      arrayToReturnToPool = null;
      chars = initialBuffer;
      position = 0;
    }

    public ValueStringBuilder(int initialCapacity)
    {
      arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
      chars = arrayToReturnToPool;
      position = 0;
    }
  }
}