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
  internal class DoubleValueSerializer : BinaryValueSerializerBase<double>
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

    public DoubleValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (double);
    }
  }
}