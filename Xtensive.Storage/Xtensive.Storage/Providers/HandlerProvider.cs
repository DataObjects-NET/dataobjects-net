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
  /// An abstract base class for any storage handler provider.
  /// </summary>
  public abstract class HandlerProvider : DomainBound
  {
    private Dictionary<Type, Func<object>> constructors = new Dictionary<Type, Func<object>>();
    private static Type handlerBaseType = typeof(HandlerBase);

    /// <summary>
    /// Creates the handler of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the handler to create.</typeparam>
    /// <param name="failIfNone">If set to <see langword="true"/>,
    /// <see cref="NotSupportedException"/> will be thrown if handler for type
    /// <typeparamref name="T"/> cannot be found.</param>
    /// <returns>A newly created handler of requested type;
    /// <see langword="null" />, if such handler cannot be created.</returns>
    public T CreateHandler<T>(bool failIfNone)
      where T: HandlerBase
    {
      return (T) CreateHandler(typeof (T), failIfNone);
    }

    /// <summary>
    /// Creates the handler of specified type <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="handlerType">Type of the handler to create.</param>
    /// <param name="failIfNone">If set to <see langword="true"/>,
    /// <see cref="NotSupportedException"/> will be thrown if handler for type
    /// <paramref name="handlerType"/> cannot be found.</param>
    /// <returns>A newly created handler of requested type;
    /// <see langword="null" />, if such handler cannot be created.</returns>
    public virtual HandlerBase CreateHandler(Type handlerType, bool failIfNone)
    {
      Func<object> constructor;
      if (!constructors.TryGetValue(handlerType, out constructor) || constructor==null) {
        if (failIfNone)
          throw new NotSupportedException(string.Format(Strings.ExCannotFindHandler, 
            handlerType.GetShortName()));
        else
          return null;
      }
      var handler = (HandlerBase) constructor();
      handler.Accessor = Domain.HandlerAccessor;
      return handler;
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
    public HandlerProvider(Domain domain)
      : base(domain)
    {
      Type type = GetType();
      while (type!=typeof(HandlerProvider)) {
        RegisterHandlersFrom(type.Assembly);
        type = type.BaseType;
      }
    }
  }
}