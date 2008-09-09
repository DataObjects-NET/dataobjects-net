// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An abstract base class for any storage factories.
  /// </summary>
  public abstract class HandlerFactory : DomainBound
  {
    private readonly Dictionary<Type, Func<object>> constructors = new Dictionary<Type, Func<object>>();
    private static readonly Type handlerBaseType = typeof(HandlerBase);

    /// <summary>
    /// Creates the handler of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the handler to create.</typeparam>
    /// <returns>A newly created handler of requested type;</returns>
    /// <exception cref="NotSupportedException">Handler for type <typeparamref name="T"/> was not found.</exception>
    public T CreateHandler<T>()
      where T: HandlerBase
    {
      return (T) CreateHandler(typeof (T));
    }

    /// <summary>
    /// Creates the handler of specified type <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="handlerType">Type of the handler to create.</param>
    /// <returns>A newly created handler of requested type;</returns>
    /// <exception cref="NotSupportedException">Handler for type <paramref name="handlerType"/> was not found.</exception>
    public virtual HandlerBase CreateHandler(Type handlerType)
    {
      HandlerBase handler = TryCreateHandler(handlerType);
      
      if (handler==null)
        throw new NotSupportedException(string.Format(Strings.ExCannotFindHandler, 
          handlerType.GetShortName()));

      return handler;
    }

    /// <summary>
    /// Creates the handler of the specified type.
    /// </summary>
    /// <typeparam name="T">Type of the handler to create.</typeparam>
    /// <returns>
    /// Created handler or <see langword="null" /> if handler of specified type was now found.
    /// </returns>
    public HandlerBase TryCreateHandler<T> ()
    {
      return TryCreateHandler(typeof(T));
    }

    /// <summary>
    /// Creates the handler of type <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="handlerType">Type of the handler to create.</param>
    /// <returns>
    /// Created handler or <see langword="null" /> if handler of specified type was now found.
    /// </returns>
    public HandlerBase TryCreateHandler(Type handlerType)
    {
      Func<object> constructor;
      if (!constructors.TryGetValue(handlerType, out constructor) || constructor==null)
        return null;

      var result = (HandlerBase) constructor();
      result.Handlers = Domain.Handlers;

      return result;
    }

    /// <summary>
    /// Registers all the handlers from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to register all the handlers from.</param>
    protected void RegisterHandlersFrom(Assembly assembly)
    {
      foreach (Type type in assembly.GetTypes()) {
        if (type.IsAbstract || !type.IsPublic || !handlerBaseType.IsAssignableFrom(type))
          continue;
        if (constructors.ContainsKey(type))
          continue;
        Type baseType = type;
        while (baseType != handlerBaseType) {
          constructors[baseType] = DelegateHelper.CreateConstructorDelegate<Func<object>>(type);
          baseType = baseType.BaseType;
        }
      }
    }


    // Constructors

    /// <inheritdoc/>
    protected HandlerFactory(Domain domain)
      : base(domain)
    {
      Type type = GetType();
      RegisterHandlersFrom(Assembly.GetExecutingAssembly());
      while (type!=typeof(HandlerFactory)) {
        RegisterHandlersFrom(type.Assembly);
        type = type.BaseType;
      }
    }
  }
}
