// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System.Collections.Generic;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class ObjectExtractor
  {
    private readonly MappingInfo mappingInfo;
    private Queue<object> referencedObjects;

    public void Extract(object source, Dictionary<object,object> resultContainer)
    {
      if (source == null)
        return;
      referencedObjects = new Queue<object>();
      referencedObjects.Enqueue(source);
      while (referencedObjects.Count > 0) {
        var current = referencedObjects.Dequeue();
        var key = mappingInfo.ExtractKey(current);
        if (resultContainer.ContainsKey(key))
          continue;
        resultContainer.Add(key, current);
        var complexProperties = mappingInfo.GetTargetComplexProperties(current.GetType());
        foreach (var property in complexProperties) {
          var value = property.GetValue(current, null);
          if (value != null)
            referencedObjects.Enqueue(value);
        }
      }
    }


    // Constructors

    public ObjectExtractor(MappingInfo mappingInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingInfo, "mappingInfo");

      this.mappingInfo = mappingInfo;
    }
  }
}