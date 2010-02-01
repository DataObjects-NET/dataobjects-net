// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.29

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage
{
  public class DbCommandTest : AutoBuildTest
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
    public void Test()
    {
      using (var session = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var article = new Article {Title = "Some title", Content = "Some content"};
          
          session.Persist();
          var dbCommand = session.GetDbCommand();
          dbCommand.CommandText = "DELETE FROM dbo.Article;";
          dbCommand.ExecuteNonQuery();
          var anotherArticle = new Article { Title = "Another title", Content = "Another content" };
          session.Persist();
          session.Invalidate();

          AssertEx.ThrowsInvalidOperationException(() => Assert.IsNotNull(article.Content));
          Assert.AreEqual(1, Query.All<Article>().Count());
          Assert.IsNotNull(anotherArticle.Content);
          t.Complete();
        }
      }
    }
  }
}