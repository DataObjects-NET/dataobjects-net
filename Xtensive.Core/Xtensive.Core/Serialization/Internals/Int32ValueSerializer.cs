// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.IO;

namespace Xtensive.Core.Serialization
{
  [Serializable]
  internal sealed class Int32ValueSerializer : ValueSerializerBase<int>
  {
    public override int Deserialize(Stream stream) 
    {
      int length = OutputLength;
      EnsureThreadBufferIsInitialized(length);
      stream.Read(ThreadBuffer, 0, length);
      return BitConverter.ToInt32(ThreadBuffer, 0);
    }

    public override void Serialize(Stream stream, int value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, OutputLength);
    }


    // Constructors

    public Int32ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (int);
    }
  }
}