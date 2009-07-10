// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

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
    private bool supportsAllBooleanExpressions;
    private bool supportsRealTimeSpan;
    private bool supportsForeignKeyConstraints;
    private bool supportsClusteredIndexes;
    private bool supportKeyColumnSortOrder;
    private bool supportSequences;
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
    public bool SupportsEnlist {
      get { return supportsEnlist; }
      set {
        this.EnsureNotLocked();
        supportsEnlist = value; 
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports collations.
    /// </summary>
    public bool SupportsCollations {
      get { return supportsCollations; }
      set {
        this.EnsureNotLocked();
        supportsCollations = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports batch query execution.
    /// </summary>
    public bool SupportsBatches {
      get { return supportsBatches; }
      set {
        this.EnsureNotLocked();
        supportsBatches = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports boolean data type.
    /// If the value of this property is <see langword="false"/>,
    /// RDMBS uses integer-like type to store boolean values.
    /// </summary>
    public bool SupportsRealTimeSpan
    {
      get { return supportsRealTimeSpan; }
      set {
        this.EnsureNotLocked();
        supportsRealTimeSpan = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports foreign key constraints.
    /// </summary>
    public bool SupportsForeignKeyConstraints {
      get { return supportsForeignKeyConstraints; }
      set {
        this.EnsureNotLocked();
        supportsForeignKeyConstraints = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports clustered indexes.
    /// </summary>
    public bool SupportsClusteredIndexes {
      get { return supportsClusteredIndexes; }
      set {
        this.EnsureNotLocked();
        supportsClusteredIndexes = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports index key columns ordering.
    /// </summary>
    public bool SupportKeyColumnSortOrder {
      get { return supportKeyColumnSortOrder; }
      set {
        this.EnsureNotLocked();
        supportKeyColumnSortOrder = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports sequences.
    /// </summary>
    public bool SupportSequences {
      get { return supportSequences; }
      set {
        this.EnsureNotLocked();
        supportSequences = value;
      }
    }

    /// <summary>
    /// Maximal length of a query text (characters).
    /// </summary>
    public int MaxQueryLength {
      get { return maxQueryLength; }
      set {
        this.EnsureNotLocked();
        maxQueryLength = value;
      }
    }

    ///<summary>
    /// Maximal number of comparison operations for a single query.
    /// </summary>
    public int MaxComparisonOperations {
      get { return maxComparisonOperations; }
      set {
        this.EnsureNotLocked();
        maxComparisonOperations = value;
      }
    }

    /// <summary>
    /// Maximal length of database name (characters).
    /// </summary>
    public int DatabaseNameLength {
      get { return databaseNameLength; }
      set {
        this.EnsureNotLocked();
        databaseNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of a table's name.
    /// </summary>
    public int MaxTableNameLength {
      get { return maxTableNameLength; }
      set {
        this.EnsureNotLocked();
        maxTableNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of an index's name.
    /// </summary>
    public int MaxIndexNameLength {
      get { return maxIndexNameLength; }
      set {
        this.EnsureNotLocked();
        maxIndexNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of a column's name.
    /// </summary>
    public int MaxColumnNameLength {
      get { return maxColumnNameLength; }
      set {
        this.EnsureNotLocked();
        maxColumnNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of the name of a foreign key.
    /// </summary>
    public int MaxForeignKeyNameLength {
      get { return maxForeignKeyNameLength; }
      set {
        this.EnsureNotLocked();
        maxForeignKeyNameLength = value;
      }
    }

    /// <summary>
    /// Maximal length of index key (bytes).
    /// </summary>
    public int MaxIndexKeyLength {
      get { return maxIndexKeyLength; }
      set {
        this.EnsureNotLocked();
        maxIndexKeyLength = value;
      }
    }

    /// <summary>
    /// Maximal count of index columns.
    /// </summary>
    public int MaxIndexColumnsCount {
      get { return maxIndexColumnsCount; }
      set {
        this.EnsureNotLocked();
        maxIndexColumnsCount = value;
      }
    }

    /// <summary>
    /// Indicates that it is possible to use named query parameters.
    /// </summary>
    public bool NamedParameters {
      get { return namedParameters; }
      set {
        this.EnsureNotLocked();
        namedParameters = value;
      }
    }

    /// <summary>
    /// Parameter prefix.
    /// </summary>
    public string ParameterPrefix {
      get { return parameterPrefix; }
      set {
        this.EnsureNotLocked();
        parameterPrefix = value;
      }
    }

    /// <summary>
    /// <see langword="True"/> if empty string ('') is considered as NULL.
    /// </summary>
    public bool EmptyStringIsNull {
      get { return emptyStringIsNull; }
      set {
        this.EnsureNotLocked();
        emptyStringIsNull = value;
      }
    }

    /// <summary>
    /// <see langword="True"/> if empty BLOB is considered as NULL.
    /// </summary>
    public bool EmptyBlobIsNull {
      get { return emptyBlobIsNull; }
      set {
        this.EnsureNotLocked();
        emptyBlobIsNull = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports included columns in indexes.
    /// </summary>
    public bool SupportsIncludedColumns {
      get { return supportsIncludedColumns; }
      set {
        this.EnsureNotLocked();
        supportsIncludedColumns = value;
      }
    }

    /// <summary>
    /// Version of the RDBMS.
    /// </summary>
    public Version Version {
      get { return version; }
      set {
        this.EnsureNotLocked();
        version = value;
      }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ProviderInfo()
    {
    }
  }
}