// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Internals
{
  [Serializable]
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
        data.AddValue(ValuePropertyName, source.Value);
    }

    public override Reference SetPropertyData(Reference target, SerializationData data, string propertyName)
    {
      if (propertyName==ValuePropertyName)
        return new Reference(data.GetValue<string>(ValuePropertyName));
      return target;
    }

    
    // Constructors

    public ReferenceSerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
    }
  }
}