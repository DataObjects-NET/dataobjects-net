using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Testing;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using System.Linq;

namespace Issue0860_DateTimeDate_Model
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public DateTime DateTime1 { get; set; }

    [Field]
    public DateTime DateTime2 { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0860_DateTimeDate : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Issue0860_DateTimeDate_Model.Document).Assembly, typeof (Issue0860_DateTimeDate_Model.Document).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transaction = Transaction.Open()) {
          var random = RandomManager.CreateRandom();
          var randomDate = InstanceGeneratorProvider.Default.GetInstanceGenerator<DateTime>().GetInstance(random);
          var date = randomDate.Date;
          var document = new Issue0860_DateTimeDate_Model.Document {DateTime1 = randomDate, DateTime2 = date};
          session.Persist();

          var query =
            from d in Query.All<Issue0860_DateTimeDate_Model.Document>()
            where d.DateTime1.Date==d.DateTime2
            select d;
          var result = query.ToList();
          Assert.AreEqual(1, result.Count);
        }
      }
    }
  }
}