// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  public sealed class TargetTypeDescription : TypeDescription
  {
    private SourceTypeDescription sourceType;

    public SourceTypeDescription SourceType
    {
      get { return sourceType; }
      set{
        this.EnsureNotLocked();
        sourceType = value;
      }
    }

    public ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription> PrimitiveProperties { get; private set; }

    public ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription> ComplexProperties { get; private set; }

    public new TargetPropertyDescription GetProperty(PropertyInfo propertyInfo)
    {
      return (TargetPropertyDescription) base.GetProperty(propertyInfo);
    }

    public void AddProperty(TargetPropertyDescription property)
    {
      base.AddProperty(property);
    }

    public override void Lock(bool recursive)
    {
      var primitiveProperties = new Dictionary<PropertyInfo, TargetPropertyDescription>();
      foreach (var pair in Properties.Where(p => p.Value.IsPrimitive))
        primitiveProperties.Add(pair.Key, (TargetPropertyDescription) pair.Value);
      PrimitiveProperties =
        new ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription>(primitiveProperties, false);
      var complexProperties = new Dictionary<PropertyInfo, TargetPropertyDescription>();
      foreach (var pair in Properties.Where(p => !p.Value.IsPrimitive))
        complexProperties.Add(pair.Key, (TargetPropertyDescription) pair.Value);
      ComplexProperties =
        new ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription>(complexProperties, false);
      base.Lock(recursive);
    }


    // Constructors

    public TargetTypeDescription(Type systemType, Func<object, object> keyExtractor)
      : base(systemType, keyExtractor)
    {}
  }
}