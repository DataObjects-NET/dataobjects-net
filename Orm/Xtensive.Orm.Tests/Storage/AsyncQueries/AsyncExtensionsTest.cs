// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  [Mute]
  public class AsyncExtensionsTest : AsyncQueriesBaseTest
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

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(int?) : stat.IntFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Average(), await nullQuery.AverageAsync());
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

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(int?)).ToList();
        Assert.AreEqual(
          nullFactors.Average(),
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(int?) : stat.IntFactor));
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
          .Where(stat => stat.IntFactor < 0).Select(stat => (long?)stat.LongFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(long?) : stat.LongFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Average(), await nullQuery.AverageAsync());
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

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(long?)).ToList();
        Assert.AreEqual(
          nullFactors.Average(),
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(long?) : stat.LongFactor));
      }
    }

    // Average<double>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDoubleExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.DoubleFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDoubleOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.DoubleFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
      }
    }

    // Average<double>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDoubleWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.DoubleFactor).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync(stat => stat.DoubleFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDoubleWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.DoubleFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.DoubleFactor));
      }
    }

    // Average<double?>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (double?)stat.DoubleFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(double?) : stat.DoubleFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Average(), await nullQuery.AverageAsync());
      }
    }

    // Average<double?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.LongFactor % 2 == 0 ? default(double?) : stat.DoubleFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Average(),
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(double?) : stat.DoubleFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (double?)stat.DoubleFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync(stat => (double?)stat.DoubleFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(double?)).ToList();
        Assert.AreEqual(
          nullFactors.Average(),
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(double?) : stat.DoubleFactor));
      }
    }

    // Average<float>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncFloatExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.FloatFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncFloatOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.FloatFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
      }
    }

    // Average<float>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncFloatWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.FloatFactor).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync(stat => stat.FloatFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncFloatWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.FloatFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.FloatFactor));
      }
    }

    // Average<float?>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (float?) stat.FloatFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(float?) : stat.FloatFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Average(), await nullQuery.AverageAsync());
      }
    }

    // Average<float?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.LongFactor % 2 == 0 ? default(float?) : stat.FloatFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Average(),
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(float?) : stat.FloatFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (float?)stat.FloatFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync(stat => (float?)stat.FloatFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(float?)).ToList();
        Assert.AreEqual(
          nullFactors.Average(),
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(float?) : stat.FloatFactor));
      }
    }

    // Average<decimal>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDecimalExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.DecimalFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDecimalOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.DecimalFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
      }
    }

    // Average<decimal>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDecimalWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.DecimalFactor).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync(stat => stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncDecimalWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.DecimalFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.DecimalFactor));
      }
    }

    // Average<decimal?>

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Average(), await query.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (decimal?) stat.DecimalFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(decimal?) : stat.DecimalFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Average(), await nullQuery.AverageAsync());
      }
    }

    // Average<decimal?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.LongFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Average(),
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (decimal?)stat.DecimalFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.IsNull(await emptyQuery.AverageAsync(stat => (decimal?)stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(decimal?)).ToList();
        Assert.AreEqual(
          nullFactors.Average(),
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(decimal?) : stat.DecimalFactor));
      }
    }

    // Contains

    [Test, TestCase(true), TestCase(false)]
    public async Task ContainsAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.IntFactor);

        Assert.IsTrue(await query.ContainsAsync(50));
        Assert.IsFalse(await query.ContainsAsync(-1));
      }
    }

    // Count

    [Test, TestCase(true), TestCase(false)]
    public async Task CountAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();

        Assert.AreEqual(0, await query.Where(stat => stat.IntFactor < 0).CountAsync());
        Assert.AreEqual(
          allStats.Count(stat => stat.IntFactor < 0),
          await query.Where(stat => stat.IntFactor < 0).CountAsync());

        Assert.AreEqual(10, await query.Where(stat => stat.IntFactor < 10).CountAsync());
        Assert.AreEqual(
          allStats.Count(stat => stat.IntFactor < 10),
          await query.Where(stat => stat.IntFactor < 10).CountAsync());

        Assert.AreEqual(100, await query.CountAsync());
        Assert.AreEqual(
          allStats.Count,
          await query.CountAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task CountAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();

        Assert.AreEqual(0, await query.CountAsync(stat => stat.IntFactor < 0));
        Assert.AreEqual(
          allStats.Count(stat => stat.IntFactor < 0),
          await query.CountAsync(stat => stat.IntFactor < 0));

        Assert.AreEqual(10, await query.CountAsync(stat => stat.IntFactor < 10));
        Assert.AreEqual(
          allStats.Count(stat => stat.IntFactor < 10),
          await query.CountAsync(stat => stat.IntFactor < 10));
      }
    }

    // First

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers[0], await query.FirstAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.ThrowsAsync<InvalidOperationException>(() => query.FirstAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var firstFemaleTeacher = query.AsEnumerable().First(teacher => teacher.Gender==Gender.Female);
        Assert.AreEqual(firstFemaleTeacher, await query.FirstAsync(teacher => teacher.Gender==Gender.Female));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.ThrowsAsync<InvalidOperationException>(() => query.FirstAsync(teacher => teacher.Id < 0));
      }
    }

    // FirstOrDefault

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstOrDefaultAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers[0], await query.FirstOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstOrDefaultAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.IsNull(await query.FirstOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstOrDefaultAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var firstFemaleTeacher = query.AsEnumerable().First(teacher => teacher.Gender==Gender.Female);
        Assert.AreEqual(firstFemaleTeacher, await query.FirstOrDefaultAsync(teacher => teacher.Gender==Gender.Female));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstOrDefaultAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.IsNull(await query.FirstOrDefaultAsync(teacher => teacher.Id < 0));
      }
    }

    // Last

    [Test, TestCase(true), TestCase(false)]
    [Mute]
    public async Task LastAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers.Last(), await query.LastAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [Mute]
    public async Task LastAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.ThrowsAsync<InvalidOperationException>(() => query.LastAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [Mute]
    public async Task LastAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var lastInListFemaleTeacher = query.AsEnumerable().Last(teacher => teacher.Gender==Gender.Female);
        Assert.AreEqual(lastInListFemaleTeacher, await query.LastAsync(teacher => teacher.Gender==Gender.Female));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task LastAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.ThrowsAsync<InvalidOperationException>(() => query.LastAsync(teacher => teacher.Id < 0));
      }
    }

    // LastOrDefault

    [Test, TestCase(true), TestCase(false)]
    public async Task LastOrDefaultAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers.Last(), await query.LastOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task LastOrDefaultAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.IsNull(await query.LastOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task LastOrDefaultAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var lastInListFemaleTeacher = query.AsEnumerable().First(teacher => teacher.Gender==Gender.Female);
        Assert.AreEqual(
          lastInListFemaleTeacher, await query.LastOrDefaultAsync(teacher => teacher.Gender==Gender.Female));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task LastOrDefaultAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.IsNull(await query.LastOrDefaultAsync(teacher => teacher.Id < 0));
      }
    }

    // LongCount

    [Test, TestCase(true), TestCase(false)]
    public async Task LongCountAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();

        Assert.AreEqual(0L, await query.Where(stat => stat.IntFactor < 0).LongCountAsync());
        Assert.AreEqual(
          allStats.LongCount(stat => stat.IntFactor < 0),
          await query.Where(stat => stat.IntFactor < 0).LongCountAsync());

        Assert.AreEqual(10L, await query.Where(stat => stat.IntFactor < 10).LongCountAsync());
        Assert.AreEqual(
          allStats.LongCount(stat => stat.IntFactor < 10),
          await query.Where(stat => stat.IntFactor < 10).LongCountAsync());

        Assert.AreEqual(100L, await query.LongCountAsync());
        Assert.AreEqual(
          allStats.LongCount(),
          await query.LongCountAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task LongCountAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();

        Assert.AreEqual(0L, await query.LongCountAsync(stat => stat.IntFactor < 0));
        Assert.AreEqual(
          allStats.LongCount(stat => stat.IntFactor < 0),
          await query.LongCountAsync(stat => stat.IntFactor < 0));

        Assert.AreEqual(10L, await query.LongCountAsync(stat => stat.IntFactor < 10));
        Assert.AreEqual(
          allStats.LongCount(stat => stat.IntFactor < 10),
          await query.LongCountAsync(stat => stat.IntFactor < 10));
      }
    }

    // Max

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Select(stat => stat.IntFactor).Max();
        var maxLong = allStats.Select(stat => stat.LongFactor).Max();
        var maxFloat = allStats.Select(stat => stat.FloatFactor).Max();
        var maxDouble = allStats.Select(stat => stat.DoubleFactor).Max();
        var maxDecimal = allStats.Select(stat => stat.DecimalFactor).Max();
        Assert.AreEqual(maxInt, await query.Select(stat => stat.IntFactor).MaxAsync());
        Assert.AreEqual(maxLong, await query.Select(stat => stat.LongFactor).MaxAsync());
        Assert.AreEqual(maxFloat, await query.Select(stat => stat.FloatFactor).MaxAsync());
        Assert.AreEqual(maxDouble, await query.Select(stat => stat.DoubleFactor).MaxAsync());
        Assert.AreEqual(maxDecimal, await query.Select(stat => stat.DecimalFactor).MaxAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncOnNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor).Max();
        var maxLong = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor).Max();
        var maxFloat = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor).Max();
        var maxDouble = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor).Max();
        var maxDecimal = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor).Max();
        Assert.AreEqual(maxInt,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor).MaxAsync());
        Assert.AreEqual(maxLong,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor).MaxAsync());
        Assert.AreEqual(maxFloat,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor).MaxAsync());
        Assert.AreEqual(maxDouble,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor).MaxAsync());
        Assert.AreEqual(maxDecimal,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor).MaxAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Where(stat => stat.Id < 0)
          .Select(stat => stat.IntFactor);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => query.MaxAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncOnEmptyNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Where(stat => stat.Id < 0)
          .Select(stat => (int?) stat.IntFactor);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.IsNull(elements.Max());
        Assert.AreEqual(elements.Max(), await query.MaxAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Max(stat => stat.IntFactor);
        var maxLong = allStats.Max(stat => stat.LongFactor);
        var maxFloat = allStats.Max(stat => stat.FloatFactor);
        var maxDouble = allStats.Max(stat => stat.DoubleFactor);
        var maxDecimal = allStats.Max(stat => stat.DecimalFactor);
        Assert.AreEqual(maxInt, await query.MaxAsync(stat => stat.IntFactor));
        Assert.AreEqual(maxLong, await query.MaxAsync(stat => stat.LongFactor));
        Assert.AreEqual(maxFloat, await query.MaxAsync(stat => stat.FloatFactor));
        Assert.AreEqual(maxDouble, await query.MaxAsync(stat => stat.DoubleFactor));
        Assert.AreEqual(maxDecimal, await query.MaxAsync(stat => stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncWithSelectorOnNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Max(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor);
        var maxLong = allStats.Max(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor);
        var maxFloat = allStats.Max(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor);
        var maxDouble = allStats.Max(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor);
        var maxDecimal = allStats.Max(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor);
        Assert.AreEqual(maxInt,
          await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor));
        Assert.AreEqual(maxLong,
          await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor));
        Assert.AreEqual(maxFloat,
          await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor));
        Assert.AreEqual(maxDouble,
          await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor));
        Assert.AreEqual(maxDecimal,
          await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.Throws<InvalidOperationException>(() => _ = elements.Max(stat => stat.IntFactor));
        Assert.ThrowsAsync<InvalidOperationException>(() => query.MaxAsync(stat => stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncWithSelectorOnEmptyNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.IsNull(elements.Max(stat => (int?) stat.IntFactor));
        Assert.AreEqual(elements.Max(stat => (int?) stat.IntFactor), await query.MaxAsync(stat => (int?) stat.IntFactor));
      }
    }

    // Min

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Select(stat => stat.IntFactor).Min();
        var maxLong = allStats.Select(stat => stat.LongFactor).Min();
        var maxFloat = allStats.Select(stat => stat.FloatFactor).Min();
        var maxDouble = allStats.Select(stat => stat.DoubleFactor).Min();
        var maxDecimal = allStats.Select(stat => stat.DecimalFactor).Min();
        Assert.AreEqual(maxInt, await query.Select(stat => stat.IntFactor).MinAsync());
        Assert.AreEqual(maxLong, await query.Select(stat => stat.LongFactor).MinAsync());
        Assert.AreEqual(maxFloat, await query.Select(stat => stat.FloatFactor).MinAsync());
        Assert.AreEqual(maxDouble, await query.Select(stat => stat.DoubleFactor).MinAsync());
        Assert.AreEqual(maxDecimal, await query.Select(stat => stat.DecimalFactor).MinAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncOnNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor).Min();
        var maxLong = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor).Min();
        var maxFloat = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor).Min();
        var maxDouble = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor).Min();
        var maxDecimal = allStats.Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor).Min();
        Assert.AreEqual(maxInt,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor).MinAsync());
        Assert.AreEqual(maxLong,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor).MinAsync());
        Assert.AreEqual(maxFloat,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor).MinAsync());
        Assert.AreEqual(maxDouble,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor).MinAsync());
        Assert.AreEqual(maxDecimal,
          await query.Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor).MinAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Where(stat => stat.Id < 0)
          .Select(stat => stat.IntFactor);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.ThrowsAsync<InvalidOperationException>(() => query.MinAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncOnEmptyNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Where(stat => stat.Id < 0)
          .Select(stat => (int?) stat.IntFactor);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.IsNull(elements.Min());
        Assert.AreEqual(elements.Min(), await query.MinAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Min(stat => stat.IntFactor);
        var maxLong = allStats.Min(stat => stat.LongFactor);
        var maxFloat = allStats.Min(stat => stat.FloatFactor);
        var maxDouble = allStats.Min(stat => stat.DoubleFactor);
        var maxDecimal = allStats.Min(stat => stat.DecimalFactor);
        Assert.AreEqual(maxInt, await query.MinAsync(stat => stat.IntFactor));
        Assert.AreEqual(maxLong, await query.MinAsync(stat => stat.LongFactor));
        Assert.AreEqual(maxFloat, await query.MinAsync(stat => stat.FloatFactor));
        Assert.AreEqual(maxDouble, await query.MinAsync(stat => stat.DoubleFactor));
        Assert.AreEqual(maxDecimal, await query.MinAsync(stat => stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncWithSelectorOnNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();
        var maxInt = allStats.Min(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor);
        var maxLong = allStats.Min(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor);
        var maxFloat = allStats.Min(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor);
        var maxDouble = allStats.Min(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor);
        var maxDecimal = allStats.Min(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor);
        Assert.AreEqual(maxInt,
          await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor));
        Assert.AreEqual(maxLong,
          await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor));
        Assert.AreEqual(maxFloat,
          await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor));
        Assert.AreEqual(maxDouble,
          await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor));
        Assert.AreEqual(maxDecimal,
          await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.Throws<InvalidOperationException>(() => _ = elements.Min(stat => stat.IntFactor));
        Assert.ThrowsAsync<InvalidOperationException>(() => query.MinAsync(stat => stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncWithSelectorOnEmptyNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.AreEqual(0, elements.Count);
        Assert.IsNull(elements.Min(stat => (int?) stat.IntFactor));
        Assert.AreEqual(elements.Min(stat => (int?) stat.IntFactor), await query.MinAsync(stat => (int?) stat.IntFactor));
      }
    }

    // Single

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id).Take(1);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers[0], await query.SingleAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers[0], await query.SingleAsync(teacher => teacher.Id==allTeachers[0].Id));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync(teacher => teacher.Id < 0));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncWithPredicateOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync(teacher => teacher.Gender==Gender.Male));
      }
    }

    // SingleOrDefault

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id).Take(1);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers[0], await query.SingleOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.IsNull(await query.SingleOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.AreEqual(allTeachers[0], await query.SingleOrDefaultAsync(teacher => teacher.Id==allTeachers[0].Id));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.IsNull(await query.SingleOrDefaultAsync(teacher => teacher.Id < 0));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncWithPredicateOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.ThrowsAsync<InvalidOperationException>(
          () => query.SingleOrDefaultAsync(teacher => teacher.Gender==Gender.Male));
      }
    }

    // Sum<int>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncIntExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.IntFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncIntOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.IntFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0, await emptyQuery.SumAsync());
      }
    }

    // Sum<int>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncIntWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.IntFactor).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync(stat => stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncIntWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.IntFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0, await emptyQuery.SumAsync(stat => stat.IntFactor));
      }
    }

    // Sum<int?>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (int?)stat.IntFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(int?) : stat.IntFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Sum(), await nullQuery.SumAsync());
      }
    }

    // Sum<int?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (int?)stat.IntFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync(stat => (int?)stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(int?)).ToList();
        Assert.AreEqual(
          nullFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(int?) : stat.IntFactor));
      }
    }

    // Sum<long>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncLongExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.LongFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncLongOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.LongFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0L, await emptyQuery.SumAsync());
      }
    }

    // Sum<long>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncLongWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.LongFactor).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync(stat => stat.LongFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncLongWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.LongFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0L, await emptyQuery.SumAsync(stat => stat.LongFactor));
      }
    }

    // Sum<long?>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (long?)stat.LongFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(long?) : stat.LongFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Sum(), await nullQuery.SumAsync());
      }
    }

    // Sum<long?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (long?)stat.LongFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync(stat => (long?)stat.LongFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(long?)).ToList();
        Assert.AreEqual(
          nullFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(long?) : stat.LongFactor));
      }
    }

    // Sum<double>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDoubleExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.DoubleFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDoubleOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.DoubleFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0.0, await emptyQuery.SumAsync());
      }
    }

    // Sum<double>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDoubleWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.DoubleFactor).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync(stat => stat.DoubleFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDoubleWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.DoubleFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0.0, await emptyQuery.SumAsync(stat => stat.DoubleFactor));
      }
    }

    // Sum<double?>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (double?)stat.DoubleFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(double?) : stat.DoubleFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Sum(), await nullQuery.SumAsync());
      }
    }

    // Sum<double?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (double?)stat.DoubleFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync(stat => (double?)stat.DoubleFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(double?)).ToList();
        Assert.AreEqual(
          nullFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(double?) : stat.DoubleFactor));
      }
    }

    // Sum<float>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncFloatExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.FloatFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncFloatOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.FloatFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0.0f, await emptyQuery.SumAsync());
      }
    }

    // Sum<float>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncFloatWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.FloatFactor).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync(stat => stat.FloatFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncFloatWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.FloatFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0.0f, await emptyQuery.SumAsync(stat => stat.FloatFactor));
      }
    }

    // Sum<float?>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (float?)stat.FloatFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(float?) : stat.FloatFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Sum(), await nullQuery.SumAsync());
      }
    }

    // Sum<float?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (float?)stat.FloatFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync(stat => (float?)stat.FloatFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(float?)).ToList();
        Assert.AreEqual(
          nullFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(float?) : stat.FloatFactor));
      }
    }

    // Sum<decimal>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDecimalExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.DecimalFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDecimalOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => stat.DecimalFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0.0m, await emptyQuery.SumAsync());
      }
    }

    // Sum<decimal>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDecimalWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync()).Select(stat => stat.DecimalFactor).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync(stat => stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncDecimalWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => stat.DecimalFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(0.0f, await emptyQuery.SumAsync(stat => stat.DecimalFactor));
      }
    }

    // Sum<decimal?>

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor);
        var allFactors = (await query.ExecuteAsync()).ToList();
        Assert.AreEqual(allFactors.Sum(), await query.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>()
          .Where(stat => stat.IntFactor < 0).Select(stat => (decimal?)stat.DecimalFactor);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var nullQuery = session.Query.All<StatRecord>()
          .Select(stat => stat.IntFactor >= 0 ? default(decimal?) : stat.DecimalFactor);

        var nullFactors = (await nullQuery.ExecuteAsync()).ToList();
        foreach (var factor in nullFactors) {
          Assert.IsNull(factor);
        }
        Assert.AreEqual(nullFactors.Sum(), await nullQuery.SumAsync());
      }
    }

    // Sum<decimal?>(selector)

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalWithSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allFactors = (await query.ExecuteAsync())
          .Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor)
          .ToList();
        Assert.AreEqual(
          allFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (decimal?)stat.DecimalFactor).ToList();
        Assert.AreEqual(0, emptyFactors.Count);
        Assert.AreEqual(emptyFactors.Sum(), await emptyQuery.SumAsync(stat => (decimal?)stat.DecimalFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(decimal?)).ToList();
        Assert.AreEqual(
          nullFactors.Sum(),
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(decimal?) : stat.DecimalFactor));
      }
    }

    // ToList

    [Test, TestCase(true), TestCase(false)]
    public async Task ToListAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        var allTeachersAsync = await query.ToListAsync();
        Assert.IsTrue(allTeachers.SequenceEqual(allTeachersAsync));
      }
    }

    // ToArray

    [Test, TestCase(true), TestCase(false)]
    public async Task ToArrayAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToArray();
        var allTeachersAsync = await query.ToArrayAsync();
        Assert.IsTrue(allTeachers.SequenceEqual(allTeachersAsync));
      }
    }

    // ToDictionary

    [Test, TestCase(true), TestCase(false)]
    public async Task ToDictionaryAsyncWithKeySelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        var allTeachers = query.ToDictionary(teacher => teacher.Id);
        var allTeachersAsync = await query.ToDictionaryAsync(teacher => teacher.Id);
        Assert.AreEqual(allTeachers.Count, allTeachersAsync.Count);
        foreach (var teacherId in allTeachers.Keys) {
          Assert.AreEqual(allTeachers[teacherId], allTeachersAsync[teacherId]);
        }
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task ToDictionaryAsyncWithKeyValueSelectorsExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        var allTeachers = query.ToDictionary(teacher => teacher.Id, teacher => teacher.Id);
        var allTeachersAsync = await query.ToDictionaryAsync(teacher => teacher.Id, teacher => teacher.Id);
        Assert.AreEqual(allTeachers.Count, allTeachersAsync.Count);
        foreach (var teacherId in allTeachers.Keys) {
          Assert.AreEqual(allTeachers[teacherId], allTeachersAsync[teacherId]);
        }
      }
    }

    // ToHashSet

    [Test, TestCase(true), TestCase(false)]
    public async Task ToHashSetAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        var allTeachers = query.ToHashSet();
        var allTeachersAsync = await query.ToHashSetAsync();
        Assert.AreEqual(allTeachers.Count, allTeachersAsync.Count);
        foreach (var teacher in allTeachers) {
          Assert.IsTrue(allTeachersAsync.Contains(teacher));
        }
      }
    }

    // ToLookup

    [Test, TestCase(true), TestCase(false)]
    public async Task ToLookupAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        var teachersByGender = query.ToLookup(teacher => teacher.Gender);
        var teachersByGenderAsync = await query.ToLookupAsync(teacher => teacher.Gender);
        Assert.AreEqual(teachersByGender.Count, teachersByGenderAsync.Count);
        foreach (var grouping in teachersByGender) {
          Assert.IsTrue(grouping.OrderBy(teacher => teacher.Id)
            .SequenceEqual(teachersByGenderAsync[grouping.Key].OrderBy(teacher => teacher.Id)));
        }
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task ToLookupAsyncWithValueSelectorExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        var teachersByGender = query.ToLookup(teacher => teacher.Gender, teacher => teacher.Id);
        var teachersByGenderAsync = await query.ToLookupAsync(teacher => teacher.Gender, teacher => teacher.Id);
        Assert.AreEqual(teachersByGender.Count, teachersByGenderAsync.Count);
        foreach (var grouping in teachersByGender) {
          Assert.IsTrue(grouping.OrderBy(teacherId => teacherId)
            .SequenceEqual(teachersByGenderAsync[grouping.Key].OrderBy(teacherId => teacherId)));
        }
      }
    }
  }
}