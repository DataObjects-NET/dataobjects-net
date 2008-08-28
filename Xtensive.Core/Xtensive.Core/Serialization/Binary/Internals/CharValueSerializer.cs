// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal sealed class CharValueSerializer : WrappingBinaryValueSerializer<char, ushort>
  {
    public override char Deserialize(Stream stream) 
    {
      unchecked {
        return (char) BaseSerializer.Deserialize(stream);
      }
    }

    public override void Serialize(Stream stream, Char value) 
    {
      BaseSerializer.Serialize(stream, value);
    }

    
    // Constructors

    public CharValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (ushort);
    }
  }
}