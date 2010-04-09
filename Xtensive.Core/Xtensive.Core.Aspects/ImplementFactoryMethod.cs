// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Aspects
{
  /// <summary>
  /// Implements static factory method that calls specified constructor found by its signature.
  /// </summary>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict, TargetTypeAttributes = MulticastAttributes.NonAbstract)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ImplementConstructor))]
  [RequirePostSharp("Xtensive.Core.Weaver", "Xtensive.PlugIn")]
  public sealed class ImplementFactoryMethod : TypeLevelAspect
  {
    /// <summary>
    /// Gets the protected constructor argument types.
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(Type type)
    {
      return !type.IsInterface && !type.IsAbstract;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parameterTypes"><see cref="ParameterTypes"/> property value.</param>
    public ImplementFactoryMethod(params Type[] parameterTypes)
    {
      ParameterTypes = parameterTypes;
    }
  }
}
