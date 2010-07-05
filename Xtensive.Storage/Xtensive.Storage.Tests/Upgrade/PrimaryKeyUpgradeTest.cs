// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.27

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version1;
using Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version2PerformSafely;
using Xtensive.Storage.Upgrade;
using System.Linq;
using Author = Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version1.Author;
using Book = Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version1.Book;
using Author2 = Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version2PerformSafely.Author;
using Book2 = Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version2PerformSafely.Book;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class PrimaryKeyUpgradeTest
  {
    private Domain domain;

    public class Upgrader : UpgradeHandler
    {
      private static bool isEnabled = false;

      /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
      public static IDisposable Enable()
      {
        if (isEnabled)
          throw new InvalidOperationException();
        isEnabled = true;
        return new Disposable(_ => {
          isEnabled = false;
        });
      }

      public override bool IsEnabled
      {
        get { return isEnabled; }
      }

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnUpgrade()
      {
#pragma warning disable 612,618
        var authorMap = new Dictionary<Guid, Author2>();
        var rcAuthors = Query.All<RcAuthor>();
        var books = Query.All<Book2>();
        foreach (var rcAuthor in rcAuthors) {
          var author = new Author2() { Name = rcAuthor.Name };
          authorMap.Add(rcAuthor.Id, author);
        }
        foreach (var book in books)
          book.Author = authorMap[book.RcAuthor.Id];
#pragma warning restore 612,618
      }

      protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
      {
        hints.Add(new RenameTypeHint("Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version1.Book", typeof(Book2)));
        hints.Add(new RemoveTypeHint("Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version1.Category"));
        hints.Add(new RemoveFieldHint("Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version1.Book", "Category"));
      }
    }

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [SetUp]
    public void SetUp()
    {
      BuildDomain("Version1", DomainUpgradeMode.Recreate);
      FillData();
    }

    [Test]
    public void PerformTest()
    {
      BuildDomain("Version2Perform", DomainUpgradeMode.Perform);
      using (var session = Session.Open(domain))
      using (var t = Transaction.Open()) {
        var authorCount = Query.All<PrimaryKeyModel.Version2Perform.Author>().Count(a => a.Name == "Jack London");
        var bookCount = Query.All<PrimaryKeyModel.Version2Perform.Book>().Count();
        // Nothing is kept, since there is no upgrade handler
        Assert.AreEqual(0, authorCount);
        Assert.AreEqual(0, bookCount);
        var author = Query.All<PrimaryKeyModel.Version2Perform.Author>().FirstOrDefault(a => a.Name == "Jack London");
        var book = Query.All<PrimaryKeyModel.Version2Perform.Book>().FirstOrDefault();
        Assert.IsNull(author);
        Assert.IsNull(book);
        t.Complete();
      }
    }

    [Test]
    public void PerformSafelyTest()
    {
      BuildDomain("Version2PerformSafely", DomainUpgradeMode.PerformSafely);
      using (var session = Session.Open(domain))
      using (var t = Transaction.Open()) {
        var authorCount = Query.All<PrimaryKeyModel.Version2PerformSafely.Author>().Count(a => a.Name == "Jack London");
        var bookCount = Query.All<PrimaryKeyModel.Version2PerformSafely.Book>().Count();
        Assert.AreEqual(1, authorCount);
        Assert.AreEqual(1, bookCount);
        var author = Query.All<PrimaryKeyModel.Version2PerformSafely.Author>().FirstOrDefault(a => a.Name == "Jack London");
        var book = Query.All<PrimaryKeyModel.Version2PerformSafely.Book>().FirstOrDefault();
        Assert.IsNotNull(author);
        Assert.IsNotNull(book);
        Assert.AreSame(author, book.Author);
        t.Complete();
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel." + version);
      configuration.Types.Register(typeof(Upgrader));
      using(upgradeMode == DomainUpgradeMode.PerformSafely ? Upgrader.Enable() : null)
        domain = Domain.Build(configuration);
    }

    private void FillData()
    {
      using (var session = Session.Open(domain))
      using (var t = Transaction.Open()) {
        new Book() {
          Author = new Author() { Name = "Jack London"},
          Category = new Category() { Name = "Novels"},
          LongText = "Some text."
        };

        t.Complete();
      }
    }
  }
}