// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System.Collections.Generic;
using Xtensive.Core.ObjectMapping.Model;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class ObjectExtractor
  {
    private readonly MappingDescription mappingDescription;
    private Queue<object> referencedObjects;

    public void Extract(object source, Dictionary<object,object> resultContainer)
    {
      if (source == null)
        return;
      referencedObjects = new Queue<object>();
      referencedObjects.Enqueue(source);
      while (referencedObjects.Count > 0) {
        var current = referencedObjects.Dequeue();
        var key = mappingDescription.ExtractTargetKey(current);
        if (resultContainer.ContainsKey(key))
          continue;
        resultContainer.Add(key, current);
        var description = mappingDescription.TargetTypes[current.GetType()];
        foreach (var property in description.ComplexProperties.Keys) {
          var value = property.GetValue(current, null);
          if (value != null)
            referencedObjects.Enqueue(value);
        }
      }
    }


    // Constructors

    public ObjectExtractor(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");

      this.mappingDescription = mappingDescription;
    }
  }
}