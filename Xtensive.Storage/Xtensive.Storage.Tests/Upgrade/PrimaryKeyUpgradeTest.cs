// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.27

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel.Version1;
using Xtensive.Storage.Upgrade;
using System.Linq;

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

      protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
      {}
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
      BuildDomain("Version2", DomainUpgradeMode.Perform);
      Validate(false);
    }

    [Test]
    public void PerformSafelyTest()
    {
      BuildDomain("Version2", DomainUpgradeMode.PerformSafely);
      Validate(true);
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Upgrade.PrimaryKeyModel." + version);
      configuration.Types.Register(typeof(Upgrader));
      using(Upgrader.Enable())
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

    private void Validate(bool keepConsistent)
    {
      using (var session = Session.Open(domain))
      using (var t = Transaction.Open()) {
        var authorCount = Query.All<PrimaryKeyModel.Version2.Author>().Count(a => a.Name == "Jack London");
        var bookCount = Query.All<PrimaryKeyModel.Version2.Book>().Count();
        Assert.AreEqual(1, authorCount);
        Assert.AreEqual(1, bookCount);
        var author = Query.All<PrimaryKeyModel.Version2.Author>().First(a => a.Name == "Jack London");
        var book = Query.All<PrimaryKeyModel.Version2.Book>().First();
        Assert.IsNotNull(author);
        Assert.IsNotNull(book);
        if (keepConsistent)
          Assert.AreSame(author, book.Author);
        else
          Assert.IsNull(book.Author);
        t.Complete();
      }
    }
  }
}