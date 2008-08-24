// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.21

namespace Xtensive.Core.Serialization
{
  /// <inheritdoc/>
  public class ReferenceSerializer : ObjectSerializerBase<Reference>
  {
    /// <inheritdoc/>
    public override bool IsReferable {
      get { return false; }
    }

    /// <inheritdoc/>
    public override Reference CreateObject() 
    {
      return Reference.Null;
    }

    /// <inheritdoc/>
    public override void GetObjectData(Reference obj, SerializationData data) 
    {
      if (obj.Value != null)
        data.AddValue("Id", obj.Value);
    }

    /// <inheritdoc/>
    public override Reference SetObjectData(Reference obj, SerializationData data) 
    {
      return new Reference(data.GetValue<string>("Id"));
    }

    
    // Constructors

    /// <inheritdoc/>
    public ReferenceSerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
    }
  }
}