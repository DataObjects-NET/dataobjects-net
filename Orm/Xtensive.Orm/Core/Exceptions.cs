// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Xtensive.Orm.Logging;
using Xtensive.Reflection;

namespace Xtensive.Core
{
  /// <summary>
  /// Most common <see cref="Exception"/> factory.
  /// </summary>
  public static class Exceptions
  {
    /// <summary>
    /// Returns an exception informing internal error has occurred.
    /// </summary>
    /// <param name="description">Error description.</param>
    /// <param name="log"><see cref="BaseLog"/> instance to log the problem;
    /// <see langword="null"/> means logging is not necessary.</param>
    /// <returns>Newly created exception.</returns>
    public static InvalidOperationException InternalError(string description, BaseLog log)
    {
      var exception = new InvalidOperationException(string.Format(Strings.ExInternalError, description));
      log.Error(null, null, exception);
      return exception;
    }

    /// <summary>
    /// Returns an exception informing that URL is invalid.
    /// </summary>
    /// <param name="url">Invalid URL.</param>
    /// <param name="parameterName">Name of method parameter where URL was passed (<see langword="null"/> if none).</param>
    /// <returns>Newly created exception.</returns>
    public static Exception InvalidUrl(string url, [InvokerParameterName] string parameterName)
    {
      if (parameterName.IsNullOrEmpty())
        return new InvalidOperationException(string.Format(Strings.ExInvalidUrl, url));
      else
        return new ArgumentException(string.Format(Strings.ExInvalidUrl, url), parameterName);
    }

    /// <summary>
    /// Returns an exception informing that object or property is already initialized.
    /// </summary>
    /// <param name="propertyName">Name of the property; <see langword="null"/>, if none.</param>
    /// <returns>Newly created exception.</returns>
    public static NotSupportedException AlreadyInitialized(string propertyName)
    {
      if (propertyName.IsNullOrEmpty())
        return new NotSupportedException(Strings.ExAlreadyInitialized);
      else
        return new NotSupportedException(string.Format(Strings.ExPropertyIsAlreadyInitialized, propertyName));
    }

    /// <summary>
    /// Returns an exception informing that object is already disposed.
    /// </summary>
    /// <param name="objectName">Name of the object; <see langword="null"/>, if none.</param>
    /// <returns>Newly created exception.</returns>
    public static ObjectDisposedException AlreadyDisposed(string objectName)
    {
      if (objectName.IsNullOrEmpty())
        return new ObjectDisposedException(string.Empty, Strings.ExAlreadyDisposed);
      else
        return new ObjectDisposedException(objectName);
    }

    /// <summary>
    /// Returns an exception informing that object or property is not initialized,
    /// or not initialized properly.
    /// </summary>
    /// <param name="propertyName">Name of the property; <see langword="null"/>, if none.</param>
    /// <returns>Newly created exception.</returns>
    public static InvalidOperationException NotInitialized(string propertyName)
    {
      if (propertyName.IsNullOrEmpty())
        return new InvalidOperationException(Strings.ExNotInitialized);
      else
        return new InvalidOperationException(string.Format(Strings.ExPropertyIsNotInitialized, propertyName));
    }

    /// <summary>
    /// Returns an exception informing that specified argument
    /// value is not allowed or invalid.
    /// </summary>
    /// <param name="value">Actual parameter value.</param>
    /// <param name="parameterName">Name of the method parameter (<see langword="null"/> if none).</param>
    /// <returns>Newly created exception.</returns>
    /// <typeparam name="T">The type of the value.</typeparam>
    public static Exception InvalidArgument<T>(T value, [InvokerParameterName] string parameterName)
    {
      if (parameterName.IsNullOrEmpty())
        return new InvalidOperationException(string.Format(
          Strings.ExValueXIsNotAllowedHere, value));
      else
        return new ArgumentOutOfRangeException(parameterName, value, string.Format(
          Strings.ExValueXIsNotAllowedHere, value));
    }

    /// <summary>
    /// Returns an exception informing that object is read-only.
    /// </summary>
    /// <param name="parameterName">Name of the method parameter (<see langword="null"/> if none).</param>
    /// <returns>Newly created exception.</returns>
    public static Exception ObjectIsReadOnly([InvokerParameterName] string parameterName)
    {
      if (parameterName.IsNullOrEmpty())
        return new NotSupportedException(Strings.ExObjectIsReadOnly);
      else
        return new ArgumentException(Strings.ExObjectIsReadOnly, parameterName);
    }

