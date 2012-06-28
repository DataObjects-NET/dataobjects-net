// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Testing
{
  /// <summary>
  /// Default <see cref="IInstanceGenerator{T}"/> provider. 
  /// Provides default instance generator for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class InstanceGeneratorProvider : AssociateProvider, IInstanceGeneratorProvider
  {
    private static readonly InstanceGeneratorProvider @default = new InstanceGeneratorProvider();
    private ThreadSafeDictionary<Type, IInstanceGeneratorBase> generators = 
      ThreadSafeDictionary<Type, IInstanceGeneratorBase>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
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
      return generators.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("GetInstanceGenerator", ArrayUtils<Type>.EmptyArray)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null)
          as IInstanceGeneratorBase,
        this);
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