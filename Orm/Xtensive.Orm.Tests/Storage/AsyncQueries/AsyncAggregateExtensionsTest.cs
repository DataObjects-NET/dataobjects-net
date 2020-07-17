using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class AsyncAggregateExtensionsTest : AsyncQueriesBaseTest
  {
    private static Task<Session> OpenSessionAsync(Domain domain, bool isClientProfile) =>
      domain.OpenSessionAsync(
        new SessionConfiguration(isClientProfile ? SessionOptions.ClientProfile : SessionOptions.ServerProfile));

    private static TransactionScope OpenTransactionAsync(Session session, bool isClientProfile) =>
      isClientProfile ? null : session.OpenTransaction();

    // All

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task AllAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.IsTrue(await query.Take(0).AllAsync(teacher => teacher.Gender==Gender.Male));
        Assert.IsTrue(await query.Take(0).AllAsync(teacher => teacher.Gender==Gender.Female));
        Assert.IsFalse(
          await query.Where(teacher => teacher.Gender==Gender.Female).AllAsync(teacher => teacher.Gender==Gender.Male));
        Assert.IsFalse(await query.AllAsync(teacher => teacher.Gender==Gender.Male));
        Assert.IsTrue(
          await query.Where(teacher => teacher.Gender==Gender.Male).AllAsync(teacher => teacher.Gender==Gender.Male));
      }
    }

    // Any

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task AnyAsyncNoPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.IsTrue(await query.AnyAsync());
        Assert.IsFalse(await query.Where(teacher => teacher.Name==null).AnyAsync());
        // TODO: Query translation fails
        // Assert.IsFalse(await query.Take(0).AnyAsync());
      }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task AnyAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.IsFalse(
          await query.Where(teacher => teacher.Name==null).AnyAsync(teacher => teacher.Gender==Gender.Male));
        Assert.IsFalse(
          await query.Where(teacher => teacher.Name==null).AnyAsync(teacher => teacher.Gender==Gender.Female));
        Assert.IsFalse(
          await query.Where(teacher => teacher.Gender==Gender.Female).AnyAsync(teacher => teacher.Gender==Gender.Male));
        Assert.IsTrue(await query.AnyAsync(teacher => teacher.Gender==Gender.Male));
        Assert.IsTrue(
          await query.Where(teacher => teacher.Gender==Gender.Male).AnyAsync(teacher => teacher.Gender==Gender.Male));
      }
    }

    // Average<int>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncIntExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.IntFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncIntOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.IntFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
      }
    }

    // Average<int>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncIntWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.IntFactor).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync(stat => stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncIntWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.IntFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.IntFactor));
      }
    }

    // Average<int?>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (int?)stat.IntFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync());
      }
    }

    // Average<int?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Average(),
          await query.AverageAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (int?)stat.IntFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync(stat => (int?)stat.IntFactor));
      }
    }

    // Average<long>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncLongExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.LongFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncLongOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.LongFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
      }
    }

    // Average<long>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncLongWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.LongFactor).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync(stat => stat.LongFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncLongWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.LongFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.LongFactor));
      }
    }

    // Average<long?>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (int?)stat.LongFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync());
      }
    }

    // Average<long?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.LongFactor % 2 == 0 ? default(long?) : stat.LongFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Average(),
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(long?) : stat.LongFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (long?)stat.LongFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync(stat => (long?)stat.LongFactor));
      }
    }
  }
}