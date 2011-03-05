// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;

namespace Xtensive.Orm.Tests.Storage.DisconnectedStateSideEffectsTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    public readonly static string Volume1Suffix = string.Empty;
    public readonly static string Volume2Suffix = " - Vol. 2";
    public readonly static string Volume3Suffix = " - Vol. 3";

    [Key, Field]
    public int Id { get; private set; }

    [Field]
    [Version(VersionMode.Manual)]
    public int Version { get; set; }

    [Field]
    public string Title { get; set; }

    protected override void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
      base.OnSetFieldValue(field, oldValue, newValue);
      if (field.Name=="Version")
        return;
      if (field.Name=="Title") {
        var newTitle = ((string) newValue);
        if (newTitle.EndsWith(Volume2Suffix)) {
          var vol1 = new Book {
            Title = newTitle.Substring(0, newTitle.Length - Volume2Suffix.Length) + Volume1Suffix
          };
          var vol3 = new Book {
            Title = newTitle.Substring(0, newTitle.Length - Volume2Suffix.Length) + Volume3Suffix
          };
          // A lot of .IdentifyAs - to test all branches there
          vol1.IdentifyAs("ToBeCreated");
          vol3.IdentifyAs("ToBeCreated");
          vol1.IdentifyAs(EntityIdentifierType.None);
          vol3.IdentifyAs(EntityIdentifierType.None);
          vol1.IdentifyAs("ToBeCreated");
          vol3.IdentifyAs("ToBeCreated");
          vol1.IdentifyAs(EntityIdentifierType.Auto);
          vol3.IdentifyAs("ToBeRemoved");
        }
      }
      if (!Session.IsDisconnected)
        Version++;
    }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    public readonly static string Error = "error";
    public readonly static string Famous = "famous";

    [Key, Field]
    public int Id { get; private set; }

    [Field]
    [Version(VersionMode.Auto)]
    public int Version { get; set; }

    [Field]
    public string Title { get; set; }

    protected override void OnInitialize()
    {
      base.OnInitialize();
      var book = Session.Query.All<Book>().FirstOrDefault();
    }

    public override string ToString()
    {
      return Title;
    }

    public Author(string title)
    {
      Title = title;
      if (title.ToLower().Contains(Error))
        throw new ArgumentException("Title is wrong.", "title");
      if (title.ToLower().Contains(Famous)) {
        var book = new Book() {
          Title = "A book of famous author '{0}'".FormatWith(title)
        };
      }
    }
  }

  [TestFixture]
  public class DisconnectedStateSideEffectsTest : AutoBuildTest
  {
    private const string DataObjectsNetManual = "DataObjects.Net Manual";
    private const string NewBookTitle = "New Book";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      configuration.Sessions.Default.Options |= SessionOptions.AutoTransactionOpenMode;
      return configuration;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Book() { Title = "Book" };
        tx.Complete();
      }
      return domain;
    }

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [Test]
    public void FiltersTest()
    {
      var ds = new DisconnectedState();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Book book;
        using (ds.Attach(session))
        using (ds.Connect()) {
          book = session.Query.All<Book>().First();
          book.Title = "Change1";
          book.Title = "Change2";
          ds.OperationLogReplayFilter = log => log.Skip(1);
          ds.ApplyChanges();
        }

        Assert.AreEqual("Change2", book.Title);

        using (ds.Attach(session))
        using (ds.Connect()) {
          ds.OperationLogReplayFilter = log => log;
          ds.VersionUpdateFilter = key => false;
          ds.ApplyChanges();
        }

        Assert.AreEqual("Change1", book.Title);
        Assert.AreEqual(0, ds.Versions.Count);
        // tx.Complete();
      }
    }

    [Test]
    public void SequentialApplyChangesTest()
    {
      var ds = new DisconnectedState();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = session.Query.All<Book>().First();
          book.Title += " Changed";
          ds.ApplyChanges();
          book.Title += " Changed";
          ds.ApplyChanges();
        }
        // tx.Complete();
      }
    }

    [Test]
    public void SequentialApplyChangesTest_DisconnectedStateAsVersionProvider()
    {
      var ds = new DisconnectedState();
      ds.OperationLogType = OperationLogType.OutermostOperationLog;
      ds.VersionsProviderType = VersionsProviderType.DisconnectedState;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = session.Query.All<Book>().First();
          book.Title += " Changed";
          ds.ApplyChanges();
          book.Title += " Changed";
          if (ds.OperationLogType==OperationLogType.SystemOperationLog)
            ds.ApplyChanges();
          else
            AssertEx.Throws<VersionConflictException>(() => ds.ApplyChanges());
        }
        // tx.Complete();
      }
    }

    [Test]
    public void OverridenOnInitializeTest()
    {
      var ds = new DisconnectedState();
      using (var session = Domain.OpenSession())
      using (ds.Attach(session))
      using (ds.Connect()) {
        var author = new Author("Author");
      }
    }

    [Test]
    public void CreateNewEntitiesAsSetFieldSideEffectTest()
    {
      var ds = new DisconnectedState();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var vol2 = session.Query.All<Book>().First();
          vol2.Title = DataObjectsNetManual + Book.Volume2Suffix;
          var vol1 = ds.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume1Suffix)).Single();
          var vol3 = ds.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume3Suffix)).Single();
          vol1.Title += "+";
          vol3.Remove();
          ds.ApplyChanges();
        }
        {
          var vol1 = session.Query.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume1Suffix + "+")).SingleOrDefault();
          var vol2 = session.Query.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume2Suffix)).SingleOrDefault();
          var vol3 = session.Query.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume3Suffix)).SingleOrDefault();
          Assert.IsNotNull(vol1);
          Assert.IsNotNull(vol2);
          Assert.IsNull(vol3);
        }
        // tx.Complete();
      }
    }

    [Test]
    public void CreateNewEntitiesAsCtorSideEffectTest()
    {
      Author testAuthor;
      var ds = new DisconnectedState();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          testAuthor = new Author("Author 1");
          AssertEx.ThrowsArgumentException(() => new Author("Author 2 ({0})".FormatWith(Author.Error)));
          new Author("Author 3 ({0})".FormatWith(Author.Famous));
          ds.ApplyChanges();
        }
        var author1 = session.Query.All<Author>().Where(b => b.Title.StartsWith("Author 1")).SingleOrDefault();
        var author2 = session.Query.All<Author>().Where(b => b.Title.StartsWith("Author 2")).SingleOrDefault();
        var author3 = session.Query.All<Author>().Where(b => b.Title.StartsWith("Author 3")).SingleOrDefault();
        var author3book = session.Query.All<Book>().Where(b => b.Title.Contains("Author 3")).SingleOrDefault();
        Assert.IsNotNull(author1);
        Assert.IsNull(author2);
        Assert.IsNotNull(author3);
        Assert.IsNotNull(author3book);
        Assert.AreSame(testAuthor, author1);
        // tx.Complete();
      }
    }

    [Test]
    public void NotDisconnectedNewEntitySideEffectTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var testAuthor = new Author("Author 1");
        AssertEx.ThrowsArgumentException(() => new Author("Author 2 ({0})".FormatWith(Author.Error)));
        new Author("Author 3 ({0})".FormatWith(Author.Famous));
        session.SaveChanges();

        var author1 = session.Query.All<Author>().Where(b => b.Title.StartsWith("Author 1")).SingleOrDefault();
        var author2 = session.Query.All<Author>().Where(b => b.Title.StartsWith("Author 2")).SingleOrDefault();
        var author3 = session.Query.All<Author>().Where(b => b.Title.StartsWith("Author 3")).SingleOrDefault();
        var author3book = session.Query.All<Book>().Where(b => b.Title.Contains("Author 3")).SingleOrDefault();
        Assert.IsNotNull(author1);
        Assert.IsNull(author2);
        Assert.IsNotNull(author3);
        Assert.IsNotNull(author3book);
        Assert.AreSame(testAuthor, author1);

      }
    }
  }
}