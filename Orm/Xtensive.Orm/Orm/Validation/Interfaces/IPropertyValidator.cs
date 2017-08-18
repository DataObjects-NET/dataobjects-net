// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.06

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Field validator contract.
  /// </summary>
  public interface IPropertyValidator : IValidator
  {
    /// <summary>
    /// Gets value indicating if current validator is immediate.
    /// Immediate validators execute just before field value changes.
    /// </summary>
    bool IsImmediate { get; set; }

    /// <summary>
    /// Gets value indicating whether validator should be skipped on transaction commit validation.
    /// </summary>
    bool SkipOnTransactionCommit { get; }

    /// <summary>
    /// Gets value indicating whether validator should be skipped if field value was not changed.
    /// </summary>
    bool ValidateOnlyIfModified { get; }

    /// <summary>
    /// Configures this instance.
    /// </summary>
    /// <param name="domain">A domain this validator is bound to.</param>
    /// <param name="type">A type this validator is bound to.</param>
    /// <param name="field">A persitent field this validator is bound to.</param>
    void Configure(Domain domain, TypeInfo type, FieldInfo field);

    /// <summary>
    /// Validates specified object considering new value of a persistent field.
    /// </summary>
    /// <param name="target">An object to validate.</param>
    /// <param name="fieldValue">Persistent field value.</param>
    ValidationResult Validate(Entity target, object fieldValue);

    /// <summary>
    /// Creates new unconfigured <see cref="IPropertyValidator"/> instance
    /// with the same parameters.
    /// </summary>
    /// <returns>Newly created validator.</returns>
    IPropertyValidator CreateNew();
  }
}