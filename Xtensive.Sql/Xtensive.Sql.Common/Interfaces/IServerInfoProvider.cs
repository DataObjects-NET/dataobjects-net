// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes <see cref="ServerInfo"/> provider interface.
  /// </summary>
  public interface IServerInfoProvider
  {
    /// <summary>
    /// Gets the collation info.
    /// </summary>
    /// <returns></returns>
    EntityInfo CollationInfo { get; }

    /// <summary>
    /// Gets the character set info.
    /// </summary>
    /// <returns></returns>
    EntityInfo CharacterSetInfo { get; }

    /// <summary>
    /// Gets the translation info.
    /// </summary>
    /// <returns></returns>
    EntityInfo TranslationInfo { get; }

    /// <summary>
    /// Gets the trigger info.
    /// </summary>
    /// <returns></returns>
    EntityInfo TriggerInfo { get; }

    /// <summary>
    /// Gets the stored procedure info.
    /// </summary>
    /// <returns></returns>
    EntityInfo StoredProcedureInfo { get; }

    /// <summary>
    /// Gets the sequence info.
    /// </summary>
    /// <returns></returns>
    SequenceInfo SequenceInfo { get; }

    /// <summary>
    /// Gets the database info.
    /// </summary>
    /// <returns></returns>
    EntityInfo DatabaseInfo { get; }

    /// <summary>
    /// Gets the column info.
    /// </summary>
    /// <returns></returns>
    ColumnInfo ColumnInfo { get; }

    /// <summary>
    /// Gets the view info.
    /// </summary>
    /// <returns></returns>
    EntityInfo ViewInfo { get; }

    /// <summary>
    /// Gets the schema info.
    /// </summary>
    /// <returns></returns>
    EntityInfo SchemaInfo { get; }

    /// <summary>
    /// Gets the table info.
    /// </summary>
    /// <returns></returns>
    TableInfo TableInfo { get; }

    /// <summary>
    /// Gets the temporary table info.
    /// </summary>
    /// <returns></returns>
    TemporaryTableInfo TemporaryTableInfo { get; }

    /// <summary>
    /// Gets the check constraint info.
    /// </summary>
    /// <returns></returns>
    CheckConstraintInfo CheckConstraintInfo { get; }

    /// <summary>
    /// Gets the primary key info.
    /// </summary>
    /// <returns></returns>
    ConstraintInfo PrimaryKeyInfo { get; }

    /// <summary>
    /// Gets the unique constraint info.
    /// </summary>
    /// <returns></returns>
    ConstraintInfo UniqueConstraintInfo { get; }

    /// <summary>
    /// Gets the index info.
    /// </summary>
    /// <returns></returns>
    IndexInfo IndexInfo { get; }

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    /// <returns></returns>
    ReferenceConstraintInfo ReferentialConstraintInfo { get; }

    /// <summary>
    /// Gets the query info.
    /// </summary>
    /// <returns></returns>
    QueryInfo QueryInfo { get; }

    /// <summary>
    /// Gets the identity info.
    /// </summary>
    /// <returns></returns>
    IdentityInfo IdentityInfo { get; }

    /// <summary>
    /// Gets the collection of supported data types.
    /// </summary>
    /// <returns></returns>
    DataTypeCollection DataTypesInfo { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    /// <returns></returns>
    VersionInfo VersionInfo { get; }

    /// <summary>
    /// Gets the supported server entities.
    /// </summary>
    /// <returns></returns>
    ServerEntities Entities { get; }

    /// <summary>
    /// Gets the supported isolation levels.
    /// </summary>
    /// <returns></returns>
    IsolationLevels IsolationLevels { get; }

    /// <summary>
    /// Gets the domain info.
    /// </summary>
    /// <returns></returns>
    EntityInfo DomainInfo { get; }

    /// <summary>
    /// Gets the assertion info.
    /// </summary>
    /// <returns></returns>
    ConstraintInfo AssertionInfo { get; }

    /// <summary>
    /// Gets the string indexing base.
    /// </summary>
    /// <returns></returns>
    int StringIndexingBase { get; }
  }
}