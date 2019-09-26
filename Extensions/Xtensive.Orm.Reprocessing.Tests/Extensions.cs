using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Services;

namespace TestCommon.Model
{
  static class Extensions
  {
    public static void EnsureTransactionIsStarted(this Session session)
    {
      var accessor = session.Services.Demand<DirectSqlAccessor>();
      DbTransaction notUsed = accessor.Transaction;
    }
  }
}
