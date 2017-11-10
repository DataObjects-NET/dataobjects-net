// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.29

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Upgrade.TypeIdUpgrade
{
  [TestFixture]
  public class VersionUpgradeTest
  {
#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    [Test]
    public void NoChangesTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      var domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Model.Person() {
          FirstName = "Alex", 
          LastName = "Kochetov"
        };
        new Model.Person() {
          FirstName = "Alex",
          LastName = "Gamzov"
        };
        new Model.Employee() {
          FirstName = "Dmitri",
          LastName = "Maximov"
        };
        t.Complete();
      }

      using (var session = domain.OpenSession(SessionType.System)) 
      using (var t = session.OpenTransaction()) {
        var handler = (SqlSessionHandler) session.Handler;
        var connection = handler.Connection;
        var command = connection.CreateCommand(string.Format(
          "ALTER TABLE [dbo].[Metadata.Assembly] ADD [TypeId] integer NOT NULL DEFAULT({0});" + 
          "ALTER TABLE [dbo].[Metadata.Extension] ADD [TypeId] integer NOT NULL DEFAULT({1});" + 
          "ALTER TABLE [dbo].[Metadata.Type] ADD [TypeId] integer NOT NULL DEFAULT({2});" +
          "CREATE UNIQUE INDEX [Type.IX_Name] ON [dbo].[Metadata.Type] ([Name]) INCLUDE (TypeId) WITH (FILLFACTOR = 80, PAD_INDEX = ON, DROP_EXISTING = ON)", 
          domain.Model.Types[typeof(Metadata.Assembly)].TypeId,
          domain.Model.Types[typeof(Metadata.Extension)].TypeId,
          domain.Model.Types[typeof(Metadata.Type)].TypeId));
        command.Transaction = connection.ActiveTransaction;
        command.ExecuteNonQuery();
        t.Complete();
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var count = session.Query.All<Model.Person>().Count();
        Assert.AreEqual(3, count);
        var list = session.Query.All<Model.Person>().ToList();
        Assert.AreEqual(3, list.Count);
        foreach (var item in list)
          Assert.IsNotNull(item);
        t.Complete();
      }
    }

    [Test]
    public void TypeAddTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model.Person));
      var domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Model.Person() {
          FirstName = "Alex", 
          LastName = "Kochetov"
        };
        new Model.Person() {
          FirstName = "Alex",
          LastName = "Gamzov"
        };
        t.Complete();
      }

      using (var session = domain.OpenSession(SessionType.System)) 
      using (var t = session.OpenTransaction()) {
        var handler = (SqlSessionHandler) session.Handler;
        var connection = handler.Connection;
        var command = connection.CreateCommand(string.Format(
          "ALTER TABLE [dbo].[Person] ADD [TypeId] integer NOT NULL DEFAULT({0});" +
          "ALTER TABLE [dbo].[Metadata.Assembly] ADD [TypeId] integer NOT NULL DEFAULT({1});" +
          "ALTER TABLE [dbo].[Metadata.Extension] ADD [TypeId] integer NOT NULL DEFAULT({2});" +
          "ALTER TABLE [dbo].[Metadata.Type] ADD [TypeId] integer NOT NULL DEFAULT({3});" +
          "CREATE UNIQUE INDEX [Type.IX_Name] ON [dbo].[Metadata.Type] ([Name]) INCLUDE (TypeId) WITH (FILLFACTOR = 80, PAD_INDEX = ON, DROP_EXISTING = ON)", 
          domain.Model.Types[typeof(Model.Person)].TypeId,
          domain.Model.Types[typeof(Metadata.Assembly)].TypeId,
          domain.Model.Types[typeof(Metadata.Extension)].TypeId,
          domain.Model.Types[typeof(Metadata.Type)].TypeId));
        command.Transaction = connection.ActiveTransaction;
        command.ExecuteNonQuery();
        t.Complete();
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var count = session.Query.All<Model.Person>().Count();
        Assert.AreEqual(2, count);
        var list = session.Query.All<Model.Person>().ToList();
        Assert.AreEqual(2, list.Count);
        foreach (var item in list)
          Assert.IsNotNull(item);
        t.Complete();
      }
    }

    [Test]
    public void TypeRemovalTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      var domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Model.Person() {
          FirstName = "Alex", 
          LastName = "Kochetov"
        };
        new Model.Person() {
          FirstName = "Alex",
          LastName = "Gamzov"
        };
        new Model.Employee() {
          FirstName = "Dmitri",
          LastName = "Maximov"
        };
        t.Complete();
      }

      using (var session = domain.OpenSession(SessionType.System)) 
      using (var t = session.OpenTransaction()) {
        var handler = (SqlSessionHandler) session.Handler;
        var connection = handler.Connection;
        var command = connection.CreateCommand(string.Format(
         "ALTER TABLE [dbo].[Metadata.Assembly] ADD [TypeId] integer NOT NULL DEFAULT({0});" +
         "ALTER TABLE [dbo].[Metadata.Extension] ADD [TypeId] integer NOT NULL DEFAULT({1});" +
         "ALTER TABLE [dbo].[Metadata.Type] ADD [TypeId] integer NOT NULL DEFAULT({2});" +
          "CREATE UNIQUE INDEX [Type.IX_Name] ON [dbo].[Metadata.Type] ([Name]) INCLUDE (TypeId) WITH (FILLFACTOR = 80, PAD_INDEX = ON, DROP_EXISTING = ON)", 
         domain.Model.Types[typeof(Metadata.Assembly)].TypeId,
         domain.Model.Types[typeof(Metadata.Extension)].TypeId,
         domain.Model.Types[typeof(Metadata.Type)].TypeId));
        command.Transaction = connection.ActiveTransaction;
        command.ExecuteNonQuery();
        t.Complete();
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable())
        domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var count = session.Query.All<Model.Person>().Count();
        Assert.AreEqual(2, count);
        var list = session.Query.All<Model.Person>().ToList();
        Assert.AreEqual(2, list.Count);
        foreach (var item in list)
          Assert.IsNotNull(item);
        t.Complete();
      }
    }
  }
}