// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using Xtensive.Storage.Metadata;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static class WellKnown
  {
    /// <summary>
    /// Name of the <see cref="Entity.Key"/> field.
    /// </summary>
    public const string KeyFieldName = "Key";

    /// <summary>
    /// Name of the <see cref="Entity.TypeId"/> field.
    /// </summary>
    public const string TypeIdFieldName = "TypeId";

    /// <summary>
    /// Name of the field that describes master in <see cref="Multiplicity.ManyToMany"/>
    /// or <see cref="Multiplicity.ZeroToMany"/> association.
    /// </summary>
    public const string MasterFieldName = "Master";

    /// <summary>
    /// Name of the field that describes slave in <see cref="Multiplicity.ManyToMany"/>
    /// or <see cref="Multiplicity.ZeroToMany"/> association.
    /// </summary>
    public const string SlaveFieldName = "Slave";

    /// <summary>
    /// Name of the <see cref="Extension"/> that describes domain model.
    /// </summary>
    public const string DomainModelExtensionName = "Xtensive.Storage.Model";

    /// <summary>
    /// Name of column in key generator table.
    /// </summary>
    public const string GeneratorColumnName = "ID";

    /// <summary>
    /// Name of <see cref="TimeSpan"/> domain.
    /// </summary>
    public const string TimeSpanDomainName = "dTimeSpan";

    /// <summary>
    /// Well-known storage protocol names.
    /// </summary>
    public static class Protocol
    {
      /// <summary>
      /// In-memory index storage.
      /// </summary>
      public const string Memory = "memory";
      /// <summary>
      /// Microsoft SQL Server.
      /// </summary>
      public const string SqlServer = "sqlserver";
      /// <summary>
      /// PostgreSQL.
      /// </summary>
      public const string PostgreSql = "postgresql";

      /// <summary>
      /// All supported protocols (for exception messages, etc).
      /// </summary>
      public const string All = "'memory', 'sqlserver', 'postgresql'";
    }

    /// <summary>
    /// Max number of key fields.
    /// </summary>
    public const int MaxKeyFieldNumber = 4;

    /// <summary>
    /// Well-known session configuration names
    /// </summary>
    public static class Sessions
    {
      /// <summary>
      /// Name of default session configuration.
      /// </summary>
      public const string Default = "Default";

      /// <summary>
      /// System session name.
      /// </summary>
      public const string System = "System";

      /// <summary>
      /// Service session name.
      /// </summary>
      public const string Service = "Service";

      /// <summary>
      /// Generator session name.
      /// </summary>
      public const string KeyGenerator = "KeyGenerator";
    }
  }
}