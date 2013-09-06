// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.06

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
        throw Exceptions.ObjectIsReadOnly(null);

      Domain = domain;
      Type = type;
    }

    /// <summary>
    /// Validates specified object.
    /// </summary>
    /// <param name="target">An object to validate.</param>
    public abstract void Validate(Persistent target);

    /// <summary>
    /// Creates new unconfigured <see cref="IObjectValidator"/> instance
    /// with the same parameters.
    /// </summary>
    /// <returns>Newly created validator.</returns>
    public abstract IObjectValidator CreateNew();
  }
}