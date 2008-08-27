// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Diagnostics;
using System.IO;
using Xtensive.Core.Serialization.Implementation;
using Xtensive.Core.IO;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Binary <see cref="SerializationData"/> writer.
  /// </summary>
  public class BinarySerializationDataWriter : SerializationDataWriter
  {
    /// <inheritdoc/>
    public override SerializationData Create(IReference reference, object source, object origin, bool preferNesting)
    {
      return new BinarySerializationData(reference, source, origin, preferNesting);
    }

    /// <inheritdoc/>
    public override void Append(SerializationData data)
    {
      var current = BinarySerializationContext.Current;
      var stream = current.Stream;
      var longSerializer = current.LongSerializer;
      
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
      var longSerializer = current.LongSerializer;
      
      // Serializing EOF marker
      longSerializer.Serialize(stream, -1);
    }
  }
}