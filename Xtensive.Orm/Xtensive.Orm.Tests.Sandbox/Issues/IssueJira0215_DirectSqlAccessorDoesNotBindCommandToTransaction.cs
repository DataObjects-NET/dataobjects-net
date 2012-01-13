// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.11

using NUnit.Framework;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0215_DirectSqlAccessorDoesNotBindCommandToTransaction : AutoBuildTest
  {
    [Test]
    public void GetDbTransactionInTransaction()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        Assert.That(accessor.Transaction, Is.Not.Null);
      }
    }

    [Test]
    public void CreateCommandInTransaction()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        var command = accessor.CreateCommand();
        Assert.That(command.Transaction, Is.Not.Null);
      }
    }

    [Test]
    public void GetDbTransactionWithoutTransaction()
    {
      using (var session = Domain.OpenSession()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        var command = accessor.CreateCommand();
        Assert.That(command.Transaction, Is.Null);
      }
    }

    [Test]
    public void CreateCommandWithoutTransaction()
    {
      using (var session = Domain.OpenSession()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        Assert.That(accessor.Transaction, Is.Null);
      }
    }
  }
}