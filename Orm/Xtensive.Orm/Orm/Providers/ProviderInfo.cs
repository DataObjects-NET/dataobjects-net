// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Provider's features provider.
  /// </summary>
  [Serializable]
  public sealed class ProviderInfo
  {
    private readonly Version storageVersion;
    private readonly ProviderFeatures providerFeatures;

    /// <summary>
    /// Determines whether the specified features are supported.
    /// </summary>
    /// <param name="required">The required feature set.</param>
    public bool Supports(ProviderFeatures required)
    {
      return (providerFeatures & required)==required;
    }

    /// <summary>
    /// Version of the underlying storage.
    /// </summary>
    public Version StorageVersion { get { return storageVersion; } }

    /// <summary>
    /// Gets the features.
    /// </summary>
    public ProviderFeatures ProviderFeatures { get { return providerFeatures; } }

    /// <summary>
    /// Maximal identifier length.
    /// </summary>
    public int MaxIdentifierLength { get; private set; }

    /// <summary>
    /// Gets or sets the constant name of the primary index.
    /// </summary>
    /// <value>The constant name of the primary index.</value>
    public string ConstantPrimaryIndexName { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ProviderInfo(Version storageVersion, ProviderFeatures providerFeatures, int maxIdentifierLength)
    {
      this.storageVersion = storageVersion;
      this.providerFeatures = providerFeatures;
      MaxIdentifierLength = maxIdentifierLength;
    }

    public ProviderInfo(Version storageVersion, ProviderFeatures providerFeatures, int maxIdentifierLength, string constantPrimaryIndexName)
    {
      this.storageVersion = storageVersion;
      this.providerFeatures = providerFeatures;
      MaxIdentifierLength = maxIdentifierLength;
      ConstantPrimaryIndexName = constantPrimaryIndexName;
    }
  }
}