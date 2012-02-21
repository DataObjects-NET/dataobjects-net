using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Issues.Issue0860_DateTimeDateModel;
using Xtensive.Testing;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0860_DateTimeDateModel
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

  public class Issue0860_DateTimeDate : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Document).Assembly, typeof (Document).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var random = RandomManager.CreateRandom();
          var randomDate = InstanceGeneratorProvider.Default.GetInstanceGenerator<DateTime>().GetInstance(random);
          var date = randomDate.Date;
          var document = new Document {DateTime1 = randomDate, DateTime2 = date};
          session.SaveChanges();

          var query =
            from d in Query.All<Document>()
            where d.DateTime1.Date==d.DateTime2
            select d;
          var result = query.ToList();
          Assert.AreEqual(1, result.Count);
        }
      }
    }
  }
}