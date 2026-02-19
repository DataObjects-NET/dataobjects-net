// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.02.09

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is thrown when currently executing operation 
  /// is timed out.
  /// </summary>
  public sealed class OperationTimeoutException : StorageException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    public OperationTimeoutException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public OperationTimeoutException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}