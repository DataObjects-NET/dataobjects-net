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
  internal sealed class UInt64ValueSerializer : ValueSerializerBase<ulong>
  {
    public override ulong Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (ulong)];
      stream.Read(buffer, 0, sizeof (ulong));
      return BitConverter.ToUInt64(buffer, 0);
    }

    public override void Serialize(Stream stream, ulong value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (ulong));
    }

    
    // Constructors

    public UInt64ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (ulong);
    }
  }
}