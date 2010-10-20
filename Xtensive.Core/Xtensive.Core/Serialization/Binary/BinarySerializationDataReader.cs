// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.IO;
using Xtensive.Serialization.Implementation;

namespace Xtensive.Serialization.Binary
{
  /// <summary>
  /// Binary <see cref="SerializationData"/> reader.
  /// </summary>
  public class BinarySerializationDataReader : SerializationDataReader
  {
    /// <inheritdoc/>
    public override IEnumerator<SerializationData> GetEnumerator()
    {
      var current = BinarySerializationContext.Current;
      var stream = current.Stream;
      var longSerializer = current.Int64Serializer;
      while (true) {
        long length = longSerializer.Deserialize(stream);
        if (length==0)
          continue;
        if (length<0)
          break;
        long lastPosition = stream.Position;
        yield return new BinarySerializationData(
          new StreamSegment(stream, new Segment<long>(stream.Position, length), true));
        stream.Position = lastPosition + length;
      }
    }
  }
}