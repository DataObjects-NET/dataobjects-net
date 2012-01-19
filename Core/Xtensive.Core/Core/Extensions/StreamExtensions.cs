// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.11

using System;
using System.IO;
using Xtensive.Core;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Helper class providing a set of useful stream operations.
  /// </summary>
  public static class StreamExtensions
  {
    // Constants
    private const int SmallBufferSize = 256;
    private const int BufferSize = 1024*128;
    private const int MaxBuffers = 10; // 1Mb

    [ThreadStatic]
    private static byte[] threadBuffer;
    private static byte[][] threadBuffers;

    /// <summary>
    /// Fills the stream with zero bytes 
    /// starting from the current position 
    /// and up to the end of it.
    /// </summary>
    /// <param name="stream">The stream to erase.</param>
    /// <returns>Count of actually erased bytes.</returns>
    public static long Erase(this Stream stream)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      return Erase(stream, stream.Length - stream.Position);
    }

    /// <summary>
    /// Fills the stream with specified count of zero bytes
    /// starting from the current position.
    /// If count is greater than the tail of the stream (count > Length-Position), 
    /// the stream will be erased up to its end.
    /// </summary>
    /// <param name="stream">The stream to erase.</param>
    /// <param name="count">Count of bytes to erase.</param>
    /// <returns>Count of actually erased bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range.</exception>
    public static long Erase(this Stream stream, long count)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", count, 
          Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);
      byte[] buffer;
      if (count<SmallBufferSize)
        buffer = new byte[count];
      else {
        EnsureThreadBufferIsInitialized();
        buffer = threadBuffer;
      }
      long bytesToWrite = Math.Min(count, (stream.Length - stream.Position));
      long erasedCount = 0;
      while (erasedCount < bytesToWrite) {
        int segmentSize = (int)Math.Min(BufferSize, bytesToWrite - erasedCount);
        stream.Write(buffer, 0, segmentSize);
        erasedCount += segmentSize;
      }
      return erasedCount;
    }

    /// <summary>
    /// Copies the part of the <paramref name="source"/> stream 
    /// to the <paramref name="destination"/> stream
    /// starting from the current positions 
    /// and up to the end of the <paramref name="source"/> stream.
    /// </summary>
    /// <param name="source">The stream to copy from.</param>
    /// <param name="destination">The stream to copy to.</param>
    /// <returns>Count of actually copied bytes.</returns>
    /// <remarks>Destination stream can be prolonged to fit the data.</remarks>
    public static long CopyTo(this Stream source, Stream destination)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      return CopyTo(source, destination, source.Length - source.Position);
    }

    /// <summary>
    /// Copies specified count of bytes 
    /// from the <paramref name="source"/> stream 
    /// to the <paramref name="destination"/> stream
    /// starting from the current positions.
    /// </summary>
    /// <param name="source">The stream to copy from.</param>
    /// <param name="destination">The stream to copy to.</param>
    /// <param name="count">Count of bytes to copy.</param>
    /// <returns>Count of actually copied bytes.</returns>
    /// <remarks>Destination stream can be prolonged to fit the data.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range.</exception>
    /// <exception cref="NotSupportedException"><paramref name="destination"/> and <paramref name="source"/> is the same stream.</exception>
    public static long CopyTo(this Stream source, Stream destination, long count)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(destination, "destination");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", count, 
          Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);
      if (source==destination)
        throw new NotSupportedException(
          Strings.ExCopyToMustOperateWithDifferentStreams);
      byte[] buffer;
      if (count<SmallBufferSize)
        buffer = new byte[count];
      else {
        EnsureThreadBufferIsInitialized();
        buffer = threadBuffer;
      }
      long bytesToWrite = Math.Min(count, (source.Length - source.Position));
      int writedCount = 0;
      while (writedCount < bytesToWrite) {
        int bytesToRead = (int)Math.Min(BufferSize, bytesToWrite - writedCount);
        int readCount = source.Read(buffer, 0, bytesToRead);
        destination.Write(buffer, 0, readCount);
        writedCount += readCount;
      }
      return writedCount;
    }

    /// <summary>
    /// Copies a part of the stream within the stream
    /// to the current position of it.
    /// </summary>
    /// <param name="stream">The stream to copy a part of.</param>
    /// <param name="offset">Offset of the part to copy from.</param>
    /// <param name="count">Count of bytes to copy.</param>
    /// <returns>Count of actually copied bytes.</returns>
    /// <remarks>Stream can be prolonged to fit the data.</remarks>
    public static long Copy(this Stream stream, long offset, long count)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      long destinationOffset = stream.Position;
      long actualCount = Math.Min(count, stream.Length - offset);
      if (destinationOffset==offset)
        return actualCount;
      EnsureBuffersAreInitialized();
      int largeBufferSize = threadBuffers.Length*BufferSize;
      long iterationCount = actualCount/largeBufferSize + 1;
      bool reverseDirection = offset < destinationOffset;
      for (long iteration = reverseDirection ? iterationCount-1 : 0;
        reverseDirection ? (iteration >= 0) : (iteration < iterationCount);
        iteration += reverseDirection ? -1 : 1) {
        stream.Seek(offset + iteration*largeBufferSize, SeekOrigin.Begin);
        long bytesRemaining = actualCount - (iteration)*largeBufferSize;
        long iterationBuffersCount = Math.Min(MaxBuffers, bytesRemaining/BufferSize + 1);
        for (int bufferIndex = 0; bufferIndex < iterationBuffersCount; bufferIndex++) {
          int bytesToRead = (int)Math.Min(bytesRemaining - bufferIndex*BufferSize, BufferSize);
          stream.Read(threadBuffers[bufferIndex], 0, bytesToRead);
        }
        stream.Seek(destinationOffset + iteration*largeBufferSize, SeekOrigin.Begin); // Write
        for (int bufferIndex = 0; bufferIndex < iterationBuffersCount; bufferIndex++) {
          stream.Write(threadBuffers[bufferIndex], 0, (int)Math.Min(bytesRemaining - bufferIndex*BufferSize, BufferSize));
        }
      }
      stream.Seek(destinationOffset + actualCount,SeekOrigin.Begin);
      return actualCount;
    }

    #region Private \ internal methods

    private static void EnsureThreadBufferIsInitialized()
    {
      if (threadBuffer==null)
        threadBuffer = new byte[BufferSize];
    }

    private static void EnsureBuffersAreInitialized()
    {
      if (threadBuffers==null) {
        threadBuffers = new byte[MaxBuffers][];
        for (int i = 0; i < threadBuffers.Length; i++)
          threadBuffers[i] = new byte[BufferSize];
      }
    }

    #endregion
  }
}