// ReSharper disable ConvertToConstant.Local
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable AccessToModifiedClosure

using System;
using System.Threading;
using System.Transactions;
using NUnit.Framework;
using TestCommon.Model;
using Xtensive.Orm.Reprocessing.Tests.ReprocessingContext;

namespace Xtensive.Orm.Reprocessing.Tests
{
  [TestFixture, Timeout(DefaultTestTimeout * 12)]
  public sealed class UniqueConstraintViolationReprocessing : ReprocessingBaseTest
  {
    private bool treatNullAsUniqueValue;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      treatNullAsUniqueValue = Domain.StorageProviderInfo.ProviderName == WellKnown.Provider.SqlServer;
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void SimpleUniqueTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(null, null, context.UniqueConstraintViolation);
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(2));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void UniqueConstraintViolationExceptionUnique()
    {
      var i = 0;
      var errorNotified = false;
      ExecuteActionStrategy.HandleUniqueConstraintViolation.Error += (sender, args) => errorNotified = true;
      Domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).Execute(
        session => {
          _ = new Foo(session) { Name = "test" };
          i++;
          if (i < 5) {
            _ = new Foo(session) { Name = "test" };
          }
        });
      Assert.That(i, Is.EqualTo(5));
      Assert.That(errorNotified, Is.True);
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void UniqueConstraintViolationExceptionPrimary()
    {
      var i = 0;
      Domain.WithStrategy(ExecuteActionStrategy.HandleUniqueConstraintViolation).Execute(
        session => {
          _ = new Foo(session, i);
          i++;
          if (i < 5) {
            _ = new Foo(session, i);
          }
        });
      if (treatNullAsUniqueValue) {
        Assert.That(i, Is.EqualTo(5));
      }
      else {
        Assert.That(i, Is.EqualTo(1));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSerializablePrimaryKeyConstraintTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Serializable,
          null,
          (b, level, open) =>
            context.Parent(
              b, level, open, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolationPrimaryKey));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSnapshotPrimaryKeyConstraintTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Snapshot,
          null,
          (b, level, open) =>
            context.Parent(
              b, level, open, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolationPrimaryKey));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSerializableUniqueIndexTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(
        IsolationLevel.Serializable,
            null,
            (b, level, open) =>
              context.Parent(
                b, level, null, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolation));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSnapshotUniqueIndexTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(
        IsolationLevel.Snapshot,
            null,
            (b, level, open) =>
              context.Parent(
                b, level, null, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolation));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSerializableExternalWithoutTxUniqueIndexTest()
    {
      //ExternalWithoutTransaction nested serializable UniqueConstraint
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Serializable,
          null,
          (b, level, open) =>
            context.External(
              b,
              null,
              open,
              (s, b1, level1, open1) =>
                context.Parent(
                  s,
                  b1,
                  IsolationLevel.Serializable,
                  open1,
                  ExecuteActionStrategy.HandleUniqueConstraintViolation,
                  context.UniqueConstraintViolation)));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSnapshotExternalWithoutTxUniqueIndexTest()
    {
      //ExternalWithoutTransaction nested snapshot UniqueConstraint
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Snapshot,
          null,
          (b, level, open) =>
            context.External(
              b,
              null,
              open,
              (s, b1, level1, open1) =>
                context.Parent(
                  s,
                  b1,
                  IsolationLevel.Snapshot,
                  open1,
                  ExecuteActionStrategy.HandleUniqueConstraintViolation,
                  context.UniqueConstraintViolation)));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSerializableExternalWithTxUniqueIndexTest()
    {
      //ExternalWithTransaction nested serializable UniqueConstraint
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Serializable,
          null,
          (b, level, open) =>
            context.External(
              b,
              level,
              open,
              (s, b1, level1, open1) =>
                context.Parent(
                  s,
                  b1,
                  level1,
                  open1,
                  ExecuteActionStrategy.HandleUniqueConstraintViolation,
                  context.UniqueConstraintViolation)));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(6));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSnapshotExternalWithTxUniqueIndexTest()
    {
      //ExternalWithTransaction nested snapshot UniqueConstraint
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Snapshot,
          null,
          (b, level, open) =>
            context.External(
              b,
              level,
              open,
              (s, b1, level1, open1) =>
                context.Parent(
                  s,
                  b1,
                  level1,
                  open1,
                  ExecuteActionStrategy.HandleUniqueConstraintViolation,
                  context.UniqueConstraintViolation)));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(6));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedUniqueIndexWithAutoTransaction()
    {
      //nested UniqueConstraint with auto transaction
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.ReadUncommitted,
          TransactionOpenMode.Auto,
          (b, l, o) =>
            context.Parent(b, l, o, ExecuteActionStrategy.HandleUniqueConstraintViolation, context.UniqueConstraintViolation));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    private int Bar2Count()
    {
      return Domain.Execute(session => session.Query.All<Bar2>().Count());
    }
  }
}

// ReSharper restore ReturnValueOfPureMethodIsNotUsed
// ReSharper restore ConvertToConstant.Local
// ReSharper restore AccessToModifiedClosure
