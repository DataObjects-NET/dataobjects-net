// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Reflection;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An abstract base class for any storage factories.
  /// </summary>
  public abstract class HandlerFactory
  {
    private static readonly Type HandlerBaseType = typeof (HandlerBase);

    private readonly Dictionary<Type, Type> serviceMapping = new Dictionary<Type, Type>();

    /// <summary>
    /// Creates the handler of specified type <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="handlerType">Type of the handler to create.</param>
    /// <returns>A newly created handler of requested type;</returns>
    /// <exception cref="NotSupportedException">Handler for type <paramref name="handlerType"/> was not found.</exception>
    public HandlerBase CreateHandler(Type handlerType)
    {
      Type implementor;

      if (serviceMapping.TryGetValue(handlerType, out implementor))
        return (HandlerBase) Activator.CreateInstance(implementor);

      throw new NotSupportedException(
        string.Format(Strings.ExCannotFindHandlerOfTypeX, handlerType.GetShortName()));
    }

    private void RegisterHandlersFrom(Assembly assembly, string @namespace)
    {
      foreach (var type in assembly.GetTypes())
        if (type.Namespace==@namespace && type.IsPublicNonAbstractInheritorOf(HandlerBaseType))
          RegisterHandler(type);
    }

    private void RegisterHandler(Type type)
    {
      var contract = type;
      var implementor = type;

      while (contract!=HandlerBaseType && !serviceMapping.ContainsKey(contract)) {
        serviceMapping[contract] = implementor;
        contract = contract.BaseType;
      }
    }

    // Constructors

    protected HandlerFactory()
    {
      var type = GetType();
      while (type!=typeof (object)) {
        RegisterHandlersFrom(type.Assembly, type.Namespace);
        type = type.BaseType;
      }
    }
  }
}
