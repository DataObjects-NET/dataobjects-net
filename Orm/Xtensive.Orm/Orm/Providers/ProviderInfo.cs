// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Provider's features provider.
  /// </summary>
  [Serializable]
  public sealed class ProviderInfo
  {
    private readonly TemporaryTableFeatures temporaryTableFeatures;

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
    /// Determines whether the specified features are supported.
    /// </summary>
    /// <param name="required">The required feature set.</param>
    public bool Supports(ProviderFeatures required)
    {
      return (ProviderFeatures & required)==required;
    }

    /// <summary>
    /// Determines whether the specified features are supported.
    /// </summary>
    /// <param name="required">The required feature set.</param>
    public bool Supports(TemporaryTableFeatures required)
    {
      return (temporaryTableFeatures & required)==required;
    }

    // Constructors

    internal ProviderInfo(
      string providerName,
      Version storageVersion,
      ProviderFeatures providerFeatures,
      TemporaryTableFeatures temporaryTableFeatures,
      int maxIdentifierLength,
      string constantPrimaryIndexName,
      string defaultDatabase,
      string defaultSchema)
    {
      ProviderName = providerName;

      StorageVersion = storageVersion;
      ProviderFeatures = providerFeatures;

      this.temporaryTableFeatures = temporaryTableFeatures;

      MaxIdentifierLength = maxIdentifierLength;
      ConstantPrimaryIndexName = constantPrimaryIndexName;

      DefaultDatabase = defaultDatabase;
      DefaultSchema = defaultSchema;
    }
  }
}