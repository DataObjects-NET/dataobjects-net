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

namespace Xtensive.Orm.Reprocessing.Tests
{
  [TestFixture, Timeout(DefaultTestTimeout * 4)]
  public class DeadlockReprocessing : ReprocessingBaseTest
  {
    [Test, Timeout(DefaultTestTimeout)]
    public void SimpleDeadlockTest()
    {
      Console.WriteLine("Test started");

      var context = new Context(Domain);
      context.Run(IsolationLevel.Serializable, null, context.Deadlock);
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(2));
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSerializableDeadlockTest()
    {
      Console.WriteLine("Test started");

      var context = new Context(Domain);
      context.Run(
        IsolationLevel.Serializable,
        null,
        (b, level, open) => context.Parent(b, level, open, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedSnapshotDeadlockTest()
    {
      Console.WriteLine("Test started");

      var context = new Context(Domain);
      context.Run(
        IsolationLevel.Snapshot,
        null,
        (b, level, open) => context.Parent(b, level, open, ExecuteActionStrategy.HandleReprocessableException, context.Deadlock));
      Assert.That(context.Count, Is.EqualTo(3));
      Assert.That(Bar2Count(), Is.EqualTo(4));
    }

    [Test, Timeout(DefaultTestTimeout)]
    public void NestedNestedSerializableSerializableTest()
    {
      Console.WriteLine("Test started");

      //nested nested serializable deadlock
      var context = new Context(Domain);
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

    private int Bar2Count()
    {
      return Domain.Execute(session => session.Query.All<Bar2>().Count());
    }
  }
}

// ReSharper restore ReturnValueOfPureMethodIsNotUsed
// ReSharper restore ConvertToConstant.Local
// ReSharper restore AccessToModifiedClosure
