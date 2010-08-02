// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;

namespace Xtensive.Storage.Tests.Storage.DisconnectedStateSideEffectsTest
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
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Version { get; set; }

    [Field]
    public string Title { get; set; }

    protected override void OnInitialize()
    {
      base.OnInitialize();
      var book = Query.All<Book>().FirstOrDefault();
    }

    public override string ToString()
    {
      return Title;
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
      return configuration;
    }

    protected override Domain  BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        new Book() { Title = "Book" };
        tx.Complete();
      }
      return domain;
    }

    [Test]
    public void SequentialApplyChangesTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = Query.All<Book>().First();
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
      ds.VersionsProviderType = VersionsProviderType.DisconnectedState;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = Query.All<Book>().First();
          book.Title += " Changed";
          ds.ApplyChanges();
          book.Title += " Changed";
          AssertEx.Throws<VersionConflictException>(() => ds.ApplyChanges());
        }
        // tx.Complete();
      }
    }

    [Test]
    public void OverridenOnInitializeTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (ds.Attach(session))
      using (ds.Connect()) {
        var author = new Author();
      }
    }

    [Test]
    public void CreateNewEntitiesAsSideEffectTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var vol2 = Query.All<Book>().First();
          vol2.Title = DataObjectsNetManual + Book.Volume2Suffix;
          var vol1 = ds.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume1Suffix)).Single();
          var vol3 = ds.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume3Suffix)).Single();
          vol1.Title += "+";
          vol3.Remove();
          ds.ApplyChanges();
        }
        {
          var vol1 = Query.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume1Suffix + "+")).SingleOrDefault();
          var vol2 = Query.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume2Suffix)).SingleOrDefault();
          var vol3 = Query.All<Book>().Where(b => b.Title==(DataObjectsNetManual + Book.Volume3Suffix)).SingleOrDefault();
          Assert.IsNotNull(vol1);
          Assert.IsNotNull(vol2);
          Assert.IsNull(vol3);
        }
        // tx.Complete();
      }
    }
  }
}