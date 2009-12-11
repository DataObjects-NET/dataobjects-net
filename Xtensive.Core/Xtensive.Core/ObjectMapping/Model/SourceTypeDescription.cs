// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  [DebuggerDisplay("SystemType = {SystemType}")]
  public sealed class SourceTypeDescription : TypeDescription
  {
    private TargetTypeDescription targetType;

    public TargetTypeDescription TargetType
    {
      get { return targetType; }
      set{
        this.EnsureNotLocked();
        targetType = value;
      }
    }

    public new SourcePropertyDescription GetProperty(PropertyInfo propertyInfo)
    {
      return (SourcePropertyDescription) base.GetProperty(propertyInfo);
    }

    public void AddProperty(SourcePropertyDescription property)
    {
      base.AddProperty(property);
    }


    // Constructors

    public SourceTypeDescription(Type systemType, Func<object, object> keyExtractor)
      : base(systemType, keyExtractor)
    {}
  }
}