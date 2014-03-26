// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes <see cref="ServerInfo"/> provider interface.
  /// </summary>
  public abstract class ServerInfoProvider
  {
    /// <summary>
    /// Gets the driver.
    /// </summary>
    public SqlDriver Driver { get; private set; }

    /// <summary>
    /// Gets the collation info.
    /// </summary>
    public abstract EntityInfo GetCollationInfo();

    /// <summary>
    /// Gets the character set info.
    /// </summary>
    public abstract EntityInfo GetCharacterSetInfo();

    /// <summary>
    /// Gets the translation info.
    /// </summary>
    public abstract EntityInfo GetTranslationInfo();

    /// <summary>
    /// Gets the trigger info.
    /// </summary>
    public abstract EntityInfo GetTriggerInfo();

    /// <summary>
    /// Gets the stored procedure info.
    /// </summary>
    public abstract EntityInfo GetStoredProcedureInfo();

    /// <summary>
    /// Gets the sequence info.
    /// </summary>
    public abstract SequenceInfo GetSequenceInfo();

    /// <summary>
    /// Gets the database info.
    /// </summary>
    public abstract EntityInfo GetDatabaseInfo();

    /// <summary>
    /// Gets the column info.
    /// </summary>
    public abstract ColumnInfo GetColumnInfo();

    /// <summary>
    /// Gets the view info.
    /// </summary>
    public abstract EntityInfo GetViewInfo();

    /// <summary>
    /// Gets the schema info.
    /// </summary>
    public abstract EntityInfo GetSchemaInfo();

    /// <summary>
    /// Gets the table info.
    /// </summary>
    public abstract TableInfo GetTableInfo();

    /// <summary>
    /// Gets the temporary table info.
    /// </summary>
    public abstract TemporaryTableInfo GetTemporaryTableInfo();

    /// <summary>
    /// Gets the check constraint info.
    /// </summary>
    public abstract CheckConstraintInfo GetCheckConstraintInfo();

    /// <summary>
    /// Gets the unique constraint info.
    /// </summary>
    public abstract UniqueConstraintInfo GetUniqueConstraintInfo();
    
    /// <summary>
    /// Gets the primary key info.
    /// </summary>
    public abstract PrimaryKeyConstraintInfo GetPrimaryKeyInfo();

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    public abstract FullTextSearchInfo GetFullTextInfo();

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    public abstract ForeignKeyConstraintInfo GetForeignKeyConstraintInfo();
    
    /// <summary>
    /// Gets the index info.
    /// </summary>
    public abstract IndexInfo GetIndexInfo();

    /// <summary>
    /// Gets the query info.
    /// </summary>
    public abstract QueryInfo GetQueryInfo();

    /// <summary>
    /// Gets the identity info.
    /// </summary>
    public abstract IdentityInfo GetIdentityInfo();

    /// <summary>
    /// Gets the collection of supported data types.
    /// </summary>
    public virtual DataTypeCollection GetDataTypesInfo()
    {
      var types = new DataTypeCollection();

      types.RegisterType(SqlType.Boolean, typeof (bool));
      types.RegisterType(SqlType.Int8, typeof (sbyte));
      types.RegisterType(SqlType.UInt8, typeof (byte));
      types.RegisterType(SqlType.Int16, typeof (short));
      types.RegisterType(SqlType.UInt16, typeof (ushort));
      types.RegisterType(SqlType.Int32, typeof (int));
      types.RegisterType(SqlType.UInt32, typeof (uint));
      types.RegisterType(SqlType.Int64, typeof (long));
      types.RegisterType(SqlType.UInt64, typeof (ulong));
      types.RegisterType(SqlType.Decimal, typeof (decimal));
      types.RegisterType(SqlType.Float, typeof (float));
      types.RegisterType(SqlType.Double, typeof (double));
      types.RegisterType(SqlType.DateTime, typeof (DateTime));
      types.RegisterType(SqlType.DateTimeOffset, typeof (DateTimeOffset));
      types.RegisterType(SqlType.Interval, typeof (TimeSpan));
      types.RegisterType(SqlType.Char, typeof (string));
      types.RegisterType(SqlType.VarChar, typeof (string));
      types.RegisterType(SqlType.VarCharMax, typeof (string));
      types.RegisterType(SqlType.Binary, typeof (byte[]));
      types.RegisterType(SqlType.VarBinary, typeof (byte[]));
      types.RegisterType(SqlType.VarBinaryMax, typeof (byte[]));
      types.RegisterType(SqlType.Guid, typeof (Guid));

      return types;
    }

    /// <summary>
    /// Gets the supported isolation levels.
    /// </summary>
    public abstract IsolationLevels GetIsolationLevels();

    /// <summary>
    /// Gets the domain info.
    /// </summary>
    public abstract EntityInfo GetDomainInfo();

    /// <summary>
    /// Gets the assertion info.
    /// </summary>
    public abstract AssertConstraintInfo GetAssertionInfo();

    /// <summary>
    /// Gets the server features.
    /// </summary>
    public abstract ServerFeatures GetServerFeatures();

    /// <summary>
    /// Gets the string indexing base.
    /// </summary>
    public abstract int GetStringIndexingBase();
    
    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected ServerInfoProvider(SqlDriver driver)
    {
      Driver = driver;
    }
  }
}