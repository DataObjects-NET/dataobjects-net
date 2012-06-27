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
  internal sealed class Int16ValueSerializer : ValueSerializerBase<short>
  {
    public override short Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (short)];
      stream.Read(buffer, 0, sizeof (short));
      return BitConverter.ToInt16(buffer, 0);
    }

    public override void Serialize(Stream stream, short value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (short));
    }

    
    // Constructors

    public Int16ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (short);
    }
  }
}