// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.12.07

using System;
using System.Runtime.Serialization;


namespace Xtensive.Orm
{
  /// <summary>
  /// LINQ query translation exception. 
  /// Describes various errors that occurs during query compilation.
  /// </summary>
  public sealed class QueryTranslationException : StorageException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public QueryTranslationException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public QueryTranslationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}