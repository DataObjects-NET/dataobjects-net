using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.BulkOperations.Tests.Issues.WrongAliassesIssue;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.BulkOperations.Tests.Issues
{
  public class JoinedTableAsSourceForOperationsCauseWrongAliases : BulkOperationBaseTest
  {
    protected override void CheckRequirements() => Require.ProviderIsNot(StorageProvider.MySql);

    [Test]
    public void CustomerCase()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<DowntimeReason>()
          .Where(r => r.DowntimeInfo.Record.Equipment.Id == 333);

        var queryResult = query.Delete();
      }
    }

    [Test]
    public void MultipleKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<DowntimeReason2>()
         .Where(r => r.DowntimeInfo.Record.Equipment.Id == 333);

        var queryResult = query.Delete();
      }
    }
  }
}

namespace Xtensive.Orm.BulkOperations.Tests.Issues.WrongAliassesIssue
{
  [HierarchyRoot]
  public class DowntimeReason : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DowntimeInfo DowntimeInfo { get; set; }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class DowntimeReason2 : Entity
  {
    [Field, Key(0)]
    public int Id { get; private set; }

    [Field, Key(1)]
    public int Id2 { get; private set; }

    [Field]
    public DowntimeInfo DowntimeInfo { get; set; }
  }

  [HierarchyRoot]
  public class DowntimeInfo : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Record Record { get; set; }
  }

  [HierarchyRoot]
  public class Record : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Equipment Equipment { get; set; }
  }

  [HierarchyRoot]
  public class Equipment : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}