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
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Storage
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
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
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
    public void SqlStorageTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      using (var session = Domain.OpenSession()) {
        var directSql = session.Services.Demand<DirectSqlAccessor>();
        Assert.IsNull(directSql.Transaction);
        
        using (var t = session.OpenTransaction()) {
          var article = new Article {Title = "Some title", Content = "Some content"};
          session.SaveChanges();
          
          // Now both are definitely not null
          Assert.IsNotNull(directSql.Connection);
          Assert.IsNotNull(directSql.Transaction);

          var command = session.Services.Demand<DirectSqlAccessor>().CreateCommand();
          command.CommandText = "DELETE FROM [dbo].[DirectSqlTest.Article];";
          command.ExecuteNonQuery();

          // Cache invalidation (~ like on rollback, but w/o rollback)
          DirectStateAccessor.Get(session).Invalidate();

          var anotherArticle = new Article { Title = "Another title", Content = "Another content" };
          session.SaveChanges();

          AssertEx.ThrowsInvalidOperationException(() => Assert.IsNotNull(article.Content));
          Assert.AreEqual(1, session.Query.All<Article>().Count());
          Assert.IsNotNull(anotherArticle.Content);
          t.Complete();
        }
      }
    }
  }
}