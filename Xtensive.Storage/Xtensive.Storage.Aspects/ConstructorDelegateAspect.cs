// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Aspects;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Implements protected constructor with a single parameter.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ConstructorDelegateAspect : LaosTypeLevelAspect
  {
    private static readonly Dictionary<Type, ConstructorDelegateAspect> aspects = new Dictionary<Type, ConstructorDelegateAspect>();

    private Type baseType;
    private Type[] parameterTypes;
    private Type delegateType;

    public Type BaseType
    {
      get { return baseType; }
    }

    public Type[] ParameterTypes
    {
      get { return parameterTypes; }
    }

    public Type DelegateType
    {
      get { return delegateType; }
    }

    /// <summary>
    /// Finds an existent or creates a new instance of this class for certain type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    /// <see cref="ImplementConstructorAspect"/> instance.
    /// </returns>
    public static ConstructorDelegateAspect FindOrCreate(Type type, Type baseType, Type[] parameterTypes, Type delegateType)
    {
      lock (aspects) {
        ConstructorDelegateAspect aspect;
        aspects.TryGetValue(type, out aspect);
        if (aspect == null) {
          aspect = new ConstructorDelegateAspect(baseType, parameterTypes, delegateType);
          aspects.Add(type, aspect);
        }
        return aspect;
      }
    }

    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      requirements.PlugIns.Add("Xtensive.Storage.Weaver");
      requirements.Tasks.Add("Xtensive.Storage.Weaver.WeaverFactory");
      return requirements;
    }


    // Constructors

    internal ConstructorDelegateAspect(Type baseType, Type[] parameterTypes, Type delegateType)
    {
      this.parameterTypes = parameterTypes;
      this.baseType = baseType;
      this.delegateType = delegateType;
    }
  }
}