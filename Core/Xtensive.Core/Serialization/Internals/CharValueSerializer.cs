// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;

namespace Xtensive.Serialization
{
  [Serializable]
  internal sealed class CharValueSerializer : WrappingValueSerializer<char, ushort>
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

    public CharValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (ushort);
    }
  }
}