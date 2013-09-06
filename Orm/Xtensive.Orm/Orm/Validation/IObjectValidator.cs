// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.06

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Persistent object validator contract.
  /// </summary>
  public interface IObjectValidator
  {
    /// <summary>
    /// Configures this instance.
    /// </summary>
    /// <param name="domain">A domain this validator is bound to.</param>
    /// <param name="type">A type this validator is bound to.</param>
    void Configure(Domain domain, TypeInfo type);

    /// <summary>
    /// Validates specified object.
    /// </summary>
    /// <param name="target">An object to validate.</param>
    void Validate(Persistent target);

    /// <summary>
    /// Creates new unconfigured <see cref="IObjectValidator"/> instance
    /// with the same parameters.
    /// </summary>
    /// <returns>Newly created validator.</returns>
    IObjectValidator CreateNew();
  }
}