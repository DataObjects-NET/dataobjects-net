// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is thrown when deadlock is detected by RDBMS.
  /// </summary>
  public sealed class DeadlockException : ReprocessableException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DeadlockException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DeadlockException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}