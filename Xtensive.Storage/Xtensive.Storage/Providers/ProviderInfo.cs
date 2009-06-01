// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Common;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Information about provider's capabilities.
  /// </summary>
  [Serializable]
  public sealed class ProviderInfo : LockableBase
  {
    private bool supportsEnlist;
    private bool supportsCollations;
    private bool supportsBatches;
    private bool supportsRealBoolean;
    private bool supportsForeignKeyConstraints;
    private bool supportsClusteredIndexes;
    private int maxQueryLength;
    private int maxComparisonOperations;
    private int databaseNameLength;
    private int maxTableNameLength;
    private int maxIndexNameLength;
    private int maxColumnNameLength;
    private int maxForeignKeyNameLength;
    private int maxIndexKeyLength;
    private int maxIndexColumnsCount;
    private bool namedParameters;
    private string parameterPrefix;
    private bool emptyStringIsNull;
    private bool emptyBlobIsNull;
    private bool supportsIncludedColumns;
    private Version version;

    /// <summary>
    /// Indicates that RDBMS supports distributed transactions.
    /// </summary>
    public bool SupportsEnlist
    {
      get { return supportsEnlist; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        supportsEnlist = value; 
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports collations.
    /// </summary>
    public bool SupportsCollations
    {
      get { return supportsCollations; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        supportsCollations = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports batch query execution.
    /// </summary>
    public bool SupportsBatches
    {
      get { return supportsBatches; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        supportsBatches = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports boolean data type.
    /// If the value of this property is <see langword="false"/>,
    /// RDMBS uses integer-like type to store boolean values.
    /// </summary>
    public bool SupportsRealBoolean
    {
      get { return supportsRealBoolean; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        supportsRealBoolean = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports foreign key constraints.
    /// </summary>
    public bool SupportsForeignKeyConstraints
    {
      get { return supportsForeignKeyConstraints; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        supportsForeignKeyConstraints = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports clustered indexes.
    /// </summary>
    public bool SupportsClusteredIndexes
    {
      get { return supportsClusteredIndexes; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        supportsClusteredIndexes = value;
      }
    }

    /// <summary>
    /// Maximal length of a query text (characters).
    /// </summary>
    public int MaxQueryLength
    {
      get { return maxQueryLength; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxQueryLength = value;
      }
    }

    ///<summary>
    /// Maximal number of comparison operations for a single query.
    /// </summary>
    public int MaxComparisonOperations
    {
      get { return maxComparisonOperations; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxComparisonOperations = value;
      }
    }

    /// <summary>
    /// Maximal length of database name (characters).
    /// </summary>
    public int DatabaseNameLength {
      get { return databaseNameLength; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        databaseNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of a table's name.
    /// </summary>
    public int MaxTableNameLength
    {
      get { return maxTableNameLength; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxTableNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of an index's name.
    /// </summary>
    public int MaxIndexNameLength
    {
      get { return maxIndexNameLength; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxIndexNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of a column's name.
    /// </summary>
    public int MaxColumnNameLength
    {
      get { return maxColumnNameLength; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxColumnNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of the name of a foreign key.
    /// </summary>
    public int MaxForeignKeyNameLength
    {
      get { return maxForeignKeyNameLength; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxForeignKeyNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of index key (bytes).
    /// </summary>
    public int MaxIndexKeyLength
    {
      get { return maxIndexKeyLength; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxIndexKeyLength = value;
      }
    }

    /// <summary>
    /// Maximal count of index columns.
    /// </summary>
    public int MaxIndexColumnsCount
    {
      get { return maxIndexColumnsCount; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        maxIndexColumnsCount = value;
      }
    }

    /// <summary>
    /// Indicates that it is possible to use named query parameters.
    /// </summary>
    public bool NamedParameters
    {
      get { return namedParameters; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        namedParameters = value;
      }
    }

    /// <summary>
    /// Parameter prefix.
    /// </summary>
    public string ParameterPrefix
    {
      get { return parameterPrefix; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        parameterPrefix = value;
      }
    }

    /// <summary>
    /// <see langword="True"/> if empty string ('') is considered as NULL.
    /// </summary>
    public bool EmptyStringIsNull
    {
      get { return emptyStringIsNull; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        emptyStringIsNull = value;
      }
    }

    /// <summary>
    /// <see langword="True"/> if empty BLOB is considered as NULL.
    /// </summary>
    public bool EmptyBlobIsNull
    {
      get { return emptyBlobIsNull; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        emptyBlobIsNull = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports included columns in indexes.
    /// </summary>
    public bool SupportsIncludedColumns
    {
      get { return supportsIncludedColumns; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        supportsIncludedColumns = value;
      }
    }

    /// <summary>
    /// Version of the RDBMS.
    /// </summary>
    public Version Version
    {
      get { return version; }
      set {
        LockableExtensions.EnsureNotLocked(this);
        version = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="ServerInfo"/> which was used to create this instance.
    /// </summary>
    public ServerInfo ServerInfo { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ProviderInfo()
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serverInfo">The SQL DOM information about RDBMS.</param>
    /// <param name="emptyBlobIsNull">The value of the <see cref="EmptyBlobIsNull"/> property.</param>
    /// <param name="emptyStringIsNull">The value of the <see cref="EmptyStringIsNull"/> property.</param>
    /// <param name="supportsEnlist">The value of the <see cref="SupportsEnlist"/> property.</param>
    public ProviderInfo(ServerInfo serverInfo, bool emptyBlobIsNull, bool emptyStringIsNull,
      bool supportsEnlist)
    {
      ArgumentValidator.EnsureArgumentNotNull(serverInfo, "serverInfo");

      ServerInfo = serverInfo;

      this.emptyBlobIsNull = emptyBlobIsNull;
      this.emptyStringIsNull = emptyStringIsNull;
      this.supportsEnlist = supportsEnlist;

      databaseNameLength = serverInfo.Database.MaxIdentifierLength;
      maxIndexColumnsCount = serverInfo.Index.MaxColumnAmount;
      maxIndexKeyLength = serverInfo.Index.MaxLength;
      maxIndexNameLength = serverInfo.Index.MaxIdentifierLength;
      maxTableNameLength = serverInfo.Table.MaxIdentifierLength;
      namedParameters = (serverInfo.Query.Features & QueryFeatures.NamedParameters)
        ==QueryFeatures.NamedParameters;
      parameterPrefix = serverInfo.Query.ParameterPrefix;
      maxComparisonOperations = serverInfo.Query.MaxComparisonOperations;
      maxQueryLength = serverInfo.Query.MaxLength;
      supportsBatches = (serverInfo.Query.Features & QueryFeatures.Batches)==QueryFeatures.Batches;
      supportsClusteredIndexes = (serverInfo.Index.Features & IndexFeatures.Clustered)
        ==IndexFeatures.Clustered;
      supportsCollations = serverInfo.Collation!=null;
      supportsForeignKeyConstraints = serverInfo.ForeignKey!=null;
      supportsRealBoolean = serverInfo.DataTypes.Boolean!=null;
      supportsIncludedColumns = (serverInfo.Index.Features & IndexFeatures.NonKeyColumns)
        ==IndexFeatures.NonKeyColumns;
      version = (Version)serverInfo.Version.ProductVersion.Clone();
    }
  }
}