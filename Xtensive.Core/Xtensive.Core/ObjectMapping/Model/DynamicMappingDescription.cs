// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.04

using System;
using System.Collections.Generic;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  internal sealed class DynamicMappingDescription : MappingDescription
  {
    private readonly MappingDescription immutableDescription;
    private Dictionary<Type, SourceTypeDescription> sourceTypes;

    public override IEnumerable<SourceTypeDescription> SourceTypes
    {
      get {
        foreach (var type in immutableDescription.SourceTypes)
          yield return type;
        if (sourceTypes==null)
          yield break;
        foreach (var type in sourceTypes.Values)
          yield return type;
      }
    }

    public override SourceTypeDescription GetSourceType(Type sourceType)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, "sourceType");
      SourceTypeDescription result;
      if (immutableDescription.TryGetSourceType(sourceType, out result))
        return result;
      if (sourceTypes==null)
        sourceTypes=new Dictionary<Type, SourceTypeDescription>();
      else if (sourceTypes.TryGetValue(sourceType, out result))
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
      sourceTypes.Add(sourceType, result);
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