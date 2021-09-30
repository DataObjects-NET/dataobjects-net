// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Default <see cref="IInstanceGenerator{T}"/> provider. 
  /// Provides default instance generator for the specified type.
  /// </summary>
  [Serializable]
  public class InstanceGeneratorProvider : AssociateProvider, IInstanceGeneratorProvider
  {
    private static readonly InstanceGeneratorProvider @default = new InstanceGeneratorProvider();
    private ConcurrentDictionary<(Type, InstanceGeneratorProvider), Lazy<IInstanceGeneratorBase>> generators = 
      new ConcurrentDictionary<(Type, InstanceGeneratorProvider), Lazy<IInstanceGeneratorBase>>();

    public static InstanceGeneratorProvider Default {
      [DebuggerStepThrough]
      get { return @default; }
    }

    #region IInstanceGeneratorProvider members

    /// <inheritdoc/>
    public virtual IInstanceGenerator<T> GetInstanceGenerator<T>()
    {
      return GetAssociate<T, IInstanceGenerator<T>, IInstanceGenerator<T>>();
    }

    /// <inheritdoc/>
    public IInstanceGeneratorBase GetInstanceGenerator(Type type)
    {
      static Lazy<IInstanceGeneratorBase> InstanceGeneratorFactory((Type, InstanceGeneratorProvider) tuple)
      {
        var (_type, _this) = tuple;
        return new Lazy<IInstanceGeneratorBase>(() => _this.GetType()
          .GetMethod("GetInstanceGenerator", Array.Empty<Type>())
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] { _type })
          .Invoke(_this, null)
        as IInstanceGeneratorBase);
      };

      return generators.GetOrAdd((type, this), InstanceGeneratorFactory).Value;
    }

    #endregion


    // Constructors


    protected InstanceGeneratorProvider()
    {
      TypeSuffixes = new[] {"InstanceGenerator"};
      Type t = typeof (InstanceGeneratorProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}