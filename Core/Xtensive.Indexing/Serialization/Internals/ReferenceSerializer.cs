// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using Xtensive.Indexing.Serialization.Implementation;

namespace Xtensive.Indexing.Serialization.Internals
{
  internal sealed class ReferenceSerializer : ObjectSerializerBase<Reference>
  {
    public const string ValuePropertyName = "Value";

    public override bool IsReferable {
      get { return false; }
    }

    /// <exception cref="ArgumentOutOfRangeException">Specified <paramref name="type"/> 
    /// is not supported by this serializer.</exception>
    public override Reference CreateObject(Type type) 
    {
      if (type!=typeof(Reference))
        throw new ArgumentOutOfRangeException("type");
      return Reference.Null;
    }

    public override void GetObjectData(Reference source, Reference origin, SerializationData data) 
    {
      base.GetObjectData(source, origin, data);
      if (source.Value!=origin.Value)
        data.AddValue(ValuePropertyName, source.Value, StringSerializer);
    }

    public override Reference SetObjectData(Reference source, SerializationData data)
    {
      if (data.HasValue(ValuePropertyName))
        return new Reference(data.GetValue(ValuePropertyName, StringSerializer));
      else
        return source;
    }

    // Constructors

    public ReferenceSerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
    }
  }
}