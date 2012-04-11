// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.18

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

using Xtensive.Orm.Building;


namespace Xtensive.Orm
{
  /// <summary>
  /// Describes schema synchronization errors 
  /// detected during <see cref="Domain"/>.<see cref="Domain.Build"/> execution.
  /// </summary>
  [Serializable]
  public sealed class SchemaSynchronizationException : StorageException
  {
    /// <summary>
    /// Gets or sets the schema comparison result.
    /// </summary>
    public SchemaComparisonResult ComparisonResult { get; private set; }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="result">The schema comparison result.</param>
    public SchemaSynchronizationException(SchemaComparisonResult result)
      : this(Strings.ExExtractedSchemaIsNotEqualToTheTargetSchema_DetailsX.FormatWith(result))
    {
      ComparisonResult = result;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SchemaSynchronizationException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SchemaSynchronizationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaSynchronizationException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    ///   
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
    protected SchemaSynchronizationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}