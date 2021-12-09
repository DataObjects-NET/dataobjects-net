// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Provider's features provider.
  /// </summary>
  [Serializable]
  public sealed class ProviderInfo
  {

    /// <summary>
    /// Gets provider name.
    /// </summary>
    public string ProviderName { get; private set; }

    /// <summary>
    /// Version of the underlying storage.
    /// </summary>
    public Version StorageVersion { get; private set; }

    /// <summary>
    /// Gets the features.
    /// </summary>
    public ProviderFeatures ProviderFeatures { get; private set; }

    /// <summary>
    /// Gets maximal identifier length.
    /// </summary>
    public int MaxIdentifierLength { get; private set; }

    /// <summary>
    /// Gets the constant name of the primary index.
    /// </summary>
    /// <value>The constant name of the primary index.</value>
    public string ConstantPrimaryIndexName { get; private set; }

    /// <summary>
    /// Gets default database for current user.
    /// </summary>
    public string DefaultDatabase { get; private set; }

    /// <summary>
    /// Gets default schema for current user.
    /// </summary>
    public string DefaultSchema { get; private set; }

    /// <summary>
    /// Gets the supported types.
    /// </summary>
    public IReadOnlySet<Type> SupportedTypes { get; private set; }

    /// <summary>
    /// Gets max query parameter count.
    /// </summary>
    public int MaxQueryParameterCount { get; private set; }

    /// <summary>
    /// Determines whether the specified features are supported.
    /// </summary>
    /// <param name="required">The required feature set.</param>
    public bool Supports(ProviderFeatures required)
    {
      return (ProviderFeatures & required)==required;
    }

    // Constructors

    internal ProviderInfo(
      string providerName,
      Version storageVersion,
      ProviderFeatures providerFeatures,
      int maxIdentifierLength,
      string constantPrimaryIndexName,
      string defaultDatabase,
      string defaultSchema, 
      IEnumerable<Type> supportedTypes,
      int maxQueryParameterCount)
    {
      ProviderName = providerName;

      StorageVersion = storageVersion;
      ProviderFeatures = providerFeatures;

      MaxIdentifierLength = maxIdentifierLength;
      ConstantPrimaryIndexName = constantPrimaryIndexName;

      DefaultDatabase = defaultDatabase;
      DefaultSchema = defaultSchema;

      SupportedTypes = new ReadOnlyHashSet<Type>(new HashSet<Type>(supportedTypes));
      MaxQueryParameterCount = maxQueryParameterCount;
    }
  }
}
