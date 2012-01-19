// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;
using System.Text;

namespace Xtensive.Serialization
{
  [Serializable]
  internal sealed class StringValueSerializer : WrappingValueSerializer<string, int>
  {
    private static readonly Encoding encoding = new UTF8Encoding(false, true);

    public override string Deserialize(Stream stream) 
    {
      int length = BaseSerializer.Deserialize(stream);
      if (length == -1)
        return null;
      var buffer = new byte[length];
      stream.Read(buffer, 0, length);
      return encoding.GetString(buffer);
    }

    public override void Serialize(Stream stream, String value) 
    {
      if (value == null) 
        BaseSerializer.Serialize(stream, -1);
      else {
        var buffer = encoding.GetBytes(value);
        int length = buffer.Length;
        BaseSerializer.Serialize(stream, length);
        stream.Write(buffer, 0, length);
      }
    }

    
    // Constructors

    public StringValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}