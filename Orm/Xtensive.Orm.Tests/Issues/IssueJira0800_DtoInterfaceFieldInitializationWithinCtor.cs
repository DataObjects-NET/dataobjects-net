// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Issues.IssueJira0800_InterfaceImplementingDtoProblemModel;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0800_DtoInterfaceFieldInitializationWithinCtor : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Example));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    [Test]
    public void FilterThroughInterfaceMethod()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query
          .All<Example>()
          .Select(x => new ExampleDto { EndDate = x.EndDate })
          .ApplyFilterUsingInterface();
        var expression = query.Expression;
        Console.WriteLine(expression);
        _ = query.ToArray();
      }
    }

    [Test]
    public void FilterThroughBaseClassMethod()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query
          .All<Example>()
          .Select(x => new ExampleDto { EndDate = x.EndDate })
          .ApplyFilterUsingBaseClass();
        var expression = query.Expression;
        Console.WriteLine(expression);
        _ = query.ToArray();
      }
    }

    [Test]
    public void FilterDirectlyWithInterface()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query
          .All<Example>()
          .Select(x => (IEndOwner) new ExampleDto { EndDate = x.EndDate })
          .Where(x => x.EndDate == null);
        var expression = query.Expression;
        Console.WriteLine(expression);
        _ = query.ToArray();
      }
    }

    [Test]
    public void FilterDirectlyWithExactType()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query
          .All<Example>()
          .Select(x => new ExampleDto { EndDate = x.EndDate })
          .Where(x => x.EndDate == null);
        var expression = query.Expression;
        Console.WriteLine(expression);
        _ = query.ToArray();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0800_InterfaceImplementingDtoProblemModel
{
  public class ExampleDto : EndOwner
  {
    public long Id { get; set; }

    public override DateTime? EndDate { get; set; }
  }

  [HierarchyRoot]
  public class Example : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public DateTime? EndDate { get; set; }
  }

  public static class ContractsExtensions
  {
    public static IQueryable<TContractOwner> ApplyFilterUsingBaseClass<TContractOwner>(this IQueryable<TContractOwner> query)
      where TContractOwner : EndOwner
    {
      return query.Where(x => x.EndDate == null);
    }

    public static IQueryable<TContractOwner> ApplyFilterUsingInterface<TContractOwner>(this IQueryable<TContractOwner> query)
      where TContractOwner : class, IEndOwner
    {
      return query.Where(x => x.EndDate == null);
    }
  }

  public abstract class EndOwner : IEndOwner
  {
    public abstract DateTime? EndDate { get; set; }
  }


  public interface IEndOwner
  {
    DateTime? EndDate { get; }
  }
}
