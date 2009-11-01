// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Represents a set of information concerning RDBMS capabilities.
  /// </summary>
  public class ServerInfo : LockableBase
  {
    private ServerEntities entities;
    private ColumnInfo column;
    private ConstraintInfo primaryKey;
    private ConstraintInfo uniqueConstraint;
    private CheckConstraintInfo checkConstraint;
    private ReferenceConstraintInfo foreignKey;
    private EntityInfo database;
    private EntityInfo domain;
    private IndexInfo index;
    private QueryInfo query;
    private EntityInfo schema;
    private SequenceInfo sequence;
    private EntityInfo storedProcedure;
    private TableInfo table;
    private EntityInfo trigger;
    private EntityInfo view;
    private VersionInfo version;
    private IdentityInfo identity;
    private DataTypeCollection dataTypes;
    private IsolationLevels isolationLevels;
    private ConstraintInfo assertion;
    private TemporaryTableInfo temporaryTable;
    private EntityInfo characterSet;
    private EntityInfo collation;
    private EntityInfo translation;
    private int stringIndexingBase;

    /// <summary>
    /// Underlying RDBMS version.
    /// </summary>
    public VersionInfo Version
    {
      get { return version; }
    }

    /// <summary>
    /// Gets the server supported entities.
    /// </summary>
    public ServerEntities Entities
    {
      get { return entities; }
    }

    /// <summary>
    /// Gets the server supported isolation levels.
    /// </summary>
    public IsolationLevels IsolationLevels
    {
      get { return isolationLevels; }
    }

    /// <summary>
    /// Gets the assertion constraint info.
    /// </summary>
    /// <value>The assertion constraint info.</value>
    public ConstraintInfo Assertion
    {
      get { return assertion; }
    }

    /// <summary>
    /// Gets the character set info.
    /// </summary>
    /// <value>The character set info.</value>
    public EntityInfo CharacterSet
    {
      get { return characterSet; }
    }

    /// <summary>
    /// Gets the collation info.
    /// </summary>
    /// <value>The collation info.</value>
    public EntityInfo Collation
    {
      get { return collation; }
    }

    /// <summary>
    /// Gets the column info.
    /// </summary>
    /// <value>The column info.</value>
    public ColumnInfo Column
    {
      get { return column; }
    }

    /// <summary>
    /// Gets the check constraint info.
    /// </summary>
    /// <value>The check constraint info.</value>
    public CheckConstraintInfo CheckConstraint
    {
      get { return checkConstraint; }
    }

    /// <summary>
    /// Gets the database info.
    /// </summary>
    /// <value>The database info.</value>
    public EntityInfo Database
    {
      get { return database; }
    }

    /// <summary>
    /// Gets the domain info.
    /// </summary>
    /// <value>The domain info.</value>
    public EntityInfo Domain
    {
      get { return domain; }
    }

    /// <summary>
    /// Gets the identity info.
    /// </summary>
    /// <value>The identity info.</value>
    public IdentityInfo Identity
    {
      get { return identity; }
    }

    /// <summary>
    /// Gets the index info.
    /// </summary>
    /// <value>The index info.</value>
    public IndexInfo Index
    {
      get { return index; }
    }

    /// <summary>
    /// Gets the primary key constraint info.
    /// </summary>
    /// <value>The primary key constraint info.</value>
    public ConstraintInfo PrimaryKey
    {
      get { return primaryKey; }
    }

    /// <summary>
    /// Gets the query info.
    /// </summary>
    /// <value>The query info.</value>
    public QueryInfo Query
    {
      get { return query; }
    }

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    /// <value>The referential constraint info.</value>
    public ReferenceConstraintInfo ForeignKey
    {
      get { return foreignKey; }
    }

    /// <summary>
    /// Gets the schema info.
    /// </summary>
    /// <value>The schema info.</value>
    public EntityInfo Schema
    {
      get { return schema; }
    }

    /// <summary>
    /// Gets the sequence info.
    /// </summary>
    /// <value>The sequence info.</value>
    public SequenceInfo Sequence
    {
      get { return sequence; }
    }

    /// <summary>
    /// Gets the stored procedure info.
    /// </summary>
    /// <value>The stored procedure info.</value>
    public EntityInfo StoredProcedure
    {
      get { return storedProcedure; }
    }

    /// <summary>
    /// Gets the table info.
    /// </summary>
    /// <value>The table info.</value>
    public TableInfo Table
    {
      get { return table; }
    }

    /// <summary>
    /// Gets the temporary table info.
    /// </summary>
    /// <value>The temporary table info.</value>
    public TemporaryTableInfo TemporaryTable
    {
      get { return temporaryTable; }
    }

    /// <summary>
    /// Gets the translation info.
    /// </summary>
    /// <value>The translation info.</value>
    public EntityInfo Translation
    {
      get { return translation; }
    }

    /// <summary>
    /// Gets the trigger info.
    /// </summary>
    /// <value>The trigger info.</value>
    public EntityInfo Trigger
    {
      get { return trigger; }
    }

    /// <summary>
    /// Gets the unique constraint info.
    /// </summary>
    /// <value>The unique constraint info.</value>
    public ConstraintInfo UniqueConstraint
    {
      get { return uniqueConstraint; }
    }

    /// <summary>
    /// Gets the view info.
    /// </summary>
    /// <value>The view info.</value>
    public EntityInfo View
    {
      get { return view; }
    }

    /// <summary>
    /// Gets the data types.
    /// </summary>
    /// <value>The data types.</value>
    public DataTypeCollection DataTypes
    {
      get { return dataTypes; }
    }

    /// <summary>
    /// Gets the string indexing base. Normally is equal to 1.
    /// </summary>
    /// <value>The string indexing base.</value>
    public int StringIndexingBase
    {
      get { return stringIndexingBase; }
    }

    /// <summary>
    /// Builds the server info using specified <see cref="IServerInfoProvider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public static ServerInfo Build(IServerInfoProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      ServerInfo info = new ServerInfo();

      info.version = provider.VersionInfo;
      info.assertion = provider.AssertionInfo;
      info.characterSet = provider.CharacterSetInfo;
      info.checkConstraint = provider.CheckConstraintInfo;
      info.collation = provider.CollationInfo;
      info.column = provider.ColumnInfo;
      info.dataTypes = provider.DataTypesInfo;
      info.database = provider.DatabaseInfo;
      info.domain = provider.DomainInfo;
      info.entities = provider.Entities;
      info.foreignKey = provider.ReferentialConstraintInfo;
      info.identity = provider.IdentityInfo;
      info.index = provider.IndexInfo;
      info.isolationLevels = provider.IsolationLevels;
      info.primaryKey = provider.PrimaryKeyInfo;
      info.query = provider.QueryInfo;
      info.schema = provider.SchemaInfo;
      info.sequence = provider.SequenceInfo;
      info.storedProcedure = provider.StoredProcedureInfo;
      info.table = provider.TableInfo;
      info.temporaryTable = provider.TemporaryTableInfo;
      info.translation = provider.TranslationInfo;
      info.trigger = provider.TriggerInfo;
      info.uniqueConstraint = provider.UniqueConstraintInfo;
      info.view = provider.ViewInfo;
      info.stringIndexingBase = provider.StringIndexingBase;

      info.Lock(true);

      return info;
    }

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      TryLockEntityInfo(assertion, recursive);
      TryLockEntityInfo(characterSet, recursive);
      TryLockEntityInfo(checkConstraint, recursive);
      TryLockEntityInfo(collation, recursive);
      TryLockEntityInfo(column, recursive);
      TryLockEntityInfo(dataTypes, recursive);
      TryLockEntityInfo(database, recursive);
      TryLockEntityInfo(domain, recursive);
      TryLockEntityInfo(foreignKey, recursive);
      TryLockEntityInfo(identity, recursive);
      TryLockEntityInfo(index, recursive);
      TryLockEntityInfo(primaryKey, recursive);
      TryLockEntityInfo(query, recursive);
      TryLockEntityInfo(schema, recursive);
      TryLockEntityInfo(sequence, recursive);
      TryLockEntityInfo(storedProcedure, recursive);
      TryLockEntityInfo(table, recursive);
      TryLockEntityInfo(temporaryTable, recursive);
      TryLockEntityInfo(translation, recursive);
      TryLockEntityInfo(trigger, recursive);
      TryLockEntityInfo(uniqueConstraint, recursive);
      TryLockEntityInfo(view, recursive);
    }

    private static void TryLockEntityInfo(ILockable info, bool recursive)
    {
      if (info!=null)
        info.Lock(recursive);
    }
  }
}