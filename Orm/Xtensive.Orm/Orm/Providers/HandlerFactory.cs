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
    private static readonly Type HandlerType = typeof (Handler);
    private static readonly Type DomainBoundHandlerType = typeof (DomainBoundHandler);

    private readonly Dictionary<Type, Type> serviceMapping = new Dictionary<Type, Type>();

    /// <summary>
    /// Creates the handler of specified type <typeparamref name="TContract"/>.
    /// </summary>
    /// <typeparam name="TContract">Handler contract</typeparam>
    /// <returns>A newly created handler of requested type;</returns>
    /// <exception cref="NotSupportedException">
    /// Handler for type <typeparamref name="TContract"/> was not found.</exception>
    public TContract CreateHandler<TContract>()
      where TContract : Handler
    {
      Type implementor;

      if (serviceMapping.TryGetValue(typeof (TContract), out implementor))
        return (TContract) Activator.CreateInstance(implementor);

      throw new NotSupportedException(
        string.Format(Strings.ExCannotFindHandlerOfTypeX, typeof (TContract).GetShortName()));
    }

    private void RegisterHandlersFrom(Assembly assembly, string @namespace)
    {
      foreach (var type in assembly.GetTypes())
        if (type.Namespace==@namespace && type.IsPublicNonAbstractInheritorOf(HandlerType))
          RegisterHandler(type);
    }

    private void RegisterHandler(Type type)
    {
      var contract = type;
      var implementor = type;

      while (contract!=HandlerType && contract!=DomainBoundHandlerType && !serviceMapping.ContainsKey(contract)) {
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
