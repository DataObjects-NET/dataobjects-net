// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  public sealed class MappingDescription : LockableBase
  {
    private readonly Dictionary<Type, TargetTypeDescription> targetTypes =
      new Dictionary<Type, TargetTypeDescription>();
    private readonly Dictionary<Type, SourceTypeDescription> sourceTypes =
      new Dictionary<Type, SourceTypeDescription>();

    public ReadOnlyDictionary<Type, TargetTypeDescription> TargetTypes { get; private set; }

    public ReadOnlyDictionary<Type, SourceTypeDescription> SourceTypes { get; private set; }

    public void Register(Type source, Func<object, object> sourceKeyExtractor, Type target,
      Func<object, object> targetKeyExtractor)
    {
      this.EnsureNotLocked();
      if (sourceTypes.ContainsKey(source))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasAlreadyBeenRegistered,
          target.FullName));
      if (targetTypes.ContainsKey(target))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasAlreadyBeenRegistered,
          target.FullName));
      var sourceDesc = new SourceTypeDescription(source, sourceKeyExtractor);
      var targetDesc = new TargetTypeDescription(target, targetKeyExtractor);
      sourceDesc.TargetType = targetDesc;
      targetDesc.SourceType = sourceDesc;
      sourceTypes.Add(source, sourceDesc);
      targetTypes.Add(target, targetDesc);
    }

    public void Register(PropertyInfo source, Func<object, object> converter, PropertyInfo target)
    {
      this.EnsureNotLocked();
      var declaringType = targetTypes[target.DeclaringType];
      var propertyDesc = new TargetPropertyDescription(target, declaringType) {
        Converter = (sourceObj, sourceProperty, targetObj, targetProperty) =>
          targetProperty.SystemProperty.SetValue(targetObj, converter.Invoke(sourceObj), null),
        IsCollection = false
      };
      declaringType.AddProperty(propertyDesc);
    }

    public override void Lock(bool recursive)
    {
      foreach (var type in sourceTypes)
        type.Value.Lock(true);
      foreach (var type in targetTypes)
        type.Value.Lock(true);
      base.Lock(recursive);
    }

    public TargetTypeDescription GetMappedType(Type sourceType)
    {
      return sourceTypes[sourceType].TargetType;
    }

    public object ExtractTargetKey(object target)
    {
      return targetTypes[target.GetType()].KeyExtractor.Invoke(target);
    }

    public object ExtractSourceKey(object source)
    {
      return sourceTypes[source.GetType()].KeyExtractor.Invoke(source);
    }


    // Constructor

    public MappingDescription()
    {
      SourceTypes = new ReadOnlyDictionary<Type, SourceTypeDescription>(sourceTypes, false);
      TargetTypes = new ReadOnlyDictionary<Type, TargetTypeDescription>(targetTypes, false);
    }
  }
}