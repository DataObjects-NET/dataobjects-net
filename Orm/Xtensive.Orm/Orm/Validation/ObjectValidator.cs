// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.06

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Standard base type for <see cref="IObjectValidator"/> implementation.
  /// </summary>
  public abstract class ObjectValidator : IObjectValidator
  {
    /// <summary>
    /// Gets domain this instance is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets type this instance is bound to.
    /// </summary>
    public TypeInfo Type { get; private set; }

    /// <summary>
    /// Configures this instance.
    /// </summary>
    /// <param name="domain">A domain this validator is bound to.</param>
    /// <param name="type">A type this validator is bound to.</param>
    public virtual void Configure(Domain domain, TypeInfo type)
    {
      if (Domain!=null)
        throw Exceptions.AlreadyInitialized(null);

      Domain = domain;
      Type = type;
    }

    /// <summary>
    /// Validates specified object.
    /// </summary>
    /// <param name="target">An object to validate.</param>
    public abstract ValidationResult Validate(Entity target);

    /// <summary>
    /// Creates new unconfigured <see cref="IObjectValidator"/> instance
    /// with the same parameters.
    /// </summary>
    /// <returns>Newly created validator.</returns>
    public abstract IObjectValidator CreateNew();

    /// <summary>
    /// Constructs successful validation result.
    /// </summary>
    /// <returns>Constructed result.</returns>
    protected ValidationResult Success()
    {
      return ValidationResult.Success;
    }

    /// <summary>
    /// Constructs validation failure result.
    /// </summary>
    /// <param name="errorMessage">Validation error message.</param>
    /// <returns>Constructed result.</returns>
    protected ValidationResult Error(string errorMessage)
    {
      return new ValidationResult(this, errorMessage);
    }

    /// <summary>
    /// Constructs validation failure result.
    /// </summary>
    /// <param name="exception">Validation error message.</param>
    /// <returns>Constructed result.</returns>
    protected ValidationResult Error(Exception exception)
    {
      return new ValidationResult(this, exception.Message);
    }

    /// <summary>
    /// Throws configuration error with specified message.
    /// </summary>
    /// <param name="message">Configuration error message.</param>
    /// <param name="innerException">An <see cref="Exception"/> instance to be used as inner exception.</param>
    protected void ThrowConfigurationError(string message, Exception innerException = null)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(message, "message");

      var exceptionMessage = string.Format(
        Strings.ExValidatorXConfigurationFailedOnTypeYWithMessageZ,
        GetType().Name, Type, message);

      var exception = innerException==null
        ? new DomainBuilderException(exceptionMessage)
        : new DomainBuilderException(message, innerException);

      throw exception;
    }
  }
}