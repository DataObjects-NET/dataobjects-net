// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.12

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class DelayedQueryServerProfileTest : DelayedQueryTestBase
  {
    protected override SessionConfiguration SessionConfiguration
    {
      get { return new SessionConfiguration(SessionOptions.ServerProfile); }
    }

    [Test]
    public async Task ExecuteWithoutTransactionTest()
    {
      await using (var session = await Domain.OpenSessionAsync(SessionConfiguration)) {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
          async () => await session.Query.CreateDelayedQuery(q => q.All<Discepline>()).ExecuteAsync());
        Assert.That(ex.Message,
          Is.EqualTo(Strings.ExActiveTransactionIsRequiredForThisOperationUseSessionOpenTransactionToOpenIt));
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

        var ex = Assert.Throws<InvalidOperationException>(() => result.ToList());
        Assert.That(ex.Message,
          Is.EqualTo(Strings.ExThisInstanceIsExpiredDueToTransactionBoundaries));
      }
    }
  }
}
