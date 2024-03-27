// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestModel = Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade.Models.OneToMany;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade
{
  [TestFixture]
  public sealed class OneToManyEntitySetTest : EntitySetUpgradeTestBase
  {
    [Test]
    public void RenameEntitySetFieldTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameField.Before.Person),
        typeof(TestModel.RenameField.Before.Address)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = new TestModel.RenameField.Before.Person();
        var address1 =  new TestModel.RenameField.Before.Address() { Person = person };
        var address2 =  new TestModel.RenameField.Before.Address() { Person = person };
        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameField.After.Person),
        typeof(TestModel.RenameField.After.Address)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = session.Query.All<TestModel.RenameField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);
        var addresses = person.AllAddresses.ToArray();
        Assert.That(addresses.Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RenameEntitySetFieldAsync()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameField.Before.Person),
        typeof(TestModel.RenameField.Before.Address)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = new TestModel.RenameField.Before.Person();
        var address1 = new TestModel.RenameField.Before.Address() { Person = person };
        var address2 = new TestModel.RenameField.Before.Address() { Person = person };
        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameField.After.Person),
        typeof(TestModel.RenameField.After.Address)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = session.Query.All<TestModel.RenameField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);
        var addresses = person.AllAddresses.ToArray();
        Assert.That(addresses.Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void RemoveEntitySetFieldTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveField.Before.Person),
        typeof(TestModel.RemoveField.Before.Address)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = new TestModel.RemoveField.Before.Person();
        var address1 = new TestModel.RemoveField.Before.Address() { Person = person };
        var address2 = new TestModel.RemoveField.Before.Address() { Person = person };
        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveField.After.Person),
        typeof(TestModel.RemoveField.After.Address)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = session.Query.All<TestModel.RemoveField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);

        var addresses = session.Query.All<TestModel.RemoveField.After.Address>().ToArray();
        Assert.That(addresses.Length, Is.EqualTo(2));
        foreach(var address in addresses) {
          Assert.That(address.Person, Is.EqualTo(person));
        }
      }
    }

    [Test]
    public async Task RemoveEntitySetFieldAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveField.Before.Person),
        typeof(TestModel.RemoveField.Before.Address)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = new TestModel.RemoveField.Before.Person();
        var address1 = new TestModel.RemoveField.Before.Address() { Person = person };
        var address2 = new TestModel.RemoveField.Before.Address() { Person = person };
        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveField.After.Person),
        typeof(TestModel.RemoveField.After.Address)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = session.Query.All<TestModel.RemoveField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);

        var addresses = session.Query.All<TestModel.RemoveField.After.Address>().ToArray();
        Assert.That(addresses.Length, Is.EqualTo(2));
        foreach (var address in addresses) {
          Assert.That(address.Person, Is.EqualTo(person));
        }
      }
    }

    [Test]
    public void RemovePairedFieldTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemovePairedField.Before.Person),
        typeof(TestModel.RemovePairedField.Before.Address)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = new TestModel.RemovePairedField.Before.Person();
        var address1 = new TestModel.RemovePairedField.Before.Address() { Person = person };
        var address2 = new TestModel.RemovePairedField.Before.Address() { Person = person };
        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemovePairedField.After.Person),
        typeof(TestModel.RemovePairedField.After.Address),
        typeof(TestModel.RemovePairedField.After.Upgrader)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = session.Query.All<TestModel.RemovePairedField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);

        Assert.That(person.Addresses.ToArray(), Is.Empty);
        Assert.That(
          session.Query.All<TestModel.RemovePairedField.After.Address>().Count(),
          Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RemovePairedFieldAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemovePairedField.Before.Person),
        typeof(TestModel.RemovePairedField.Before.Address)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = new TestModel.RemovePairedField.Before.Person();
        var address1 = new TestModel.RemovePairedField.Before.Address() { Person = person };
        var address2 = new TestModel.RemovePairedField.Before.Address() { Person = person };
        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemovePairedField.After.Person),
        typeof(TestModel.RemovePairedField.After.Address),
        typeof(TestModel.RemovePairedField.After.Upgrader)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = session.Query.All<TestModel.RemovePairedField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);

        Assert.That(person.Addresses.ToArray(), Is.Empty);
        Assert.That(
          session.Query.All<TestModel.RemovePairedField.After.Address>().Count(),
          Is.EqualTo(2));
      }
    }

    [Test]
    public void ChangeTypeOfEntitySetFieldTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetType.Before.Person),
        typeof(TestModel.ChangeEntitySetType.Before.Address),
        typeof(TestModel.ChangeEntitySetType.Before.Location)
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = new TestModel.ChangeEntitySetType.Before.Person();

        var address1 = new TestModel.ChangeEntitySetType.Before.Address() {Person = person };
        var address2 = new TestModel.ChangeEntitySetType.Before.Address() {Person = person };

        var location1 = new TestModel.ChangeEntitySetType.Before.Location() { Person = person };
        var location2 = new TestModel.ChangeEntitySetType.Before.Location() { Person = person };
        var location3 = new TestModel.ChangeEntitySetType.Before.Location() { Person = person };

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetType.After.Person),
        typeof(TestModel.ChangeEntitySetType.After.Address),
        typeof(TestModel.ChangeEntitySetType.After.Location)
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = session.Query.All<TestModel.ChangeEntitySetType.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);

        var addresses = session.Query.All<TestModel.ChangeEntitySetType.After.Address>().ToArray();
        foreach (var address in addresses) {
          Assert.That(address.Person, Is.EqualTo(person));
        }

        var locations = session.Query.All<TestModel.ChangeEntitySetType.After.Location>().ToArray();
        foreach(var location in locations) {
          Assert.That(location.Person, Is.EqualTo(person));
        }

        Assert.That(person.Locations.ToArray().Length, Is.EqualTo(3));
      }
    }

    [Test]
    public async Task ChangeTypeOfEntitySetFieldAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetType.Before.Person),
        typeof(TestModel.ChangeEntitySetType.Before.Address),
        typeof(TestModel.ChangeEntitySetType.Before.Location)
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = new TestModel.ChangeEntitySetType.Before.Person();

        var address1 = new TestModel.ChangeEntitySetType.Before.Address() { Person = person };
        var address2 = new TestModel.ChangeEntitySetType.Before.Address() { Person = person };

        var location1 = new TestModel.ChangeEntitySetType.Before.Location() { Person = person };
        var location2 = new TestModel.ChangeEntitySetType.Before.Location() { Person = person };
        var location3 = new TestModel.ChangeEntitySetType.Before.Location() { Person = person };

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetType.After.Person),
        typeof(TestModel.ChangeEntitySetType.After.Address),
        typeof(TestModel.ChangeEntitySetType.After.Location)
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = session.Query.All<TestModel.ChangeEntitySetType.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);

        var addresses = session.Query.All<TestModel.ChangeEntitySetType.After.Address>().ToArray();
        foreach (var address in addresses) {
          Assert.That(address.Person, Is.EqualTo(person));
        }

        var locations = session.Query.All<TestModel.ChangeEntitySetType.After.Location>().ToArray();
        foreach (var location in locations) {
          Assert.That(location.Person, Is.EqualTo(person));
        }

        Assert.That(person.Locations.ToArray().Length, Is.EqualTo(3));
      }
    }

    [Test]
    public void RenameKeyFieldTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameKeyField.Before.Person),
        typeof(TestModel.RenameKeyField.Before.Address),
      });

      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = new TestModel.RenameKeyField.Before.Person();

        var address1 = new TestModel.RenameKeyField.Before.Address() { Person = person };
        var address2 = new TestModel.RenameKeyField.Before.Address() { Person = person };

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyField.After.Person),
        typeof(TestModel.RenameKeyField.After.Address),
        typeof(TestModel.RenameKeyField.After.Upgrader),
      });

      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = session.Query.All<TestModel.RenameKeyField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);
        Assert.That(session.Query.All<TestModel.RenameKeyField.After.Address>().Count(), Is.EqualTo(2));

        Assert.That(person.Addresses.ToArray().Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RenameKeyFieldAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Primary Key dropping.");

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RenameKeyField.Before.Person),
        typeof(TestModel.RenameKeyField.Before.Address),
      });

      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = new TestModel.RenameKeyField.Before.Person();

        var address1 = new TestModel.RenameKeyField.Before.Address() { Person = person };
        var address2 = new TestModel.RenameKeyField.Before.Address() { Person = person };

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyField.After.Person),
        typeof(TestModel.RenameKeyField.After.Address),
        typeof(TestModel.RenameKeyField.After.Upgrader),
      });

      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var person = session.Query.All<TestModel.RenameKeyField.After.Person>().FirstOrDefault();
        Assert.That(person, Is.Not.Null);
        Assert.That(session.Query.All<TestModel.RenameKeyField.After.Address>().Count(), Is.EqualTo(2));

        Assert.That(person.Addresses.ToArray().Length, Is.EqualTo(2));
      }
    }
  }
}
