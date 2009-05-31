// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using Xtensive.Storage.Metadata;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static class StorageWellKnown
  {
    /// <summary>
    /// Name of the <see cref="Entity.Key"/>
    /// </summary>
    public const string Key = "Key";

    /// <summary>
    /// Name of the <see cref="Extension"/> that describes domain model.
    /// </summary>
    public const string DomainModelExtension = "Xtensive.Storage.Model";

    /// <summary>
    /// Name of the field that describes master in <see cref="Multiplicity.ManyToMany"/>
    /// or <see cref="Multiplicity.ZeroToMany"/> association.
    /// </summary>
    public const string MasterField = "Master";

    /// <summary>
    /// Name of the field that describes slave in <see cref="Multiplicity.ManyToMany"/>
    /// or <see cref="Multiplicity.ZeroToMany"/> association.
    /// </summary>
    public const string SlaveField = "Slave";
  }
}