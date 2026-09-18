// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is thrown when a CHECK constraint violation is detected.
  /// This also includes violations of a NOT NULL constraints.
  /// </summary>
  public sealed class CheckConstraintViolationException : ConstraintViolationException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CheckConstraintViolationException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CheckConstraintViolationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}