// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.09.18

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Describes various errors detected during <see cref="Domain"/>.<see cref="Domain.Build"/> execution.
  /// </summary>
  public sealed class DomainBuilderException : StorageException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DomainBuilderException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DomainBuilderException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}