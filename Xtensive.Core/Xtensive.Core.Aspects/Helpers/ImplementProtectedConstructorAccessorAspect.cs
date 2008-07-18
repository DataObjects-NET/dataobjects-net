// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Protected constructor accessors aspect - provides an accessor (delegate)
  /// for the specified protected constructor of a type.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  [Serializable]
  public sealed class ImplementProtectedConstructorAccessorAspect : LaosTypeLevelAspect
  {
    /// <summary>
    /// Gets the type of the constructor accessor - the delegate type
    /// with the same arguments as of accessed constructor and with compatible 
    /// return type (e.g. some base type of aspected type).
    /// </summary>
    public Type AccessorType { get; private set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(Type type)
    {
      // TODO: Implement
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
    /// <typeparam name="TAccessor">The accessor delegate specifying constructor arguments and required return type.</typeparam>
    /// <param name="type">The type to apply the aspect to.</param>
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ImplementProtectedConstructorAccessorAspect ApplyOnce<TAccessor>(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      var tAccessor = typeof (TAccessor);

      return AppliedAspectSet.Add(new Pair<Type, Type>(type, tAccessor), 
        () => new ImplementProtectedConstructorAccessorAspect(tAccessor));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="accessorType"><see cref="AccessorType"/> property value.</param>
    internal ImplementProtectedConstructorAccessorAspect(Type accessorType)
    {
      AccessorType = accessorType;
    }
  }
}