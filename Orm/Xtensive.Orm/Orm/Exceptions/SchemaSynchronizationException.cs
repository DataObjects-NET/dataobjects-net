// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.09.18

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

using Xtensive.Orm.Upgrade;

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
    public SchemaComparisonResult ComparisonResult { get; }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="result">The schema comparison result.</param>
    public SchemaSynchronizationException(SchemaComparisonResult result)
      : this(string.Format(Strings.ExExtractedSchemaIsNotEqualToTheTargetSchema_DetailsX, result))
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
  }
}