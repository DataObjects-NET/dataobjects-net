// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestModel = Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade.Models.ZeroToMany;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade
{
  [TestFixture]
  public sealed class ZeroToManyEntitySetTest : EntitySetUpgradeTestBase
  {
    [Test]
    public void RenameEntitySetItemTypeTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "Issue with Primary Key column rename via table recreation");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemType.Before.Staff),
        typeof(TestModel.RenameEntitySetItemType.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.RenameEntitySetItemType.Before.Staff();
        var staff2 = new TestModel.RenameEntitySetItemType.Before.Staff();

        var brigade = new TestModel.RenameEntitySetItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemType.After.Staff1),
        typeof(TestModel.RenameEntitySetItemType.After.Brigade),
        typeof(TestModel.RenameEntitySetItemType.After.Upgrader),
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RenameEntitySetItemType.After.Staff1>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.RenameEntitySetItemType.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RenameEntitySetItemTypeAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "Issue with Primary Key column rename via table recreation");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemType.Before.Staff),
        typeof(TestModel.RenameEntitySetItemType.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.RenameEntitySetItemType.Before.Staff();
        var staff2 = new TestModel.RenameEntitySetItemType.Before.Staff();

        var brigade = new TestModel.RenameEntitySetItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemType.After.Staff1),
        typeof(TestModel.RenameEntitySetItemType.After.Brigade),
        typeof(TestModel.RenameEntitySetItemType.After.Upgrader),
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RenameEntitySetItemType.After.Staff1>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.RenameEntitySetItemType.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void RenameEntitySetFieldTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameEntitySetField.Before.Staff),
        typeof(TestModel.RenameEntitySetField.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.RenameEntitySetField.Before.Staff();
        var staff2 = new TestModel.RenameEntitySetField.Before.Staff();

        var brigade = new TestModel.RenameEntitySetField.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetField.After.Staff),
        typeof(TestModel.RenameEntitySetField.After.Brigade),
        typeof(TestModel.RenameEntitySetField.After.Upgrader),
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RenameEntitySetField.After.Staff>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.RenameEntitySetField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys1.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RenameEntitySetFieldAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameEntitySetField.Before.Staff),
        typeof(TestModel.RenameEntitySetField.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.RenameEntitySetField.Before.Staff();
        var staff2 = new TestModel.RenameEntitySetField.Before.Staff();

        var brigade = new TestModel.RenameEntitySetField.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetField.After.Staff),
        typeof(TestModel.RenameEntitySetField.After.Brigade),
        typeof(TestModel.RenameEntitySetField.After.Upgrader),
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RenameEntitySetField.After.Staff>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.RenameEntitySetField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys1.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void RenameEntitySetFieldAndTypeTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "Issue with Primary Key column rename via table recreation");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndType.Before.Staff),
        typeof(TestModel.RenameEntitySetFieldAndType.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.RenameEntitySetFieldAndType.Before.Staff();
        var staff2 = new TestModel.RenameEntitySetFieldAndType.Before.Staff();

        var brigade = new TestModel.RenameEntitySetFieldAndType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndType.After.Staff1),
        typeof(TestModel.RenameEntitySetFieldAndType.After.Brigade),
        typeof(TestModel.RenameEntitySetFieldAndType.After.Upgrader),
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RenameEntitySetFieldAndType.After.Staff1>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.RenameEntitySetFieldAndType.After.Brigade>().FirstOrDefault();
        Assert.That(brigade.Guys1.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RenameEntitySetFieldAndTypeAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "Issue with Primary Key column rename via table recreation");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndType.Before.Staff),
        typeof(TestModel.RenameEntitySetFieldAndType.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.RenameEntitySetFieldAndType.Before.Staff();
        var staff2 = new TestModel.RenameEntitySetFieldAndType.Before.Staff();

        var brigade = new TestModel.RenameEntitySetFieldAndType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndType.After.Staff1),
        typeof(TestModel.RenameEntitySetFieldAndType.After.Brigade),
        typeof(TestModel.RenameEntitySetFieldAndType.After.Upgrader),
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RenameEntitySetFieldAndType.After.Staff1>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.RenameEntitySetFieldAndType.After.Brigade>().FirstOrDefault();
        Assert.That(brigade.Guys1.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void RenameKeyFieldTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "Issue with Primary Key column rename via table recreation");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameKeyField.Before.Staff),
        typeof(TestModel.RenameKeyField.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.RenameKeyField.Before.Staff();
        var staff2 = new TestModel.RenameKeyField.Before.Staff();

        var brigade = new TestModel.RenameKeyField.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyField.After.Staff),
        typeof(TestModel.RenameKeyField.After.Brigade),
        typeof(TestModel.RenameKeyField.After.Upgrader),
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RenameKeyField.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.RenameKeyField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RenameKeyFieldAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "Issue with Primary Key column rename via table recreation");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameKeyField.Before.Staff),
        typeof(TestModel.RenameKeyField.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.RenameKeyField.Before.Staff();
        var staff2 = new TestModel.RenameKeyField.Before.Staff();

        var brigade = new TestModel.RenameKeyField.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyField.After.Staff),
        typeof(TestModel.RenameKeyField.After.Brigade),
        typeof(TestModel.RenameKeyField.After.Upgrader),
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RenameKeyField.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.RenameKeyField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void ChangeTypeOfKeyFieldConvertibleTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.Before.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.ChangeTypeOfKeyFieldConvertible.Before.Staff();
        var staff2 = new TestModel.ChangeTypeOfKeyFieldConvertible.Before.Staff();

        var brigade = new TestModel.ChangeTypeOfKeyFieldConvertible.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.After.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.After.Brigade)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.ChangeTypeOfKeyFieldConvertible.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.ChangeTypeOfKeyFieldConvertible.After.Brigade>().FirstOrDefault();
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task ChangeTypeOfKeyFieldConvertibleAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.Before.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.ChangeTypeOfKeyFieldConvertible.Before.Staff();
        var staff2 = new TestModel.ChangeTypeOfKeyFieldConvertible.Before.Staff();

        var brigade = new TestModel.ChangeTypeOfKeyFieldConvertible.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.After.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldConvertible.After.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.ChangeTypeOfKeyFieldConvertible.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.ChangeTypeOfKeyFieldConvertible.After.Brigade>().FirstOrDefault();
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test, Explicit]
    public void ChangeTypeOfKeyFieldUnconvertibleTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types' fields can't change type with hints, ChangeFieldTypeHint
      // require System.Type instance but there is no source where it is possible to get.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Staff("1");
        var staff2 = new TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Staff("3");

        var brigade = new TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.After.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.After.Brigade)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

      }
    }

    [Test, Explicit]
    public async Task ChangeTypeOfKeyFieldUnconvertibleAsyncTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types' fields can't change type with hints, ChangeFieldTypeHint
      // require System.Type instance but there is no source where it is possible to get.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Staff("1");
        var staff2 = new TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Staff("3");

        var brigade = new TestModel.ChangeTypeOfKeyFieldUnconvertible.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.After.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldUnconvertible.After.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {

      }
    }

    [Test, Explicit]
    public void ChangeTypeOfKeyFieldsUnconvertibleTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types' fields can't change type with hints, ChangeFieldTypeHint
      // require System.Type instance but there is no source where it is possible to get.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Staff("1", "2");
        var staff2 = new TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Staff("3", "4");

        var brigade = new TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.After.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.After.Brigade),
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.After.Upgrader)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

      }
    }

    [Test, Explicit]
    public async Task ChangeTypeOfKeyFieldsUnconvertibleAsyncTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types' fields can't change type with hints, ChangeFieldTypeHint
      // require System.Type instance but there is no source where it is possible to get.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Staff("1", "2");
        var staff2 = new TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Staff("3", "4");

        var brigade = new TestModel.ChangeTypeOfKeyFieldsUnconvertible.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.After.Staff),
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.After.Brigade),
        typeof(TestModel.ChangeTypeOfKeyFieldsUnconvertible.After.Upgrader)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        // no 
      }
    }

    [Test, Explicit]
    public void ChangeEntitySetItemTypeTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types are imposible to remove with hints.
      // the hints are not taken into account.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.Before.User),
        typeof(TestModel.ChangeEntitySetItemType.Before.Staff),
        typeof(TestModel.ChangeEntitySetItemType.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var user1 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user2 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user3 = new TestModel.ChangeEntitySetItemType.Before.User();

        var staff1 = new TestModel.ChangeEntitySetItemType.Before.Staff();
        var staff2 = new TestModel.ChangeEntitySetItemType.Before.Staff();

        var brigade = new TestModel.ChangeEntitySetItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.After2.User),
        typeof(TestModel.ChangeEntitySetItemType.After2.Staff),
        typeof(TestModel.ChangeEntitySetItemType.After2.Brigade)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.ChangeEntitySetItemType.After2.User>().Count(),
          Is.EqualTo(3));

        Assert.That(
          session.Query.All<TestModel.ChangeEntitySetItemType.After2.Staff>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.ChangeEntitySetItemType.After2.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(0));
      }
    }

    [Test, Explicit]
    public async Task ChangeEntitySetItemTypeAsyncTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types are imposible to remove with hints.
      // the hints are not taken into account.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.Before.User),
        typeof(TestModel.ChangeEntitySetItemType.Before.Staff),
        typeof(TestModel.ChangeEntitySetItemType.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var user1 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user2 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user3 = new TestModel.ChangeEntitySetItemType.Before.User();

        var staff1 = new TestModel.ChangeEntitySetItemType.Before.Staff();
        var staff2 = new TestModel.ChangeEntitySetItemType.Before.Staff();

        var brigade = new TestModel.ChangeEntitySetItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.After2.User),
        typeof(TestModel.ChangeEntitySetItemType.After2.Staff),
        typeof(TestModel.ChangeEntitySetItemType.After2.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.ChangeEntitySetItemType.After2.User>().Count(),
          Is.EqualTo(3));

        Assert.That(
          session.Query.All<TestModel.ChangeEntitySetItemType.After2.Staff>().Count(),
          Is.EqualTo(2));

        var brigade = session.Query.All<TestModel.ChangeEntitySetItemType.After2.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(0));
      }
    }

    [Test, Explicit]
    public void ChangeESTypeFromRemovedTypeTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types are imposible to remove with hints.
      // the hints are not taken into account.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.Before.User),
        typeof(TestModel.ChangeEntitySetItemType.Before.Staff),
        typeof(TestModel.ChangeEntitySetItemType.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var user1 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user2 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user3 = new TestModel.ChangeEntitySetItemType.Before.User();

        var staff1 = new TestModel.ChangeEntitySetItemType.Before.Staff();
        var staff2 = new TestModel.ChangeEntitySetItemType.Before.Staff();

        var brigade = new TestModel.ChangeEntitySetItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.After1.User),
        typeof(TestModel.ChangeEntitySetItemType.After1.Brigade),
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.ChangeEntitySetItemType.After1.User>().Count(),
          Is.EqualTo(3));

        var brigade = session.Query.All<TestModel.ChangeEntitySetItemType.After1.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(0));
      }
    }

    [Test, Explicit]
    public async Task ChangeESTypeFromRemovedTypeAsyncTest()
    {
      // Impossible to do this in PerformSafely
      // There is no set of hints that would allow this;
      // even internally available data of UpgradeContext
      // provides no help.

      // Auxilary types are imposible to remove with hints.
      // the hints are not taken into account.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.Before.User),
        typeof(TestModel.ChangeEntitySetItemType.Before.Staff),
        typeof(TestModel.ChangeEntitySetItemType.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var user1 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user2 = new TestModel.ChangeEntitySetItemType.Before.User();
        var user3 = new TestModel.ChangeEntitySetItemType.Before.User();

        var staff1 = new TestModel.ChangeEntitySetItemType.Before.Staff();
        var staff2 = new TestModel.ChangeEntitySetItemType.Before.Staff();

        var brigade = new TestModel.ChangeEntitySetItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemType.After1.User),
        typeof(TestModel.ChangeEntitySetItemType.After1.Brigade),
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.ChangeEntitySetItemType.After1.User>().Count(),
          Is.EqualTo(3));

        var brigade = session.Query.All<TestModel.ChangeEntitySetItemType.After1.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(0));
      }
    }

    [Test]
    public void RemoveEntitySetFieldTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetField.Before.Staff),
        typeof(TestModel.RemoveEntitySetField.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.RemoveEntitySetField.Before.Staff();
        var staff2 = new TestModel.RemoveEntitySetField.Before.Staff();

        var brigade = new TestModel.RemoveEntitySetField.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetField.After.Staff),
        typeof(TestModel.RemoveEntitySetField.After.Brigade),
        typeof(TestModel.RemoveEntitySetField.After.Upgrader)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetField.After.Staff>().Count(),
          Is.EqualTo(2));
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetField.After.Brigade>().FirstOrDefault(),
          Is.Not.Null);
      }
    }

    [Test]
    public async Task RemoveEntitySetFieldAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetField.Before.Staff),
        typeof(TestModel.RemoveEntitySetField.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.RemoveEntitySetField.Before.Staff();
        var staff2 = new TestModel.RemoveEntitySetField.Before.Staff();

        var brigade = new TestModel.RemoveEntitySetField.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetField.After.Staff),
        typeof(TestModel.RemoveEntitySetField.After.Brigade),
        typeof(TestModel.RemoveEntitySetField.After.Upgrader)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetField.After.Staff>().Count(),
          Is.EqualTo(2));
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetField.After.Brigade>().FirstOrDefault(),
          Is.Not.Null);
      }
    }

    [Test]
    public void RemoveEntitySetFieldAndItemTypeTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemType.Before.User),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.Before.Staff),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var user1 = new TestModel.RemoveEntitySetFieldAndItemType.Before.User();
        var user2 = new TestModel.RemoveEntitySetFieldAndItemType.Before.User();
        var user3 = new TestModel.RemoveEntitySetFieldAndItemType.Before.User();

        var staff1 = new TestModel.RemoveEntitySetFieldAndItemType.Before.Staff();
        var staff2 = new TestModel.RemoveEntitySetFieldAndItemType.Before.Staff();

        var brigade = new TestModel.RemoveEntitySetFieldAndItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemType.After.User),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.After.Brigade),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.After.Upgrader)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetFieldAndItemType.After.User>().Count(),
          Is.EqualTo(3));
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetFieldAndItemType.After.Brigade>().FirstOrDefault(),
          Is.Not.Null);
      }
    }

    [Test]
    public async Task RemoveEntitySetFieldAndItemTypeAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemType.Before.User),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.Before.Staff),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var user1 = new TestModel.RemoveEntitySetFieldAndItemType.Before.User();
        var user2 = new TestModel.RemoveEntitySetFieldAndItemType.Before.User();
        var user3 = new TestModel.RemoveEntitySetFieldAndItemType.Before.User();

        var staff1 = new TestModel.RemoveEntitySetFieldAndItemType.Before.Staff();
        var staff2 = new TestModel.RemoveEntitySetFieldAndItemType.Before.Staff();

        var brigade = new TestModel.RemoveEntitySetFieldAndItemType.Before.Brigade();
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemType.After.User),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.After.Brigade),
        typeof(TestModel.RemoveEntitySetFieldAndItemType.After.Upgrader)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetFieldAndItemType.After.User>().Count(),
          Is.EqualTo(3));
        Assert.That(
          session.Query.All<TestModel.RemoveEntitySetFieldAndItemType.After.Brigade>().FirstOrDefault(),
          Is.Not.Null);
      }
    }

    [Test]
    public void RemoveSlaveKeyFieldTypeTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyField.Before.Staff),
        typeof(TestModel.RemoveSlaveKeyField.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.RemoveSlaveKeyField.Before.Staff("1", "2");
        var staff2 = new TestModel.RemoveSlaveKeyField.Before.Staff("3", "4");

        var brigade = new TestModel.RemoveSlaveKeyField.Before.Brigade("5");
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyField.After.Staff),
        typeof(TestModel.RemoveSlaveKeyField.After.Brigade),
        typeof(TestModel.RemoveSlaveKeyField.After.Upgrader)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RemoveSlaveKeyField.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.RemoveSlaveKeyField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RemoveSlaveKeyFieldTypeAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyField.Before.Staff),
        typeof(TestModel.RemoveSlaveKeyField.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.RemoveSlaveKeyField.Before.Staff("1", "2");
        var staff2 = new TestModel.RemoveSlaveKeyField.Before.Staff("3", "4");

        var brigade = new TestModel.RemoveSlaveKeyField.Before.Brigade("5");
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyField.After.Staff),
        typeof(TestModel.RemoveSlaveKeyField.After.Brigade),
        typeof(TestModel.RemoveSlaveKeyField.After.Upgrader)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RemoveSlaveKeyField.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.RemoveSlaveKeyField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void RemoveMasterKeyFieldTypeTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyField.Before.Staff),
        typeof(TestModel.RemoveMasterKeyField.Before.Brigade)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var staff1 = new TestModel.RemoveMasterKeyField.Before.Staff("1");
        var staff2 = new TestModel.RemoveMasterKeyField.Before.Staff("3");

        var brigade = new TestModel.RemoveMasterKeyField.Before.Brigade("5", "6");
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyField.After.Staff),
        typeof(TestModel.RemoveMasterKeyField.After.Brigade),
        typeof(TestModel.RemoveMasterKeyField.After.Upgrader)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(
          session.Query.All<TestModel.RemoveMasterKeyField.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.RemoveMasterKeyField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RemoveMasterKeyFieldTypeAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyField.Before.Staff),
        typeof(TestModel.RemoveMasterKeyField.Before.Brigade)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var staff1 = new TestModel.RemoveMasterKeyField.Before.Staff("1");
        var staff2 = new TestModel.RemoveMasterKeyField.Before.Staff("3");

        var brigade = new TestModel.RemoveMasterKeyField.Before.Brigade("5", "6");
        _ = brigade.Guys.Add(staff1);
        _ = brigade.Guys.Add(staff2);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyField.After.Staff),
        typeof(TestModel.RemoveMasterKeyField.After.Brigade),
        typeof(TestModel.RemoveMasterKeyField.After.Upgrader)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        Assert.That(
          session.Query.All<TestModel.RemoveMasterKeyField.After.Staff>().Count(),
          Is.EqualTo(2));
        var brigade = session.Query.All<TestModel.RemoveMasterKeyField.After.Brigade>().FirstOrDefault();
        Assert.That(brigade, Is.Not.Null);
        Assert.That(brigade.Guys.ToArray().Length, Is.EqualTo(2));
      }
    }
  }
}