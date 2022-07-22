// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.25

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// A information required for creating a driver.
  /// </summary>
  public class CoreServerInfo : LockableBase
  {
    private Version serverVersion;
    private string connectionString;
    private string databaseName;
    private string defaultSchemaName;
    private bool multipleActiveResultSets;

    /// <summary>
    /// Gets or sets the server version.
    /// </summary>
    public Version ServerVersion {
      get {
        return serverVersion;
      }
      set {
        EnsureNotLocked();
        serverVersion = value;
      }
    }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString {
      get {
        return connectionString;
      }
      set {
        EnsureNotLocked();
        connectionString = value;
      }
    }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string DatabaseName {
      get {
        return databaseName;
      }
      set {
        EnsureNotLocked();
        databaseName = value;
      }
    }

    /// <summary>
    /// Gets or sets the default name of the schema.
    /// </summary>
    public string DefaultSchemaName {
      get {
        return defaultSchemaName;
      }
      set {
        EnsureNotLocked();
        defaultSchemaName = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether multiple active result sets are supported.
    /// </summary>
    public bool MultipleActiveResultSets {
      get {
        return multipleActiveResultSets;
      }
      set {
        EnsureNotLocked();
        multipleActiveResultSets = value;
      }
    }
  }
}