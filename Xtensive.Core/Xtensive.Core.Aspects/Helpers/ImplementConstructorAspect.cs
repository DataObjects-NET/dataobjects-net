// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using System.Reflection;
using System.Linq;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Implements protected constructor with a set of specified parameter types.
  /// Implemented constructor will call the constructor with the same set of arguments from the base type.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementConstructorAspect : LaosTypeLevelAspect
  {
    /// <summary>
    /// Gets the constructor parameter types.
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(Type type)
    {
      ConstructorInfo constructor;

      return AspectHelper.ValidateConstructor(this, SeverityType.Error,
        type, false, BindingFlags.Public | BindingFlags.NonPublic, ParameterTypes, out constructor);
    }

    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      requirements.PlugIns.Add("Xtensive.Core.Weaver");
      requirements.Tasks.Add("Xtensive.Core.Weaver.WeaverFactory");
      return requirements;
    }

    /// <summary>
    /// Applies this aspect to the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to apply the aspect to.</param>
    /// <param name="parameterTypes">Types of constructor parameters.</param>
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ImplementConstructorAspect ApplyOnce(Type type, Type[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(parameterTypes, "parameterTypes");

      return AppliedAspectSet.Add(new Pair<Type, string>(type, parameterTypes.Select(t => t.FullName).ToCommaDelimitedString()), 
        () => new ImplementConstructorAspect(parameterTypes));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parameterTypes"><see cref="ParameterTypes"/> property value.</param>
    public ImplementConstructorAspect(Type[] parameterTypes)
    {
      ParameterTypes = parameterTypes;
    }
  }
}