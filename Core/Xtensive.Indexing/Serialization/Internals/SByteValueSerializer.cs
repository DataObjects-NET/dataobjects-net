// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.IO;

namespace Xtensive.Indexing.Serialization
{
  [Serializable]
  internal sealed class SByteValueSerializer : ValueSerializerBase<sbyte>
  {
    public override sbyte Deserialize(Stream stream) 
    {
      return (sbyte) stream.ReadByte();
    }

    public override void Serialize(Stream stream, sbyte value) 
    {
      stream.WriteByte((byte) value);
    }

    
    // Constructors

    public SByteValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (sbyte);
    }
  }
}