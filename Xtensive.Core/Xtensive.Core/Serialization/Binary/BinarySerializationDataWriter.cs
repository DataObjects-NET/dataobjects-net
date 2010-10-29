// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.IO;
using Xtensive.Serialization.Implementation;

namespace Xtensive.Serialization.Binary
{
  /// <summary>
  /// Binary <see cref="SerializationData"/> writer.
  /// </summary>
  public class BinarySerializationDataWriter : SerializationDataWriter
  {
    /// <inheritdoc/>
    public override SerializationData Create(IReference reference, object source, object origin)
    {
      return new BinarySerializationData(reference, source, origin);
    }

    /// <inheritdoc/>
    public override void Append(SerializationData data)
    {
      var current = BinarySerializationContext.Current;
      var stream = current.Stream;
      var longSerializer = current.Int64Serializer;
      
      var binaryData = (BinarySerializationData) data;
      var src = binaryData.Stream;
      long oldPosition = src.Position;
      try {
        src.Position = 0;
        // Part length marker
        longSerializer.Serialize(stream, src.Length);
        // Part itself
        src.CopyTo(stream);
      }
      finally {
        src.Position = oldPosition;
      }
    }

    /// <inheritdoc/>
    public override void Complete()
    {
      var current = BinarySerializationContext.Current;
      var stream = current.Stream;
      var longSerializer = current.Int64Serializer;
      
      // Serializing EOF marker
      longSerializer.Serialize(stream, -1);
    }
  }
}