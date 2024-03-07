// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System.Runtime.Serialization;


namespace Xtensive.Orm
{
  /// <summary>
  /// An exception that is thrown when referential constaint (aka foreign key) is violated.
  /// This differs from <see cref="ReferentialIntegrityException"/>.
  /// <see cref="ReferentialConstraintViolationException"/> is thrown when RDBMS detects a violation.
  /// <see cref="Xtensive.Orm.ReferentialIntegrity"/> is thrown when internal referential integrity
  /// mechanism detects a violation.
  /// </summary>
  [Serializable]
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

    // Serialization

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferentialConstraintViolationException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    private ReferentialConstraintViolationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}