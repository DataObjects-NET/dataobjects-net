using Xtensive.Orm.Model;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.BulkOperations
{
  internal sealed class PrimaryIndexMapping
  {
    public IndexInfo PrimaryIndex { get; private set; }

    public Table Table { get; private set; }

    public PrimaryIndexMapping(IndexInfo index, Table table)
    {
      PrimaryIndex = index;
      Table = table;
    }
  }
}