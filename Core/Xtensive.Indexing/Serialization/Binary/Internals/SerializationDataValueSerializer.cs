// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.IO;
using Xtensive.Core;
using Xtensive.Indexing.IO;

namespace Xtensive.Indexing.Serialization.Binary
{
  [Serializable]
  internal sealed class SerializationDataValueSerializer : WrappingValueSerializer<SerializationData, long>
  {
    public override SerializationData Deserialize(Stream stream)
    {
      long length = BaseSerializer.Deserialize(stream);
      long offset = stream.Position;
      stream.Position = offset + length;
      return new BinarySerializationData(
        new StreamSegment(stream, new Segment<long>(offset, length), true));
    }

    public override void Serialize(Stream stream, SerializationData value)
    {
      var bsd = (BinarySerializationData) value;
      var source = bsd.Stream;
      long position = source.Position;
      try {
        source.Position = 0;
        BaseSerializer.Serialize(stream, source.Length);
        source.CopyTo(stream);
      }
      finally {
        source.Position = position;
      }
    }


    // Constructors
    
    public SerializationDataValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}