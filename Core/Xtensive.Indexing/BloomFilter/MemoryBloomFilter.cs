// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.09

using System;
using System.Diagnostics;
using System.IO;

namespace Xtensive.Indexing.BloomFilter
{
  /// <summary>
  /// Bloom filter storing its underlying data in memory.
  /// </summary>
  /// <typeparam name="T">Type of value to filter.</typeparam>
  [Serializable]
  public class MemoryBloomFilter<T> : BloomFilter<T>
  {
    private readonly byte[][] data;
    private long filledBitCount;

    /// <inheritdoc/>
    public override long FilledBitCount
    {
      [DebuggerStepThrough]
      get { return filledBitCount; }
    }

    /// <inheritdoc/>
    public override void AddHashes(long[] hashes)
    {
      for (int i = 0; i < hashes.Length; i++) {
        long bitIndex = Math.Abs(hashes[i]%Size);
        long byteIndex = bitIndex/8;
        byte mask = (byte)(1 << (int)(bitIndex%8));
        long index0 = byteIndex / int.MaxValue;
        int index1 = (int)(byteIndex % int.MaxValue);
        int b1 = data[index0][index1];
        int b2 = b1 | mask;
        if (b1!=b2) {
          data[index0][index1] = (byte)b2;
          filledBitCount++;
        }
      }
    }

    /// <inheritdoc/>
    public override bool HasHashes(long[] hashes)
    {
      for (int i = 0; i < hashes.Length; i++) {
        long bitIndex = Math.Abs(hashes[i]%Size);
        long byteIndex = bitIndex/8;
        long index0 = byteIndex/int.MaxValue;
        int index1 = (int)(byteIndex % int.MaxValue);
        int storedValue = data[index0][index1];
        if (0 == (storedValue & (byte)(1 << (int)(bitIndex % 8))))
          return false;
      }
      return true;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      Array.Clear(data, 0, data.Length);
      filledBitCount = 0;
    }

    /// <inheritdoc/>
    public override void Serialize(Stream stream)
    {
      base.Serialize(stream);
      LongSerializer.Serialize(stream, filledBitCount);
      for (int i = 0; i < data.Length; i++)
        stream.Write(data[i], 0, data[i].Length);
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="MemoryBloomFilter{T}"/>.
    /// </summary>
    /// <param name="size">Count of bits to use in filter.</param>
    /// <param name="hashCount">Count of hash functions to use. See <see cref="BloomFilter{T}.GetOptimalHashCount"/> to get this parameter optimal.</param>
    public MemoryBloomFilter(long size, int hashCount)
      : base(size, hashCount)
    {
      int remainer = (int)(SizeInBytes%int.MaxValue);
      data = new byte[SizeInBytes/int.MaxValue + (remainer==0 ? 0 : 1)][];
      for (int i = 0; i < data.Length; i++) {
        int length;
        if ((i==data.Length - 1) && (remainer!=0)) 
          length = remainer;
        else 
          length = int.MaxValue;
        data[i] = new byte[length];
      }
    }

    /// <summary>
    /// Creates new instance of <see cref="MemoryBloomFilter{T}"/>.
    /// </summary>
    /// <param name="stream">Stream to deserialize the filter from.</param>
    public MemoryBloomFilter(Stream stream)
      : base(stream)
    {
      filledBitCount = LongSerializer.Deserialize(stream);
      data = new byte[SizeInBytes/int.MaxValue + 1][];
      int remainer = (int)(SizeInBytes % int.MaxValue);
      for (int i = 0; i < data.Length; i++)
      {
        int length;
        if ((i == data.Length - 1) && (remainer != 0))
          length = remainer;
        else
          length = int.MaxValue;
        data[i] = new byte[length];
        stream.Read(data[i], 0, length);
      }
    }
  }
}