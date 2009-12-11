// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Reflection;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  public sealed class SourcePropertyDescription : PropertyDescription
  {
    public new SourceTypeDescription DeclaringType {
      get { return (SourceTypeDescription) base.DeclaringType; }
    }


    // Constructors

    public SourcePropertyDescription(PropertyInfo systemProperty, SourceTypeDescription declaringType)
      : base(systemProperty, declaringType)
    {}
  }
}