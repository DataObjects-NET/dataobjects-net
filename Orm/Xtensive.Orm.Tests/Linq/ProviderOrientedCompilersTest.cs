using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Linq.MemberCompilation;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class ProviderOrientedCompilersTest : AutoBuildTest
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Field, Key(0)]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }
    }

    public static class CustomCompilerStringExtensions
    {
      public static char GetThirdChar(string source)
      {
        return source[2];
      }
    }

    [CompilerContainer(typeof(SqlExpression))]
    public static class CustomStringCompilerContainer
    {
      [Compiler(typeof(CustomCompilerStringExtensions), "GetThirdChar", TargetKind.Method | TargetKind.Static)]
      public static SqlExpression GetThirdChar(SqlExpression _this)
      {
        Assert.IsNotNull(CompilerContainerInfo.Current.ProviderInfo);
        return SqlDml.Substring(_this, 2, 1);
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(TestEntity));
      config.Types.Register(typeof(CustomStringCompilerContainer));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var testEntity = new TestEntity()
        {
          Text = "Hello World!"
        };
        Assert.IsNull(CompilerContainerInfo.Current);

        var query = session.Query.All<TestEntity>().Where(x => CustomCompilerStringExtensions.GetThirdChar(x.Text) == 'l');
        Assert.IsNull(CompilerContainerInfo.Current);

        Assert.IsNotEmpty(query.ToArray());
        Assert.IsNull(CompilerContainerInfo.Current);
        //transaction.Complete();
      }
    }
  }
}
