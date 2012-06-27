// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.09


using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core;
using Xtensive.Indexing.Hashing;
using Xtensive.Serialization;
using Xtensive.Serialization.Binary;
using Xtensive.Serialization.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.BloomFilter
{
  /// <summary>
  /// Base class for <see cref="MemoryBloomFilter{T}"/> and <see cref="StreamBloomFilter{T}"/>. 
  /// Encapsulates common functionality of both classes.
  /// </summary>
  /// <typeparam name="T">Type of value to filter.</typeparam>
  [Serializable]
  public abstract class BloomFilter<T> : IBloomFilter<T>
  {
    protected readonly static ValueSerializer<long> LongSerializer = 
      ValueSerializerProvider.Default.GetSerializer<long>();
    protected readonly static ValueSerializer<int>  IntSerializer  = 
      ValueSerializerProvider.Default.GetSerializer<int>();
    private readonly long size;
    private Hasher<T> hasher;
    private readonly int hashCount;

    /// <inheritdoc/>
    public int HashCount
    {
      [DebuggerStepThrough]
      get { return hashCount; }
    }

    /// <inheritdoc/>
    public long SizeInBytes
    {
      [DebuggerStepThrough]
      get { return ((Size + 7)/8); }
    }

    /// <summary>
    /// Gets size of filter's data in bits.
    /// </summary>
    public long Size
    {
      [DebuggerStepThrough]
      get { return size; }
    }

    /// <summary>
    /// Gets filter's fill factor.
    /// </summary>
    public double FillFactor
    {
      [DebuggerStepThrough]
      get { return ((double)FilledBitCount)/size; }
    }

    /// <inheritdoc/>
    public abstract long FilledBitCount { get; }

    /// <inheritdoc/>
    public Hasher<T> Hasher
    {
      [DebuggerStepThrough]
      get { return hasher; }
      [DebuggerStepThrough]
      protected set { hasher = value; }
    }

    /// <inheritdoc/>
    public void AddValue(T value)
    {
      AddHashes(Hasher.GetHashes(value, HashCount));
    }

    /// <inheritdoc/>
    public bool HasValue(T value)
    {
      return HasHashes(Hasher.GetHashes(value, HashCount));
    }

    /// <inheritdoc/>
    public abstract void AddHashes(long[] hashes);
    
    /// <inheritdoc/>
    public abstract bool HasHashes(long[] hashes);

    /// <inheritdoc/>
    public abstract void Clear();

    /// <inheritdoc/>
    public virtual void Serialize(Stream stream)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      SerializeDescriptor(stream);
    }

    /// <summary>
    /// Serializes filter's descriptor to the specified <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">Stream to serialize the descriptor into.</param>
    protected void SerializeDescriptor(Stream stream)
    {
      LongSerializer.Serialize(stream, size);
      IntSerializer.Serialize(stream, hashCount);
    }

    /// <summary>
    /// Calculates optimal count of hash functions for specified <paramref name="bitsPerValue"/> factor.
    /// </summary>
    /// <param name="bitsPerValue">Count of bits per value.</param>
    /// <returns>Optimal count of hash functions.</returns>
    public static int GetOptimalHashCount(double bitsPerValue)
    {
      ArgumentValidator.EnsureArgumentIsInRange(bitsPerValue, 1, double.MaxValue, "bitsPerValue");
      return (int)Math.Ceiling(Math.Log(2)*bitsPerValue);
    }


    // Constructors

    /// <summary>
    /// Initializes bloom filter with serialized data.
    /// </summary>
    /// <param name="stream">Stream to deserialize the filter from.</param>
    public BloomFilter(Stream stream)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      hasher = Hasher<T>.Default;
      if (hasher == null)
        throw new InvalidOperationException(String.Format(Strings.ExHasherNotFound, typeof(T)));
      size = LongSerializer.Deserialize(stream);
      hashCount = IntSerializer.Deserialize(stream);
    }

    /// <summary>
    /// Initializes bloom filter with size and hash functions. Uses default hash provider to calculate hashes.
    /// </summary>
    /// <param name="size">Count of bits to use in filter.</param>
    /// <param name="hashCount">Count of hash functions to use. See <see cref="GetOptimalHashCount"/> to get this parameter optimal.</param>
    public BloomFilter(long size, int hashCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange<long>(size, 1, long.MaxValue, "size");
      ArgumentValidator.EnsureArgumentIsInRange(hashCount, 1, int.MaxValue, "hashCount");
      this.size = size;
      this.hashCount = hashCount;
      hasher = Hasher<T>.Default;
      if(hasher==null)
        throw new InvalidOperationException(String.Format(Strings.ExHasherNotFound, typeof(T)));
    }
  }
}