using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Services;

namespace TestCommon.Model
{
  internal static class Extensions
  {
    public static void EnsureTransactionIsStarted(this Session session)
    {
      var accessor = session.Services.Demand<DirectSqlAccessor>();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
      var notUsed = accessor.Transaction;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }
  }
}
