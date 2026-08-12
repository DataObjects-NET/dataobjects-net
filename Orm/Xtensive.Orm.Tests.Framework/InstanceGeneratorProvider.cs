// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
  public class InstanceGeneratorProvider : AssociateProvider, IInstanceGeneratorProvider
  {
    private readonly ConcurrentDictionary<(Type, InstanceGeneratorProvider), Lazy<IInstanceGeneratorBase>> generators = new();

    public static InstanceGeneratorProvider Default {
      [DebuggerStepThrough]
      get;
    } = new InstanceGeneratorProvider();

    #region IInstanceGeneratorProvider members

    /// <inheritdoc/>
    public virtual IInstanceGenerator<T> GetInstanceGenerator<T>() => GetAssociate<T, IInstanceGenerator<T>, IInstanceGenerator<T>>();

    /// <inheritdoc/>
    public IInstanceGeneratorBase GetInstanceGenerator(Type type)
    {
      return generators.GetOrAdd((type, this), InstanceGeneratorFactory).Value;

      static Lazy<IInstanceGeneratorBase> InstanceGeneratorFactory((Type, InstanceGeneratorProvider) tuple)
      {
        var (_type, _this) = tuple;
        return new Lazy<IInstanceGeneratorBase>(() => _this.GetType()
          .GetMethod(nameof(GetInstanceGenerator), Array.Empty<Type>())
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] { _type })
          .Invoke(_this, null)
        as IInstanceGeneratorBase);
      }
    }

    #endregion


    // Constructors


    protected InstanceGeneratorProvider()
    {
      TypeSuffixes = ["InstanceGenerator"];
      var t = typeof (InstanceGeneratorProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}