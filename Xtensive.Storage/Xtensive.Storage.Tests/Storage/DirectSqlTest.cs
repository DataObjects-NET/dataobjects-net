// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.29

using System;
using System.Data.Common;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Tests.Storage
{
  public class DirectSqlTest : AutoBuildTest
  {
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

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        for (int i = 0; i < 20; i++) {
          new Article() {
            Title = "Title " + i,
            Content = " Content " + i
          };
        }
        t.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Article));
      return config;
    }

    [Test]
    public void IndexStorageTest()
    {
      Require.ProviderIs(StorageProvider.Index);
      using (var session = Session.Open(Domain)) {
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
      Require.ProviderIs(StorageProvider.SqlServer);

      using (var session = Session.Open(Domain)) {
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

          // Cache invalidation (~ like on rollback, but w/o rollback)
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
  }
}