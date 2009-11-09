// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Represents a set of information concerning RDBMS capabilities.
  /// </summary>
  public class ServerInfo : LockableBase
  {
    /// <summary>
    /// Underlying RDBMS version.
    /// </summary>
    public VersionInfo Version { get; private set; }

    /// <summary>
    /// Gets the server supported isolation levels.
    /// </summary>
    public IsolationLevels IsolationLevels { get; private set; }

    /// <summary>
    /// Gets the assertion constraint info.
    /// </summary>
    /// <value>The assertion constraint info.</value>
    public AssertConstraintInfo Assertion { get; private set; }

    /// <summary>
    /// Gets the character set info.
    /// </summary>
    /// <value>The character set info.</value>
    public EntityInfo CharacterSet { get; private set; }

    /// <summary>
    /// Gets the collation info.
    /// </summary>
    /// <value>The collation info.</value>
    public EntityInfo Collation { get; private set; }

    /// <summary>
    /// Gets the column info.
    /// </summary>
    /// <value>The column info.</value>
    public ColumnInfo Column { get; private set; }

    /// <summary>
    /// Gets the check constraint info.
    /// </summary>
    /// <value>The check constraint info.</value>
    public CheckConstraintInfo CheckConstraint { get; private set; }

    /// <summary>
    /// Gets the database info.
    /// </summary>
    /// <value>The database info.</value>
    public EntityInfo Database { get; private set; }

    /// <summary>
    /// Gets the domain info.
    /// </summary>
    /// <value>The domain info.</value>
    public EntityInfo Domain { get; private set; }

    /// <summary>
    /// Gets the identity info.
    /// </summary>
    /// <value>The identity info.</value>
    public IdentityInfo Identity { get; private set; }

    /// <summary>
    /// Gets the index info.
    /// </summary>
    /// <value>The index info.</value>
    public IndexInfo Index { get; private set; }

    /// <summary>
    /// Gets the primary key constraint info.
    /// </summary>
    /// <value>The primary key constraint info.</value>
    public PrimaryKeyConstraintInfo PrimaryKey { get; private set; }

    /// <summary>
    /// Gets the query info.
    /// </summary>
    /// <value>The query info.</value>
    public QueryInfo Query { get; private set; }

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    /// <value>The referential constraint info.</value>
    public ForeignKeyConstraintInfo ForeignKey { get; private set; }

    /// <summary>
    /// Gets the schema info.
    /// </summary>
    /// <value>The schema info.</value>
    public EntityInfo Schema { get; private set; }

    /// <summary>
    /// Gets the sequence info.
    /// </summary>
    /// <value>The sequence info.</value>
    public SequenceInfo Sequence { get; private set; }

    /// <summary>
    /// Gets the stored procedure info.
    /// </summary>
    /// <value>The stored procedure info.</value>
    public EntityInfo StoredProcedure { get; private set; }

    /// <summary>
    /// Gets the table info.
    /// </summary>
    /// <value>The table info.</value>
    public TableInfo Table { get; private set; }

    /// <summary>
    /// Gets the temporary table info.
    /// </summary>
    /// <value>The temporary table info.</value>
    public TemporaryTableInfo TemporaryTable { get; private set; }

    /// <summary>
    /// Gets the translation info.
    /// </summary>
    /// <value>The translation info.</value>
    public EntityInfo Translation { get; private set; }

    /// <summary>
    /// Gets the trigger info.
    /// </summary>
    /// <value>The trigger info.</value>
    public EntityInfo Trigger { get; private set; }

    /// <summary>
    /// Gets the unique constraint info.
    /// </summary>
    /// <value>The unique constraint info.</value>
    public UniqueConstraintInfo UniqueConstraint { get; private set; }

    /// <summary>
    /// Gets the view info.
    /// </summary>
    /// <value>The view info.</value>
    public EntityInfo View { get; private set; }

    /// <summary>
    /// Gets the data types.
    /// </summary>
    /// <value>The data types.</value>
    public DataTypeCollection DataTypes { get; private set; }

    /// <summary>
    /// Gets the string indexing base. Normally is equal to 1.
    /// </summary>
    /// <value>The string indexing base.</value>
    public int StringIndexingBase { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple active result sets are supported.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multiple active result sets are supported; otherwise, <see langword="false"/>.
    /// </value>
    public bool MultipleActiveResultSets { get; private set; }

    /// <summary>
    /// Builds the server info using specified <see cref="ServerInfoProvider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public static ServerInfo Build(ServerInfoProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      var info = new ServerInfo();

      info.Version = provider.GetVersionInfo();
      info.Assertion = provider.GetAssertionInfo();
      info.CharacterSet = provider.GetCharacterSetInfo();
      info.CheckConstraint = provider.GetCheckConstraintInfo();
      info.Collation = provider.GetCollationInfo();
      info.Column = provider.GetColumnInfo();
      info.DataTypes = provider.GetDataTypesInfo();
      info.Database = provider.GetDatabaseInfo();
      info.Domain = provider.GetDomainInfo();
      info.ForeignKey = provider.GetForeignKeyConstraintInfo();
      info.Identity = provider.GetIdentityInfo();
      info.Index = provider.GetIndexInfo();
      info.IsolationLevels = provider.GetIsolationLevels();
      info.PrimaryKey = provider.GetPrimaryKeyInfo();
      info.Query = provider.GetQueryInfo();
      info.Schema = provider.GetSchemaInfo();
      info.Sequence = provider.GetSequenceInfo();
      info.StoredProcedure = provider.GetStoredProcedureInfo();
      info.Table = provider.GetTableInfo();
      info.TemporaryTable = provider.GetTemporaryTableInfo();
      info.Translation = provider.GetTranslationInfo();
      info.Trigger = provider.GetTriggerInfo();
      info.UniqueConstraint = provider.GetUniqueConstraintInfo();
      info.View = provider.GetViewInfo();
      info.StringIndexingBase = provider.GetStringIndexingBase();
      info.MultipleActiveResultSets = provider.GetMultipleActiveResultSets();

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
      TryLockEntityInfo(Assertion, recursive);
      TryLockEntityInfo(CharacterSet, recursive);
      TryLockEntityInfo(CheckConstraint, recursive);
      TryLockEntityInfo(Collation, recursive);
      TryLockEntityInfo(Column, recursive);
      TryLockEntityInfo(DataTypes, recursive);
      TryLockEntityInfo(Database, recursive);
      TryLockEntityInfo(Domain, recursive);
      TryLockEntityInfo(ForeignKey, recursive);
      TryLockEntityInfo(Identity, recursive);
      TryLockEntityInfo(Index, recursive);
      TryLockEntityInfo(PrimaryKey, recursive);
      TryLockEntityInfo(Query, recursive);
      TryLockEntityInfo(Schema, recursive);
      TryLockEntityInfo(Sequence, recursive);
      TryLockEntityInfo(StoredProcedure, recursive);
      TryLockEntityInfo(Table, recursive);
      TryLockEntityInfo(TemporaryTable, recursive);
      TryLockEntityInfo(Translation, recursive);
      TryLockEntityInfo(Trigger, recursive);
      TryLockEntityInfo(UniqueConstraint, recursive);
      TryLockEntityInfo(View, recursive);
    }

    private static void TryLockEntityInfo(ILockable info, bool recursive)
    {
      if (info!=null)
        info.Lock(recursive);
    }
  }
}