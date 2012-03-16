// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.18

using System;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="result">The schema comparison result.</param>
    public SchemaSynchronizationException(SchemaComparisonResult result)
      : this(Strings.ExExtractedSchemaIsNotEqualToTheTargetSchema_DetailsX.FormatWith(result))
    {
      ComparisonResult = result;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public SchemaSynchronizationException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SchemaSynchronizationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected SchemaSynchronizationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}