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
  [Serializable]
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

    // Serialization

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTranslationException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    private QueryTranslationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}