// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;

namespace Xtensive.Core.Serialization.Implementation
{
  /// <inheritdoc/>
  public class ReferenceSerializer : ObjectSerializerBase<Reference>
  {
    protected const string ValuePropertyName = "Value";

    /// <inheritdoc/>
    public override bool IsReferable {
      get { return false; }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">Specified <paramref name="type"/> 
    /// is not supported by this serializer.</exception>
    public override Reference CreateObject(Type type) 
    {
      if (type!=typeof(Reference))
        throw new ArgumentOutOfRangeException("type");
      return Reference.Null;
    }

    /// <inheritdoc/>
    public override void GetObjectData(Reference source, Reference origin, SerializationData data) 
    {
      if (source.Value!=origin.Value)
        data.AddValue(ValuePropertyName, source.Value);
    }

    /// <inheritdoc/>
    public override Reference SetPropertyData(Reference target, SerializationData data, string propertyName)
    {
      if (propertyName==ValuePropertyName)
        return new Reference(data.GetValue<string>(ValuePropertyName));
      return target;
    }

    
    // Constructors

    /// <inheritdoc/>
    public ReferenceSerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
    }
  }
}