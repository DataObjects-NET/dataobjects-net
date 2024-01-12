// Copyright (C) 2015-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2015.05.21

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0584_IncorrectMappingOfColumnInQueryModel;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Issues.IssueJira0584_IncorrectMappingOfColumnInQueryModel
{
  public enum PacioliAccountType
  {
    Passive,
    Active,
    ActivePassive
  }

  public enum DealType
  {
    RoundsPlannedDeal,
    LeftOffPlannedDeal,
    RealDeal,
  }

  public enum DebitCredit
  {
    Debit,
    Credit,
  }

  [KeyGenerator(KeyGeneratorKind.None)]
  public abstract class EntityBase : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    protected EntityBase(Guid id)
      : base(id)
    {
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

  public class RealPortfolio : Portfolio
  {
    public RealPortfolio(Guid id)
      :base(id)
    {}
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
    public new Guid Id { get; set; }

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

    [Field(Indexed = false)]
    public FinToolBase DebitFinTool { get; set; }

    [Field(Indexed = false)]
    public FinToolBase CreditFinTool { get; set; }

    [Field]
    public DealType Deal { get; set; }

    public PacioliPosting(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public class RegTransaction : Entity
  {
    [Field, Key]
    public Guid Id { get; set; }

    [Field]
    public DateTime? AuthorizeDate { get; set; }

    [Field]
    public DateTime? OperDate { get; set; }

    [Field]
    public Portfolio Portfolio { get; set; }

    [Field]
    public TablePartBase.FinToolKind FinTool { get; set; }

    [Field]
    public TransactionMethod Method { get; set; }

    [Field]
    public decimal Qty { get; set; }

    [Field]
    public decimal Price { get; set; }
  }

  public enum TransactionMethod
  {
    Debit
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

  public static class GuidModifier
  {
    public static Guid ReplaceOne(Guid source, string replacement)
    {
      return Guid.Parse(source.ToString().Substring(0, 35) + replacement);
    }
  }

  [CompilerContainer(typeof(SqlExpression))]
  public class GuidCompilers
  {
    [Compiler(typeof(GuidModifier), "ReplaceOne", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression CompileReplaceOne(SqlExpression sourceSqlExpression, SqlExpression replacement)
    {
      var stringId = SqlDml.Cast(sourceSqlExpression, SqlType.VarCharMax);
      var substringId = SqlDml.Substring(stringId, 0, 35);
      return SqlDml.Cast(SqlDml.Concat(substringId, replacement), SqlType.Guid);
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0584_IncorrectMappingOfColumnInQuery : AutoBuildTest
  {
    private ProviderKind? previousProviderKind;

    [Test(Description = "Case when calculated column in the midle of selected columns averege")]
    public void IncludeProviderOptimizationTest01()
    {
      EnsureRightDateIsInStorage(ProviderKind.IncludeProvider);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var priceCalculation = from tablePart in Query.All<TablePartBase.FinToolKind.TpPriceCalc>() where !tablePart.OnlyCurrentType select tablePart;

        var masterDebit = from posting in
          Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              Sum = (posting.DebitAccount.AccountType==PacioliAccountType.Passive) ? -posting.Sum : posting.Sum,
              MasterAccount = posting.DebitAccount,
              MasterFinToolBase = posting.FinDebit,
              SlaveAccount = posting.CreditAccount,
              SlaveFinToolBase = posting.FinCredit
            };

        var masterCredit = from posting in Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              Sum =
                posting.CreditAccount.AccountType==PacioliAccountType.Active
                || posting.CreditAccount.AccountType==PacioliAccountType.ActivePassive
                  ? -posting.Sum
                  : posting.Sum,
              MasterAccount = posting.CreditAccount,
              MasterFinToolBase = posting.FinCredit,
              SlaveAccount = posting.DebitAccount,
              SlaveFinToolBase = posting.FinDebit
            };

        var usefulColumns = masterCredit.Union(masterDebit);
        var readyForFilterQuery = from joinResult in usefulColumns
          .LeftJoin(priceCalculation, a => a.SlaveAccount, a => a.Account, (pp, ps) => new {pp, ps})
          .LeftJoin(priceCalculation, a => a.pp.MasterAccount, a => a.Account, (a, pm) => new {a.pp, a.ps, pm})
          let item = joinResult.pp
          select new CustomPosting() {
            Id = item.Id,
            Status = item.Status,
            BalanceHolder = item.BalanceHolder,
            CreationDate = item.CreationDate,
            Currency = item.Currency,
            ExecutionDate = item.ExecutionDate,
            Operation = item.Operation,
            OperationType = item.Operation.OperationType,
            Sum = item.Sum,
            MasterAccount = item.MasterAccount,
            MasterFinToolBase = item.MasterFinToolBase,
            SlaveAccount = item.SlaveAccount,
            SlaveFinToolBase = item.SlaveFinToolBase,
            MasterFinToolKind = joinResult.pm!=null ? joinResult.pm.Owner : item.MasterFinToolBase.FinToolKind,
            SlaveFinToolKind = joinResult.pm!=null ? joinResult.ps.Owner : item.SlaveFinToolBase.FinToolKind,
          };

        var id = session.Query.All<PacioliAccount>().First().Id;

        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id == id).LongCount());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    [Test(Description = "Case when calculated column is first columns of selection")]
    public void IncludeProviderOptimizationTest02()
    {
      EnsureRightDateIsInStorage(ProviderKind.IncludeProvider);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var priceCalculation = from tablePart in Query.All<TablePartBase.FinToolKind.TpPriceCalc>() where !tablePart.OnlyCurrentType select tablePart;

        var masterDebit = from posting in
          Query.All<PacioliPosting>()
          select
            new {
              Sum = (posting.DebitAccount.AccountType==PacioliAccountType.Passive) ? -posting.Sum : posting.Sum,
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              MasterAccount = posting.DebitAccount,
              MasterFinToolBase = posting.FinDebit,
              SlaveAccount = posting.CreditAccount,
              SlaveFinToolBase = posting.FinCredit
            };

        var masterCredit = from posting in Query.All<PacioliPosting>()
          select
            new {
              Sum =
                posting.CreditAccount.AccountType==PacioliAccountType.Active
                || posting.CreditAccount.AccountType==PacioliAccountType.ActivePassive
                  ? -posting.Sum
                  : posting.Sum,
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              MasterAccount = posting.CreditAccount,
              MasterFinToolBase = posting.FinCredit,
              SlaveAccount = posting.DebitAccount,
              SlaveFinToolBase = posting.FinDebit
            };

        var usefulColumns = masterCredit.Union(masterDebit);
        var readyForFilterQuery = from joinResult in usefulColumns
          .LeftJoin(priceCalculation, a => a.SlaveAccount, a => a.Account, (pp, ps) => new {pp, ps})
          .LeftJoin(priceCalculation, a => a.pp.MasterAccount, a => a.Account, (a, pm) => new {a.pp, a.ps, pm})
          let item = joinResult.pp
          select new CustomPosting {
            Id = item.Id,
            Status = item.Status,
            BalanceHolder = item.BalanceHolder,
            CreationDate = item.CreationDate,
            Currency = item.Currency,
            ExecutionDate = item.ExecutionDate,
            Operation = item.Operation,
            OperationType = item.Operation.OperationType,
            Sum = item.Sum,
            MasterAccount = item.MasterAccount,
            MasterFinToolBase = item.MasterFinToolBase,
            SlaveAccount = item.SlaveAccount,
            SlaveFinToolBase = item.SlaveFinToolBase,
            MasterFinToolKind = joinResult.pm!=null ? joinResult.pm.Owner : item.MasterFinToolBase.FinToolKind,
            SlaveFinToolKind = joinResult.pm!=null ? joinResult.ps.Owner : item.SlaveFinToolBase.FinToolKind,
          };

        var id = session.Query.All<PacioliAccount>().First().Id;

        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id == id).LongCount());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());
        Assert.DoesNotThrow(()=>readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    [Test(Description = "Case when calculated column is the second column in selection")]
    public void IncludeProviderOptimizationTest03()
    {
      EnsureRightDateIsInStorage(ProviderKind.IncludeProvider);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var priceCalculation = from tablePart in Query.All<TablePartBase.FinToolKind.TpPriceCalc>() where !tablePart.OnlyCurrentType select tablePart;

        var masterDebit = from posting in
          Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Sum = (posting.DebitAccount.AccountType==PacioliAccountType.Passive) ? -posting.Sum : posting.Sum,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              MasterAccount = posting.DebitAccount,
              MasterFinToolBase = posting.FinDebit,
              SlaveAccount = posting.CreditAccount,
              SlaveFinToolBase = posting.FinCredit
            };

        var masterCredit = from posting in Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Sum =
                posting.CreditAccount.AccountType==PacioliAccountType.Active
                || posting.CreditAccount.AccountType==PacioliAccountType.ActivePassive
                  ? -posting.Sum
                  : posting.Sum,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              MasterAccount = posting.CreditAccount,
              MasterFinToolBase = posting.FinCredit,
              SlaveAccount = posting.DebitAccount,
              SlaveFinToolBase = posting.FinDebit
            };

        var usefulColumns = masterCredit.Union(masterDebit);
        var readyForFilterQuery = from joinResult in usefulColumns
          .LeftJoin(priceCalculation, a => a.SlaveAccount, a => a.Account, (pp, ps) => new {pp, ps})
          .LeftJoin(priceCalculation, a => a.pp.MasterAccount, a => a.Account, (a, pm) => new {a.pp, a.ps, pm})
          let item = joinResult.pp
          select new CustomPosting {
            Id = item.Id,
            Status = item.Status,
            BalanceHolder = item.BalanceHolder,
            CreationDate = item.CreationDate,
            Currency = item.Currency,
            ExecutionDate = item.ExecutionDate,
            Operation = item.Operation,
            OperationType = item.Operation.OperationType,
            Sum = item.Sum,
            MasterAccount = item.MasterAccount,
            MasterFinToolBase = item.MasterFinToolBase,
            SlaveAccount = item.SlaveAccount,
            SlaveFinToolBase = item.SlaveFinToolBase,
            MasterFinToolKind = joinResult.pm!=null ? joinResult.pm.Owner : item.MasterFinToolBase.FinToolKind,
            SlaveFinToolKind = joinResult.pm!=null ? joinResult.ps.Owner : item.SlaveFinToolBase.FinToolKind,
          };

        var id = session.Query.All<PacioliAccount>().First().Id;

        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id == id).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    [Test(Description = "Case when calculated columns is right before the last columns in selection")]
    public void IncludeProviderOptimizationTest04()
    {
      EnsureRightDateIsInStorage(ProviderKind.IncludeProvider);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var priceCalculation = from tablePart in Query.All<TablePartBase.FinToolKind.TpPriceCalc>() where !tablePart.OnlyCurrentType select tablePart;

        var masterDebit = from posting in
          Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              MasterAccount = posting.DebitAccount,
              MasterFinToolBase = posting.FinDebit,
              SlaveAccount = posting.CreditAccount,
              Sum = (posting.DebitAccount.AccountType==PacioliAccountType.Passive) ? -posting.Sum : posting.Sum,
              SlaveFinToolBase = posting.FinCredit
            };

        var masterCredit = from posting in Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,

              MasterAccount = posting.CreditAccount,
              MasterFinToolBase = posting.FinCredit,
              SlaveAccount = posting.DebitAccount,
              Sum =
                posting.CreditAccount.AccountType==PacioliAccountType.Active
                || posting.CreditAccount.AccountType==PacioliAccountType.ActivePassive
                  ? -posting.Sum
                  : posting.Sum,
              SlaveFinToolBase = posting.FinDebit
            };

        var usefulColumns = masterCredit.Union(masterDebit);
        var readyForFilterQuery = from joinResult in usefulColumns
          .LeftJoin(priceCalculation, a => a.SlaveAccount, a => a.Account, (pp, ps) => new {pp, ps})
          .LeftJoin(priceCalculation, a => a.pp.MasterAccount, a => a.Account, (a, pm) => new {a.pp, a.ps, pm})
          let item = joinResult.pp
          select new CustomPosting {
            Id = item.Id,
            Status = item.Status,
            BalanceHolder = item.BalanceHolder,
            CreationDate = item.CreationDate,
            Currency = item.Currency,
            ExecutionDate = item.ExecutionDate,
            Operation = item.Operation,
            OperationType = item.Operation.OperationType,
            Sum = item.Sum,
            MasterAccount = item.MasterAccount,
            MasterFinToolBase = item.MasterFinToolBase,
            SlaveAccount = item.SlaveAccount,
            SlaveFinToolBase = item.SlaveFinToolBase,
            MasterFinToolKind = joinResult.pm!=null ? joinResult.pm.Owner : item.MasterFinToolBase.FinToolKind,
            SlaveFinToolKind = joinResult.pm!=null ? joinResult.ps.Owner : item.SlaveFinToolBase.FinToolKind,
          };

        var id = session.Query.All<PacioliAccount>().First().Id;

        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id == id).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    [Test (Description = "Case when calculated column is the last column in selection")]
    public void IncludeProviderOptimizationTest05()
    {
      EnsureRightDateIsInStorage(ProviderKind.IncludeProvider);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var priceCalculation = from tablePart in Query.All<TablePartBase.FinToolKind.TpPriceCalc>() where !tablePart.OnlyCurrentType select tablePart;

        var masterDebit = from posting in
          Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              MasterAccount = posting.DebitAccount,
              MasterFinToolBase = posting.FinDebit,
              SlaveAccount = posting.CreditAccount,
              SlaveFinToolBase = posting.FinCredit,
              Sum = (posting.DebitAccount.AccountType==PacioliAccountType.Passive) ? -posting.Sum : posting.Sum
            };

        var masterCredit = from posting in Query.All<PacioliPosting>()
          select
            new {
              Id = posting.Id,
              Status = posting.Status,
              BalanceHolder = posting.BalanceHolder,
              CreationDate = posting.CreationDate,
              Currency = posting.Currency,
              ExecutionDate = posting.ExecutionDate,
              Operation = posting.Operation,
              OperationType = posting.Operation.OperationType,
              MasterAccount = posting.CreditAccount,
              MasterFinToolBase = posting.FinCredit,
              SlaveAccount = posting.DebitAccount,
              SlaveFinToolBase = posting.FinDebit,
              Sum =
                posting.CreditAccount.AccountType==PacioliAccountType.Active
                || posting.CreditAccount.AccountType==PacioliAccountType.ActivePassive
                  ? -posting.Sum
                  : posting.Sum,
            };

        var usefulColumns = masterCredit.Union(masterDebit);
        var readyForFilterQuery = from joinResult in usefulColumns
          .LeftJoin(priceCalculation, a => a.SlaveAccount, a => a.Account, (pp, ps) => new {pp, ps})
          .LeftJoin(priceCalculation, a => a.pp.MasterAccount, a => a.Account, (a, pm) => new {a.pp, a.ps, pm})
          let item = joinResult.pp
          select new CustomPosting {
            Id = item.Id,
            Status = item.Status,
            BalanceHolder = item.BalanceHolder,
            CreationDate = item.CreationDate,
            Currency = item.Currency,
            ExecutionDate = item.ExecutionDate,
            Operation = item.Operation,
            OperationType = item.Operation.OperationType,
            Sum = item.Sum,
            MasterAccount = item.MasterAccount,
            MasterFinToolBase = item.MasterFinToolBase,
            SlaveAccount = item.SlaveAccount,
            SlaveFinToolBase = item.SlaveFinToolBase,
            MasterFinToolKind = joinResult.pm!=null ? joinResult.pm.Owner : item.MasterFinToolBase.FinToolKind,
            SlaveFinToolKind = joinResult.pm!=null ? joinResult.ps.Owner : item.SlaveFinToolBase.FinToolKind,
          };

        var id = session.Query.All<PacioliAccount>().First().Id;

        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id == id).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    [Test]
    public void AdditionalTest01()
    {
      EnsureRightDateIsInStorage(ProviderKind.IncludeProvider);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var portfolioId = session.Query.All<Portfolio>().Select(el=>el.Id).First();
        var assistant = new {PortfolioId = portfolioId, PriceOnDate = DateTime.Now, DateOfFormation = DateTime.Now,};
        var transactions = Session.Current.Query.All<RegTransaction>()
                       .Where(a => a.AuthorizeDate!=null && a.OperDate!=null && a.Portfolio.Id==assistant.PortfolioId)
                       .Where(a => a.AuthorizeDate.Value.Date <= assistant.DateOfFormation && a.OperDate.Value.Date <= assistant.PriceOnDate);

        var balanceFinTools = transactions.Select(
          z => new {
                     z.FinTool,
                     Sum = z.Method==TransactionMethod.Debit ? z.Qty : -z.Qty,
                     SumPrice = z.Method==TransactionMethod.Debit ? z.Price : -z.Price
                   }).GroupBy(r => r.FinTool)
          .Select(f => new {FinTool = f.Key, Summ = f.Sum(s => s.Sum), SummPrice = f.Sum(s => s.SumPrice)})
          .Where(a => a.Summ!=0);

        Assert.DoesNotThrow(() => { balanceFinTools.Select(a => a.FinTool.Id).Distinct().ToArray(); });
      }
    }

    [Test]
    public void TakeProviderOptimizationTest01()
    {
      EnsureRightDateIsInStorage(ProviderKind.TakeProvider);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var allPostings = Query.All<PacioliPosting>();
        var masterDebit =
            allPostings.Select(
                posting =>
                new {
                  Id = GuidModifier.ReplaceOne(posting.Id, "1"),
                  Posting = posting,
                  Sum = posting.DebitAccount.AccountType==PacioliAccountType.Passive ? -posting.Sum : posting.Sum,
                  MasterFinToolBase = posting.DebitFinTool
                });

        var masterCredit =
            allPostings.Select(
                posting =>
                new {
                  Id = GuidModifier.ReplaceOne(posting.Id, "9"),
                  Posting = posting,
                  Sum = posting.CreditAccount.AccountType==PacioliAccountType.Active || posting.CreditAccount.AccountType==PacioliAccountType.ActivePassive
                    ? -posting.Sum
                    : posting.Sum,
                  MasterFinToolBase = posting.CreditFinTool
                });

        var concatination = masterCredit.Concat(masterDebit);
        var usefulColumnsSelection = from raw in concatination
          select new {
            Id = raw.Id,
            Posting = raw.Posting,
            Sum = raw.Sum,
            MasterFinToolBase = raw.MasterFinToolBase,
          };

        var finalQuery = from raw in usefulColumnsSelection.Select(
          selectionItem => new {
            MasterFinToolBase = selectionItem.MasterFinToolBase,
            Sum = selectionItem.Sum,
            Id = selectionItem.Posting.Id
          })
          where raw.MasterFinToolBase!=null
          group raw by new {
            MasterFinToolBase = raw.MasterFinToolBase
          }
          into grouping
          select new {
            MasterFinToolBase = grouping.Key.MasterFinToolBase,
            Sum = grouping.Sum(s => s.Sum),
            Id = grouping.Select(x => x.Id).First()
          };
        Assert.DoesNotThrow(() => { finalQuery.ToArray(); });
        transaction.Complete();
      }
    }

    [Test]
    public void JoinAsSourceOfSetOperation()
    {
      EnsureRightDateIsInStorage(ProviderKind.TakeProvider);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var tp = from q in Query.All<TablePartBase.FinToolKind.TpPriceCalc>()
          select
            new {
              q.Account,
              OnlyCurrentType = !q.OnlyCurrentType,
              FinToolKind = q.OnlyCurrentType ? Guid.Empty : q.Owner.Id
            };

        var masterDebit = from q in Query.All<PacioliPosting>()
          select
            new {
              Id = q.Id,
              Posting = q,
              DebitCredit = DebitCredit.Debit,
              Status = q.Status,
              BalanceHolder = q.BalanceHolder,
              CreationDate = q.CreationDate,
              Currency = q.Currency,
              Deal = q.Deal,
              ExecutionDate = q.ExecutionDate,
              Sum = q.Sum,
              MasterAccount = q.DebitAccount,
              MasterFinToolBase = q.DebitFinTool,
            };

        var masterCredit = from q in Query.All<PacioliPosting>()
          select
            new {
              Id = q.Id,
              Posting = q,
              DebitCredit = DebitCredit.Credit,
              Status = q.Status,
              BalanceHolder = q.BalanceHolder,
              CreationDate = q.CreationDate,
              Currency = q.Currency,
              Deal = q.Deal,
              ExecutionDate = q.ExecutionDate,
              Sum = q.Sum,
              MasterAccount = q.CreditAccount,
              MasterFinToolBase = q.CreditFinTool,
            };

        var preResult = masterCredit.Concat(masterDebit);

        var result =
          from r in
            preResult.Join(tp.Distinct(), a => a.MasterAccount, a => a.Account, (a, pm) => new {pp = a, pm})
              .LeftJoin(Query.All<TablePartBase.FinToolKind>(), a => a.pm.FinToolKind, a => a.Id, (a, b) => new {pp = a.pp, pm = a.pm, fk = b})
          let q = r.pp
          select
            new {
              Id = q.Id,
              Posting = q.Posting,
              Status = q.Status,
              BalanceHolder = q.BalanceHolder,
              CreationDate = q.CreationDate,
              Currency = q.Currency,
              Deal = q.Deal,
              ExecutionDate = q.ExecutionDate,
              Sum = q.Sum,
              MasterAccount = q.MasterAccount,
              MasterFinToolBase = q.MasterFinToolBase,
              MasterFinToolKind = r.pm.OnlyCurrentType ? r.fk : q.MasterFinToolBase.FinToolKind,
              AccountType = q.MasterAccount.AccountType
            };

        var xx =
          from a in
            result
              .Select(
                a =>
                  new {
                    MasterFinToolBase = a.MasterFinToolBase,
                    MasterFinToolKind = a.MasterFinToolKind,
                    Currency = a.Currency,
                    BalanceHolder = a.BalanceHolder,
                    CreationDate = a.CreationDate,
                    ExecutionDate = a.ExecutionDate,
                    Deal = a.Deal,
                    MasterAccount = a.MasterAccount,
                    Sum = a.Sum,
                    Posting = a.Posting
                  })
          where a.MasterFinToolBase!=null && a.MasterFinToolKind!=null
          group a by
            new {
              Currency = a.Currency,
              BalanceHolder = a.BalanceHolder,
              CreationDate = a.CreationDate,
              Deal = a.Deal,
              ExecutionDate = a.ExecutionDate,
              MasterAccount = a.MasterAccount,
              MasterFinToolBase = a.MasterFinToolBase
            }
          into gr
          select
            new {
              ExecutionDate = gr.Key.ExecutionDate,
              BalanceHolder = gr.Key.BalanceHolder,
              MasterFinToolBase = gr.Key.MasterFinToolBase,
              Currency = gr.Key.Currency,
              CreationDate = gr.Key.CreationDate,
              Deal = gr.Key.Deal,
              MasterAccount = gr.Key.MasterAccount,
              Sum = gr.Sum(s => s.Sum),
              Id = gr.Select(a => a.Posting).First().Id
            };
 
          Assert.DoesNotThrow(() => result.Run());
      }
    }

    private void PopulateDataForIncludeProvider()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var currency = new Currency(Guid.NewGuid()) { CurCode = "RU", Name = "Ruble" };

        var debitPassiveAccount = new PacioliAccount(Guid.NewGuid());
        debitPassiveAccount.AccountType = PacioliAccountType.Passive;
        debitPassiveAccount.Code = "12345";
        debitPassiveAccount.Description = "debitPassiveAccount";
        debitPassiveAccount.FastAccess = "debitPassiveAccount";
        debitPassiveAccount.Name = "debitPassiveAccount";

        var debitActiveAccount = new PacioliAccount(Guid.NewGuid());
        debitActiveAccount.AccountType = PacioliAccountType.Active;
        debitActiveAccount.Code = "23456";
        debitActiveAccount.Description = "debitActiveAccount";
        debitActiveAccount.FastAccess = "debitActiveAccount";
        debitActiveAccount.Name = "debitActiveAccount";

        var debitActivePassiveAccount = new PacioliAccount(Guid.NewGuid());
        debitActivePassiveAccount.AccountType = PacioliAccountType.ActivePassive;
        debitActivePassiveAccount.Code = "34567";
        debitActivePassiveAccount.Description = "debitActivePassiveAccount";
        debitActivePassiveAccount.FastAccess = "debitActivePassiveAccount";
        debitActivePassiveAccount.Name = "debitActivePassiveAccount";

        var creditPassiveAccount = new PacioliAccount(Guid.NewGuid());
        creditPassiveAccount.AccountType = PacioliAccountType.Passive;
        creditPassiveAccount.Code = "45678";
        creditPassiveAccount.Description = "creditPassiveAccount";
        creditPassiveAccount.FastAccess = "creditPassiveAccount";
        creditPassiveAccount.Name = "creditPassiveAccount";

        var creditActiveAccount = new PacioliAccount(Guid.NewGuid());
        creditActiveAccount.AccountType = PacioliAccountType.Active;
        creditActiveAccount.Code = "56789";
        creditActiveAccount.Description = "creditActiveAccount";
        creditActiveAccount.FastAccess = "creditActiveAccount";
        creditActiveAccount.Name = "creditActiveAccount";

        var creditActivePassiveAccount = new PacioliAccount(Guid.NewGuid());
        creditActivePassiveAccount.AccountType = PacioliAccountType.ActivePassive;
        creditActivePassiveAccount.Code = "67890";
        creditActivePassiveAccount.Description = "creditActivePassiveAccount";
        creditActivePassiveAccount.FastAccess = "creditActivePassiveAccount";
        creditActivePassiveAccount.Name = "creditActivePassiveAccount";

        var fintoolkindOwner = new TablePartBase.FinToolKind(Guid.NewGuid()) { Name = "fgjlhjfglkhjfg", FullName = "jfhjhgfkdhkgjhfkgjh" };
        var priceCalc1 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc1.OnlyCurrentType = true;
        priceCalc1.Account = debitActiveAccount;

        var priceCalc2 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc2.OnlyCurrentType = true;
        priceCalc2.Account = debitPassiveAccount;

        var priceCalc3 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc3.OnlyCurrentType = true;
        priceCalc3.Account = debitActivePassiveAccount;

        var priceCalc4 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc4.OnlyCurrentType = true;
        priceCalc4.Account = creditActiveAccount;

        var priceCalc5 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc5.OnlyCurrentType = true;
        priceCalc5.Account = creditPassiveAccount;

        var priceCalc6 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc6.OnlyCurrentType = true;
        priceCalc6.Account = creditActivePassiveAccount;

        var priceCalc7 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc7.OnlyCurrentType = false;
        priceCalc7.Account = debitActiveAccount;

        var priceCalc8 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc8.OnlyCurrentType = false;
        priceCalc8.Account = debitPassiveAccount;

        var priceCalc9 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc9.OnlyCurrentType = false;
        priceCalc9.Account = debitActivePassiveAccount;

        var priceCalc10 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc10.OnlyCurrentType = false;
        priceCalc10.Account = creditActiveAccount;

        var priceCalc11 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc11.OnlyCurrentType = false;
        priceCalc11.Account = creditPassiveAccount;

        var priceCalc12 = new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), fintoolkindOwner);
        priceCalc12.OnlyCurrentType = false;
        priceCalc12.Account = creditActivePassiveAccount;

        var posting1 = new PacioliPosting(Guid.NewGuid());
        posting1.Sum = new decimal(11);
        posting1.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "dfhgkjhdfgkjhdfj", FullName = "dhsfhkjh khgkjdfhgkj", Identifier = "uyiyriet"};
        posting1.CreationDate = DateTime.Now;
        posting1.CreditAccount = creditPassiveAccount;
        posting1.DebitAccount = debitPassiveAccount;
        posting1.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "aaaa", Name = "aaaa"};
        posting1.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "bbbb", Name = "bbbb"};
        posting1.Operation = new TechOperation(Guid.NewGuid());

        var posting2 = new PacioliPosting(Guid.NewGuid());
        posting2.Sum = new decimal(12);
        posting2.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "dfhgkjhdfg", FullName = "dhsfhkjh khgkj", Identifier = "udfjgkjhfdk"};
        posting2.CreationDate = DateTime.Now;
        posting2.CreditAccount = creditPassiveAccount;
        posting2.DebitAccount = debitActiveAccount;
        posting2.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "cccc", Name = "cccc"};
        posting2.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "dddd", Name = "dddd"};
        posting2.Operation = new TechOperation(Guid.NewGuid());

        var posting3 = new PacioliPosting(Guid.NewGuid());
        posting3.Sum = new decimal(13);
        posting3.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "dfhgkjhdfj", FullName = "dhsfhkjh", Identifier = "utrtoiore"};
        posting3.CreationDate = DateTime.Now;
        posting3.CreditAccount = creditPassiveAccount;
        posting3.DebitAccount = debitActivePassiveAccount;
        posting3.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "eeee", Name = "eeee"};
        posting3.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "ffff", Name = "ffff"};
        posting3.Operation = new TechOperation(Guid.NewGuid());

        var posting4 = new PacioliPosting(Guid.NewGuid());
        posting4.Sum = new decimal(14);
        posting4.BalanceHolder = new RealPortfolio(Guid.NewGuid()) { Name = "kjhdfgkjhdfj", FullName = "kjh khgkjdfhgkj", Identifier = "dfkgkfdj" };
        posting4.CreationDate = DateTime.Now;
        posting4.CreditAccount = creditActiveAccount;
        posting4.DebitAccount = debitPassiveAccount;
        posting4.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "gggg", Name = "gggg"};
        posting4.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "hhh", Name = "hhh"};
        posting4.Operation = new TechOperation(Guid.NewGuid());

        var posting5 = new PacioliPosting(Guid.NewGuid());
        posting5.Sum = new decimal(-15);
        posting5.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "reotoihre", FullName = "fghdg hdufgihiu", Identifier = "ghdfgjhf"};
        posting5.CreationDate = DateTime.Now;
        posting5.CreditAccount = creditActiveAccount;
        posting5.DebitAccount = debitActiveAccount;
        posting5.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "iii", Name = "iiii"};
        posting5.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "jjj", Name = "jjj"};
        posting5.Operation = new TechOperation(Guid.NewGuid());

        var posting6 = new PacioliPosting(Guid.NewGuid());
        posting6.Sum = new decimal(-14);
        posting6.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "hfgdhdfhjghhkj", FullName = "kjh jdfhghjdgh", Identifier = "jdhfkgjhfkjgh"};
        posting6.CreationDate = DateTime.Now;
        posting6.CreditAccount = creditActiveAccount;
        posting6.DebitAccount = debitActivePassiveAccount;
        posting6.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "kkkk", Name = "kkkk"};
        posting6.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "llll", Name = "llll"};
        posting6.Operation = new TechOperation(Guid.NewGuid());

        var posting7 = new PacioliPosting(Guid.NewGuid());
        posting7.Sum = new decimal(-13);
        posting7.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "hjdjhfghjdfhg", FullName = "dfgjdfhkgkj khgkjdfhgkj", Identifier = "dfbghkjdfhgkj"};
        posting7.CreationDate = DateTime.Now;
        posting7.CreditAccount = creditActivePassiveAccount;
        posting7.DebitAccount = debitPassiveAccount;
        posting7.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "mmmm", Name = "mmm"};
        posting7.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "nnn", Name = "nnn"};
        posting7.Operation = new TechOperation(Guid.NewGuid());

        var posting8 = new PacioliPosting(Guid.NewGuid());
        posting8.Sum = new decimal(130);
        posting8.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "yrietyiury", FullName = "dhkgkj khgkjdfhgkj", Identifier = "dfbghj"};
        posting8.CreationDate = DateTime.Now;
        posting8.CreditAccount = creditActivePassiveAccount;
        posting8.DebitAccount = debitActiveAccount;
        posting8.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "oooo", Name = "ooo"};
        posting8.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "ppp", Name = "ppp"};
        posting8.Operation = new TechOperation(Guid.NewGuid());

        var posting9 = new PacioliPosting(Guid.NewGuid());
        posting9.Sum = new decimal(-113);
        posting9.BalanceHolder = new RealPortfolio(Guid.NewGuid()) {Name = "biibibibob", FullName = "nbknf riewuruiet", Identifier = "ncvbmnvbcm"};
        posting9.CreationDate = DateTime.Now;
        posting9.CreditAccount = creditActivePassiveAccount;
        posting9.DebitAccount = debitActivePassiveAccount;
        posting9.FinDebit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "qqqq", Name = "qqq"};
        posting9.FinCredit = new OtherFinTools(Guid.NewGuid()) {Cur = currency, FinToolKind = fintoolkindOwner, DocumentIdentifier = "rrr", Name = "rrr"};
        posting9.Operation = new TechOperation(Guid.NewGuid());

        transaction.Complete();
      }
    }

    public void PopulateDataForTakeProvider()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var status = new Status(Guid.NewGuid()) { Name = "aa" };
        var currency = new Currency(Guid.NewGuid()) { Name = "aa", CurCode = "qwe", DigitCode = "123", EngName = "re" };
        session.SaveChanges();

        var port = new RealPortfolio(Guid.NewGuid()) { Name = "ww", FullName = "ee", Identifier = "w" };
        var operation = new TechOperation(Guid.NewGuid());
        operation.Status = status;
        session.SaveChanges();

        var finToolKind = new TablePartBase.FinToolKind(Guid.NewGuid()) { Name = "aa", FullName = "qq" };
        var finTool1 = new OtherFinTools(Guid.NewGuid()) { Name = "aa", DocumentIdentifier = "wewe", Cur = currency, FinToolKind = finToolKind };
        var finTool2 = new OtherFinTools(Guid.NewGuid())
        {
          Name = "aaw",
          DocumentIdentifier = "wewee",
          Cur = currency,
          FinToolKind = finToolKind,
        };
        session.SaveChanges();

        var pp = new PacioliAccount(Guid.NewGuid()) { AccountType = PacioliAccountType.Active, Name = "aa", FastAccess = "w", Code = "x" };
        new PacioliPosting(Guid.NewGuid()) {
          Name = "a",
          Status = status,
          DebitAccount = pp,
          CreditAccount = pp,
          CreditFinTool = finTool1,
          DebitFinTool = finTool2,
          Sum = 1,
          Currency = currency,
          BalanceHolder = port,
          Operation = operation
        };
        transaction.Complete();
      }
    }

    public void EnsureRightDateIsInStorage(ProviderKind providerKind)
    {
      if (previousProviderKind.HasValue && previousProviderKind!=providerKind) {
        RebuildDomain();
        previousProviderKind = providerKind;
      }
      switch (providerKind) {
        case ProviderKind.IncludeProvider: {
          PopulateDataForTakeProvider();
          return;
        }
        case ProviderKind.TakeProvider: {
          PopulateDataForIncludeProvider();
          return;
        }
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

  public enum ProviderKind
  {
    IncludeProvider,
    TakeProvider
  }
}
