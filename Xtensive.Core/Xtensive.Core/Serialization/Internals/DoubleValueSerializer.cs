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
  internal sealed class DoubleValueSerializer : ValueSerializerBase<double>
  {
    public override double Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (double)];
      stream.Read(buffer, 0, sizeof (double));
      return BitConverter.ToDouble(buffer, 0);
    }

    public override void Serialize(Stream stream, double value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (double));
    }


    // Constructors

    public DoubleValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (double);
    }
  }
}