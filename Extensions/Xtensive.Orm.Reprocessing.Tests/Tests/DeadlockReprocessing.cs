// ReSharper disable ConvertToConstant.Local
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable AccessToModifiedClosure

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using TestCommon.Model;
using Xtensive.Orm.Reprocessing.Tests.ReprocessingContext;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Reprocessing.Tests
{
  [TestFixture]
  public class DeadlockReprocessing : ReprocessingBaseTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIsNot(StorageProvider.Firebird, "Throws timeout operation instead of deadlock, which is not reprocessible.");
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void SimpleDeadlockTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(IsolationLevel.Serializable, null, context.Deadlock);
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(2));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSerializableDeadlockTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Serializable,
          null,
          (b, level, open) => context.Parent(b, level, open, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSnapshotDeadlockTest()
    {
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Snapshot,
          null,
          (b, level, open) => context.Parent(b, level, open, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(4));
      }
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedNestedSerializableSerializableTest()
    {
      //nested nested serializable deadlock
      using (var context = new Context(Domain)) {
        context.Run(
          IsolationLevel.Serializable,
          null,
          (b, level, open) =>
            context.Parent(
              b,
              level,
              open,
              ExecuteActionStrategy.HandleReprocessableException,
              (b1, level1, open1) =>
                context.Parent(b1, level1, open1, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock)));
        Assert.That(context.Count, Is.EqualTo(3));
        Assert.That(Bar2Count(), Is.EqualTo(6));
      }
    }

    private int Bar2Count() => Domain.Execute(session => session.Query.All<Bar2>().Count());
  }
}

// ReSharper restore ReturnValueOfPureMethodIsNotUsed
// ReSharper restore ConvertToConstant.Local
// ReSharper restore AccessToModifiedClosure
