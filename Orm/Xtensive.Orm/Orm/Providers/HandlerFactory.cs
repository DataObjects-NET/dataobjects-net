// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Orm;
using Xtensive.Reflection;


namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An abstract base class for any storage factories.
  /// </summary>
  public abstract class HandlerFactory : DomainBound
  {
    private readonly Dictionary<Type, Func<object>> constructors = new Dictionary<Type, Func<object>>();
    private static readonly Type HandlerBaseType = typeof(HandlerBase);
    private static readonly Type InitializableHandlerBaseType = typeof (InitializableHandlerBase);

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
    public HandlerBase CreateHandler(Type handlerType)
    {
      HandlerBase handler = TryCreateHandler(handlerType);
      
      if (handler==null)
        throw new NotSupportedException(
          string.Format(Strings.ExCannotFindHandlerOfTypeX, handlerType.GetShortName()));

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

      var result = (HandlerBase) constructor.Invoke();
      result.Handlers = Domain.Handlers;

      return result;
    }
    
    #region Private / internal methods

    private void RegisterHandlersFrom(Assembly assembly, string @namespace)
    {
      foreach (Type type in assembly.GetTypes())
        if (type.Namespace==@namespace && type.IsPublicNonAbstractInheritorOf(HandlerBaseType))
          RegisterHandler(type);
    }

    private void RegisterHandler(Type type)
    {
      var constructorDelegate = DelegateHelper.CreateConstructorDelegate<Func<object>>(type);
      while (type!=HandlerBaseType && type!=InitializableHandlerBaseType && !constructors.ContainsKey(type)) {
        constructors[type] = constructorDelegate;
        type = type.BaseType;
      }
    }

    internal void Initialize()
    {
      Type type = GetType();
      while (type!=typeof (HandlerFactory)) {
        RegisterHandlersFrom(type.Assembly, type.Namespace);
        type = type.BaseType;
      }
      RegisterHandlersFrom(typeof (HandlerFactory).Assembly, typeof (HandlerFactory).Namespace);
    }

    #endregion
  }
}
