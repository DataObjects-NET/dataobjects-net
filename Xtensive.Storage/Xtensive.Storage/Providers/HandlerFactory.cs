// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
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
    /// <returns>A newly created handler of requested type;
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
    /// <returns>A newly created handler of requested type;
    /// <exception cref="NotSupportedException">Handler for type <paramref name="handlerType"/> was not found.</exception>
    public virtual HandlerBase CreateHandler(Type handlerType)
    {
      HandlerBase handler;
      if (!TryCreateHandler(handlerType, out handler))
        throw new NotSupportedException(string.Format(Strings.ExCannotFindHandler, 
          handlerType.GetShortName()));
      return handler;
    }

    /// <summary>
    /// Creates the handler of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the handler to create.</typeparam>
    /// <param name="handler">When this method returns, contains the handler associated 
    /// with the specified <typeparamref name="T"/>, if the handler is found; 
    /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the handler was found;
    /// otherwise <see langword="false"/>.</returns>
    public bool TryCreateHandler<T> (out HandlerBase handler)
    {
      return TryCreateHandler(typeof(T), out handler);
    }

    /// <summary>
    /// Creates the handler of type <param name="handlerType".
    /// </summary>
    /// <param name="handlerType">Type of the handler to create.</param>
    /// <param name="handler">When this method returns, contains the handler associated 
    /// with the specified <paramref name="handlerType"/>, if the handler is found; 
    /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the handler was found;
    /// otherwise <see langword="false"/>.</returns>
    public bool TryCreateHandler(Type handlerType, out HandlerBase handler)
    {
      Func<object> constructor;
      handler = null;
      if (!constructors.TryGetValue(handlerType, out constructor) || constructor==null)
        return false;
      handler = (HandlerBase) constructor();
      handler.Accessor = Domain.HandlerAccessor;
      return true;
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
        Type baseType = type.BaseType;
        while (baseType != handlerBaseType) {
          if (baseType.IsAbstract) {
            // Any abstract HandlerBase descendant is considered as
            // "key" for handler requests
            var constructor = DelegateHelper.CreateConstructorDelegate<Func<object>>(type);
            if (!constructors.ContainsKey(baseType))
              constructors.Add(baseType, constructor);
          }
          baseType = baseType.BaseType;
        }
      }
    }


    // Constructors

    /// <inheritdoc/>
    public HandlerFactory(Domain domain)
      : base(domain)
    {
      Type type = GetType();
      while (type!=typeof(HandlerFactory)) {
        RegisterHandlersFrom(type.Assembly);
        type = type.BaseType;
      }
    }
  }
}