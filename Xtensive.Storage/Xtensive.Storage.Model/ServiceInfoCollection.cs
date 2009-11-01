// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.07

using System;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class ServiceInfoCollection: NodeCollection<ServiceInfo> //,
    //IIndexable<Type, ServiceInfo>
  {
    /// <summary>
    /// An indexer that provides access to collection items.
    /// Returns <see langword="null"/> if there is no such item.
    /// </summary>
    public ServiceInfo this[Type key]
    {
      get
      {
        if (key == null)
          return null;
        foreach (ServiceInfo item in this) {
          if (item.UnderlyingType == key)
            return item;
        }
        throw new ArgumentException(String.Format(String.Format("Item '{0}' not found.", key)));
      }
    }
  }
}