// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.09

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core;
using Xtensive.Indexing.IO;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.BloomFilter
{
  /// <summary>
  /// Bloom filter. Uses stream to store and retrieve its data.
  /// </summary>
  /// <typeparam name="T">Type of value to filter.</typeparam>
  [Serializable]
  public class StreamBloomFilter<T> : BloomFilter<T>
  {
    // Constants
    private const int serializeBufferSize = 1024*64;

    // Private fields
    private readonly Stream stream;
    private readonly long dataOffset;
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
      for (int i = 0; i < HashCount; i++) {
        long bitIndex = Math.Abs(hashes[i]%Size);
        long byteIndex = bitIndex/8;
        int mask = (byte)(1 << (int)(bitIndex%8));
        stream.Seek(dataOffset + byteIndex, SeekOrigin.Begin);
        int b1 = stream.ReadByte();
        int b2 = b1 | mask;
        if (b1!=b2) {
          stream.Seek(-1, SeekOrigin.Current);
          stream.WriteByte((byte)b2);
          filledBitCount++;
        }
      }
    }

    /// <inheritdoc/>
    public override bool HasHashes(long[] hashes)
    {
      for (int i = 0; i < HashCount; i++) {
        long bitIndex = Math.Abs(hashes[i]%Size);
        stream.Seek(dataOffset + bitIndex/8, SeekOrigin.Begin);
        if (0==(stream.ReadByte() & (1 << (int)(bitIndex%8))))
          return false;
      }
      return true;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      byte[] buffer = new byte[serializeBufferSize];
      stream.Seek(dataOffset, SeekOrigin.Begin);
      int count = 0;
      long bytesToWrite = Math.Min(SizeInBytes, (int)(stream.Length - dataOffset));
      while (count < bytesToWrite) {
        int segmentSize = (int)Math.Min(serializeBufferSize, SizeInBytes - count);
        stream.Write(buffer, 0, segmentSize);
        count += segmentSize;
      }
      filledBitCount = 0;
    }

    /// <summary>
    /// Serializes filter to stream.
    /// </summary>
    /// <param name="stream">Stream to serialize to.</param>
    public override void Serialize(Stream stream)
    {
      base.Serialize(stream);
      LongSerializer.Serialize(stream, filledBitCount);
      this.stream.Seek(dataOffset, SeekOrigin.Begin);
      this.stream.CopyTo(stream, SizeInBytes);
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="MemoryBloomFilter{T}"/> from serialize stream.
    /// </summary>
    /// <param name="stream">Stream to deserialize the filter from.</param>
    /// <remarks><paramref name="stream"/> must contains serialized data. See <see cref="IBloomFilter{T}.Serialize"/> for details about filter serialization.</remarks>
    public StreamBloomFilter(Stream stream)
      : base(stream)
    {
      filledBitCount = LongSerializer.Deserialize(stream);
      dataOffset = stream.Position;
      if (dataOffset + SizeInBytes > stream.Length)
        throw new InvalidOperationException(Strings.ExCantDeserializeBloomFilter);
      this.stream = stream;
    }

    /// <summary>
    /// Creates new instance of <see cref="MemoryBloomFilter{T}"/>. 
    /// Uses specified stream to store filter's data.
    /// </summary>
    /// <param name="stream">Stream to store the filter's data in.</param>
    /// <param name="size">Count of bits to use in filter.</param>
    /// <param name="hashCount">Count of hash functions to use. See <see cref="BloomFilter{T}.GetOptimalHashCount"/> to get this parameter optimal.</param>
    public StreamBloomFilter(Stream stream, long size, int hashCount)
      : base(size, hashCount)
    {
      SerializeDescriptor(stream);
      LongSerializer.Serialize(stream, filledBitCount);
      dataOffset = stream.Position;
      stream.Erase(SizeInBytes);
      if (stream.Length < (dataOffset + SizeInBytes))
        stream.SetLength(dataOffset + SizeInBytes);
      this.stream = stream;
    }
  }
}