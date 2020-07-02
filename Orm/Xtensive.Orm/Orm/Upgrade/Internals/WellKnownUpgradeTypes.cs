using System;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Upgrade
{
  public static class WellKnownUpgradeTypes
  {
    public static readonly Type TableInfo = typeof(TableInfo);
    public static readonly Type StorageColumnInfo = typeof(StorageColumnInfo);
    public static readonly Type PrimaryIndexInfo = typeof(PrimaryIndexInfo);
    public static readonly Type SecondaryIndexInfo = typeof(SecondaryIndexInfo);
    public static readonly Type ForeignKeyInfo = typeof(ForeignKeyInfo);
    public static readonly Type StorageSequenceInfo = typeof(StorageSequenceInfo);
    public static readonly Type StorageFullTextIndexInfo = typeof(StorageFullTextIndexInfo);
  }
}