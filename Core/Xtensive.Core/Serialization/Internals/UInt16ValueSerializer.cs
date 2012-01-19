// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.IO;

namespace Xtensive.Serialization
{
  [Serializable]
  internal sealed class UInt16ValueSerializer : ValueSerializerBase<ushort>
  {
    public override ushort Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (short)];
      stream.Read(buffer, 0, sizeof (short));
      return BitConverter.ToUInt16(buffer, 0);
    }

    public override void Serialize(Stream stream, ushort value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (short));
    }

    
    // Constructors

    public UInt16ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (ushort);
    }
  }
}