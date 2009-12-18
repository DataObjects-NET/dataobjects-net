// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Reflection;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  public sealed class TargetPropertyDescription : PropertyDescription
  {
    private SourcePropertyDescription sourceProperty;
    private Action<object, SourcePropertyDescription, object, TargetPropertyDescription> converter;
    private bool isImmutable;
    private bool isIgnored;

    public new TargetTypeDescription ReflectedType {
      get {
        return (TargetTypeDescription) base.ReflectedType;
      }
    }

    public SourcePropertyDescription SourceProperty
    {
      get { return sourceProperty; }
      set{
        this.EnsureNotLocked();
        sourceProperty = value;
      }
    }

    public bool IsIgnored {
      get { return isIgnored; }
      internal set {
        this.EnsureNotLocked();
        isIgnored = value;
      }
    }

    

    public bool IsImmutable {
      get { return isImmutable; }
      internal set {
        this.EnsureNotLocked();
        isImmutable = value;
      }
    }

    internal Action<object, SourcePropertyDescription, object, TargetPropertyDescription> Converter
    {
      get { return converter; }
      set{
        this.EnsureNotLocked();
        converter = value;
      }
    }

    
    // Constructors

    public TargetPropertyDescription(PropertyInfo systemProperty, TargetTypeDescription reflectedType)
      : base(systemProperty, reflectedType)
    {}
  }
}