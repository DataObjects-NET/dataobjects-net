// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.ObjectMapping.Model;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class ObjectExtractor
  {
    private readonly MappingDescription mappingDescription;
    private Queue<object> referencedObjects;

    public void Extract(object root, Dictionary<object,object> resultContainer)
    {
      if (root == null)
        return;
      referencedObjects = new Queue<object>();
      InitializeExtraction(root);
      while (referencedObjects.Count > 0) {
        var current = referencedObjects.Dequeue();
        if (current == null)
          continue;
        var key = mappingDescription.ExtractTargetKey(current);
        if (resultContainer.ContainsKey(key))
          continue;
        resultContainer.Add(key, current);
        var description = mappingDescription.TargetTypes[current.GetType()];
        foreach (var property in description.ComplexProperties.Values) {
          var value = property.SystemProperty.GetValue(current, null);
          if (value==null)
            continue;
          if (!property.IsCollection)
            referencedObjects.Enqueue(value);
          else
            foreach (var obj in (IEnumerable) value)
              referencedObjects.Enqueue(obj);
        }
      }
    }

    private void InitializeExtraction(object root)
    {
      var type = root.GetType();
      Type interfaceType;
      if (MappingHelper.IsCollectionCandidate(type)
        && MappingHelper.TryGetCollectionInterface(type, out interfaceType))
          foreach (var obj in (IEnumerable) root)
            referencedObjects.Enqueue(obj);
      else
        referencedObjects.Enqueue(root);
    }


    // Constructors

    public ObjectExtractor(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");

      this.mappingDescription = mappingDescription;
    }
  }
}