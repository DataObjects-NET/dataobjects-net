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
  internal class CharValueSerializer : WrappingValueSerializer<char, int>
  {
    public override char Deserialize(Stream stream) {
      unchecked {
        return (char) baseValueSerializer.Deserialize(stream);
      }
    }

    public override void Serialize(Stream stream, Char value) {
      baseValueSerializer.Serialize(stream, value);
    }

    // Constructors

    public CharValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}