// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Tests.Rse
{
  [Serializable]
  [TestFixture, Category("Rse")]
  public class SqlTemporaryDataProvider : ChinookDOModelTest
  {
    [Test]
    public void TempTableMustNotBeCached() =>
      Assert.AreNotEqual(ExecuteIn(ExecuteCachedIn), ExecuteIn(ExecuteCachedIn));

    [Test]
    public void TempTableMustNotBeCachedScalar() =>
      Assert.AreNotEqual(ExecuteIn(ExecuteCachedInScalar), ExecuteIn(ExecuteCachedInScalar));

    // returns temporary table Name
    private string ExecuteIn(Action<Session> action)
    {
      string tempTableName = null;
      EventHandler<DbCommandEventArgs> sqlPrinter = (o, ea) => {
        if (Regex.Match(ea.Command.CommandText, @"#([^\]]+)") is var m && m.Success) {
          tempTableName = m.Groups[1].Value;
        }
      };
      var session = Session.Demand();
      session.Events.DbCommandExecuted += sqlPrinter;
      action(session);
      session.Events.DbCommandExecuted -= sqlPrinter;
      return tempTableName;
    }

    private void ExecuteCachedIn(Session session)
    {
      var ids = Enumerable.Range(0, 257).ToArray();
      _ = session.Query.Execute("CachedInclude", q => q.All<Track>().Where(o => o.TrackId.In(ids))).Count();
    }

    private void ExecuteCachedInScalar(Session session)
    {
      var ids = Enumerable.Range(0, 257).ToArray();
      _ = session.Query.Execute("CachedInclude", q => q.All<Track>().Where(o => o.TrackId.In(ids)).Count());
    }
  }
}
