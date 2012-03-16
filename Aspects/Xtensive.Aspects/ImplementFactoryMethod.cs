// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Implements static factory method that calls the specified constructor found by its signature.
  /// </summary>
  /// <remarks>
  /// If you're really interested in actual behavior, we recommend you to
  /// study the decompiled MSIL code of class having this attribute applied 
  /// using .NET Reflector.
  /// </remarks>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict, TargetTypeAttributes = MulticastAttributes.NonAbstract)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ImplementConstructor))]
  [RequirePostSharp("Xtensive.Aspects.Weaver", "Xtensive.PlugIn")]
#if NET40
  [SecuritySafeCritical]
#endif
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
    /// Initializes a new instance of the <see cref="ImplementFactoryMethod"/> class.
    /// </summary>
    /// <param name="parameterTypes"><see cref="ParameterTypes"/> property value.</param>
    public ImplementFactoryMethod(params Type[] parameterTypes)
    {
      ParameterTypes = parameterTypes;
    }
  }
}
