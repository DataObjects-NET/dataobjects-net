// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.BatchingOfVersionizedEntitiesTestModel;

namespace Xtensive.Orm.Tests.Storage.BatchingOfVersionizedEntitiesTestModel
{
  public abstract class VersionedEntityBase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public long Version { get; private set; }
  }

  [HierarchyRoot]
  public class Store : VersionedEntityBase
  {
    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Book> StoredBooks { get; set; }
  }

  [HierarchyRoot]
  public class Book : VersionedEntityBase
  {
    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "StoredBooks")]
    public EntitySet<Store> Stores { get; set; }
  }

  [HierarchyRoot]
  public class Author : VersionedEntityBase
  {

    [Field(Length = 50)]
    public string FirstName { get; set; }

    [Field(Length = 50)]
    public string LastName { get; set; }

    [Field]
    [Association(PairTo = "Authors")]
    public EntitySet<Book> Books { get; set; }
  }

  public class CommandCounter : IDisposable
  {
    private readonly Session session;
    private bool isAttached;
    private int count;

    public int Count => count;

    public IDisposable Attach()
    {
      if (isAttached)
        new Disposable((a) => Detach());
      session.Events.DbCommandExecuting += EventsOnDbCommandExecuting;
      isAttached = true;
      return new Disposable((a) => Detach());
    }

    public void Detach()
    {
      if (!isAttached)
        return;
      session.Events.DbCommandExecuting -= EventsOnDbCommandExecuting;
      isAttached = false;
    }

    public void Reset()
    {
      count = 0;
    }

    public void Dispose()
    {
      Detach();
    }

    private void EventsOnDbCommandExecuting(object sender, DbCommandEventArgs dbCommandEventArgs)
    {
      count++;
      Console.WriteLine(dbCommandEventArgs.Command.CommandText);
    }

    public CommandCounter(Session session)
    {
      this.session = session;
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class BatchingOfVersionizedEntitiesTest : AutoBuildTest
  {
    private readonly SessionConfiguration sessionWithCheck = new SessionConfiguration(
      SessionOptions.ServerProfile | SessionOptions.AutoActivation | SessionOptions.ValidateEntityVersions);

    private readonly SessionConfiguration sessionWithoutCheck =
      new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.AutoActivation);

    protected override void CheckRequirements() => Require.AllFeaturesSupported(ProviderFeatures.Batches);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = new Author {
          FirstName = "William",
          LastName = "Shakespeare"
        };
        var store = new Store { Name = "Store" };
        _ = new Store { Name = "RemovedStore" };
        var book = new Book { Title = "Romeo and Juliet" };
        _ = book.Authors.Add(author);
        _ = book.Stores.Add(store);
        transaction.Complete();
      }
    }

    [Test]
    public void VersionedEntityUpdateWithVersionCheckTest01()
    {
      using (var session = Domain.OpenSession(sessionWithCheck))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<Store>().Single(el => el.Name == "Store");
        var book = session.Query.All<Book>().Single(el => el.Title == "Romeo and Juliet");
        var author = session.Query.All<Author>().Single(el => el.FirstName == "William");

        store.Name = "AnotherStore";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        counter.Reset();

        book.Title = "AnotherTitle";
        author.FirstName = "NotWilliam";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void VersionedEntityUpdateWithVersionCheckTest02()
    {
      using (var session = Domain.OpenSession(sessionWithCheck))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<Store>().Single(el => el.Name == "Store");
        var book = session.Query.All<Book>().Single(el => el.Title == "Romeo and Juliet");
        var author = session.Query.All<Author>().Single(el => el.FirstName == "William");

        _ = new Store { Name = "NewStore1" };
        _ = new Store { Name = "NewStore2" };
        book.Title = "AnotherTitle";
        store.Name = "AnotherStore";


        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(3));
        counter.Reset();

        _ = new Store { Name = "NewStore3" };
        _ = new Store { Name = "NewStore4" };
        _ = new Store { Name = "NewStore5" };

        author.FirstName = "NotWilliam";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void VersionedEntityUpdateWithVersionCheckTest03()
    {
      using (var session = Domain.OpenSession(sessionWithCheck))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<Store>().Single(el => el.Name == "Store");
        var book = session.Query.All<Book>().Single(el => el.Title == "Romeo and Juliet");
        var author = session.Query.All<Author>().Single(el => el.FirstName == "William");

        _ = new Store { Name = "NewStore1" };
        _ = new Store { Name = "NewStore2" };
        book.Title = "AnotherTitle";
        store.Name = "AnotherStore";

        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 1));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 2));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 3));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 4));

        using (counter.Attach()) {
          session.Query.All<Author>().Run();
        }

        if (IsUpdatesFirst()) {
          Assert.That(counter.Count, Is.EqualTo(3));
        }
        else {
          Assert.That(counter.Count, Is.EqualTo(4));
        }

        counter.Reset();

        _ = new Store { Name = "NewStore3" };
        _ = new Store { Name = "NewStore4" };
        _ = new Store { Name = "NewStore5" };

        author.FirstName = "NotWilliam";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(2));
      }
    }

    [Test]
    public void VersionedEntityUpdateWithoutVersionCheckTest01()
    {
      using (var session = Domain.OpenSession(sessionWithoutCheck))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<Store>().Single(el => el.Name == "Store");
        var book = session.Query.All<Book>().Single(el => el.Title == "Romeo and Juliet");
        var author = session.Query.All<Author>().Single(el => el.FirstName == "William");

        store.Name = "AnotherStore";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        counter.Reset();

        book.Title = "AnotherTitle";
        author.FirstName = "NotWilliam";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void VersionedEntityUpdateWithoutVersionCheckTest02()
    {
      using (var session = Domain.OpenSession(sessionWithoutCheck))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<Store>().Single(el => el.Name == "Store");
        var book = session.Query.All<Book>().Single(el => el.Title == "Romeo and Juliet");
        var author = session.Query.All<Author>().Single(el => el.FirstName == "William");

        _ = new Store { Name = "NewStore1" };
        _ = new Store { Name = "NewStore2" };
        book.Title = "AnotherTitle";
        store.Name = "AnotherStore";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        counter.Reset();

        _ = new Store { Name = "NewStore3" };
        _ = new Store { Name = "NewStore4" };
        _ = new Store { Name = "NewStore5" };

        author.FirstName = "NotWilliam";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void VersionedEntityUpdateWithoutVersionCheckTest03()
    {
      using (var session = Domain.OpenSession(sessionWithoutCheck))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<Store>().Single(el => el.Name == "Store");
        var book = session.Query.All<Book>().Single(el => el.Title == "Romeo and Juliet");
        var author = session.Query.All<Author>().Single(el => el.FirstName == "William");

        _ = new Store { Name = "NewStore1" };
        _ = new Store { Name = "NewStore2" };
        book.Title = "AnotherTitle";
        store.Name = "AnotherStore";

        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 1));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 2));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 3));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 4));

        using (counter.Attach()) {
          session.Query.All<Author>().Run();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        counter.Reset();

        _ = new Store { Name = "NewStore3" };
        _ = new Store { Name = "NewStore4" };
        _ = new Store { Name = "NewStore5" };

        author.FirstName = "NotWilliam";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public async Task VersionedEntityUpdateWithoutVersionCheckAsyncTest03()
    {
      using (var session = await Domain.OpenSessionAsync(sessionWithoutCheck))
      using (var counter = new CommandCounter(session))
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<Store>().Single(el => el.Name == "Store");
        var book = session.Query.All<Book>().Single(el => el.Title == "Romeo and Juliet");
        var author = session.Query.All<Author>().Single(el => el.FirstName == "William");

        _ = new Store { Name = "NewStore1" };
        _ = new Store { Name = "NewStore2" };
        book.Title = "AnotherTitle";
        store.Name = "AnotherStore";

        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 1));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 2));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 3));
        _ = session.Query.ExecuteDelayed(q => q.All<Store>().Take(() => 4));

        using (counter.Attach()) {
          (await session.Query.All<Author>().AsAsync()).Run();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
        counter.Reset();

        _ = new Store { Name = "NewStore3" };
        _ = new Store { Name = "NewStore4" };
        _ = new Store { Name = "NewStore5" };

        author.FirstName = "NotWilliam";

        using (counter.Attach()) {
          session.SaveChanges();
        }

        Assert.That(counter.Count, Is.EqualTo(1));
      }
    }

    private bool IsUpdatesFirst()
    {
      return !(Domain.Configuration.Supports(ForeignKeyMode.Reference)
        && ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints)
        && !ProviderInfo.Supports(ProviderFeatures.DeferrableConstraints));
    }
  }
}