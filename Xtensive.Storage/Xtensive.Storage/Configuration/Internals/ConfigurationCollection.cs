// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Collections.Generic;
using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal class ConfigurationCollection<T> : ConfigurationElementCollection,
    IEnumerable<T>
    where T : ConfigurationCollectionElementBase, new()
  {
    protected override ConfigurationElement CreateNewElement()
    { 
      return new T();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((ConfigurationCollectionElementBase) element).GetKey();
    }

    public new IEnumerator<T> GetEnumerator()
    {
      foreach (object element in (ConfigurationElementCollection)this) {
        yield return (T) element;
      };
    }
  }
}