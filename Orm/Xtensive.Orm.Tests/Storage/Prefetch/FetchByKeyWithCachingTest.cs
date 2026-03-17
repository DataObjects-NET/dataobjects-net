// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class FetchByKeyWithCachingTest : AutoBuildTest
  {
    private Dictionary<TypeInfo, List<Key>> existingKeys;
    private TypeInfo customerType;


    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.RegisterCaching(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      PrefetchTestHelper.FillDataBase(Domain, out existingKeys);
      customerType = Domain.Model.Types[typeof(Customer)];
    }

    [Test]
    public void SingleByExistingKeyTest()
    {
      RunWithinSession((s) => {
        var existingKey = existingKeys[customerType][0];
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = s.Query.Single(existingKey);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = s.Query.Single(existingKey);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleByExistingKeyAsyncTest()
    {
      await RunWithinSessionAsync(async (s) => {
        var existingKey = existingKeys[customerType][0];
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = await s.Query.SingleAsync(existingKey);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = await s.Query.SingleAsync(existingKey);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public void SingleByInexistentKeyTest()
    {
      RunWithinSession((s) => {
        var inexistentKey = Key.Create<Customer>(s.Domain, Tuples.Tuple.Create<int>(9999));
        
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          _ = Assert.Throws<KeyNotFoundException>(() => s.Query.Single(inexistentKey));
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          _ = Assert.Throws<KeyNotFoundException>(() => s.Query.Single(inexistentKey));
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleByInexistentKeyAsyncTest()
    {
      await RunWithinSessionAsync(async (s) => {
        var nonExistingKey = Key.Create<Customer>(s.Domain, Tuples.Tuple.Create<int>(9999));

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          _ = Assert.ThrowsAsync<KeyNotFoundException>(async () => await s.Query.SingleAsync(nonExistingKey));
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          _ = Assert.ThrowsAsync<KeyNotFoundException>(async () => await s.Query.SingleAsync(nonExistingKey));
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
        await Task.CompletedTask;
      });
    }

    [Test]
    public void SingleByExistingIdTest()
    {
      RunWithinSession((s) => {
        var existingId = (int) existingKeys[customerType][0].Value.GetValue(0, out var _);
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = s.Query.Single<Customer>(existingId);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = s.Query.Single<Customer>(existingId);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleByExistingIdAsyncTest()
    {
      await RunWithinSessionAsync(async (s) => {
        var existingId = (int) existingKeys[customerType][0].Value.GetValue(0, out var _);
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = await s.Query.SingleAsync<Customer>(new object[] { existingId });
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = await s.Query.SingleAsync<Customer>(new object[] { existingId });
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public void SingleByInexistentIdTest()
    {
      RunWithinSession((s) => {
        var inexistentId = 9999;

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          _ = Assert.Throws<KeyNotFoundException>(() => s.Query.Single<Customer>(inexistentId));
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          _ = Assert.Throws<KeyNotFoundException>(() => s.Query.Single<Customer>(inexistentId));
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleByInexistentIdAsyncTest()
    {
      await RunWithinSessionAsync(async (s) => {
        var inexistentId = 9999;

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          _ = Assert.ThrowsAsync<KeyNotFoundException>(async () => await s.Query.SingleAsync<Customer>(new object[] { inexistentId }));
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          _ = Assert.ThrowsAsync<KeyNotFoundException>(async () => await s.Query.SingleAsync<Customer>(new object[] { inexistentId }));
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
        await Task.CompletedTask;
      });
    }

    [Test]
    public void SingleOrDefaultByExistingKeyTest()
    {
      RunWithinSession((s) => {
        var existingKey = existingKeys[customerType][0];
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = s.Query.SingleOrDefault(existingKey);
          Assert.That(existingEntity, Is.Not.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = s.Query.Single(existingKey);
          Assert.That(existingEntity, Is.Not.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleOrDefaultByExistingKeyAsyncTest()
    {
      await RunWithinSessionAsync(async (s) => {
        var existingKey = existingKeys[customerType][0];
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = await s.Query.SingleOrDefaultAsync(existingKey);
          Assert.That(existingEntity, Is.Not.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = await s.Query.SingleOrDefaultAsync(existingKey);
          Assert.That(existingEntity, Is.Not.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public void SingleOrDefaultByInexistentKeyTest()
    {
      RunWithinSession((s) => {
        var inexistentKey = Key.Create<Customer>(s.Domain, Tuples.Tuple.Create<int>(9999));

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          var shouldBeNull = s.Query.SingleOrDefault(inexistentKey);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          var shouldBeNull = s.Query.SingleOrDefault(inexistentKey);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleOrDefaultByInexistentKeyAsyncTest()
    {
      await RunWithinSessionAsync(async (s) => {
        var inexistentKey = Key.Create<Customer>(s.Domain, Tuples.Tuple.Create<int>(9999));

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          var shouldBeNull = await s.Query.SingleOrDefaultAsync(inexistentKey);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          var shouldBeNull = await s.Query.SingleOrDefaultAsync(inexistentKey);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
        await Task.CompletedTask;
      });
    }

    [Test]
    public void SingleOrDefaultByExistingIdTest()
    {
      RunWithinSession((s) => {
        var existingId = (int) existingKeys[customerType][0].Value.GetValue(0, out var _);
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = s.Query.SingleOrDefault<Customer>(existingId);
          Assert.That(existingEntity, Is.Not.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = s.Query.SingleOrDefault<Customer>(existingId);
          Assert.That(existingEntity, Is.Not.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleOrDefaultByExistingIdAsyncTest1()
    {
      await RunWithinSessionAsync(async (s) => {
        var existingId = (int) existingKeys[customerType][0].Value.GetValue(0, out var _);
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = await s.Query.SingleOrDefaultAsync<Customer>(new object[] { existingId });
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = await s.Query.SingleOrDefaultAsync<Customer>(new object[] { existingId });
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleOrDefaultByExistingIdAsyncTest2()
    {
      await RunWithinSessionAsync(async (s) => {
        var existingId = (int) existingKeys[customerType][0].Value.GetValue(0, out var _);
        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          // Entity is not in cache yet
          var existingEntity = await s.Query.SingleOrDefaultAsync<Customer>(existingId);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          // now it is in cache
          var existingEntity = await s.Query.SingleOrDefaultAsync<Customer>(existingId);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public void SingleOrDefaultByInexistentIdTest()
    {
      RunWithinSession((s) => {
        var inexistentId = 9999;

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          var shouldBeNull = s.Query.SingleOrDefault<Customer>(inexistentId);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          var shouldBeNull = s.Query.SingleOrDefault<Customer>(inexistentId);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
      });
    }

    [Test]
    public async Task SingleOrDefaultByInexistentIdAsyncTest1()
    {
      await RunWithinSessionAsync(async (s) => {
        var inexistentId = 9999;

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          var shouldBeNull = await s.Query.SingleOrDefaultAsync<Customer>(new object[] { inexistentId });
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          var shouldBeNull = await s.Query.SingleOrDefaultAsync<Customer>(new object[] { inexistentId });
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
        await Task.CompletedTask;
      });
    }

    [Test]
    public async Task SingleOrDefaultByInexistentIdAsyncTest2()
    {
      await RunWithinSessionAsync(async (s) => {
        var inexistentId = 9999;

        var detector = new QueryExecutionDetector();
        using (detector.Attach(s)) {
          var shouldBeNull = await s.Query.SingleOrDefaultAsync<Customer>(inexistentId);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.True);
        detector.Reset();

        using (detector.Attach(s)) {
          var shouldBeNull = await s.Query.SingleOrDefaultAsync<Customer>(inexistentId);
          Assert.That(shouldBeNull, Is.Null);
        }
        Assert.That(detector.DbCommandsDetected, Is.False);
        detector.Reset();
        await Task.CompletedTask;
      });
    }

    private void RunWithinSession(Action<Session> testAction)
    {
      using(var session = Domain.OpenSession())
      using(var tx = session.OpenTransaction()) {

        testAction(session);
      }
    }

    private async Task RunWithinSessionAsync(Func<Session, Task> testAction)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        await testAction(session);
      }
    }
  }
}
