// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.IO
{
  /// <summary>
  /// Exposes a <see cref="Segment"/> of the specified <see cref="Stream"/> 
  /// as the independent stream.
  /// </summary>
  /// <remarks>
  /// Any read and write operations on this instance don't affect on
  /// <see cref="System.IO.Stream.Position"/> of the underlying <see cref="Stream"/>.
  /// </remarks>
  [Serializable]
  [DebuggerDisplay("Offset = {Segment.Offset}, Length = {Segment.Length}")]
  public class StreamSegment : Stream
  {
    private readonly bool isReadOnly;
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
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, Segment.Length, "value");
        EnsureCanSeek();
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
      get { return isReadOnly ? false : Stream.CanWrite; }
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
      EnsureCanWrite();
      if (Stream.Length==Segment.EndOffset)
        Stream.SetLength(Segment.Offset + value);
      Segment = new Segment<long>(Segment.Offset, value);
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
      long oldPosition = Stream.Position;
      try {
        Stream.Position = Segment.Offset + position;
        var maxCount = Segment.Length-position;
        if (count > maxCount)
          count = (int) maxCount;
        int actualCount = Stream.Read(buffer, offset, count);
        position += actualCount;
        return actualCount;
      }
      finally {
        Stream.Position = oldPosition;
      }
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
      EnsureCanWrite();
      long oldPosition = Stream.Position;
      try {
        Stream.Position = Segment.Offset + position;
        var maxCount = Segment.Length-position;
        if (count > maxCount)
          Segment = new Segment<long>(Segment.Offset, count);
        Stream.Write(buffer, offset, count);
        position += count;
      }
      finally {
        Stream.Position = oldPosition;
      }
    }

    /// <inheritdoc/>
    public override void Flush()
    {
      if (!isReadOnly)
        Stream.Flush();
    }

    #region Private \ internal methods

    private void EnsureCanSeek()
    {
      if (!CanSeek)
        throw new NotSupportedException();
    }

    private void EnsureCanWrite()
    {
      if (!CanWrite)
        throw Exceptions.ObjectIsReadOnly(null);
    }

    #endregion


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
    /// <param name="isReadOnly">If set to <see langword="true"/>, <see cref="Write"/> 
    /// and <see cref="SetLength"/> operations will throw an exception.</param>
    public StreamSegment(Stream stream, Segment<long> segment, bool isReadOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      Stream = stream;
      Segment = segment;
      this.isReadOnly = isReadOnly;
    }
  }
}