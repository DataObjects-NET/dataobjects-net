// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

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
    private bool supportsForeignKeyConstraints;
    private bool supportsClusteredIndexes;
    private bool supportsKeyColumnSortOrder;
    private bool supportsSequences;
    private bool supportsAutoincrementColumns;
    private bool supportsPaging;
    private bool supportsIncludedColumns;
    private bool supportsDeferredForeignKeyConstraints;
    private bool supportsApplyProvider;
    private bool supportsAllBooleanExpressions;
    private bool supportsLargeObjects;

    private bool namedParameters;
    private string parameterPrefix;
    private bool emptyStringIsNull;
    private bool emptyBlobIsNull;
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
    /// Indicates that RDBMS supports foreign key constraints.
    /// </summary>
    public bool SupportsDeferredForeignKeyConstraints {
      get { return supportsDeferredForeignKeyConstraints; }
      set {
        this.EnsureNotLocked();
        supportsDeferredForeignKeyConstraints = value;
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
    public bool SupportsKeyColumnSortOrder {
      get { return supportsKeyColumnSortOrder; }
      set {
        this.EnsureNotLocked();
        supportsKeyColumnSortOrder = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports autoincrement columns.
    /// </summary>
    public bool SupportsAutoincrementColumns {
      get { return supportsAutoincrementColumns; }
      set {
        this.EnsureNotLocked();
        supportsAutoincrementColumns = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports sequences.
    /// </summary>
    public bool SupportsSequences {
      get { return supportsSequences; }
      set {
        this.EnsureNotLocked();
        supportsSequences = value;
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
    /// Indicates that RDBMS natively supports paging operations.
    /// </summary>
    public bool SupportsPaging {
      get { return supportsPaging; }
      set {
        this.EnsureNotLocked();
        supportsPaging = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS natively suppirts <see cref="ApplyProvider"/>.
    /// </summary>
    public bool SupportsApplyProvider {
      get { return supportsApplyProvider; }
      set {
        this.EnsureNotLocked();
        supportsApplyProvider = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS supports all boolean expressions,
    /// including direct comparison, usage inside coalesce and case expressions.
    /// </summary>
    public bool SupportsAllBooleanExpressions {
      get { return supportsAllBooleanExpressions; } 
      set {
        this.EnsureNotLocked();
        supportsAllBooleanExpressions = value;
      }
    }

    /// <summary>
    /// Indicates that RDBMS providers special API for handling of large objects (LOBs).
    /// </summary>
    public bool SupportsLargeObjects {
      get { return supportsLargeObjects; }
      set {
        this.EnsureNotLocked();
        supportsLargeObjects = value;
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