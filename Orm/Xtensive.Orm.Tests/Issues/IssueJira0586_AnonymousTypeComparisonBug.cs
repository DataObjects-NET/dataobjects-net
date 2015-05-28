// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.05.28

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0586_AnonymousTypeComparisonBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0586_AnonymousTypeComparisonBugModel
{
  public abstract class EntityBase : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    protected EntityBase(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class TpPriceCalc : EntityBase
  {
    [Field]
    public bool Check { get; set; }

    [Field]
    public Account Account { get; set; }

    [Field]
    public FinToolKind Owner { get; set; }

    public TpPriceCalc(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class FinToolKind : EntityBase
  {
    public FinToolKind(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class PacioliPosting : EntityBase
  {
    [Field]
    public Account CreditAccount { get; set; }

    [Field]
    public FinToolBase FinCredit { get; set; }

    public PacioliPosting(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class FinToolBase : EntityBase
  {
    [Field]
    public FinToolKind FinToolKind { get; set; }

    public FinToolBase(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class Account : EntityBase
  {
    public Account(Guid id)
      : base(id)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0586_AnonymousTypeComparisonBug : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var tableParts = from q in Query.All<TpPriceCalc>()
          select new {
                       q.Account,
                       Check = !q.Check,
                       FinToolKind = q.Check
                         ? null
                         : q.Owner
                     };

        var masterCredit = Query.All<PacioliPosting>();

        var join = from r in masterCredit.LeftJoin(
          tableParts,
          a => a.CreditAccount.Id,
          a => a.Account.Id,
          (a, pm) => new {pp = a, pm})
          let q = r.pp
          select new {
                       Id = q.Id,
                       MasterFinToolKind = r.pm==null
                         ? (FinToolKind) null
                         : (!r.pm.Check ? r.pm.FinToolKind : q.FinCredit.FinToolKind)
                     };

        var result = join.ToArray();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(EntityBase).Assembly, typeof(EntityBase).Namespace);
      return configuration;
    }
  }
}
