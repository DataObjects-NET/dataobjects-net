// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using PostSharp.Extensibility;
using PostSharp.CodeModel;
using PostSharp.Laos;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Implements protected constructor with a set of specified parameter types.
  /// Implemented constructor will call the constructor with the same set of arguments from the base type.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ConstructorAspect : CompoundAspect
  {
    private static Type surrogateType = null; 

    /// <summary>
    /// Gets the constructor parameter types.
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <summary>
    /// Gets or sets the target type.
    /// </summary>
    public Type TargetType { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(object element)
    {
      var type = (Type)element;
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

    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      TargetType = (Type)element;
      if (surrogateType == null)
        surrogateType = TargetType.Module.GetTypes().Where(type => !typeof(ILaosAspect).IsAssignableFrom(type)).First();
      collection.AddAspect(surrogateType, new DeclareConstructorAspect(this));
      collection.AddAspect(surrogateType, new BuildConstructorAspect(this));
    }


    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      AspectHelper.AddStandardRequirements(requirements);
      return requirements;
    }

    /// <summary>
    /// Applies this aspect to the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to apply the aspect to.</param>
    /// <param name="parameterTypes">Types of constructor parameters.</param>
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ConstructorAspect ApplyOnce(Type type, params Type[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(parameterTypes, "parameterTypes");

      return AppliedAspectSet.Add(
        string.Format("{0}({1})", type.FullName, parameterTypes.Select(t => t.FullName).ToCommaDelimitedString()),
        () => new ConstructorAspect(parameterTypes));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parameterTypes"><see cref="ParameterTypes"/> property value.</param>
    public ConstructorAspect(params Type[] parameterTypes)
    {
      ParameterTypes = parameterTypes;
    }
  }
}