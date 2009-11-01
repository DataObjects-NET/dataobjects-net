// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Resources;
using System.Text;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal class StringValueSerializer : WrappingValueSerializer<string, int, char>
  {
    private static Encoding encoding = Encoding.UTF8;

    public override string Deserialize(Stream stream)
    {
      if (stream.Length - stream.Position < sizeof (Int16))
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      int length = BaseValueSerializer1.Deserialize(stream);
      if (length==-1)
        return null;
      if (stream.Length - stream.Position < length)
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      byte[] buffer = new byte[length];
      stream.Read(buffer,0,length);
      return encoding.GetString(buffer);
    }

    public override void Serialize(Stream stream, String value)
    {
      if (value==null) {
        BaseValueSerializer1.Serialize(stream, -1);
      }
      else {
        byte[] byteRepresentation = encoding.GetBytes(value);
        int length = byteRepresentation.Length;
        BaseValueSerializer1.Serialize(stream, length);
        stream.Write(byteRepresentation,0, length);
      }
    }


    // Constructors

    public StringValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}