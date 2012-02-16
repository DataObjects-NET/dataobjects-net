// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using System.Collections.Generic;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Metadata accessor used to read persistent metadata
  /// on early stage of <see cref="Domain"/> build.
  /// </summary>
  public abstract class MetadataAccessor : HandlerBase
  {
    /// <summary>
    /// Gets all <see cref="AssemblyMetadata"/> found in the storage.
    /// </summary>
    /// <returns>Stored <see cref="AssemblyMetadata"/> instances.</returns>
    public abstract IEnumerable<AssemblyMetadata> GetAssemblies(string database, string schema);

    /// <summary>
    /// Gets all <see cref="TypeMetadata"/> found in the storage.
    /// </summary>
    /// <returns>Stored <see cref="TypeMetadata"/> instances.</returns>
    public abstract IEnumerable<TypeMetadata> GetTypes(string database, string schema);

    /// <summary>
    /// Gets <see cref="ExtensionMetadata"/>
    /// with the specified <paramref name="name"/> from the storage.
    /// </summary>
    /// <returns>Stored <see cref="ExtensionMetadata"/> instance
    /// with the specified <paramref name="name"/>.</returns>
    public abstract IEnumerable<ExtensionMetadata> GetExtension(string database, string schema, string name);
  }
}