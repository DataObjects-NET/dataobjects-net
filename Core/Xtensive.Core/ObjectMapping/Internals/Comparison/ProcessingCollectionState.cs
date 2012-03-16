// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.23

using System.Collections;
using System.Collections.Generic;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping.Comparison
{
  internal abstract class ProcessingCollectionState : ComparerStateBase
  {
    private Dictionary<object, object> originalKeyCache;
    private Dictionary<object, object> modifiedKeyCache;

    protected Dictionary<object, object> ExtractOriginalKeys(IEnumerable collection)
    {
      if (originalKeyCache==null)
        originalKeyCache = new Dictionary<object, object>();
      else
        originalKeyCache.Clear();
      ExtractKeys(collection, originalKeyCache);
      return originalKeyCache;
    }

    protected Dictionary<object, object> ExtractModifiedKeys(IEnumerable collection)
    {
      if (modifiedKeyCache==null)
        modifiedKeyCache = new Dictionary<object, object>();
      else
        modifiedKeyCache.Clear();
      ExtractKeys(collection, modifiedKeyCache);
      return modifiedKeyCache;
    }

    protected void NotifyAboutCollectionModification(TargetPropertyDescription property,
      bool adding, object item)
    {
      var path = GetFullPath(property);
      GraphComparer.Subscriber
        .Invoke(new Operation(GraphComparer.ComparisonInfo.Owner,
          adding ? OperationType.AddItem : OperationType.RemoveItem, path, item));
    }

    private void ExtractKeys(IEnumerable collection, IDictionary<object, object> keyCache)
    {
      if (collection == null)
        return;
      foreach (var item in collection) {
        if (item==null)
          continue;
        keyCache.Add(GraphComparer.MappingDescription.ExtractTargetKey(item), item);
      }
    }


    // Constructors

    protected ProcessingCollectionState(GraphComparer graphComparer)
      : base(graphComparer)
    {}
  }
}