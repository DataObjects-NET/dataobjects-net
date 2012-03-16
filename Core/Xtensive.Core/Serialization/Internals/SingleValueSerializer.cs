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
  internal sealed class SingleValueSerializer : ValueSerializerBase<float>
  {
    public override float Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (float)];
      stream.Read(buffer, 0, sizeof (float));
      return BitConverter.ToSingle(buffer, 0);
    }

    public override void Serialize(Stream stream, float value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (float));
    }


    // Constructors

    public SingleValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (float);
    }
  }
}