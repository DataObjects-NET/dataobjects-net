// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.05.21

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0584_IncorrectMappingOfColumnInQueryModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0584_IncorrectMappingOfColumnInQueryModel
{
  public enum PacioliAccountType
  {
    Passive,
    Active,
    ActivePassive
  }

  public abstract class EntityBase : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    protected EntityBase(Guid id)
    {
      Id = id;
    }
  }

  public abstract class TablePartBase : EntityBase
  {
    [Field]
    public FinToolKind Owner { get; private set; }

    protected TablePartBase(Guid id, FinToolKind owner)
      : base(id)
    {
      Owner = owner;
    }

    [HierarchyRoot]
    public class FinToolKind : EntityBase
    {
      [Field(Nullable = false, Length = 100)]
      public string Name { get; set; }

      [Field(Nullable = false, Length = 100)]
      public string FullName { get; set; }

      [Field]
      public FinToolKind Parent { get; set; }

      [Field]
      public string Entity { get; set; }

      [Field]
      public bool Quantitative { get; set; }

      [Field]
      [Association(PairTo = "Owner", OnTargetRemove = OnRemoveAction.Clear, OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<TpPriceCalc> PriceCalc { get; private set; }

      public FinToolKind(Guid id)
        : base(id)
      {
      }

      [HierarchyRoot]
      public class TpPriceCalc : TablePartBase
      {
        [Field(Nullable = false)]
        public PacioliAccount Account { get; set; }

        [Field]
        public bool OnlyCurrentType { get; set; }

        public TpPriceCalc(Guid id, FinToolKind owner)
          : base(id, owner)
        {
        }
      }
    }
  }

  [HierarchyRoot]
  public abstract class FinToolBase : EntityBase
  {
    [Field(Length = 300, Nullable = false)]
    public virtual string Name { get; set; }

    [Field(Length = 50, Nullable = false)]
    public string DocumentIdentifier { get; set; }

    [Field(Nullable = false)]
    public TablePartBase.FinToolKind FinToolKind { get; set; }

    [Field(Nullable = false)]
    public virtual Currency Cur { get; set; }

    public FinToolBase(Guid id)
      : base(id)
    {
    }
  }

  public class OtherFinTools : FinToolBase
  {
    public OtherFinTools(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class Status : EntityBase
  {
    [Field]
    public string Description { get; set; }

    [Field]
    public string Name { get; set; }

    public Status(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class Currency : EntityBase
  {
    [Field(Length = 20)]
    public string CurCode { get; set; }

    [Field(Length = 3)]
    public string DigitCode { get; set; }

    [Field]
    public int? MinorUnits { get; set; }

    [Field(Length = 400, Nullable = false)]
    public string Name { get; set; }

    [Field(Length = 400)]
    public string Description { get; set; }

    [Field(Length = 20)]
    public string Sign { get; set; }

    [Field(Length = 400)]
    public string EngName { get; set; }

    public Currency(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public abstract class Subject : EntityBase
  {
    [Field(Length = 8)]
    public int? Code { get; set; }

    [Field(Nullable = false, Length = 255)]
    public string Name { get; set; }

    [Field(Nullable = false, Length = 400)]
    public string FullName { get; set; }

    [Field(Nullable = false, Length = 20)]
    public virtual string Identifier { get; set; }

    public Subject(Guid id)
      : base(id)
    {
    }
  }

  public abstract class Portfolio : Subject
  {
    [Field(Length = 400)]
    public string LegalName { get; set; }

    [Field]
    public DateTime? DateOnService { get; set; }

    [Field]
    public DateTime? DateRemovalFromService { get; set; }

    [Field]
    public DateTime? EisTransitionDate { get; set; }

    [Field]
    public DateTime? ImportedToDate { get; set; }

    protected Portfolio(Guid id)
      : base(id)
    {
    }
  }

  public abstract class DocumentBase : EntityBase
  {
    [Field]
    public Status Status { get; set; }

    public DocumentBase(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public abstract class OperationBase : DocumentBase
  {
    [Field(Length = 50)]
    public string Number { get; set; }

    [Field]
    public DateTime? InputDate { get; set; }

    [Field]
    public DateTime? AuthorizationDate { get; set; }

    [Field]
    public OperationType OperationType { get; set; }

    [Field]
    public Portfolio Portfolio { get; set; }

    [Field(Length = 1024)]
    public string Comment { get; set; }

    protected OperationBase(Guid id)
      : base(id)
    {
    }
  }

  public class TechOperation : OperationBase
  {
    [Field]
    public string ForeignId { get; set; }

    public TechOperation(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class OperationType : EntityBase
  {
    [Field(Length = 255, Nullable = false)]
    public string Name { get; set; }

    [Field(Length = 255)]
    public string Description { get; set; }

    [Field]
    public PacioliAccount Account { get; set; }

    public OperationType(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class PacioliAccount : EntityBase
  {
    [Field, Key]
    public Guid Id { get; set; }

    [Field]
    public PacioliAccount Parent { get; set; }

    [Field(Length = 128, Nullable = false)]
    public string Name { get; set; }

    [Field(Length = 4000)]
    public string Description { get; set; }

    [Field(Length = 32, Nullable = false)]
    public string Code { get; set; }

    [Field(Length = 32, Nullable = false)]
    public string FastAccess { get; set; }

    [Field]
    public PacioliAccountType AccountType { get; set; }

    public PacioliAccount(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class PacioliPosting : DocumentBase
  {
    [Field]
    public FinToolBase FinDebit { get; set; }

    [Field]
    public FinToolBase FinCredit { get; set; }

    [Field(Nullable = false)]
    public Portfolio BalanceHolder { get; set; }

    [Field]
    public string Name { get; set; }

    [Field(Nullable = false)]
    public OperationBase Operation { get; set; }

    [Field]
    public string ForeignId { get; set; }

    [Field(Length = 12)]
    public string ForeignIdHash { get; set; }

    [Field]
    public PacioliAccount DebitAccount { get; set; }

    [Field]
    public PacioliAccount CreditAccount { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public DateTime? ExecutionDate { get; set; }

    [Field]
    public decimal Sum { get; set; }

    [Field]
    public Currency Currency { get; set; }

    public PacioliPosting(Guid id)
      : base(id)
    {
    }
  }

  public class CustomPosting
  {
    public Guid Id { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ExecutionDate { get; set; }

    public Portfolio BalanceHolder { get; set; }

    public OperationBase Operation { get; set; }

    public OperationType OperationType { get; set; }

    public decimal Sum { get; set; }

    public Currency Currency { get; set; }

    public Status Status { get; set; }

    public PacioliAccount MasterAccount { get; set; }

    public PacioliAccount SlaveAccount { get; set; }

    public FinToolBase MasterFinToolBase { get; set; }

    public FinToolBase SlaveFinToolBase { get; set; }

    public TablePartBase.FinToolKind MasterFinToolKind { get; set; }

    public TablePartBase.FinToolKind SlaveFinToolKind { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0584_IncorrectMappingOfColumnInQuery : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var tp = from q in Query.All<TablePartBase.FinToolKind.TpPriceCalc>() where !q.OnlyCurrentType select q;

        var masterDebit = from q in
                            Query.All<PacioliPosting>()
                          select
                             new {
                               Id = q.Id,
                               Status = q.Status,
                               BalanceHolder = q.BalanceHolder,
                               CreationDate = q.CreationDate,
                               Currency = q.Currency,
                               ExecutionDate = q.ExecutionDate,
                               Operation = q.Operation,
                               OperationType = q.Operation.OperationType,
                               Sum = q.DebitAccount.AccountType==PacioliAccountType.Passive ? -q.Sum : q.Sum,
                               MasterAccount = q.DebitAccount,
                               MasterFinToolBase = q.FinDebit,
                               SlaveAccount = q.CreditAccount,
                               SlaveFinToolBase = q.FinCredit
                             };

        var masterCredit = from q in Query.All<PacioliPosting>()
                           select
                              new {
                                Id = q.Id,
                                Status = q.Status,
                                BalanceHolder = q.BalanceHolder,
                                CreationDate = q.CreationDate,
                                Currency = q.Currency,
                                ExecutionDate = q.ExecutionDate,
                                Operation = q.Operation,
                                OperationType = q.Operation.OperationType,
                                Sum =
                                   q.CreditAccount.AccountType==PacioliAccountType.Active
                                   || q.CreditAccount.AccountType==PacioliAccountType.ActivePassive
                                      ? -q.Sum
                                      : q.Sum,
                                MasterAccount = q.CreditAccount,
                                MasterFinToolBase = q.FinCredit,
                                SlaveAccount = q.DebitAccount,
                                SlaveFinToolBase = q.FinDebit
                              };

        var preResult = masterCredit.Concat(masterDebit);
        var result = from r in preResult
                         .LeftJoin(tp, a => a.SlaveAccount, a => a.Account, (pp, ps) => new { pp, ps })
                         .LeftJoin(tp, a => a.pp.MasterAccount, a => a.Account, (a, pm) => new { a.pp, a.ps, pm })
                     let q = r.pp
                     select new CustomPosting() {
                       Id = q.Id,
                       Status = q.Status,
                       BalanceHolder = q.BalanceHolder,
                       CreationDate = q.CreationDate,
                       Currency = q.Currency,
                       ExecutionDate = q.ExecutionDate,
                       Operation = q.Operation,
                       OperationType = q.Operation.OperationType,
                       Sum = q.Sum,
                       MasterAccount = q.MasterAccount,
                       MasterFinToolBase = q.MasterFinToolBase,
                       SlaveAccount = q.SlaveAccount,
                       SlaveFinToolBase = q.SlaveFinToolBase,
                       MasterFinToolKind = r.pm!=null ? r.pm.Owner : q.MasterFinToolBase.FinToolKind,
                       SlaveFinToolKind = r.pm!=null ? r.ps.Owner : q.SlaveFinToolBase.FinToolKind,
                     };


        
        var id = Guid.NewGuid();

        Assert.DoesNotThrow(()=>result.Where(a => a.MasterAccount.Id == id).LongCount());
        Assert.DoesNotThrow(()=>result.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(()=>result.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(()=>result.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());

        // И только вот это всё валит
        Assert.DoesNotThrow(()=>result.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(CustomPosting).Assembly, typeof(CustomPosting).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