    /// <summary>
    /// Returns an exception informing that collection is empty.
    /// </summary>
    /// <param name="parameterName">Name of the method parameter (<see langword="null"/> if none).</param>
    /// <returns>Newly created exception.</returns>
    public static Exception CollectionIsEmpty([InvokerParameterName] string parameterName)
    {
      if (parameterName.IsNullOrEmpty())
        return new InvalidOperationException(Strings.ExCollectionIsEmpty);
      else
        return new ArgumentException(Strings.ExCollectionIsEmpty, parameterName);
    }

    /// <summary>
    /// Returns an exception informing that collection is read-only.
    /// </summary>
    /// <param name="parameterName">Name of the method parameter (<see langword="null"/> if none).</param>
    /// <returns>Newly created exception.</returns>
    public static Exception CollectionIsReadOnly([InvokerParameterName] string parameterName)
    {
      if (parameterName.IsNullOrEmpty())
        return new NotSupportedException(Strings.ExCollectionIsReadOnly);
      else
        return new ArgumentException(Strings.ExCollectionIsReadOnly, parameterName);
    }

    /// <summary>
    /// Returns an exception informing that collection has been changed during the enumeration.
    /// </summary>
    /// <param name="parameterName">Name of the method parameter (<see langword="null"/> if none).</param>
    /// <returns>Newly created exception.</returns>
    public static Exception CollectionHasBeenChanged([InvokerParameterName] string parameterName)
    {
      if (parameterName.IsNullOrEmpty())
        return new InvalidOperationException(Strings.ExCollectionHasBeenChanged);
      else
        return new ArgumentException(Strings.ExCollectionHasBeenChanged, parameterName);
    }

    /// <summary>
    /// Returns an exception informing that context is required.
    /// </summary>
    /// <param name="contextType">Type of required context.</param>
    /// <param name="scopeType">Type of <see cref="Scope{TContext}"/> used to set the context.</param>
    /// <returns>Newly created exception.</returns>
    public static InvalidOperationException ContextRequired(Type contextType, Type scopeType)
    {
      ArgumentNullException.ThrowIfNull(contextType);
      ArgumentNullException.ThrowIfNull(scopeType);
      return new InvalidOperationException(
        string.Format(Strings.ExContextRequired, contextType.GetShortName(), scopeType.GetShortName()));
    }

    /// <summary>
    /// Returns an exception informing that context is required.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TScope">The type of the scope.</typeparam>
    /// <returns>Newly created exception.</returns>
    public static InvalidOperationException ContextRequired<TContext, TScope>()
    {
      var contextType = typeof (TContext);
      var scopeType = typeof (TScope);
      return ContextRequired(contextType, scopeType);
    }

    /// <summary>
    /// Returns an exception informing that scope is required.
    /// </summary>
    /// <param name="scopeType">Type of <see cref="SimpleScope{TContext}"/> used to set the context.</param>
    /// <returns>Newly created exception.</returns>
    public static InvalidOperationException ScopeRequired(Type scopeType)
    {
      ArgumentNullException.ThrowIfNull(scopeType);
      return new InvalidOperationException(
        string.Format(Strings.ExScopeRequired, scopeType.GetShortName()));
    }

    /// <summary>
    /// Returns an exception informing that scope is required.
    /// </summary>
    /// <typeparam name="TScope">The type of the scope.</typeparam>
    /// <returns>Newly created exception.</returns>
    public static InvalidOperationException ScopeRequired<TScope>()
    {
      var scopeType = typeof (TScope);
      return ScopeRequired(scopeType);
    }

    /// <summary>
    /// Returns an exception informing that specified <see cref="ParameterExpression"/> is out of scope.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>Newly created exception.</returns>
    public static InvalidOperationException LambdaParameterIsOutOfScope(ParameterExpression parameter)
    {
      ArgumentNullException.ThrowIfNull(parameter);
      return new InvalidOperationException(string.Format(Strings.ExLambdaParameterXIsOutOfScope, parameter.Name));
    }
  }
}