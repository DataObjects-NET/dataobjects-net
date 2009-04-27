// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.23

using System;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Configuration.Internals
{
  /// <summary>
  /// Base class for any <see cref="ITypeRegistrationHandler"/> implementation.
  /// </summary>
  [Serializable]
  public abstract class TypeRegistrationHandlerBase : ITypeRegistrationHandler
  {
    /// <summary>
    /// Gets base type.
    /// </summary>
    public abstract Type BaseType { get; }

    /// <summary>
    /// Processes the specified registration in the specified registration context.
    /// </summary>
    /// <param name="registry">The type registry.</param>
    /// <param name="registration">The action.</param>
    public virtual void Process(TypeRegistry registry, TypeRegistration registration)
    {
      var types =
        registration.Type==null
          ? registration.Assembly.FindTypes(BaseType, (type, typeFilter) => IsAcceptable(registry, registration, type))
          : EnumerableUtils.One(registration.Type).Where(t => IsAcceptable(registry, registration, t));
      foreach (var type in types)
        Process(registry, registration, type);
    }

    /// <summary>
    /// Processes the single type registration.
    /// </summary>
    /// <param name="registry">The type registry.</param>
    /// <param name="registration">The registration.</param>
    /// <param name="type">The type.</param>
    protected virtual void Process(TypeRegistry registry, TypeRegistration registration, Type type)
    {
      registry.Register(type);
    }

    /// <summary>
    /// Determines whether the specified type is acceptable for registration.
    /// </summary>
    /// <param name="registry">The type registry.</param>
    /// <param name="registration">The currently processed registration.</param>
    /// <param name="type">The type to check.</param>
    /// <returns>
    /// 	<see langword="true"/> if the specified type is acceptable for registration;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsAcceptable(TypeRegistry registry, TypeRegistration registration, Type type)
    {
      string ns = registration.Namespace;
      return
        !type.IsGenericTypeDefinition &&
          type.IsSubclassOf(BaseType) &&
            (ns.IsNullOrEmpty() || (type.FullName.IndexOf(ns + ".") >= 0));
    }
  }
}