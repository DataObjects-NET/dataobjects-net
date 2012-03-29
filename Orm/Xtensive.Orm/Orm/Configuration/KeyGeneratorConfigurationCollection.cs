// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.28

using System;
using Xtensive.Collections;

namespace Xtensive.Orm.Configuration
{
  public sealed class KeyGeneratorConfigurationCollection : CollectionBaseSlim<KeyGeneratorConfiguration>, ICloneable
  {
    public override void Lock(bool recursive)
    {
      if (recursive)
        foreach (var generator in this)
          generator.Lock(true);

      base.Lock(recursive);
    }

    public object Clone()
    {
      var clone = new KeyGeneratorConfigurationCollection();
      foreach (var generator in this)
        clone.Add(new KeyGeneratorConfiguration(generator.Name, generator.Seed, generator.CacheSize));
      return clone;
    }
  }
}