// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.28

using System;
using Xtensive.Collections;
using Xtensive.Reflection;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="KeyGeneratorConfiguration"/> collection.
  /// </summary>
  public sealed class KeyGeneratorConfigurationCollection : CollectionBaseSlim<KeyGeneratorConfiguration>, ICloneable
  {
    /// <summary>
    /// Adds new <see cref="KeyGeneratorConfiguration"/> to this collection
    /// and starts <see cref="IKeyGeneratorConfigurationFlow"/>.
    /// </summary>
    /// <param name="name">Name of the key generator.</param>
    /// <returns>Key generator configuration flow</returns>
    public IKeyGeneratorConfigurationFlow Add(string name)
    {
      var configuration = new KeyGeneratorConfiguration(name);
      Add(configuration);
      return new KeyGeneratorConfigurationFlow(configuration);
    }

    /// <summary>
    /// Adds new <see cref="KeyGeneratorConfiguration"/> to this collection
    /// and starts <see cref="IKeyGeneratorConfigurationFlow"/>.
    /// </summary>
    /// <typeparam name="TKeyType">Key value type (like <see cref="int"/> or <see cref="long"/>).</typeparam>
    /// <returns>Key generator configuration flow.</returns>
    public IKeyGeneratorConfigurationFlow Add<TKeyType>()
      where TKeyType : struct
    {
      // TODO: validate TKeyType
      return Add(typeof (TKeyType).GetShortName());
    }

    /// <inheritdoc/>
    public object Clone()
    {
      var result = new KeyGeneratorConfigurationCollection();
      foreach (var generator in this)
        result.Add(generator.Clone());
      return result;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      if (recursive)
        foreach (var generator in this)
          generator.Lock(true);

      base.Lock(recursive);
    }
  }
}