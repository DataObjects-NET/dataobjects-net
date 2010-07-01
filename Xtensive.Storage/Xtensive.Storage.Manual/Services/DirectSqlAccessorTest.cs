// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
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
    public void CombinedTest()
    {
      var domain = BuildDomain(true);

      using (var session = Session.Open(domain)) {
        var directSql = session.Services.Demand<DirectSqlAccessor>();
        if (!directSql.IsAvailable) {
          Console.WriteLine("DirectSqlAccessor is not available - ");
          Console.WriteLine("indexing storage provider (e.g. memory) is used.");
          return;
        }

        using (var t = Transaction.Open()) {
          var article = new Article {Title = "Some title", Content = "Some content"};
          session.Persist(); // Ensures changes are flushed
          
          // Article is created:
          Assert.IsFalse(article.IsRemoved);
          
          // Direct SQL command execution
          var command = session.Services.Demand<DirectSqlAccessor>().CreateCommand();
          command.CommandText = "DELETE FROM [dbo].[DirectSqlAccessorTest.Article];";
          command.ExecuteNonQuery();

          // Let's invalidate session cache after this
          DirectStateAccessor.Get(session).Invalidate();

          // Entity is really removed:
          Assert.IsTrue(article.IsRemoved);
          Assert.IsNull(Query.SingleOrDefault(article.Key));

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