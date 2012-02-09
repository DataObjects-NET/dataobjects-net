// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.07

using System;
using System.Reflection;
using Xtensive.Collections;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="MappingRule"/> collection.
  /// </summary>
  public sealed class MappingRuleCollection : CollectionBaseSlim<MappingRule>, ICloneable
  {
    /// <summary>
    /// Starts construction of <see cref="MappingRule"/>
    /// for the specified <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">Assembly to map from.</param>
    /// <returns><see cref="MappingRule"/> construction flow.</returns>
    public IMappingRuleConstructionFlow Map(Assembly assembly)
    {
      return new MappingRuleConstructionFlow(this, assembly, null);
    }

    /// <summary>
    /// Starts construction if <see cref="MappingRule"/>
    /// for the specified <paramref name="namespace"/>.
    /// </summary>
    /// <param name="namespace"></param>
    /// <returns><see cref="MappingRule"/> construction flow.</returns>
    public IMappingRuleConstructionFlow Map(string @namespace)
    {
      return new MappingRuleConstructionFlow(this, null, @namespace);
    }

    /// <summary>
    /// Starts construction of <see cref="MappingRule"/>
    /// for the specified <paramref name="namespace"/>
    /// in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">Assembly to map from.</param>
    /// <param name="namespace">Namespace to map from.</param>
    /// <returns><see cref="MappingRule"/> construction flow.</returns>
    public IMappingRuleConstructionFlow Map(Assembly assembly, string @namespace)
    {
      return new MappingRuleConstructionFlow(this, assembly, @namespace);
    }

    /// <inheritdoc />
    public object Clone()
    {
      var clone = new MappingRuleCollection();
      clone.AddRange(this);
      return clone;
    }
  }
}