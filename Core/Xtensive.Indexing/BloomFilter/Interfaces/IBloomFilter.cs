// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.09

using System.IO;
using Xtensive.Indexing.Hashing;

namespace Xtensive.Indexing.BloomFilter
{
  /// <summary>
  /// Describes Bloom filter.
  /// </summary>
  /// <typeparam name="T">Type of value to filter.</typeparam>
  public interface IBloomFilter<T>
  {
    /// <summary>
    /// Puts value in bloom filter.
    /// </summary>
    /// <param name="value">Value to put to bloom filter.</param>
    void AddValue(T value);

    /// <summary>
    /// Checks filter's state of value.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns><see langword="True"/> if value's hash found in filter, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <see langword="True"/> result is not guarantee that requested value was put in filter. More verification
    /// required to be sure. <see langword="False"/> result guarantee that requested values was never put in filter.
    /// </remarks>
    bool HasValue(T value);

    /// <summary>
    /// Clears filter's state
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets size of filter's data in bits.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Gets size of filter's data in bytes.
    /// </summary>
    long SizeInBytes { get; }

    /// <summary>
    /// Gets the count of set bits in filter's date.
    /// </summary>
    long FilledBitCount { get; }

    /// <summary>
    /// Gets filter's fill factor.
    /// </summary>
    double FillFactor { get; }

    /// <summary>
    /// Gets hash functions.
    /// </summary>
    Hasher<T> Hasher { get; }

    /// <summary>
    /// Gets count of hash functions used in bloom filter.
    /// </summary>
    int HashCount { get; }

    /// <summary>
    /// Serializes filter to stream.
    /// </summary>
    /// <param name="stream">Stream to serialize the data to.</param>
    void Serialize(Stream stream);
  }
}