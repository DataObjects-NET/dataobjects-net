using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0440_CustomCompilerLoosesImplicitCastToNullableModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace TranslationIssue_IndexOutOfRangeModel
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public decimal Split { get; set; }
    }

  }

  [TestFixture]
  public class TranslationIssue_IndexOutOfRange : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TranslationIssue_IndexOutOfRangeModel.TestEntity1));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();

      _ = (
        from e in session.Query.All<TranslationIssue_IndexOutOfRangeModel.TestEntity1>()
        group new {
          Split = e.Split * 0.01M
        } by e.Id into g
        select g
            .Select(x => x.Split)
            .Distinct()
            .Sum()
        ).ToList();
    }
  }
}
