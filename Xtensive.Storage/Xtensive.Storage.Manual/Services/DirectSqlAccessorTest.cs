// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Manual.Services
{
  [TestFixture]
  public sealed class DirectSqlAccessorTest
  {
    #region Model

    [HierarchyRoot]
    public class Article : Entity
    {
      [Field, Key]
      public int Id {get; private set;}
      [Field]
      public string Title { get; set; }
      [Field]
      public string Content { get; set; }
    }

    #endregion

    [Test]
    public void IndexStorageTest()
    {
      var domain = BuildDomain(false);
      using (var session = Session.Open(domain)) {
        using (var t = Transaction.Open()) {
          var directSql = session.Services.Demand<DirectSqlAccessor>();
          Assert.IsFalse(directSql.IsAvailable);
          t.Complete();
        }
      }
    }

    [Test]
    public void SqlStorageTest()
    {
      var domain = BuildDomain(true);

      using (var session = Session.Open(domain)) {
        var directSql = session.Services.Demand<DirectSqlAccessor>();
        Assert.IsTrue(directSql.IsAvailable);
        Assert.IsNull(directSql.Transaction);
        
        using (var t = Transaction.Open()) {
          var article = new Article {Title = "Some title", Content = "Some content"};
          session.Persist();
          
          // Now both are definitely not null
          Assert.IsNotNull(directSql.Connection);
          Assert.IsNotNull(directSql.Transaction);

          var command = session.Services.Demand<DirectSqlAccessor>().CreateCommand();
          command.CommandText = "DELETE FROM [dbo].[Article];";
          command.ExecuteNonQuery();

          // Invalidate the session cache
          DirectStateAccessor.Get(session).Invalidate();

          var anotherArticle = new Article { Title = "Another title", Content = "Another content" };
          session.Persist();

          AssertEx.ThrowsInvalidOperationException(() => Assert.IsNotNull(article.Content));
          Assert.AreEqual(1, Query.All<Article>().Count());
          Assert.IsNotNull(anotherArticle.Content);
          t.Complete();
        }
      }
    }

    private static Domain BuildDomain(bool sql)
    {
      DomainConfiguration config;
      if (sql)
        config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
          UpgradeMode = DomainUpgradeMode.Recreate
        };
      else
        config = new DomainConfiguration("memory://localhost/DO40-Tests")  {
          UpgradeMode = DomainUpgradeMode.Recreate
        };

      config.Types.Register(typeof(Simple).Assembly, typeof(Simple).Namespace);
      return Domain.Build(config);
    }
  }
}