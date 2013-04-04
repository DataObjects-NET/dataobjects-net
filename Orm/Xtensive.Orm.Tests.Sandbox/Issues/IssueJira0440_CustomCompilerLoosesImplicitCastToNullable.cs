using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0440_CustomCompilerLoosesImplicitCastToNullableModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0440_CustomCompilerLoosesImplicitCastToNullableModel
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
        [Key, Field]
        public Guid Id { get; private set; }

        [Field]
        public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
        [Key, Field]
        public Guid Id { get; private set; }

        public DateTime? VirtualDateTime { get; set; }

        [Field]
        public TestEntity1 LinkWithDate { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0440_CustomCompilerLoosesImplicitCastToNullable : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity1));
      configuration.Types.Register(typeof (TestEntity2));
      Expression<Func<TestEntity2, DateTime?>> substitution = e => e.LinkWithDate.DateField;
      configuration.LinqExtensions.Register(typeof (TestEntity2).GetProperty("VirtualDateTime"), substitution);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<TestEntity2>()
          .Where(e => e.VirtualDateTime.Value.Date==DateTime.Now.Date).ToList();
        tx.Complete();
      }
    }
  }
}