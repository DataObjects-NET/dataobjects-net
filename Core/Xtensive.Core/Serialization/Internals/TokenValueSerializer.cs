// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.28

using System;
using System.IO;
using Xtensive.Serialization.Implementation;

namespace Xtensive.Serialization
{
  [Serializable]
  internal class TokenValueSerializer<T> : WrappingValueSerializer<Token<T>, int, T>
  {
    public override Token<T> Deserialize(Stream stream)
    {
      var identifier = BaseSerializer1.Deserialize(stream);
      if (identifier < 0)
        // First deserialization, so we're registering a new token
        return new Token<T>(BaseSerializer2.Deserialize(stream), ~identifier);
      else
        // Otherwise - getting the known one
        return Token.Get<T>(SerializationContext.Current, identifier);
    }

    public override void Serialize(Stream stream, Token<T> value)
    {
      int identifier = value.Identifier;
      BaseSerializer1.Serialize(stream, identifier);
      if (identifier<0) {
        // First serialization, so we must write value as well
        BaseSerializer2.Serialize(stream, value.Value);
        value.Identifier = ~identifier;
      }
    }


    // Constructors
    
    public TokenValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}