// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.15

using System;
using System.IO;

namespace Xtensive.Core.Serialization
{
  [Serializable]
  internal sealed class GuidValueSerializer : ValueSerializerBase<Guid>
  {
    public override Guid Deserialize(Stream stream) 
    {
      int length = OutputLength;
      EnsureThreadBufferIsInitialized(length);
      stream.Read(ThreadBuffer, 0, length);
      return new Guid(ThreadBuffer);
    }

    public override void Serialize(Stream stream, Guid value) 
    {
      stream.Write(value.ToByteArray(), 0, OutputLength);
    }

    
    // Constructors

    public GuidValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = 16;
    }
  }
}