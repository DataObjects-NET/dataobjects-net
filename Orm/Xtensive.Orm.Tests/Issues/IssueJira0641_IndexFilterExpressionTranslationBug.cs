// Copyright (C) 2016-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.08.09

using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.Issues.IssueJira0641_IndexFilterExpressionTranslationBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0641_IndexFilterExpressionTranslationBugModel
{
  [HierarchyRoot]
  [Index("Text", Filter = "NotBilledCalls")]
  public class BandwidthCall : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 250)]
    public string Text { get; set; }

    [Field]
    public bool IsBilled { get; set; }

    [Field]
    public DirectionEnum Direction { get; set; }

#pragma warning disable IDE0051 // Remove unused private members
    private static Expression<Func<BandwidthCall, bool>> NotBilledCalls()
#pragma warning restore IDE0051 // Remove unused private members
    {
      return e => e.IsBilled == false && (e.Direction).In(DirectionEnum.FirstIn, DirectionEnum.FirstOut);
    }
  }

  [HierarchyRoot]
  public class IndexlessBandwidthCall : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public bool IsBilled { get; set; }

    [Field]
    public DirectionEnum Direction { get; set; }
  }

  public enum DirectionEnum
  {
    FirstIn,
    FirstOut
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public sealed class IssueJira0641_IndexFilterExpressionTranslationBug
  {
    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (BandwidthCall).Assembly, typeof (BandwidthCall).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      if (domain!=null)
        domain.Dispose();
    }

    [Test]
    public void InOperationInQueryTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (IndexlessBandwidthCall));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => session.Query.All<IndexlessBandwidthCall>().Where(e => e.Direction.In(DirectionEnum.FirstIn, DirectionEnum.FirstOut)).Run());
      }
    }
  }
}
