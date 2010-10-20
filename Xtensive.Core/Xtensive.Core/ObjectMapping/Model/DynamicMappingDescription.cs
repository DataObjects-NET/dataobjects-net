// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.04

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.ObjectMapping.Model
{
  [Serializable]
  internal sealed class DynamicMappingDescription : MappingDescription
  {
    private readonly MappingDescription immutableDescription;

    public override IEnumerable<SourceTypeDescription> SourceTypes
    {
      get {
        foreach (var type in immutableDescription.SourceTypes)
          yield return type;
        foreach (var type in base.SourceTypes)
          yield return type;
      }
    }

    public override SourceTypeDescription GetSourceType(Type sourceType)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, "sourceType");
      SourceTypeDescription result;
      if (immutableDescription.TryGetSourceType(sourceType, out result))
        return result;
      if (TryGetSourceType(sourceType, out result))
        return result;
      var ancestor = sourceType;
      SourceTypeDescription ancestorDescription;
      do {
        ancestor = ancestor.BaseType;
        if (ancestor==null || ancestor==typeof (object))
          ThrowTypeHasNotBeenRegistered(sourceType);
      } while (!immutableDescription.TryGetSourceType(ancestor, out ancestorDescription));
      result = new SourceTypeDescription(sourceType, ancestorDescription.KeyExtractor) {
        TargetType = ancestorDescription.TargetType
      };
      foreach (var property in ancestorDescription.Properties.Values)
        result.AddProperty((SourcePropertyDescription) property);
      result.Lock();
      AddSourceType(result);
      return result;
    }


    // Constructors

    internal DynamicMappingDescription(MappingDescription immutableDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(immutableDescription, "immutableDescription");

      this.immutableDescription = immutableDescription;
    }
  }
}