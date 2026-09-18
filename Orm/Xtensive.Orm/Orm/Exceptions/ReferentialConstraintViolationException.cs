// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is thrown when referential constaint (aka foreign key) is violated.
  /// This differs from <see cref="ReferentialIntegrityException"/>.
  /// <see cref="ReferentialConstraintViolationException"/> is thrown when RDBMS detects a violation.
  /// <see cref="Xtensive.Orm.ReferentialIntegrity"/> is thrown when internal referential integrity
  /// mechanism detects a violation.
  /// </summary>
  public sealed class ReferentialConstraintViolationException : ConstraintViolationException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ReferentialConstraintViolationException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ReferentialConstraintViolationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}