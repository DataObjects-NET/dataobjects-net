// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.IO
{
  /// <summary>
  /// Exposes a <see cref="Segment"/> of the specified <see cref="Stream"/> 
  /// as the independent stream.
  /// </summary>
  /// <remarks>
  /// Any read and write operations on this instance affect on
  /// <see cref="System.IO.Stream.Position"/> of the underlying <see cref="Stream"/>.
  /// </remarks>
  [Serializable]
  [DebuggerDisplay("Offset = {Segment.Offset}, Length = {Segment.Length}")]
  public class StreamSegment : Stream
  {
    private long position;

    /// <summary>
    /// Gets the stream wrapped by this instance.
    /// </summary>
    public Stream Stream { get; private set; }

    /// <summary>
    /// Gets or sets the segment exposed by this instance.
    /// </summary>
    public Segment<long> Segment { get; private set; }

    /// <inheritdoc/>
    public override long Position {
      get { return position; }
      set {
        if (position>=Segment.Length || position<0)
          ArgumentValidator.EnsureArgumentIsInRange(value, 0, Segment.Length, "value");
        position = value;
      }
    }

    /// <inheritdoc/>
    public override long Length
    {
      get { return Segment.Length; }
    }

    /// <inheritdoc/>
    public override bool CanSeek {
      get { return Stream.CanSeek; }
    }

    /// <inheritdoc/>
    public override bool CanRead {
      get { return Stream.CanRead; }
    }

    /// <inheritdoc/>
    public override bool CanWrite {
      get { return Stream.CanWrite; }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">Either <paramref name="offset"/> or 
    /// <paramref name="origin"/> is out of range.</exception>
    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin) {
      case SeekOrigin.Begin:
        Position = Segment.Offset + offset;
        break;
      case SeekOrigin.Current:
        Position += offset;
        break;
      case SeekOrigin.End:
        Position = Segment.EndOffset - offset;
        break;
      default:
        throw new ArgumentOutOfRangeException("origin");
      }
      return position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
      if (Stream.Length==Segment.EndOffset)
        Stream.SetLength(Segment.Offset + value);
      Segment = new Segment<long>(Segment.Offset, value);
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
      Stream.Position = Segment.Offset + position;
      var maxCount = Segment.Length-position;
      if (count > maxCount)
        count = (int) maxCount;
      int actualCount = Stream.Read(buffer, offset, count);
      position += actualCount;
      return actualCount;
    }

    /// <inheritdoc/>
    public override int ReadByte()
    {
      if (position==Segment.Length)
        return -1;
      Stream.Position = Segment.Offset + position;
      int result = Stream.ReadByte();
      if (result>=0)
        position += 1;
      return result;
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
      Stream.Position = Segment.Offset + position;
      var maxCount = Segment.Length-position;
      Stream.Write(buffer, offset, count);
      if (count > maxCount)
        Segment = new Segment<long>(Segment.Offset, count);
      position += count;
    }

    /// <inheritdoc/>
    public override void WriteByte(byte value)
    {
      long segmentOffset = Segment.Offset;
      Stream.Position = segmentOffset + position;
      Stream.WriteByte(value);
      long segmentLength = Segment.Length;
      if (position==segmentLength)
        Segment = new Segment<long>(segmentOffset, ++segmentLength);
      position++;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
      Stream.Flush();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to wrap.</param>
    /// <param name="segment">The <see cref="Segment"/> to expose.</param>
    public StreamSegment(Stream stream, Segment<long> segment)
      : this(stream, segment, false)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to wrap.</param>
    /// <param name="segment">The <see cref="Segment"/> to expose.</param>
    /// <param name="stripStreamSegments">Indicates whether all <see cref="StreamSegment"/>s
    /// must be stripped from the <paramref name="stream"/>.</param>
    public StreamSegment(Stream stream, Segment<long> segment, bool stripStreamSegments)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      if (stripStreamSegments) {
        StreamSegment streamSegment = null;
        while (null!=(streamSegment = (stream as StreamSegment))) {
          var outerSegment = streamSegment.Segment;
          stream = streamSegment.Stream;
          segment = new Segment<long>(outerSegment.Offset+segment.Offset, segment.Length);
        }
      }
      Stream = stream;
      Segment = segment;
    }
  }
}