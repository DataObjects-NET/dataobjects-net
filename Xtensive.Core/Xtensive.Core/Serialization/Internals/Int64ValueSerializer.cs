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
  internal sealed class Int64ValueSerializer : ValueSerializerBase<long>
  {
    public override long Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (long)];
      stream.Read(buffer, 0, sizeof (long));
      return BitConverter.ToInt64(buffer, 0);
    }

    public override void Serialize(Stream stream, long value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (long));
    }


    // Constructors

    public Int64ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (long);
    }
  }
}