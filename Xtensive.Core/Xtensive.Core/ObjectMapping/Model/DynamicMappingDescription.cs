// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using System.Linq;

namespace Xtensive.ObjectMapping.Model
{
  [Serializable]
  internal sealed class DynamicMappingDescription : MappingDescription
  {
    private readonly MappingDescription immutableDescription;

    public override IEnumerable<SourceTypeDescription> SourceTypes
    {
      get
      {
        return immutableDescription.SourceTypes.Concat(base.SourceTypes);
      }
    }

    public override SourceTypeDescription GetSourceType(Type sourceType)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, "sourceType");
      SourceTypeDescription result;
      if (TryGetSourceType(sourceType, out result))
        return result;
      ThrowTypeHasNotBeenRegistered(sourceType);
      return null;
    }

    internal override bool TryGetSourceType(Type sourceType, out SourceTypeDescription result)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, "sourceType");

      if (immutableDescription.TryGetSourceType(sourceType, out result))
        return true;
      if (base.TryGetSourceType(sourceType, out result))
        return true;
      var ancestor = sourceType;
      SourceTypeDescription ancestorDescription;
      do {
        ancestor = ancestor.BaseType;
        if (ancestor == null || ancestor == typeof(object))
          return false;
      } while (!immutableDescription.TryGetSourceType(ancestor, out ancestorDescription));
      result = new SourceTypeDescription(sourceType, ancestorDescription.KeyExtractor) {
        TargetType = ancestorDescription.TargetType
      };
      foreach (var property in ancestorDescription.Properties.Values)
        result.AddProperty((SourcePropertyDescription)property);
      result.Lock();
      AddSourceType(result);
      return true;
    }

    public override TargetTypeDescription GetTargetType(Type targetType)
    {
      ArgumentValidator.EnsureArgumentNotNull(targetType, "targetType");
      TargetTypeDescription result;
      if (TryGetTargetType(targetType, out result))
        return result;
      ThrowTypeHasNotBeenRegistered(targetType);
      return null;
    }

    internal override bool TryGetTargetType(Type targetType, out TargetTypeDescription result)
    {
      ArgumentValidator.EnsureArgumentNotNull(targetType, "sourceType");
      if (immutableDescription.TryGetTargetType(targetType, out result))
        return true;
      return false;
    }


    // Constructors

    internal DynamicMappingDescription(MappingDescription immutableDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(immutableDescription, "immutableDescription");

      this.immutableDescription = immutableDescription;
    }
  }
}