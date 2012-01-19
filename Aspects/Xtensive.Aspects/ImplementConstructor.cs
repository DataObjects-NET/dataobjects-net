// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Implements protected constructor with a set of specified parameter types.
  /// Implemented constructor will call the constructor with the same set of arguments from the base type.
  /// </summary>
  /// <remarks>
  /// If you're really interested in actual behavior, we recommend you to
  /// study the decompiled MSIL code of class having this attribute applied 
  /// using .NET Reflector.
  /// </remarks>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(ImplementFactoryMethod))]
  [RequirePostSharp("Xtensive.Aspects.Weaver", "Xtensive.PlugIn")]
#if NET40
  [SecuritySafeCritical]
#endif
  public sealed class ImplementConstructor : TypeLevelAspect
  {
    /// <summary>
    /// Gets the constructor parameter types.
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(Type type)
    {
      var ci = type.GetConstructor(
        BindingFlags.Static | 
          BindingFlags.Instance |
          BindingFlags.Public |
          BindingFlags.NonPublic |
          BindingFlags.ExactBinding,
        null,
        ParameterTypes,
        null);
      if (ci != null)
        return false;
      return true;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ImplementConstructor"/> class.
    /// </summary>
    /// <param name="parameterTypes"><see cref="ParameterTypes"/> property value.</param>
    public ImplementConstructor(params Type[] parameterTypes)
    {
      ParameterTypes = parameterTypes;
    }
  }
}