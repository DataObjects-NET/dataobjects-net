// Copyright (C) 2019 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.12

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class DelayedQueryClientProfileTest : DelayedQueryTestBase
  {
    protected override SessionConfiguration SessionConfiguration
    {
      get { return new SessionConfiguration(SessionOptions.ClientProfile); }
    }

    [Test]
    public async Task ExecuteWithoutTransactionTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        _ = await session.Query.CreateDelayedQuery(q => q.All<Discepline>()).ExecuteAsync();
      }
    }

    [Test]
    public async Task EnumerationOutsideTransactionTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        QueryResult<Discepline> result;
        using (var transaction = session.OpenTransaction()) {
          result = await session.Query.CreateDelayedQuery(q => q.All<Discepline>())
            .ExecuteAsync();
        }
        _ = Assert.Throws<InvalidOperationException>(() => result.ToList());
      }
    }
  }
}
