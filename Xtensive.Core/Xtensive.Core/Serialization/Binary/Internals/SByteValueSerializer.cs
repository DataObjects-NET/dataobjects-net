// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal class SByteValueSerializer : BinaryValueSerializerBase<sbyte>
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

    public SByteValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (sbyte);
    }
  }
}