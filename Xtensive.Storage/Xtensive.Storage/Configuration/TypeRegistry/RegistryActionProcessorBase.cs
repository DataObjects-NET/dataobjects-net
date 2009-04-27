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

namespace Xtensive.Storage.Configuration.TypeRegistry
{
  /// <summary>
  /// Base class for any <see cref="IRegistryActionProcessor"/> implementation.
  /// </summary>
  [Serializable]
  public abstract class RegistryActionProcessorBase : IRegistryActionProcessor
  {
    /// <summary>
    /// Gets base type.
    /// </summary>
    public abstract Type BaseType { get; }

    /// <summary>
    /// Processes the specified action in the specified registration context.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="action">The action.</param>
    public virtual void Process(Registry registry, RegistryAction action)
    {
      var types =
        action.Type==null
          ? action.Assembly.FindTypes(BaseType, (type, typeFilter) => IsAcceptable(registry, action, type))
          : EnumerableUtils.One(action.Type).Where(t => IsAcceptable(registry, action, t));
      foreach (var type in types)
        Process(registry, action, type);
    }

    /// <summary>
    /// Processes the single type registration.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="action">The action.</param>
    /// <param name="type">The type.</param>
    protected virtual void Process(Registry registry, RegistryAction action, Type type)
    {
      registry.Register(type);
    }

    /// <summary>
    /// Determines whether the specified type is acceptable for registration.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="action">The currently processed action.</param>
    /// <param name="type">The type to check.</param>
    /// <returns>
    /// 	<see langword="true"/> if the specified type is acceptable for registration;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsAcceptable(Registry registry, RegistryAction action, Type type)
    {
      string ns = action.Namespace;
      return
        !type.IsGenericTypeDefinition &&
          type.IsSubclassOf(BaseType) &&
            (ns.IsNullOrEmpty() || (type.FullName.IndexOf(ns + ".") >= 0));
    }
  }
}