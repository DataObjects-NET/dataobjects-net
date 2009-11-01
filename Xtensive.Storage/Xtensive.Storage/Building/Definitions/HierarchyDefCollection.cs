// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Building.Definitions
{
  public class HierarchyDefCollection : CollectionBase<HierarchyDef>
  {
    /// <inheritdoc/>
    public override bool Contains(HierarchyDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      HierarchyDef result;
      return TryGetValue(item.Root, out result);
    }

    public bool TryGetValue(TypeDef key, out HierarchyDef value)
    {
      return TryGetValue(key.UnderlyingType, out value);
    }

    public bool TryGetValue(Type key, out HierarchyDef value)
    {
      foreach (HierarchyDef item in this) {
        if (item.Root.UnderlyingType == key) {
          value = item;
          return true;
        }
      }
      value = null;
      return false;
    }

    public HierarchyDef this[Type key]
    {
      get
      {
        HierarchyDef result;
        if (TryGetValue(key, out result))
          return result;
        throw new ArgumentException(String.Format(String.Format("Item '{0}' not found.", key)));
      }
    }
  }
}