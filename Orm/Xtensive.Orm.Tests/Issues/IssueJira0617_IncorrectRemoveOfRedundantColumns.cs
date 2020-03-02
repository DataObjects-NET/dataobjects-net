// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.12.01

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Tests.Issues.IssueJira0617_IncorrectRedundantColumnRemoveModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0617_IncorrectRedundantColumnRemove : AutoBuildTest
  {
    [Test]
    public void Test01()
    {
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
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    [Test]
    public void Test02()
    {
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
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code == "123").LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).ToArray());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] { id })).LongCount());
      }
    }

    [Test]
    public void Test03()
    {
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

    [Test]
    public void Test04()
    {
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

    [Test]
    public void Test05()
    {
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

        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id==id).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code=="123").LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Code.In("123")).LongCount());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] {id})).ToArray());
        Assert.DoesNotThrow(() => readyForFilterQuery.Where(a => a.MasterAccount.Id.In(new[] {id})).LongCount());
      }
    }

    [Test]
    public void Test06()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var transactions = Session.Current.Query.All<RegTransaction>();

        var balanceFinTools = transactions.Select(
          z => new {
            z.FinTool,
            Sum = z.Method==TransactionMethod.Debit ? z.Qty : -z.Qty,
            SumPrice = z.Method==TransactionMethod.Debit ? z.Price : -z.Price
          }).GroupBy(r => r.FinTool)
          .Select(f => new {FinTool = f.Key, Summ = f.Sum(s => s.Sum), SummPrice = f.Sum(s => s.SumPrice)})
          .Where(a => a.Summ!=0);
        var resultQuery = balanceFinTools.Select(a => a.FinTool.Id).Distinct();

        var objects = resultQuery.ToArray();
        Assert.That(objects, Is.Not.Null);
        Assert.That(objects.Length, Is.Not.EqualTo(0));
      }
    }

    [Test]
    public void Test07()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
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

        object[] objects = null;
        Assert.DoesNotThrow(() => { objects = finalQuery.ToArray<object>(); });
        Assert.That(objects, Is.Not.Null);
        Assert.That(objects.Length, Is.Not.EqualTo(0));
      }
    }

    [Test]
    public void Test08()
    {
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
        List<object> objects = null;
        Assert.DoesNotThrow(() => objects = result.ToList<object>());
        Assert.That(objects, Is.Not.Null);
        Assert.That(objects.Count, Is.Not.EqualTo(0));
      }
    }

    [Test]
    public void Test09()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var pacioliPostings = Query.All<PacioliPosting>().Select(el => el.Id).ToList();
        Assert.That(pacioliPostings.Count, Is.EqualTo(10));
        var finTools = Query.All<PacioliPosting>().Where(a => a.Id.In(pacioliPostings)).ToArray().Select(x => x.CreditFinTool.Id).Distinct();

        var query = session.Query.All<PacioliPosting>()
          .Select(a => new {
            Id = GuidModifier.ReplaceOne(a.Id, "9"),
            Posting = a,
            Sum = a.CreditAccount.AccountType==PacioliAccountType.Passive || a.CreditAccount.AccountType==PacioliAccountType.ActivePassive ? -a.Sum : a.Sum,
            MasterAccount = a.CreditAccount,
            MasterFinToolBase = a.CreditFinTool,
            SlaveAccount = a.DebitAccount,
            SlaveFinToolBase = a.DebitFinTool
          })
          .Concat(session.Query.All<PacioliPosting>().Select(b => new {
            Id = GuidModifier.ReplaceOne(b.Id, "1"),
            Posting = b,
            Sum = b.DebitAccount.AccountType==PacioliAccountType.Active ? -b.Sum : b.Sum,
            MasterAccount = b.DebitAccount,
            MasterFinToolBase = b.DebitFinTool,
            SlaveAccount = b.CreditAccount,
            SlaveFinToolBase = b.CreditFinTool
          }))
          .Select(r => new {r = r, q = r})
          .Select(el => new CustomPosting() {
            Id = el.q.Id,
            Posting = el.q.Posting,
            Status = el.q.Posting.Status,
            BalanceHolder = el.q.Posting.BalanceHolder,
            CreationDate = el.q.Posting.CreationDate,
            Currency = el.q.Posting.Currency,
            Deal = el.q.Posting.Deal,
            ExecutionDate = el.q.Posting.ExecutionDate,
            OperationType = el.q.Posting.Operation.OperationType,
            Sum = el.q.Sum,
            MasterAccount = el.q.MasterAccount,
            MasterFinToolBase = el.q.MasterFinToolBase,
            SlaveAccount = el.q.SlaveAccount,
            SlaveFinToolBase = el.q.SlaveFinToolBase,
            MasterFinToolKind = el.q.MasterFinToolBase.FinToolKind
          })
          .Where(a => (a.MasterFinToolBase.Id.In(finTools)))
          .Select(a =>
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
              Id = a.Posting.Id
            })
          .Where(a => ((a.MasterFinToolBase!=null) && (a.MasterFinToolKind!=null)))
          .GroupBy(a => new {
            Currency = a.Currency,
            BalanceHolder = a.BalanceHolder,
            CreationDate = a.CreationDate,
            Deal = a.Deal,
            ExecutionDate = a.ExecutionDate,
            MasterAccount = a.MasterAccount,
            MasterFinToolBase = a.MasterFinToolBase
          });
        var query1 = query.Select(gr =>
          new CustomPostingGroup() {
            ExecutionDate = gr.Key.ExecutionDate,
            BalanceHolder = gr.Key.BalanceHolder,
            MasterFinToolBase = gr.Key.MasterFinToolBase,
            Currency = gr.Key.Currency,
            CreationDate = gr.Key.CreationDate,
            Deal = gr.Key.Deal,
            MasterAccount = gr.Key.MasterAccount,
            Sum = gr.Sum(s => s.Sum),
            Id = gr.First().Id
          })
          .Where(a => true)
          .OrderBy(a => a.CreationDate)
          .Select(c => new {Item = c, FakeKey = 0})
          .GroupBy(i => i.FakeKey);

        object[] objects = null;
        objects = query1.ToArray<object>();
        Assert.That(objects, Is.Not.Null);
        Assert.That(objects.Length, Is.Not.EqualTo(0));

        object @object = null;
        dynamic q = query1.Select("new (Sum(Item.Sum) as Sum, Count() as Count)");
        Assert.DoesNotThrow(() => @object = Queryable.FirstOrDefault<object>(q));
        Assert.That(@object, Is.Not.Null);
      }
    }

    [Test]
    public void Test10()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var pacioliPostings = Query.All<PacioliPosting>().Select(el => el.Id).ToList();
        var finTools = Query.All<PacioliPosting>().Where(a => a.Id.In(pacioliPostings)).ToArray().Select(x => x.CreditFinTool.Id).Distinct();

        var query = session.Query.All<PacioliPosting>()
          .Select(a => new {
            Id = GuidModifier.ReplaceOne(a.Id, "9"),
            Posting = a,
            Sum = a.CreditAccount.AccountType==PacioliAccountType.Passive || a.CreditAccount.AccountType==PacioliAccountType.ActivePassive ? -a.Sum : a.Sum,
            MasterAccount = a.CreditAccount,
            MasterFinToolBase = a.CreditFinTool,
            SlaveAccount = a.DebitAccount,
            SlaveFinToolBase = a.DebitFinTool
          })
          .Concat(session.Query.All<PacioliPosting>().Select(b => new {
            Id = GuidModifier.ReplaceOne(b.Id, "1"),
            Posting = b,
            Sum = b.DebitAccount.AccountType==PacioliAccountType.Active ? -b.Sum : b.Sum,
            MasterAccount = b.DebitAccount,
            MasterFinToolBase = b.DebitFinTool,
            SlaveAccount = b.CreditAccount,
            SlaveFinToolBase = b.CreditFinTool
          }))
          .Select(r => new {r = r, q = r})
          .Select(el => new CustomPosting {
            Id = el.q.Id,
            Posting = el.q.Posting,
            Status = el.q.Posting.Status,
            BalanceHolder = el.q.Posting.BalanceHolder,
            CreationDate = el.q.Posting.CreationDate,
            Currency = el.q.Posting.Currency,
            Deal = el.q.Posting.Deal,
            ExecutionDate = el.q.Posting.ExecutionDate,
            OperationType = el.q.Posting.Operation.OperationType,
            Sum = el.q.Sum,
            MasterAccount = el.q.MasterAccount,
            MasterFinToolBase = el.q.MasterFinToolBase,
            SlaveAccount = el.q.SlaveAccount,
            SlaveFinToolBase = el.q.SlaveFinToolBase,
            MasterFinToolKind = el.q.MasterFinToolBase.FinToolKind
          })
          .Where(a => (a.MasterFinToolBase.Id.In(finTools)))
          .Select(a =>
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
              Id = a.Posting.Id
            })
          .Where(a => ((a.MasterFinToolBase!=null) && (a.MasterFinToolKind!=null)))
          .GroupBy(a => new {
            Currency = a.Currency,
            BalanceHolder = a.BalanceHolder,
            CreationDate = a.CreationDate,
            Deal = a.Deal,
            ExecutionDate = a.ExecutionDate,
            MasterAccount = a.MasterAccount,
            MasterFinToolBase = a.MasterFinToolBase
          })
          .Select(gr =>
            new CustomPostingGroup() {
              ExecutionDate = gr.Key.ExecutionDate,
              BalanceHolder = gr.Key.BalanceHolder,
              MasterFinToolBase = gr.Key.MasterFinToolBase,
              Currency = gr.Key.Currency,
              CreationDate = gr.Key.CreationDate,
              Deal = gr.Key.Deal,
              MasterAccount = gr.Key.MasterAccount,
              Sum = gr.Sum(s => s.Sum),
              Id = gr.First().Id
            })
          .Where(a => true)
          .OrderBy(a => a.CreationDate)
          .Take(10);

        var simpleSelection = query.Select(el => new {
          el.Id,
          el.CreationDate,
          el.ExecutionDate,
          BalanceHolder = el.BalanceHolder,
          MasterAccount = el.MasterAccount.Name,
          MasterFinToolBase = el.MasterFinToolBase.Name,
          Currency = el.Currency.Name,
          Sum = el.Sum
        });
        object[] elements = null;
        Assert.DoesNotThrow(() => elements = simpleSelection.ToArray<object>());
        Assert.That(elements, Is.Not.Null);
        Assert.That(elements.Length, Is.Not.EqualTo(0));

        dynamic selection = query.Select("new (" +
                                         "Id, " +
                                         " CreationDate," +
                                         " ExecutionDate," +
                                         " BalanceHolder.Name as BalanceHolder," +
                                         " MasterAccount.Name as MasterAccount," +
                                         " MasterFinToolBase.Name as MasterFinToolBase," +
                                         " Currency.Name as Currency," +
                                         " Sum)");

        object[] objects = null;
        Assert.DoesNotThrow(() => objects = Enumerable.ToArray<object>((IEnumerable<object>)selection));
        Assert.That(objects, Is.Not.Null);
        Assert.That(objects.Length, Is.Not.EqualTo(0));
      }
    }

    [Test]
    public void Test11()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var pacioliPostings = Query.All<PacioliPosting>().Select(el => el.Id).ToList();
        var finTools = Query.All<PacioliPosting>().Where(a => a.Id.In(pacioliPostings)).ToArray().Select(x => x.CreditFinTool.Id).Distinct();

        var query = session.Query.All<PacioliPosting>()
          .Select(el => new {
            Id = GuidModifier.ReplaceOne(el.Id, "9"),
            Posting = el,
            Sum = el.CreditAccount.AccountType==PacioliAccountType.Passive || el.CreditAccount.AccountType==PacioliAccountType.ActivePassive ? -el.Sum : el.Sum,
            MasterAccount = el.CreditAccount,
            MasterFinToolBase = el.CreditFinTool,
            SlaveAccount = el.DebitAccount,
            SlaveFinToolBase = el.DebitFinTool
          })
          .Union(
            session.Query.All<PacioliPosting>()
              .Select(el => new {
                Id = GuidModifier.ReplaceOne(el.Id, "1"),
                Posting = el,
                Sum = el.DebitAccount.AccountType==PacioliAccountType.Active ? -el.Sum : el.Sum,
                MasterAccount = el.DebitAccount,
                MasterFinToolBase = el.DebitFinTool,
                SlaveAccount = el.CreditAccount,
                SlaveFinToolBase = el.CreditFinTool
              })
          )
          .Select(r => new {r = r, q = r})
          .Select(el => new CustomPosting {
            Id = el.q.Id,
            Posting = el.q.Posting,
            Status = el.q.Posting.Status,
            BalanceHolder = el.q.Posting.BalanceHolder,
            CreationDate = el.q.Posting.CreationDate,
            Currency = el.q.Posting.Currency,
            Deal = el.q.Posting.Deal,
            ExecutionDate = el.q.Posting.ExecutionDate,
            OperationType = el.q.Posting.Operation.OperationType,
            Sum = el.q.Sum,
            MasterAccount = el.q.MasterAccount,
            MasterFinToolBase = el.q.MasterFinToolBase,
            SlaveAccount = el.q.SlaveAccount,
            SlaveFinToolBase = el.q.SlaveFinToolBase,
            MasterFinToolKind = el.q.MasterFinToolBase.FinToolKind
          })
          .Where(a => (a.MasterFinToolBase.Id.In(finTools)))
          .Where(a => true)
          .OrderBy(el => el.CreationDate)
          .Take(10);

        var simpleSelection = query.Select(el => new {
          el.Id,
          el.Posting.Name,
          el.CreationDate,
          el.ExecutionDate,
          BalanceHolder = el.BalanceHolder.Name,
          OperationType = el.OperationType.Name,
          el.Sum,
          Currency = el.Currency.Name,
          Status = el.Status.Name,
          MasterAccount = el.MasterAccount.Name,
          SlaveAccount = el.SlaveAccount.Name,
          MasterFinToolBase = el.MasterFinToolBase.Name,
          SlaveFinToolBase = el.SlaveFinToolBase.Name,
          MasterFinToolKind = el.MasterFinToolKind
        }).ToArray();
        Assert.That(simpleSelection, Is.Not.Null);
        Assert.That(simpleSelection.Length, Is.Not.EqualTo(0));

        dynamic selection = query.Select("new (Id," +
                                         " Posting.Name as Posting," +
                                         " CreationDate, ExecutionDate," +
                                         " ExecutionDate," +
                                         " BalanceHolder.Name as BalanceHolder," +
                                         " OperationType.Name as OperationType," +
                                         " Sum," +
                                         " Currency.Name as Currency," +
                                         " Status.Name as Status," +
                                         " MasterAccount.Name as MasterAccount," +
                                         " SlaveAccount.Name as SlaveAccount," +
                                         " MasterFinToolBase.Name as MasterFinToolBase," +
                                         " SlaveFinToolBase.Name as SlaveFinToolBase," +
                                         " MasterFinToolKind.Name as MasterFinToolKind)");

        object[] objects = null;
        Assert.DoesNotThrow(() => objects = Enumerable.ToArray<object>((IEnumerable<object>)selection));
        Assert.That(objects, Is.Not.Null);
        Assert.That(objects.Length, Is.Not.EqualTo(0));
      }
    }

    [Test]
    public void Test12()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var pacioliPostings = Query.All<PacioliPosting>().Select(el => el.Id).ToList();
        var finTools = Query.All<PacioliPosting>().Where(a => a.Id.In(pacioliPostings)).ToArray().Select(x => x.CreditFinTool.Id).Distinct();

        var query = session.Query.All<PacioliPosting>()
          .Select(q => new {
            Id = GuidModifier.ReplaceOne(q.Id, "9"),
            Posting = q,
            Sum = q.CreditAccount.AccountType==PacioliAccountType.Passive || q.CreditAccount.AccountType==PacioliAccountType.ActivePassive ? -q.Sum : q.Sum,
            MasterAccount = q.CreditAccount,
            MasterFinToolBase = q.CreditFinTool,
            SlaveAccount = q.DebitAccount,
            SlaveFinToolBase = q.DebitFinTool
          })
          .Concat(
            session.Query.All<PacioliPosting>()
              .Select(q => new {
                Id = GuidModifier.ReplaceOne(q.Id, "1"),
                Posting = q,
                Sum = q.DebitAccount.AccountType==PacioliAccountType.Active ? -q.Sum : q.Sum,
                MasterAccount = q.DebitAccount,
                MasterFinToolBase = q.DebitFinTool,
                SlaveAccount = q.CreditAccount,
                SlaveFinToolBase = q.CreditFinTool
              }))
          .Select(r => new {r = r, q = r})
          .Select(a => new CustomPosting {
            Id = a.q.Id,
            Posting = a.q.Posting,
            Status = a.q.Posting.Status,
            BalanceHolder = a.q.Posting.BalanceHolder,
            CreationDate = a.q.Posting.CreationDate,
            Currency = a.q.Posting.Currency,
            Deal = a.q.Posting.Deal,
            ExecutionDate = a.q.Posting.ExecutionDate,
            OperationType = a.q.Posting.Operation.OperationType,
            Sum = a.q.Sum,
            MasterAccount = a.q.MasterAccount,
            MasterFinToolBase = a.q.MasterFinToolBase,
            SlaveAccount = a.q.SlaveAccount,
            SlaveFinToolBase = a.q.SlaveFinToolBase,
            MasterFinToolKind = a.q.MasterFinToolBase.FinToolKind
          })
          .Where(a => (a.MasterFinToolBase.Id.In(finTools)))
          .Where(a => true)
          .OrderBy(a => a.CreationDate)
          .Select(q => new {Item = q, FakeKey = 0})
          .GroupBy(i => i.FakeKey);

        dynamic selection = query.Select("new (Sum(Item.Sum) as Sum, Count() as sysTotal_Item_Count)");
        object @object = null;
        Assert.DoesNotThrow(() => @object = Queryable.FirstOrDefault<object>(selection));
        Assert.That(@object, Is.Not.Null);
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(PacioliAccount).Assembly, typeof(PacioliAccount).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      base.PopulateData();
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var randomNumbersProvider = new Random();
        var statuses = new List<Status>(10);
        for (var i = 0; i < 10; i++) {
          statuses.Add(new Status(Guid.NewGuid()) {Description = "Status", Name = "Status" + i});
        }

        var modules = new List<Module>();
        for (var i = 0; i < 10; i++) {
          modules.Add(new Module(Guid.NewGuid()) {Name = "Module" + i, SysName = "Module" + i, FileVersion = i + "." + (40 - i) + "." + i});
        }

        var currencies = new List<Currency>(10);
        for (int i = 0; i < 10; i++) {
          currencies.Add(new Currency(Guid.NewGuid()) {Name = "Currency" + i, Status = statuses[randomNumbersProvider.Next(0, 10)]});
        }

        var regEntities = new List<RegEntity>(10);
        for (var i = 0; i < 10; i++) {
          regEntities.Add(new RegEntity(Guid.NewGuid()) {Name = "RegEntity" + i, Module = modules[randomNumbersProvider.Next(0, 10)]});
        }

        var finToolKinds = new List<TablePartBase.FinToolKind>(10);
        for (var i = 0; i < 10; i++) {
          finToolKinds.Add(new TablePartBase.FinToolKind(Guid.NewGuid()) {
            Entity = regEntities[randomNumbersProvider.Next(0, 10)],
            Name = "FinToolKind" + i,
            FullName = "FinToolKind" + i
          });
        }

        var pacioliAccounts = new List<PacioliAccount>(10);
        for (var i = 0; i < 10; i++) {
          pacioliAccounts.Add(new PacioliAccount(Guid.NewGuid()) {
            AccountType = (PacioliAccountType)randomNumbersProvider.Next(0, 3),
            Name = "PacioliAccount" + i,
            Code = (i * i).ToString(),
            FastAccess = "dfghdfjghkjdfg" + i,
          });
        }

        var tablePriceCalcs = new List<TablePartBase.FinToolKind.TpPriceCalc>();
        for (var i = 0; i < 10; i++) {
          tablePriceCalcs.Add(new TablePartBase.FinToolKind.TpPriceCalc(Guid.NewGuid(), finToolKinds[randomNumbersProvider.Next(0, 10)]) {
            Account = pacioliAccounts[randomNumbersProvider.Next(0, 10)]
          });
        }

        var operationTypes = new List<OperationType>(10);
        for (var i = 0; i < 10; i++) {
          operationTypes.Add(new OperationType(Guid.NewGuid()) {
            Name = "OperationType",
            Account = pacioliAccounts[randomNumbersProvider.Next(0, 10)]
          });
        }

        var regTransactions = new List<RegTransaction>(10);
        for (var i = 0; i < 10; i++) {
          regTransactions.Add(new RegTransaction(Guid.NewGuid()) {
            Portfolio = null,
            FinTool = finToolKinds[randomNumbersProvider.Next(0, 10)],
            Method = TransactionMethod.Debit,
            Price = (decimal)i / 10,
            Qty = (decimal)i * 10,
          });
        }

        var portfolios = new List<Portfolio>(20);
        for (var i = 0; i < 10; i++) {
          portfolios.Add(new AggregatePortfolio(Guid.NewGuid()) {
            Name = "AggregatePortfolio" + i,
            FullName = "AggregatePortfolio" + i,
            Identifier = "AggregatePortfolio" + i,
          });
        }
        for (var i = 0; i < 10; i++) {
          portfolios.Add(new RealPortfolio(Guid.NewGuid()) {
            Name = "RealPortfolio" + i,
            FullName = "RealPortfolio" + i,
            Identifier = "RealPortfolio" + i,
          });
        }

        var operationBases = new List<OperationBase>(20);
        for (var i = 0; i < 10; i++) {
          operationBases.Add(new TechOperation(Guid.NewGuid()) {
            Portfolio = portfolios[randomNumbersProvider.Next(0, 20)],
          });
        }

        for (var i = 0; i < 10; i++) {
          operationBases.Add(new ActOfNavDetectedErrors(Guid.NewGuid()) {
            Portfolio = portfolios[randomNumbersProvider.Next(0, 20)]
          });
        }

        var finToolBases = new List<FinToolBase>(10);
        for (var i = 0; i < 10; i++) {
          finToolBases.Add(new OtherFinTools(Guid.NewGuid()) {
            Name = "FinToolBase" + i,
            DocumentIdentifier = "FinToolBase" + Guid.NewGuid(),
            Cur = currencies[randomNumbersProvider.Next(0, 10)],
            FinToolKind = finToolKinds[randomNumbersProvider.Next(0, 10)],
          });
        }

        var pacioliPostings = new List<PacioliPosting>();
        for (int i = 0; i < 10; i++) {
          pacioliPostings.Add(new PacioliPosting(Guid.NewGuid()) {
            Name = "PacioliPosting" + i,
            Status = statuses[randomNumbersProvider.Next(0, 10)],
            DebitAccount = pacioliAccounts[randomNumbersProvider.Next(0, 10)],
            CreditAccount = pacioliAccounts[randomNumbersProvider.Next(0, 10)],
            CreditFinTool = finToolBases[randomNumbersProvider.Next(0, 10)],
            DebitFinTool = finToolBases[randomNumbersProvider.Next(0, 10)],
            ExecutionDate = DateTime.Today,
            Sum = 1,
            Currency = currencies[randomNumbersProvider.Next(0, 10)],
            BalanceHolder = portfolios[randomNumbersProvider.Next(0, 20)],
            Operation = operationBases[randomNumbersProvider.Next(0, 20)],
          });
        }
        transaction.Complete();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0617_IncorrectRedundantColumnRemoveModel
{
  public enum ProviderKind
  {
    IncludeProvider,
    TakeProvider
  }

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

  [HierarchyRoot]
  public sealed class Module : EntityBase
  {
    [Field(Length = 40)]
    public string LongRevision { get; set; }

    [Field(Length = 400, Nullable = false)]
    public string Name { get; set; }

    [Field]
    public ulong Revision { get; set; }

    [Field(Length = 400, Nullable = false)]
    public string SysName { get; set; }

    [Field(Length = 100, Nullable = false, NullableOnUpgrade = true)]
    public string FileVersion { get; set; }

    public Module(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public sealed class RegEntity : EntityBase
  {
    [Field]
    public string Description { get; set; }

    [Field]
    public string FullName { get; set; }


    [Field(Nullable = false)]
    public Module Module { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string SysName { get; set; }

    public bool IsAbstract
    {
      get { return Session.Domain.Model.Types[SysName].IsAbstract; }
    }

    public RegEntity(Guid id)
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
    public sealed class FinToolKind : EntityBase
    {
      [Field(Nullable = false, Length = 100)]
      public string Name { get; set; }

      [Field(Nullable = false, Length = 100)]
      public string FullName { get; set; }

      [Field]
      public FinToolKind Parent { get; set; }

      [Field]
      public RegEntity Entity { get; set; }

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
      public sealed class TpPriceCalc : TablePartBase
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

    [Field]
    public string ForeignId { get; set; }

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
  public sealed class Status : EntityBase
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
  public sealed class Currency : EntityBase
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

    [Field]
    public Status Status { get; set; }

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

  [Serializable]
  public sealed class AggregatePortfolio : Portfolio
  {
    public AggregatePortfolio(Guid id)
      : base(id)
    {
    }
  }

  public sealed class RealPortfolio : Portfolio
  {
    public RealPortfolio(Guid id)
      : base(id)
    { }
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

  public sealed class TechOperation : OperationBase
  {
    [Field]
    public string ForeignId { get; set; }

    public TechOperation(Guid id)
      : base(id)
    {
    }
  }

  public sealed class ActOfNavDetectedErrors : OperationBase
  {
    public ActOfNavDetectedErrors(Guid id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  public sealed class OperationType : EntityBase
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
  public sealed class RegTransaction : EntityBase
  {
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

    public RegTransaction(Guid id)
      : base(id)
    {
    }
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

    public PacioliPosting Posting { get; set; }

    public DealType Deal { get; set; }
  }

  public class CustomPostingGroup
  {
    public Guid Id { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ExecutionDate { get; set; }

    public Portfolio BalanceHolder { get; set; }

    public DealType Deal { get; set; }

    public PacioliAccount MasterAccount { get; set; }

    public FinToolBase MasterFinToolBase { get; set; }

    public Currency Currency { get; set; }

    public Decimal Sum { get; set; }
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
