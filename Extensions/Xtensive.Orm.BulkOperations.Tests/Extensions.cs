using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Xtensive.Orm.BulkOperations.Tests
{
  public static class Extensions
  {
    public static void AssertCommandCount(this Session session, IResolveConstraint expression, Action action)
    {
      int count = 0;
      session.Events.DbCommandExecuting += (sender, args) => count++;
      action();
      Assert.That(count, expression);
    }
  }
}