// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.Collections.Generic;
using System.IO;
using Xtensive.Core;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Integrity.Resources;
using Xtensive.Core.Serialization;

namespace Xtensive.Integrity.Atomicity.OperationLogs
{
  public class StreamOperationLog: OperationLogBase
  {
    private ISerializer<Stream> serializer;
    private Stream stream;

    public override void Append(IRedoDescriptor redoDescriptor)
    {
      ValidateDescriptor(redoDescriptor);
      long position = stream.Position;
      try {
        stream.Seek(0, SeekOrigin.End);
        serializer.Serialize(stream, redoDescriptor);
      }
      finally {
        stream.Seek(position, SeekOrigin.Begin);
      }
    }

    public override IEnumerator<IRedoDescriptor> GetEnumerator(Direction direction)
    {
      if (direction==Direction.Positive) {
        long position = 0;
        while (true) {
          if (position>=stream.Length)
            break;
          long streamPosition = stream.Position;
          object descriptor;
          try {
            stream.Seek(position, SeekOrigin.Begin);
            descriptor = serializer.Deserialize(stream);
            position = stream.Position;
          }
          finally {
            stream.Seek(streamPosition, SeekOrigin.Begin);
          }
          yield return (IRedoDescriptor)descriptor;
        }
      }
      else {
        throw new NotSupportedException();
      }
    }


    // Constructors

    public StreamOperationLog()
      : this(new MemoryStream())
    {
    }

    public StreamOperationLog(ISerializer<Stream> serializer)
      : this(serializer, new MemoryStream())
    {
    }

    public StreamOperationLog(Stream stream)
      : this(new LegacyBinarySerializer(), stream)
    {
    }

    public StreamOperationLog(ISerializer<Stream> serializer, Stream stream)
    {
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      this.serializer = serializer;
      this.stream = stream;
    }
  }
}