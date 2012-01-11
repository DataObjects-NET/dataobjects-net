// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.11

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0215_DirectSqlAccessorDoesNotBindCommandToTransaction : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    [Test]
    public void GetDbTransactionInTransaction()
    {
      using (var session = OpenSession())
      using (var tx = Transaction.Open(session)) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        Assert.That(accessor.Transaction, Is.Not.Null);
      }
    }

    [Test]
    public void CreateCommandInTransaction()
    {
      using (var session = OpenSession())
      using (var tx = Transaction.Open(session)) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        var command = accessor.CreateCommand();
        Assert.That(command.Transaction, Is.Not.Null);
      }
    }

    [Test]
    public void GetDbTransactionWithoutTransaction()
    {
      using (var session = OpenSession()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        var command = accessor.CreateCommand();
        Assert.That(command.Transaction, Is.Null);
      }
    }

    [Test]
    public void CreateCommandWithoutTransaction()
    {
      using (var session = OpenSession()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        Assert.That(accessor.Transaction, Is.Null);
      }
    }

    private Session OpenSession()
    {
      return Session.Open(Domain, new SessionConfiguration {Options = SessionOptions.AutoShortenTransactions});
    }
  }
}