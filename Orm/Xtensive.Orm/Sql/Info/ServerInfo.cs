// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Represents a set of information concerning RDBMS capabilities.
  /// </summary>
  public class ServerInfo : LockableBase
  {
    /// <summary>
    /// Gets the server supported isolation levels.
    /// </summary>
    public IsolationLevels IsolationLevels { get; private set; }

    /// <summary>
    /// Gets the server supported isolation levels.
    /// </summary>
    public FullTextSearchInfo FullTextSearch { get; private set; }

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
    public DataTypeCollection DataTypes { get; private set; }

    /// <summary>
    /// Gets the server features.
    /// </summary>
    public ServerFeatures ServerFeatures { get; private set; }

    /// <summary>
    /// Gets the string indexing base. Normally is equal to 1.
    /// </summary>
    /// <value>The string indexing base.</value>
    public int StringIndexingBase { get; private set; }
    
    /// <summary>
    /// Builds the server info using specified <see cref="ServerInfoProvider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public static ServerInfo Build(ServerInfoProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      var info = new ServerInfo {
        Assertion = provider.GetAssertionInfo(), 
        CharacterSet = provider.GetCharacterSetInfo(), 
        CheckConstraint = provider.GetCheckConstraintInfo(), 
        Collation = provider.GetCollationInfo(), 
        Column = provider.GetColumnInfo(), 
        DataTypes = provider.GetDataTypesInfo(), 
        Database = provider.GetDatabaseInfo(), 
        Domain = provider.GetDomainInfo(), 
        ForeignKey = provider.GetForeignKeyConstraintInfo(), 
        FullTextSearch = provider.GetFullTextInfo(), 
        Identity = provider.GetIdentityInfo(), 
        Index = provider.GetIndexInfo(), 
        IsolationLevels = provider.GetIsolationLevels(), 
        PrimaryKey = provider.GetPrimaryKeyInfo(), 
        Query = provider.GetQueryInfo(), 
        Schema = provider.GetSchemaInfo(), 
        Sequence = provider.GetSequenceInfo(), 
        StoredProcedure = provider.GetStoredProcedureInfo(), 
        Table = provider.GetTableInfo(), 
        TemporaryTable = provider.GetTemporaryTableInfo(), 
        Translation = provider.GetTranslationInfo(), 
        Trigger = provider.GetTriggerInfo(), 
        UniqueConstraint = provider.GetUniqueConstraintInfo(), 
        View = provider.GetViewInfo(), 
        ServerFeatures = provider.GetServerFeatures(),
        StringIndexingBase = provider.GetStringIndexingBase(), 
      };

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