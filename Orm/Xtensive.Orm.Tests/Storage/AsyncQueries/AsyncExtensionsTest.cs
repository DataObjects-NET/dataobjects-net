// Copyright (C) 2020-2024 Xtensive LLC.
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
        Assert.That(await query.Take(0).AllAsync(teacher => teacher.Gender==Gender.Male), Is.True);
        Assert.That(await query.Take(0).AllAsync(teacher => teacher.Gender==Gender.Female), Is.True);
        Assert.That(
          await query.Where(teacher => teacher.Gender==Gender.Female).AllAsync(teacher => teacher.Gender==Gender.Male), Is.False);
        Assert.That(await query.AllAsync(teacher => teacher.Gender==Gender.Male), Is.False);
        Assert.That(
          await query.Where(teacher => teacher.Gender==Gender.Male).AllAsync(teacher => teacher.Gender==Gender.Male), Is.True);
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
        Assert.That(await query.AnyAsync(), Is.True);
        Assert.That(await query.Where(teacher => teacher.Name==null).AnyAsync(), Is.False);
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
        Assert.That(
          await query.Where(teacher => teacher.Name==null).AnyAsync(teacher => teacher.Gender==Gender.Male), Is.False);
        Assert.That(
          await query.Where(teacher => teacher.Name==null).AnyAsync(teacher => teacher.Gender==Gender.Female), Is.False);
        Assert.That(
          await query.Where(teacher => teacher.Gender==Gender.Female).AnyAsync(teacher => teacher.Gender==Gender.Male), Is.False);
        Assert.That(await query.AnyAsync(teacher => teacher.Gender==Gender.Male), Is.True);
        Assert.That(
          await query.Where(teacher => teacher.Gender==Gender.Male).AnyAsync(teacher => teacher.Gender==Gender.Male), Is.True);
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
        //"If Field is of an integer type, AVG is always rounded towards 0.
        // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
        // © Firebird documentation
        // Funny, isn't it?
        var expectedValue = (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird))
          ? Math.Truncate(allFactors.Average())
          : allFactors.Average();

        Assert.That(await query.AverageAsync(), Is.EqualTo(expectedValue));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
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

        //"If Field is of an integer type, AVG is always rounded towards 0.
        // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
        // © Firebird documentation
        // Funny, isn't it?
        var expectedValue = (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird))
          ? Math.Truncate(allFactors.Average())
          : allFactors.Average();

        Assert.That(await query.AverageAsync(stat => stat.IntFactor), Is.EqualTo(expectedValue));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.IntFactor));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(), Is.Null);
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.AverageAsync(), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(
          await query.AverageAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor), Is.EqualTo(allFactors.Average()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (int?)stat.IntFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(stat => (int?)stat.IntFactor), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableIntWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(int?)).ToList();
        Assert.That(
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(int?) : stat.IntFactor), Is.EqualTo(nullFactors.Average()));
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

        //"If Field is of an integer type, AVG is always rounded towards 0.
        // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
        // © Firebird documentation
        // Funny, isn't it?
        var expectedValue = (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird))
          ? Math.Truncate(allFactors.Average())
          : allFactors.Average();

        Assert.That(await query.AverageAsync(), Is.EqualTo(expectedValue));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
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

        //"If Field is of an integer type, AVG is always rounded towards 0.
        // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
        // © Firebird documentation
        // Funny, isn't it?
        var expectedValue = (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird))
          ? Math.Truncate(allFactors.Average())
          : allFactors.Average();

        Assert.That(await query.AverageAsync(stat => stat.LongFactor), Is.EqualTo(expectedValue));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.LongFactor));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(), Is.Null);
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.AverageAsync(), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(long?) : stat.LongFactor), Is.EqualTo(allFactors.Average()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (long?)stat.LongFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(stat => (long?)stat.LongFactor), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableLongWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(long?)).ToList();
        Assert.That(
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(long?) : stat.LongFactor), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
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
        Assert.That(await query.AverageAsync(stat => stat.DoubleFactor), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.DoubleFactor));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(), Is.Null);
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.AverageAsync(), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(double?) : stat.DoubleFactor), Is.EqualTo(allFactors.Average()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (double?)stat.DoubleFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(stat => (double?)stat.DoubleFactor), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDoubleWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(double?)).ToList();
        Assert.That(
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(double?) : stat.DoubleFactor), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
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
        Assert.That(await query.AverageAsync(stat => stat.FloatFactor), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.FloatFactor));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(), Is.Null);
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.AverageAsync(), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(float?) : stat.FloatFactor), Is.EqualTo(allFactors.Average()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (float?)stat.FloatFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(stat => (float?)stat.FloatFactor), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableFloatWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(float?)).ToList();
        Assert.That(
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(float?) : stat.FloatFactor), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync());
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
        Assert.That(await query.AverageAsync(stat => stat.DecimalFactor), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => emptyQuery.AverageAsync(stat => stat.DecimalFactor));
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
        Assert.That(await query.AverageAsync(), Is.EqualTo(allFactors.Average()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(), Is.Null);
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.AverageAsync(), Is.EqualTo(nullFactors.Average()));
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
        Assert.That(
          await query.AverageAsync(stat => stat.LongFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor), Is.EqualTo(allFactors.Average()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (decimal?)stat.DecimalFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.AverageAsync(stat => (decimal?)stat.DecimalFactor), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task AverageAsyncNullableDecimalWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(decimal?)).ToList();
        Assert.That(
          await query.AverageAsync(stat => stat.IntFactor >= 0 ? default(decimal?) : stat.DecimalFactor), Is.EqualTo(nullFactors.Average()));
      }
    }

    // Contains

    [Test, TestCase(true), TestCase(false)]
    public async Task ContainsAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Select(stat => stat.IntFactor);

        Assert.That(await query.ContainsAsync(50), Is.True);
        Assert.That(await query.ContainsAsync(-1), Is.False);
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

        Assert.That(await query.Where(stat => stat.IntFactor < 0).CountAsync(), Is.EqualTo(0));
        Assert.That(
          await query.Where(stat => stat.IntFactor < 0).CountAsync(), Is.EqualTo(allStats.Count(stat => stat.IntFactor < 0)));

        Assert.That(await query.Where(stat => stat.IntFactor < 10).CountAsync(), Is.EqualTo(10));
        Assert.That(
          await query.Where(stat => stat.IntFactor < 10).CountAsync(), Is.EqualTo(allStats.Count(stat => stat.IntFactor < 10)));

        Assert.That(await query.CountAsync(), Is.EqualTo(100));
        Assert.That(
          await query.CountAsync(), Is.EqualTo(allStats.Count));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task CountAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();

        Assert.That(await query.CountAsync(stat => stat.IntFactor < 0), Is.EqualTo(0));
        Assert.That(
          await query.CountAsync(stat => stat.IntFactor < 0), Is.EqualTo(allStats.Count(stat => stat.IntFactor < 0)));

        Assert.That(await query.CountAsync(stat => stat.IntFactor < 10), Is.EqualTo(10));
        Assert.That(
          await query.CountAsync(stat => stat.IntFactor < 10), Is.EqualTo(allStats.Count(stat => stat.IntFactor < 10)));
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
        Assert.That(await query.FirstAsync(), Is.EqualTo(allTeachers[0]));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.FirstAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var firstFemaleTeacher = query.AsEnumerable().First(teacher => teacher.Gender==Gender.Female);
        Assert.That(await query.FirstAsync(teacher => teacher.Gender==Gender.Female), Is.EqualTo(firstFemaleTeacher));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.FirstAsync(teacher => teacher.Id < 0));
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
        Assert.That(await query.FirstOrDefaultAsync(), Is.EqualTo(allTeachers[0]));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstOrDefaultAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.That(await query.FirstOrDefaultAsync(), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstOrDefaultAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var firstFemaleTeacher = query.AsEnumerable().First(teacher => teacher.Gender==Gender.Female);
        Assert.That(await query.FirstOrDefaultAsync(teacher => teacher.Gender==Gender.Female), Is.EqualTo(firstFemaleTeacher));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task FirstOrDefaultAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.That(await query.FirstOrDefaultAsync(teacher => teacher.Id < 0), Is.Null);
      }
    }

    // Last

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastAsync is not supported yet.")]
    public async Task LastAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.That(await query.LastAsync(), Is.EqualTo(allTeachers.Last()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastAsync is not supported yet.")]
    public async Task LastAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.LastAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastAsync is not supported yet.")]
    public async Task LastAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var lastInListFemaleTeacher = query.AsEnumerable().Last(teacher => teacher.Gender==Gender.Female);
        Assert.That(await query.LastAsync(teacher => teacher.Gender==Gender.Female), Is.EqualTo(lastInListFemaleTeacher));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastAsync is not supported yet.")]
    public async Task LastAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.LastAsync(teacher => teacher.Id < 0));
      }
    }

    // LastOrDefault

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastOrDefaultAsync is not supported yet.")]
    public async Task LastOrDefaultAsyncExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.That(await query.LastOrDefaultAsync(), Is.EqualTo(allTeachers.Last()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastOrDefaultAsync is not supported yet.")]
    public async Task LastOrDefaultAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.That(await query.LastOrDefaultAsync(), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastOrDefaultAsync is not supported yet.")]
    public async Task LastOrDefaultAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var lastInListFemaleTeacher = query.AsEnumerable().First(teacher => teacher.Gender==Gender.Female);
        Assert.That(
await query.LastOrDefaultAsync(teacher => teacher.Gender==Gender.Female), Is.EqualTo(lastInListFemaleTeacher));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    [IgnoreIfGithubActions("LastOrDefaultAsync is not supported yet.")]
    public async Task LastOrDefaultAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.That(await query.LastOrDefaultAsync(teacher => teacher.Id < 0), Is.Null);
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

        Assert.That(await query.Where(stat => stat.IntFactor < 0).LongCountAsync(), Is.EqualTo(0L));
        Assert.That(
          await query.Where(stat => stat.IntFactor < 0).LongCountAsync(), Is.EqualTo(allStats.LongCount(stat => stat.IntFactor < 0)));

        Assert.That(await query.Where(stat => stat.IntFactor < 10).LongCountAsync(), Is.EqualTo(10L));
        Assert.That(
          await query.Where(stat => stat.IntFactor < 10).LongCountAsync(), Is.EqualTo(allStats.LongCount(stat => stat.IntFactor < 10)));

        Assert.That(await query.LongCountAsync(), Is.EqualTo(100L));
        Assert.That(
          await query.LongCountAsync(), Is.EqualTo(allStats.LongCount()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task LongCountAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();
        var allStats = query.ToList();

        Assert.That(await query.LongCountAsync(stat => stat.IntFactor < 0), Is.EqualTo(0L));
        Assert.That(
          await query.LongCountAsync(stat => stat.IntFactor < 0), Is.EqualTo(allStats.LongCount(stat => stat.IntFactor < 0)));

        Assert.That(await query.LongCountAsync(stat => stat.IntFactor < 10), Is.EqualTo(10L));
        Assert.That(
          await query.LongCountAsync(stat => stat.IntFactor < 10), Is.EqualTo(allStats.LongCount(stat => stat.IntFactor < 10)));
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
        Assert.That(await query.Select(stat => stat.IntFactor).MaxAsync(), Is.EqualTo(maxInt));
        Assert.That(await query.Select(stat => stat.LongFactor).MaxAsync(), Is.EqualTo(maxLong));
        Assert.That(await query.Select(stat => stat.FloatFactor).MaxAsync(), Is.EqualTo(maxFloat));
        Assert.That(await query.Select(stat => stat.DoubleFactor).MaxAsync(), Is.EqualTo(maxDouble));
        Assert.That(await query.Select(stat => stat.DecimalFactor).MaxAsync(), Is.EqualTo(maxDecimal));
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
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor).MaxAsync(), Is.EqualTo(maxInt));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor).MaxAsync(), Is.EqualTo(maxLong));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor).MaxAsync(), Is.EqualTo(maxFloat));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor).MaxAsync(), Is.EqualTo(maxDouble));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor).MaxAsync(), Is.EqualTo(maxDecimal));
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
        Assert.That(elements.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.MaxAsync());
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
        Assert.That(elements.Count, Is.EqualTo(0));
        Assert.That(elements.Max(), Is.Null);
        Assert.That(await query.MaxAsync(), Is.EqualTo(elements.Max()));
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
        Assert.That(await query.MaxAsync(stat => stat.IntFactor), Is.EqualTo(maxInt));
        Assert.That(await query.MaxAsync(stat => stat.LongFactor), Is.EqualTo(maxLong));
        Assert.That(await query.MaxAsync(stat => stat.FloatFactor), Is.EqualTo(maxFloat));
        Assert.That(await query.MaxAsync(stat => stat.DoubleFactor), Is.EqualTo(maxDouble));
        Assert.That(await query.MaxAsync(stat => stat.DecimalFactor), Is.EqualTo(maxDecimal));
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
        Assert.That(await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor), Is.EqualTo(maxInt));
        Assert.That(await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor), Is.EqualTo(maxLong));
        Assert.That(await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor), Is.EqualTo(maxFloat));
        Assert.That(await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor), Is.EqualTo(maxDouble));
        Assert.That(await query.MaxAsync(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor), Is.EqualTo(maxDecimal));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.That(elements.Count, Is.EqualTo(0));
        _ = Assert.Throws<InvalidOperationException>(() => _ = elements.Max(stat => stat.IntFactor));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.MaxAsync(stat => stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MaxAsyncWithSelectorOnEmptyNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.That(elements.Count, Is.EqualTo(0));
        Assert.That(elements.Max(stat => (int?) stat.IntFactor), Is.Null);
        Assert.That(await query.MaxAsync(stat => (int?) stat.IntFactor), Is.EqualTo(elements.Max(stat => (int?) stat.IntFactor)));
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
        Assert.That(await query.Select(stat => stat.IntFactor).MinAsync(), Is.EqualTo(maxInt));
        Assert.That(await query.Select(stat => stat.LongFactor).MinAsync(), Is.EqualTo(maxLong));
        Assert.That(await query.Select(stat => stat.FloatFactor).MinAsync(), Is.EqualTo(maxFloat));
        Assert.That(await query.Select(stat => stat.DoubleFactor).MinAsync(), Is.EqualTo(maxDouble));
        Assert.That(await query.Select(stat => stat.DecimalFactor).MinAsync(), Is.EqualTo(maxDecimal));
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
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor).MinAsync(), Is.EqualTo(maxInt));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor).MinAsync(), Is.EqualTo(maxLong));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor).MinAsync(), Is.EqualTo(maxFloat));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor).MinAsync(), Is.EqualTo(maxDouble));
        Assert.That(await query.Select(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor).MinAsync(), Is.EqualTo(maxDecimal));
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
        Assert.That(elements.Count, Is.EqualTo(0));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.MinAsync());
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
        Assert.That(elements.Count, Is.EqualTo(0));
        Assert.That(elements.Min(), Is.Null);
        Assert.That(await query.MinAsync(), Is.EqualTo(elements.Min()));
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
        Assert.That(await query.MinAsync(stat => stat.IntFactor), Is.EqualTo(maxInt));
        Assert.That(await query.MinAsync(stat => stat.LongFactor), Is.EqualTo(maxLong));
        Assert.That(await query.MinAsync(stat => stat.FloatFactor), Is.EqualTo(maxFloat));
        Assert.That(await query.MinAsync(stat => stat.DoubleFactor), Is.EqualTo(maxDouble));
        Assert.That(await query.MinAsync(stat => stat.DecimalFactor), Is.EqualTo(maxDecimal));
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
        Assert.That(await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor), Is.EqualTo(maxInt));
        Assert.That(await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor), Is.EqualTo(maxLong));
        Assert.That(await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor), Is.EqualTo(maxFloat));
        Assert.That(await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor), Is.EqualTo(maxDouble));
        Assert.That(await query.MinAsync(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor), Is.EqualTo(maxDecimal));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.That(elements.Count, Is.EqualTo(0));
        _ = Assert.Throws<InvalidOperationException>(() => _ = elements.Min(stat => stat.IntFactor));
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.MinAsync(stat => stat.IntFactor));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task MinAsyncWithSelectorOnEmptyNullableSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>().Where(stat => stat.Id < 0);
        var elements = query.ToList();
        Assert.That(elements.Count, Is.EqualTo(0));
        Assert.That(elements.Min(stat => (int?) stat.IntFactor), Is.Null);
        Assert.That(await query.MinAsync(stat => (int?) stat.IntFactor), Is.EqualTo(elements.Min(stat => (int?) stat.IntFactor)));
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
        Assert.That(await query.SingleAsync(), Is.EqualTo(allTeachers[0]));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.That(await query.SingleAsync(teacher => teacher.Id==allTeachers[0].Id), Is.EqualTo(allTeachers[0]));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync(teacher => teacher.Id < 0));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleAsyncWithPredicateOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync(teacher => teacher.Gender==Gender.Male));
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
        Assert.That(await query.SingleOrDefaultAsync(), Is.EqualTo(allTeachers[0]));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().Take(0);
        Assert.That(await query.SingleOrDefaultAsync(), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        _ = Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncWithPredicateExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>().OrderBy(teacher => teacher.Id);
        var allTeachers = query.ToList();
        Assert.That(await query.SingleOrDefaultAsync(teacher => teacher.Id==allTeachers[0].Id), Is.EqualTo(allTeachers[0]));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncWithPredicateOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        Assert.That(await query.SingleOrDefaultAsync(teacher => teacher.Id < 0), Is.Null);
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SingleOrDefaultAsyncWithPredicateOnSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<Teacher>();
        _ = Assert.ThrowsAsync<InvalidOperationException>(
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(0));
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
        Assert.That(await query.SumAsync(stat => stat.IntFactor), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => stat.IntFactor), Is.EqualTo(0));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(emptyFactors.Sum()));
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.SumAsync(), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(int?) : stat.IntFactor), Is.EqualTo(allFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (int?)stat.IntFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => (int?)stat.IntFactor), Is.EqualTo(emptyFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableIntWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(int?)).ToList();
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(int?) : stat.IntFactor), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(0L));
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
        Assert.That(await query.SumAsync(stat => stat.LongFactor), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => stat.LongFactor), Is.EqualTo(0L));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(emptyFactors.Sum()));
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.SumAsync(), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(long?) : stat.LongFactor), Is.EqualTo(allFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (long?)stat.LongFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => (long?)stat.LongFactor), Is.EqualTo(emptyFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableLongWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(long?)).ToList();
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(long?) : stat.LongFactor), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(0.0));
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
        Assert.That(await query.SumAsync(stat => stat.DoubleFactor), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => stat.DoubleFactor), Is.EqualTo(0.0));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(emptyFactors.Sum()));
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.SumAsync(), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(double?) : stat.DoubleFactor), Is.EqualTo(allFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (double?)stat.DoubleFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => (double?)stat.DoubleFactor), Is.EqualTo(emptyFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDoubleWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(double?)).ToList();
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(double?) : stat.DoubleFactor), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(0.0f));
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
        Assert.That(await query.SumAsync(stat => stat.FloatFactor), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => stat.FloatFactor), Is.EqualTo(0.0f));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(emptyFactors.Sum()));
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.SumAsync(), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(float?) : stat.FloatFactor), Is.EqualTo(allFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (float?)stat.FloatFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => (float?)stat.FloatFactor), Is.EqualTo(emptyFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableFloatWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(float?)).ToList();
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(float?) : stat.FloatFactor), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(0.0m));
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
        Assert.That(await query.SumAsync(stat => stat.DecimalFactor), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => stat.DecimalFactor), Is.EqualTo(0.0f));
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
        Assert.That(await query.SumAsync(), Is.EqualTo(allFactors.Sum()));
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
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(), Is.EqualTo(emptyFactors.Sum()));
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
          Assert.That(factor, Is.Null);
        }
        Assert.That(await nullQuery.SumAsync(), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor % 2 == 0 ? default(decimal?) : stat.DecimalFactor), Is.EqualTo(allFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalWithSelectorOnEmptySequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var emptyQuery = session.Query.All<StatRecord>().Where(stat => stat.IntFactor < 0);

        var emptyFactors = (await emptyQuery.ExecuteAsync()).Select(stat => (decimal?)stat.DecimalFactor).ToList();
        Assert.That(emptyFactors.Count, Is.EqualTo(0));
        Assert.That(await emptyQuery.SumAsync(stat => (decimal?)stat.DecimalFactor), Is.EqualTo(emptyFactors.Sum()));
      }
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task SumAsyncNullableDecimalWithSelectorOnNullSequenceExtensionTest(bool isClientProfile)
    {
      await using var session = await OpenSessionAsync(Domain, isClientProfile);
      await using (OpenTransactionAsync(session, isClientProfile)) {
        var query = session.Query.All<StatRecord>();

        var nullFactors = (await query.ExecuteAsync()).Select(stat => default(decimal?)).ToList();
        Assert.That(
          await query.SumAsync(stat => stat.IntFactor >= 0 ? default(decimal?) : stat.DecimalFactor), Is.EqualTo(nullFactors.Sum()));
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
        Assert.That(allTeachers.SequenceEqual(allTeachersAsync), Is.True);

        var firstTeacher = allTeachers[0];
        var disceplines = firstTeacher.Disciplines.Where(d => d.Discepline != null).ToList();
        var disceplinesAsync = await firstTeacher.Disciplines.Where(d => d.Discepline != null).ToListAsync();
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);

        var secondTeacher = allTeachers[1];
        disceplines = secondTeacher.Disciplines.ToList();
        disceplinesAsync = await secondTeacher.Disciplines.ToListAsync();
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);
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
        Assert.That(allTeachers.SequenceEqual(allTeachersAsync), Is.True);

        var firstTeacher = allTeachers[0];
        var disceplines = firstTeacher.Disciplines.Where(d => d.Discepline != null).ToArray();
        var disceplinesAsync = await firstTeacher.Disciplines.Where(d => d.Discepline != null).ToArrayAsync();
        Assert.That(disceplines.Length, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Length, Is.EqualTo(disceplines.Length));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);

        var secondTeacher = allTeachers[1];
        disceplines = secondTeacher.Disciplines.ToArray();
        disceplinesAsync = await secondTeacher.Disciplines.ToArrayAsync();
        Assert.That(disceplines.Length, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Length, Is.EqualTo(disceplines.Length));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);
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
        Assert.That(allTeachersAsync.Count, Is.EqualTo(allTeachers.Count));
        foreach (var teacherId in allTeachers.Keys) {
          Assert.That(allTeachersAsync[teacherId], Is.EqualTo(allTeachers[teacherId]));
        }

        var firstTeacher = allTeachers.Values.First();
        var disceplines = firstTeacher.Disciplines.Where(d => d.Discepline != null).ToDictionary(d => d.Course.Id);
        var disceplinesAsync = await firstTeacher.Disciplines.Where(d => d.Discepline != null).ToDictionaryAsync(d => d.Course.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);

        disceplines = firstTeacher.Disciplines.ToDictionary(d => d.Course.Id);
        disceplinesAsync = await firstTeacher.Disciplines.ToDictionaryAsync(d => d.Course.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);
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
        Assert.That(allTeachersAsync.Count, Is.EqualTo(allTeachers.Count));
        foreach (var teacherId in allTeachers.Keys) {
          Assert.That(allTeachersAsync[teacherId], Is.EqualTo(allTeachers[teacherId]));
        }

        var firstTeacher = session.Query.All<Teacher>().First();
        var disceplines = firstTeacher.Disciplines.Where(d => d.Discepline != null).ToDictionary(d => d.Course.Id, d => d.Discepline.Id);
        var disceplinesAsync = await firstTeacher.Disciplines.Where(d => d.Discepline != null).ToDictionaryAsync(d => d.Course.Id, d => d.Discepline.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);


        disceplines = firstTeacher.Disciplines.ToDictionary(d => d.Course.Id, d => d.Discepline.Id);
        disceplinesAsync = await firstTeacher.Disciplines.ToDictionaryAsync(d => d.Course.Id, d => d.Discepline.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);
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
        Assert.That(allTeachersAsync.Count, Is.EqualTo(allTeachers.Count));
        foreach (var teacher in allTeachers) {
          Assert.That(allTeachersAsync.Contains(teacher), Is.True);
        }

        var firstTeacher = session.Query.All<Teacher>().First();
        var disceplines = firstTeacher.Disciplines.Where(d => d.Discepline != null).ToHashSet();
        var disceplinesAsync = await firstTeacher.Disciplines.Where(d => d.Discepline != null).ToHashSetAsync();
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);

        disceplines = firstTeacher.Disciplines.ToHashSet();
        disceplinesAsync = await firstTeacher.Disciplines.ToHashSetAsync();
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        Assert.That(disceplines.Except(disceplinesAsync), Is.Empty);
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
        Assert.That(teachersByGenderAsync.Count, Is.EqualTo(teachersByGender.Count));
        foreach (var grouping in teachersByGender) {
          Assert.That(grouping.OrderBy(teacher => teacher.Id)
            .SequenceEqual(teachersByGenderAsync[grouping.Key].OrderBy(teacher => teacher.Id)), Is.True);
        }

        var firstTeacher = session.Query.All<Teacher>().First();
        var disceplines = firstTeacher.Disciplines.Where(d => d.Discepline != null).ToLookup(d => d.Discepline.Id);
        var disceplinesAsync = await firstTeacher.Disciplines.Where(d => d.Discepline != null).ToLookupAsync(d => d.Discepline.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        var zipped = disceplines
          .Zip(disceplinesAsync,
            (first, second) => new {
              FirstKey = first.Key,
              FirstSeq = first.AsEnumerable(),
              SecondKey = second.Key,
              SecondSeq = second.AsEnumerable()
            });
        foreach (var pair in zipped) {
          Assert.That(pair.FirstKey, Is.EqualTo(pair.SecondKey));
          Assert.That(pair.FirstSeq.Except(pair.SecondSeq), Is.Empty);
        }

        disceplines = firstTeacher.Disciplines.ToLookup(d => d.Discepline.Id);
        disceplinesAsync = await firstTeacher.Disciplines.ToLookupAsync(d => d.Discepline.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        zipped = disceplines
          .Zip(disceplinesAsync,
            (first, second) => new {
              FirstKey = first.Key,
              FirstSeq = first.AsEnumerable(),
              SecondKey = second.Key,
              SecondSeq = second.AsEnumerable()
            });
        foreach (var pair in zipped) {
          Assert.That(pair.FirstKey, Is.EqualTo(pair.SecondKey));
          Assert.That(pair.FirstSeq.Except(pair.SecondSeq), Is.Empty);
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
        Assert.That(teachersByGenderAsync.Count, Is.EqualTo(teachersByGender.Count));
        foreach (var grouping in teachersByGender) {
          Assert.That(grouping.OrderBy(teacherId => teacherId)
            .SequenceEqual(teachersByGenderAsync[grouping.Key].OrderBy(teacherId => teacherId)), Is.True);
        }

        var firstTeacher = session.Query.All<Teacher>().First();
        var disceplines = firstTeacher.Disciplines.Where(d => d.Discepline != null).ToLookup(d => d.Discepline.Id, d => d.Course.Id);
        var disceplinesAsync = await firstTeacher.Disciplines.Where(d => d.Discepline != null).ToLookupAsync(d => d.Discepline.Id, d => d.Course.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        var zipped = disceplines
          .Zip(disceplinesAsync,
            (first, second) => new {
              FirstKey = first.Key,
              FirstSeq = first.AsEnumerable(),
              SecondKey = second.Key,
              SecondSeq = second.AsEnumerable()
            });
        foreach (var pair in zipped) {
          Assert.That(pair.FirstKey, Is.EqualTo(pair.SecondKey));
          Assert.That(pair.FirstSeq.Except(pair.SecondSeq), Is.Empty);
        }

        disceplines = firstTeacher.Disciplines.ToLookup(d => d.Discepline.Id, d => d.Course.Id);
        disceplinesAsync = await firstTeacher.Disciplines.ToLookupAsync(d => d.Discepline.Id, d => d.Course.Id);
        Assert.That(disceplines.Count, Is.GreaterThan(0));
        Assert.That(disceplinesAsync.Count, Is.EqualTo(disceplines.Count));
        zipped = disceplines
          .Zip(disceplinesAsync,
            (first, second) => new {
              FirstKey = first.Key,
              FirstSeq = first.AsEnumerable(),
              SecondKey = second.Key,
              SecondSeq = second.AsEnumerable()
            });
        foreach (var pair in zipped) {
          Assert.That(pair.FirstKey, Is.EqualTo(pair.SecondKey));
          Assert.That(pair.FirstSeq.Except(pair.SecondSeq), Is.Empty);
        }
      }
    }
  }
}