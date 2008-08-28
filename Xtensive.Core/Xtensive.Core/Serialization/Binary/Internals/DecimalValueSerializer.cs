// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal sealed class DecimalValueSerializer : BinaryValueSerializerBase<decimal>
  {
    [ThreadStatic]
    private static int[] intBuffer;

    public override decimal Deserialize(Stream stream) 
    {
      EnsureBuffersAreInitialized();
      int length = OutputLength;
      stream.Read(ThreadBuffer, 0, length);
      for (int i = 0; i < sizeof (decimal) / sizeof (int); i++)
        intBuffer[i] = BitConverter.ToInt32(ThreadBuffer, i * sizeof (int));
      return new decimal(intBuffer);
    }

    public override void Serialize(Stream stream, decimal value) 
    {
      var newIntBuffer = decimal.GetBits(value);
      for (int i = 0; i < sizeof (decimal) / sizeof (int); i++)
        BitConverter.GetBytes(newIntBuffer[i]).Copy(ThreadBuffer, i * sizeof (int));
      stream.Write(ThreadBuffer, 0, OutputLength);
    }

    private static void EnsureBuffersAreInitialized()
    {
      if (intBuffer==null) {
        intBuffer = new int[sizeof (decimal) / sizeof (int)];
        EnsureThreadBufferIsInitialized(sizeof (decimal));
      }
    }


    // Constructors

    public DecimalValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (decimal);
    }
  }
}