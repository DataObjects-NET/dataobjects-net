// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestModel = Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade.Models.ManyToMany;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade
{
  [TestFixture]
  public sealed class ManyToManyEntitySetTest: EntitySetUpgradeTestBase
  {
    [Test]
    public void RenameEntitySetItemTypeOnMasterTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemTypeOnMaster.After.Book),
        typeof(TestModel.RenameEntitySetItemTypeOnMaster.After.Author1),
        typeof(TestModel.RenameEntitySetItemTypeOnMaster.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameEntitySetItemTypeOnMaster.After.Author1>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetItemTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameEntitySetItemTypeOnMasterAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemTypeOnMaster.After.Book),
        typeof(TestModel.RenameEntitySetItemTypeOnMaster.After.Author1),
        typeof(TestModel.RenameEntitySetItemTypeOnMaster.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameEntitySetItemTypeOnMaster.After.Author1>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetItemTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RenameEntitySetItemTypeOnSlaveTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemTypeOnSlave.After.Book1),
        typeof(TestModel.RenameEntitySetItemTypeOnSlave.After.Author),
        typeof(TestModel.RenameEntitySetItemTypeOnSlave.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameEntitySetItemTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetItemTypeOnSlave.After.Book1>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameEntitySetItemTypeOnSlaveAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetItemTypeOnSlave.After.Book1),
        typeof(TestModel.RenameEntitySetItemTypeOnSlave.After.Author),
        typeof(TestModel.RenameEntitySetItemTypeOnSlave.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameEntitySetItemTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetItemTypeOnSlave.After.Book1>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RenameEntitySetOnMasterTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetOnMaster.After.Book),
        typeof(TestModel.RenameEntitySetOnMaster.After.Author),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameEntitySetOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors1.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameEntitySetOnMasterAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetOnMaster.After.Book),
        typeof(TestModel.RenameEntitySetOnMaster.After.Author),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameEntitySetOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors1.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RenameEntitySetOnSlaveTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetOnSlave.After.Book),
        typeof(TestModel.RenameEntitySetOnSlave.After.Author),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameEntitySetOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books1.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameEntitySetOnSlaveAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetOnSlave.After.Book),
        typeof(TestModel.RenameEntitySetOnSlave.After.Author),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameEntitySetOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books1.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RenameEntitySetFieldAndTypeOnMasterTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Book1),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Author),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Book1>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors1.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameEntitySetFieldAndTypeOnMasterAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Book1),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Author),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnMaster.After.Book1>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors1.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RenameEntitySetFieldAndTypeOnSlaveTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Book),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Author1),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Author1>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books1.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameEntitySetFieldAndTypeOnSlaveAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Book),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Author1),
        typeof(TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Author1>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books1.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameEntitySetFieldAndTypeOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RenameKeyFieldOnMasterTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyFieldOnMaster.After.Book),
        typeof(TestModel.RenameKeyFieldOnMaster.After.Author),
        typeof(TestModel.RenameKeyFieldOnMaster.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameKeyFieldOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameKeyFieldOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameKeyFieldOnMasterAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyFieldOnMaster.After.Book),
        typeof(TestModel.RenameKeyFieldOnMaster.After.Author),
        typeof(TestModel.RenameKeyFieldOnMaster.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameKeyFieldOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameKeyFieldOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RenameKeyFieldOnSlaveTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyFieldOnSlave.After.Book),
        typeof(TestModel.RenameKeyFieldOnSlave.After.Author),
        typeof(TestModel.RenameKeyFieldOnSlave.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RenameKeyFieldOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameKeyFieldOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RenameKeyFieldOnSlaveAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RenameKeyFieldOnSlave.After.Book),
        typeof(TestModel.RenameKeyFieldOnSlave.After.Author),
        typeof(TestModel.RenameKeyFieldOnSlave.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RenameKeyFieldOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RenameKeyFieldOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void ChangeTypeOfKeyFieldOnMasterConvertibleTest()
    {
      // no hints required
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Author),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task ChangeTypeOfKeyFieldOnMasterConvertibleAsyncTest()
    {
      // no hints required
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Author),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterConvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void ChangeTypeOfKeyFieldOnSlaveConvertibleTest()
    {
      // no hints required
      // automaticly creates hint

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Author),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task ChangeTypeOfKeyFieldOnSlaveConvertibleAsyncTest()
    {
      // no hints required
      // automaticly creates hint

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Author),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveConvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test, Explicit]
    public void ChangeTypeOfKeyFieldOnMasterUnconvertibleTest()
    {
      //imposible to upgrade with hints

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Book("Key1") { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Book("Key2") { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Author),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test, Explicit]
    public async Task ChangeTypeOfKeyFieldOnMasterUnconvertibleAsyncTest()
    {
      //imposible to upgrade with hints

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Author { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Book("Key1") { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.Before.Book("Key2") { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Author),
        typeof(TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnMasterUnconvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test, Explicit]
    public void ChangeTypeOfKeyFieldOnSlaveUnconvertibleTest()
    {
      //imposible to upgrade with hints

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author("key1") { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author("key2") { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author("key3") { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Author),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test, Explicit]
    public async Task ChangeTypeOfKeyFieldOnSlaveUnconvertibleAsyncTest()
    {
      //imposible to upgrade with hints

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author("key1") { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author("key2") { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Author("key3") { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.Before.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Book),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Author),
        typeof(TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.ChangeTypeOfKeyFieldOnSlaveUnconvertible.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void ChangeEntitySetItemTypeOnMasterTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.Before.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.Before.Creator),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var creator1 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Creator() { Name = "Some Creator 1" };
        var creator2 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Creator() { Name = "Some Creator 2" };

        var author1 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Creator),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Upgrader),

      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.ChangeEntitySetItemTypeOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.ChangeEntitySetItemTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));
        Assert.That(books.All(b => b.Creators.ToArray().Length == 0), Is.True);

        var creators = session.Query.All<TestModel.ChangeEntitySetItemTypeOnMaster.After.Creator>().ToArray();
        Assert.That(creators.Length, Is.EqualTo(2));
        Assert.That(creators.All(c => c.Books.ToArray().Length == 0), Is.True);
      }
    }

    [Test]
    public async Task ChangeEntitySetItemTypeOnMasterAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.Before.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.Before.Creator),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var creator1 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Creator() { Name = "Some Creator 1" };
        var creator2 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Creator() { Name = "Some Creator 2" };

        var author1 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Author { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeEntitySetItemTypeOnMaster.Before.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Creator),
        typeof(TestModel.ChangeEntitySetItemTypeOnMaster.After.Upgrader),

      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.ChangeEntitySetItemTypeOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.ChangeEntitySetItemTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));
        Assert.That(books.All(b => b.Creators.ToArray().Length == 0), Is.True);

        var creators = session.Query.All<TestModel.ChangeEntitySetItemTypeOnMaster.After.Creator>().ToArray();
        Assert.That(creators.Length, Is.EqualTo(2));
        Assert.That(creators.All(c => c.Books.ToArray().Length == 0), Is.True);
      }
    }

    [Test]
    public void ChangeEntitySetItemTypeOnSlaveTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.Before.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.Before.Brochure),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        var brochure1 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Brochure { Name = "Some brochure 1" };
        _ = brochure1.Authors.Add(author1);

        var brochure2 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Brochure { Name = "Some brochure 1" };
        _ = brochure2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Brochure),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.ChangeEntitySetItemTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.ChangeEntitySetItemTypeOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        var creators = session.Query.All<TestModel.ChangeEntitySetItemTypeOnSlave.After.Brochure>().ToArray();
        Assert.That(creators.Length, Is.EqualTo(2));
        Assert.That(creators.All(c => c.Authors.ToArray().Length == 1), Is.True);
      }
    }

    [Test]
    public async Task ChangeEntitySetItemTypeOnSlaveAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.Before.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.Before.Brochure),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author { Name = "Some Author 1" };
        var author2 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author { Name = "Some Author 2" };
        var author3 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Author { Name = "Some Author 3" };

        var book1 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        var brochure1 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Brochure { Name = "Some brochure 1" };
        _ = brochure1.Authors.Add(author1);

        var brochure2 = new TestModel.ChangeEntitySetItemTypeOnSlave.Before.Brochure { Name = "Some brochure 1" };
        _ = brochure2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Book),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Author),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Brochure),
        typeof(TestModel.ChangeEntitySetItemTypeOnSlave.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.ChangeEntitySetItemTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.ChangeEntitySetItemTypeOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        var creators = session.Query.All<TestModel.ChangeEntitySetItemTypeOnSlave.After.Brochure>().ToArray();
        Assert.That(creators.Length, Is.EqualTo(2));
        Assert.That(creators.All(c => c.Authors.ToArray().Length == 1), Is.True);
      }
    }

    [Test]
    public void ChangeESTypeFromRemovedTypeOnMasterTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Book),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Creator),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var creators = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Creator>().ToArray();
        Assert.That(creators.Length, Is.EqualTo(0));
        Assert.That(creators.All(a => a.Books.ToArray().Length == 0), Is.True);

        var books = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Creators.ToArray().Length == 0), Is.True);
      }
    }

    [Test]
    public async Task ChangeESTypeFromRemovedTypeOnMasterAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Book),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Creator),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var creators = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Creator>().ToArray();
        Assert.That(creators.Length, Is.EqualTo(0));
        Assert.That(creators.All(a => a.Books.ToArray().Length == 0), Is.True);

        var books = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Creators.ToArray().Length == 0), Is.True);
      }
    }

    [Test]
    public void ChangeESTypeFromRemovedTypeOnSlaveTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Brochure),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Author),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Brochures.ToArray().Length == 0), Is.True);

        var brochure = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Brochure>().ToArray();
        Assert.That(brochure.Length, Is.EqualTo(0));
      }
    }

    [Test]
    public async Task ChangeESTypeFromRemovedTypeOnSlaveAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Brochure),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Author),
        typeof(TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Brochures.ToArray().Length == 0), Is.True);

        var brochure = session.Query.All<TestModel.ChangeESTypeFromRemovedTypeOnSlave.After.Brochure>().ToArray();
        Assert.That(brochure.Length, Is.EqualTo(0));
      }
    }

    [Test, Explicit]
    public void RemoveEntitySetFieldOnMasterTest()
    {
      // this requires to rename auxilary type,
      // which is imposible because renaming require instance of System.Type.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldOnMaster.After.Book),
        typeof(TestModel.RemoveEntitySetFieldOnMaster.After.Author),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RemoveEntitySetFieldOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RemoveEntitySetFieldOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));
      }
    }

    [Test, Explicit]
    public async Task RemoveEntitySetFieldOnMasterAsyncTest()
    {
      // this requires to rename auxilary type,
      // which is imposible because renaming require instance of System.Type.

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldOnMaster.After.Book),
        typeof(TestModel.RemoveEntitySetFieldOnMaster.After.Author),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RemoveEntitySetFieldOnMaster.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
        Assert.That(authors.All(a => a.Books.ToArray().Length > 0), Is.True);

        var books = session.Query.All<TestModel.RemoveEntitySetFieldOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void RemoveEntitySetFieldOnSlaveTest()
    {
      // auxilary type remains the same so remove of paired EntitySet
      // needs no hints

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldOnSlave.After.Book),
        typeof(TestModel.RemoveEntitySetFieldOnSlave.After.Author),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RemoveEntitySetFieldOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.RemoveEntitySetFieldOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RemoveEntitySetFieldOnSlaveAsyncTest()
    {
      // auxilary type remains the same so remove of paired EntitySet
      // needs no hints

      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldOnSlave.After.Book),
        typeof(TestModel.RemoveEntitySetFieldOnSlave.After.Author),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RemoveEntitySetFieldOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.RemoveEntitySetFieldOnSlave.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RemoveEntitySetFieldAndItemTypeOnMasterTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnMaster.After.Book),
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnMaster.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var books = session.Query.All<TestModel.RemoveEntitySetFieldAndItemTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));
      }
    }

    [Test]
    public async Task RemoveEntitySetFieldAndItemTypeOnMasterAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnMaster.After.Book),
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnMaster.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var books = session.Query.All<TestModel.RemoveEntitySetFieldAndItemTypeOnMaster.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void RemoveEntitySetFieldAndItemTypeOnSlaveTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnSlave.After.Author),
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnSlave.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RemoveEntitySetFieldAndItemTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
      }
    }

    [Test]
    public async Task RemoveEntitySetFieldAndItemTypeOnSlaveAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.SharedBefore.Book),
        typeof(TestModel.SharedBefore.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.SharedBefore.Author { Name = "Some Author 1" };
        var author2 = new TestModel.SharedBefore.Author { Name = "Some Author 2" };
        var author3 = new TestModel.SharedBefore.Author { Name = "Some Author 3" };

        var book1 = new TestModel.SharedBefore.Book { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.SharedBefore.Book { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnSlave.After.Author),
        typeof(TestModel.RemoveEntitySetFieldAndItemTypeOnSlave.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RemoveEntitySetFieldAndItemTypeOnSlave.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));
      }
    }

    [Test]
    public void RemoveMasterKeyFieldTypeTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyFieldType.Before.Book),
        typeof(TestModel.RemoveMasterKeyFieldType.Before.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.RemoveMasterKeyFieldType.Before.Author(1, 2) { Name = "Some Author 1" };
        var author2 = new TestModel.RemoveMasterKeyFieldType.Before.Author(3, 4) { Name = "Some Author 2" };
        var author3 = new TestModel.RemoveMasterKeyFieldType.Before.Author(5, 6) { Name = "Some Author 3" };

        var book1 = new TestModel.RemoveMasterKeyFieldType.Before.Book(10, 11) { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.RemoveMasterKeyFieldType.Before.Book(12, 13) { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyFieldType.After.Book),
        typeof(TestModel.RemoveMasterKeyFieldType.After.Author),
        typeof(TestModel.RemoveMasterKeyFieldType.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RemoveMasterKeyFieldType.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.RemoveMasterKeyFieldType.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RemoveMasterKeyFieldTypeAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyFieldType.Before.Book),
        typeof(TestModel.RemoveMasterKeyFieldType.Before.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.RemoveMasterKeyFieldType.Before.Author(1, 2) { Name = "Some Author 1" };
        var author2 = new TestModel.RemoveMasterKeyFieldType.Before.Author(3, 4) { Name = "Some Author 2" };
        var author3 = new TestModel.RemoveMasterKeyFieldType.Before.Author(5, 6) { Name = "Some Author 3" };

        var book1 = new TestModel.RemoveMasterKeyFieldType.Before.Book(10, 11) { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.RemoveMasterKeyFieldType.Before.Book(12, 13) { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveMasterKeyFieldType.After.Book),
        typeof(TestModel.RemoveMasterKeyFieldType.After.Author),
        typeof(TestModel.RemoveMasterKeyFieldType.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RemoveMasterKeyFieldType.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.RemoveMasterKeyFieldType.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public void RemoveSlaveKeyFieldTypeTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyFieldType.Before.Book),
        typeof(TestModel.RemoveSlaveKeyFieldType.Before.Author),
      });
      using (var domain = Domain.Build(initConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author1 = new TestModel.RemoveSlaveKeyFieldType.Before.Author(1, 2) { Name = "Some Author 1" };
        var author2 = new TestModel.RemoveSlaveKeyFieldType.Before.Author(3, 4) { Name = "Some Author 2" };
        var author3 = new TestModel.RemoveSlaveKeyFieldType.Before.Author(5, 6) { Name = "Some Author 3" };

        var book1 = new TestModel.RemoveSlaveKeyFieldType.Before.Book(10, 11) { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.RemoveSlaveKeyFieldType.Before.Book(12, 13) { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyFieldType.After.Book),
        typeof(TestModel.RemoveSlaveKeyFieldType.After.Author),
        typeof(TestModel.RemoveSlaveKeyFieldType.After.Upgrader),
      });
      using (var domain = Domain.Build(upgradeConfig))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var authors = session.Query.All<TestModel.RemoveSlaveKeyFieldType.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.RemoveSlaveKeyFieldType.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }

    [Test]
    public async Task RemoveSlaveKeyFieldTypeAsyncTest()
    {
      var initConfig = CreateInitConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyFieldType.Before.Book),
        typeof(TestModel.RemoveSlaveKeyFieldType.Before.Author),
      });
      await using (var domain = await Domain.BuildAsync(initConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var author1 = new TestModel.RemoveSlaveKeyFieldType.Before.Author(1, 2) { Name = "Some Author 1" };
        var author2 = new TestModel.RemoveSlaveKeyFieldType.Before.Author(3, 4) { Name = "Some Author 2" };
        var author3 = new TestModel.RemoveSlaveKeyFieldType.Before.Author(5, 6) { Name = "Some Author 3" };

        var book1 = new TestModel.RemoveSlaveKeyFieldType.Before.Book(10, 11) { Name = "Some Book 1" };
        _ = book1.Authors.Add(author1);
        _ = book1.Authors.Add(author2);

        var book2 = new TestModel.RemoveSlaveKeyFieldType.Before.Book(12, 13) { Name = "Some Book 2" };
        _ = book2.Authors.Add(author2);
        _ = book2.Authors.Add(author3);

        tx.Complete();
      }

      var upgradeConfig = CreateSafeUpgradeConfiguration(new[] {
        typeof(TestModel.RemoveSlaveKeyFieldType.After.Book),
        typeof(TestModel.RemoveSlaveKeyFieldType.After.Author),
        typeof(TestModel.RemoveSlaveKeyFieldType.After.Upgrader),
      });
      await using (var domain = await Domain.BuildAsync(upgradeConfig))
      await using (var session = await domain.OpenSessionAsync())
      await using (var tx = await session.OpenTransactionAsync()) {
        var authors = session.Query.All<TestModel.RemoveSlaveKeyFieldType.After.Author>().ToArray();
        Assert.That(authors.Length, Is.EqualTo(3));

        var books = session.Query.All<TestModel.RemoveSlaveKeyFieldType.After.Book>().ToArray();
        Assert.That(books.Length, Is.EqualTo(2));

        Assert.That(books.All(b => b.Authors.ToArray().Length == 2), Is.True);
      }
    }
  }
}