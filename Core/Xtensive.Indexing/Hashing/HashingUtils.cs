// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Hashing
{
  /// <summary>
  /// Helps to manipulate hashes.
  /// </summary>
  [Serializable]
  public static class HashingUtils
  {
    /// <summary>
    /// Calculates hash.
    /// </summary>
    /// <param name="value">Object to calculate hash to.</param>
    /// <returns>Hash.</returns>
    public static long GetHash(byte[] value)
    {
      return CalculateHash(value);
    }

    /// <summary>
    /// Calculates <paramref name="count"/> of different hashes at once.
    /// </summary>
    /// <param name="data">Object to calculate hashes for.</param>
    /// <param name="count">Count of hashes to calculate, should be less then Byte.MaxValue.</param>
    /// <returns>Array of <paramref name="count"/> hashes.</returns>
    public static long[] GetHashes(byte[] data, int count)
    {
      var result = new long[count];      
      WriteHashes(result, 0, data, count, true);
      return result;
    }

    /// <summary>
    /// Calculates <paramref name="count"/> of different hashes at once.
    /// </summary>
    /// <param name="data">Object to calculate hashes for.</param>
    /// <param name="count">Count of hashes to calculate, should be less then Byte.MaxValue.</param>
    /// <param name="firstHash">First hash value.</param>
    /// <returns>Array of <paramref name="count"/> hashes.</returns>
    public static long[] GetHashes(byte[] data, int count, long firstHash)
    {
      var result = new long[count];
      result[0] = firstHash;
      WriteHashes(result, 0, data, count, false);
      return result;
    }

    /// <summary>
    /// Calculates <paramref name="count"/> of different hashes at once
    /// and fills them into <paramref name="writeTo"/> array.
    /// </summary>
    /// <param name="writeTo">Array to write calculated hashes to.</param>
    /// <param name="offset">Offset to start writing from.</param>
    /// <param name="data">Object to calculate hashes for.</param>    
    /// <param name="count">Count of hashes to calculate, should be less then Byte.MaxValue.</param>
    /// <param name="writeFirstHash">Specifies whether to write first hash code or not.</param>
    public static void WriteHashes(long[] writeTo, int offset, byte[] data, int count, bool writeFirstHash)
    {
      if (count < 0 || count > byte.MaxValue)
        throw new ArgumentOutOfRangeException("count", count,
          string.Format(Strings.ExArgumentShouldBeInRange, 0, byte.MaxValue));
      if (data==null || count <= 0)
        return;
      if (writeFirstHash)
        writeTo[offset] = CalculateHash(data);
      if (count > 1)
        for (int iteration = 1; iteration < count; iteration++) {          
          writeTo[offset + iteration] = CalculateHash(data, iteration);        
      }
    }

    private const ulong FnvPrime = 1099511628211;
    private const ulong FnvOffsetBasis = 14695981039346656037;

    private static long CalculateHash(byte[] data, int iteration)
    {      
      // FNV-1a 64bit Algorithm
      unchecked
      {
        ulong hash = FnvOffsetBasis * FnvPrime;
        hash = hash ^ (byte) (iteration);        

        int length = data.Length;
        for (int i = 0; i < length; i++) {
          hash = hash * FnvPrime;
          hash = hash ^ data[i];
        }
        return (long) hash;
      }
    }

    private static long CalculateHash(byte[] data)
    {
      // FNV-1a 64bit Algorithm
      unchecked {
        ulong hash = FnvOffsetBasis;
        int length = data.Length;
        for (int i = 0; i < length; i++) {
          hash = hash * FnvPrime;
          hash = hash ^ data[i];
        }
        return (long) hash;
      }
    }
  }
}