// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Protected constructor accessors aspect - provides an accessor (delegate)
  /// for the specified protected constructor of a type.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Struct)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
  [Serializable]
  public sealed class ImplementProtectedConstructorAccessorAspect : LaosTypeLevelAspect
  {
    /// <summary>
    /// Gets the compatible return type (e.g. some base type of aspected type).
    /// </summary>
    public Type ReturnType { get; private set; }

    /// <summary>
    /// Gets the protected constructor argument types.
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(Type type)
    {
      ConstructorInfo existingConstructor = AspectHelper.ValidateConstructor(this, SeverityType.Error,
        type, false, BindingFlags.Public | BindingFlags.NonPublic, ParameterTypes);
      if (existingConstructor != null)
        return false;

      return true;
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
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ImplementProtectedConstructorAccessorAspect ApplyOnce(Type type, Type[] argumentTypes, Type returnType)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      var aspect = AppliedAspectSet.Add(new Pair<Type, Type>(type, returnType),
        () => new ImplementProtectedConstructorAccessorAspect(argumentTypes, returnType));
      return aspect;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="ParameterTypes"><see cref="ParameterTypes"/> property value.</param>
    /// <param name="returnType"><see cref="ReturnType"/> property value.</param>
    public ImplementProtectedConstructorAccessorAspect(Type[] ParameterTypes, Type returnType)
    {
      ReturnType = returnType;
      this.ParameterTypes = ParameterTypes;
    }
  }
}
