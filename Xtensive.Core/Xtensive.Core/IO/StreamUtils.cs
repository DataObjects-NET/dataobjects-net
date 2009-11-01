// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.11

using System;
using System.IO;
using Xtensive.Core.Resources;

namespace Xtensive.Core.IO
{
  /// <summary>
  /// Helper class providing a set of useful stream operations.
  /// </summary>
  public static class StreamUtils
  {
    // Constants
    private const int BufferSize = 1024*64;
    private const int MaxBuffers = 256; // 16Mb

    /// <summary>
    /// Fills stream with zero bytes up to end.
    /// </summary>
    /// <param name="stream">Stream to erase.</param>
    /// <returns>Count of bytes actually erased.</returns>
    public static long Erase(Stream stream)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      return Erase(stream, stream.Length - stream.Position);
    }

    /// <summary>
    /// Fills stream with specified count of zero bytes. If count is greater than tail of stream (count>Lenght-Position), stream will be erased up to it's end.
    /// </summary>
    /// <param name="stream">Stream to erase.</param>
    /// <param name="count">Count of bytes to erase in stream.</param>
    /// <returns>Count of bytes actually erased.</returns>
    public static long Erase(Stream stream, long count)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", count, Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);
      byte[] buffer = new byte[BufferSize];
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
    /// Writes one stream to another up to the end of source stream.
    /// </summary>
    /// <param name="source">Source stream to write from.</param>
    /// <param name="destination">Destination stream to write to.</param>
    /// <returns>Count of bytes actually writed.</returns>
    /// <remarks>Destination stream will be prolonged if needed to fit data.</remarks>
    public static long WriteTo(Stream source, Stream destination)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(destination, "destination");
      return WriteTo(source, destination, source.Length - source.Position);
    }

    /// <summary>
    /// Writes specified count of bytes from one stream to another.
    /// </summary>
    /// <param name="source">Source stream to write from.</param>
    /// <param name="destination">Destination stream to write to.</param>
    /// <param name="count">Count of bytes to write.</param>
    /// <returns>Count of bytes actually writed.</returns>
    /// <remarks>Destination stream will be prolonged if needed to fit data. If count is greater than tail of source stream (Length-Position), source stream will be writed up to the end.</remarks>
    public static long WriteTo(Stream source, Stream destination, long count)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(destination, "destination");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", count, Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);
      if (source==destination)
        throw new ArgumentException(Strings.ExStreamCopyMustOperateDifferentStreams, "destination");
      byte[] buffer = new byte[BufferSize];
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
    /// Copies data within stream. 
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="sourcePosition">Position from where to read data.</param>
    /// <param name="count">Count of bytes to write.</param>
    /// <returns>Count of bytes actually writed.</returns>
    /// <remarks>Stream position must be set to requred destination position.</remarks>
    public static long InstreamCopy(Stream stream, long sourcePosition, long count)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      long destinationPosition = stream.Position;
      long dataLength = Math.Min(count, stream.Length - sourcePosition);
      if (destinationPosition==sourcePosition)
        return dataLength;
      byte[][] buffers = new byte[Math.Min(dataLength/BufferSize + 1, MaxBuffers)][];
      for (int i = 0; i < buffers.Length; i++) {
        buffers[i] = new byte[BufferSize];
      }
      int largeBufferSize = buffers.Length*BufferSize;
      long iterationCount = dataLength/largeBufferSize + 1;
      bool reverseDirection = sourcePosition < destinationPosition;
      for (long iteration = reverseDirection ? iterationCount-1 : 0;
        reverseDirection ? (iteration >= 0) : (iteration < iterationCount);
        iteration += reverseDirection ? -1 : 1) {
        // Read;
        stream.Seek(sourcePosition + iteration*largeBufferSize, SeekOrigin.Begin);
        long bytesRemaining = dataLength - (iteration)*largeBufferSize;
        long iterationBuffersCount = Math.Min(MaxBuffers, bytesRemaining/BufferSize + 1);
        for (int bufferIndex = 0; bufferIndex < iterationBuffersCount; bufferIndex++) {
          int bytesToRead = (int)Math.Min(bytesRemaining - bufferIndex*BufferSize, BufferSize);
          stream.Read(buffers[bufferIndex], 0, bytesToRead);
        }
        stream.Seek(destinationPosition + iteration*largeBufferSize, SeekOrigin.Begin); // Write
        for (int bufferIndex = 0; bufferIndex < iterationBuffersCount; bufferIndex++) {
          stream.Write(buffers[bufferIndex], 0, (int)Math.Min(bytesRemaining - bufferIndex*BufferSize, BufferSize));
        }
      }
      stream.Seek(destinationPosition + dataLength,SeekOrigin.Begin);
      return dataLength;
    }
  }
}