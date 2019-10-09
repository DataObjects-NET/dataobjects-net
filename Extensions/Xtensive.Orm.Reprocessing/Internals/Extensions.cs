using System.Data.Common;
using Xtensive.Core;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Reprocessing
{
  internal static class Extensions
  {
    public static void EnsureTransactionIsStarted(this Session session)
    {
      var accessor = session.Services.Demand<DirectSqlAccessor>();
      DbTransaction notUsed = accessor.Transaction;
    }
  }
}