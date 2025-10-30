// Copyright (C) 2010-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.02.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Upgrade;
using M1 = Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel.Version1;
using M2P = Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel.Version2Perform;
using M2PS = Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel.Version2PerformSafely;


namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class PrimaryKeyUpgradeTest
  {
    public class Upgrader : UpgradeHandler
    {
      private static bool isEnabled = false;

      /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
      public static IDisposable Enable()
      {
        if (isEnabled) {
          throw new InvalidOperationException();
        }
        isEnabled = true;
        return new Disposable(_ => {
          isEnabled = false;
        });
      }

      public override bool IsEnabled => isEnabled;

      public override bool CanUpgradeFrom(string oldVersion) => true;

      public override void OnUpgrade()
      {
#pragma warning disable 612,618
        var authorMap = new Dictionary<Guid, M2PS.Author>();
        var rcAuthors = Session.Demand().Query.All<M2PS.RcAuthor>();
        var books = Session.Demand().Query.All<M2PS.Book>();
        foreach (var rcAuthor in rcAuthors) {
          var author = new M2PS.Author() { Name = rcAuthor.Name };
          authorMap.Add(rcAuthor.Id, author);
        }
        foreach (var book in books)
          book.Author = authorMap[book.RcAuthor.Id];
#pragma warning restore 612,618
      }

      protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
      {
        _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel.Version1.Book", typeof(M2PS.Book)));
        _ = hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel.Version1.Category"));
        _ = hints.Add(new RemoveFieldHint("Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel.Version1.Book", "Category"));
      }
    }

    [SetUp]
    public void SetUp()
    {
      var domain = BuildDomain("Version1", DomainUpgradeMode.Recreate);
      using (domain) {
        FillData(domain);
      }
    }

    [Test, Ignore("Default behavior changed. Namespace-only renames are tracked automatically.")]
    public void PerformTest()
    {
      var domain = BuildDomain("Version2Perform", DomainUpgradeMode.Perform);
      using (domain)
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var authorCount = session.Query.All<M2P.Author>().Count(a => a.Name == "Jack London");
        var bookCount = session.Query.All<M2P.Book>().Count();
        // Nothing is kept, since there is no upgrade handler
        Assert.AreEqual(0, authorCount);
        Assert.AreEqual(0, bookCount);
        var author = session.Query.All<M2P.Author>().FirstOrDefault(a => a.Name == "Jack London");
        var book = session.Query.All<M2P.Book>().FirstOrDefault();
        Assert.IsNull(author);
        Assert.IsNull(book);
        t.Complete();
      }
    }

    [Test]
    [IgnoreIfGithubActions(StorageProvider.Firebird)]
    public void PerformSafelyTest()
    {
      var domain = BuildDomain("Version2PerformSafely", DomainUpgradeMode.PerformSafely);
      using (domain)
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var authorCount = session.Query.All<M2PS.Author>().Count(a => a.Name == "Jack London");
        var bookCount = session.Query.All<M2PS.Book>().Count();
        Assert.AreEqual(1, authorCount);
        Assert.AreEqual(1, bookCount);
        var author = session.Query.All<M2PS.Author>().FirstOrDefault(a => a.Name == "Jack London");
        var book = session.Query.All<M2PS.Book>().FirstOrDefault();
        Assert.IsNotNull(author);
        Assert.IsNotNull(book);
        Assert.AreSame(author, book.Author);
        t.Complete();
      }
    }

    [Test]
    [IgnoreIfGithubActions(StorageProvider.Firebird)]
    public async Task PerformSafelyAsyncTest()
    {
      var domain = await BuildDomainAsync("Version2PerformSafely", DomainUpgradeMode.PerformSafely);
      using (domain)
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var authorCount = session.Query.All<M2PS.Author>().Count(a => a.Name == "Jack London");
        var bookCount = session.Query.All<M2PS.Book>().Count();
        Assert.AreEqual(1, authorCount);
        Assert.AreEqual(1, bookCount);
        var author = session.Query.All<M2PS.Author>().FirstOrDefault(a => a.Name == "Jack London");
        var book = session.Query.All<M2PS.Book>().FirstOrDefault();
        Assert.IsNotNull(author);
        Assert.IsNotNull(book);
        Assert.AreSame(author, book.Author);
        t.Complete();
      }
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.RegisterCaching(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel." + version);
      configuration.Types.Register(typeof(Upgrader));
      using (upgradeMode == DomainUpgradeMode.PerformSafely ? Upgrader.Enable() : null) {
        var domain = Domain.Build(configuration);
        return domain;
      }
    }

    private async Task<Domain> BuildDomainAsync(string version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.RegisterCaching(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Upgrade.PrimaryKeyModel." + version);
      configuration.Types.Register(typeof(Upgrader));
      using (upgradeMode == DomainUpgradeMode.PerformSafely ? Upgrader.Enable() : null) {
        var domain = await Domain.BuildAsync(configuration);
        return domain;
      }
    }

    private void FillData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new M1.Book() {
          Author = new M1.Author() { Name = "Jack London"},
          Category = new M1.Category() { Name = "Novels"},
          LongText = "Some text."
        };

        t.Complete();
      }
    }
  }
}