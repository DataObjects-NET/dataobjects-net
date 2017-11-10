// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Manual.Services
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

      public Article(Session session)
        : base(session)
      {}
    }

    #endregion

    [Test]
    public void CombinedTest()
    {
      var domain = BuildDomain();

      using (var session = domain.OpenSession()) {
        var directSql = session.Services.Demand<DirectSqlAccessor>();
        using (var t = session.OpenTransaction()) {
          var article = new Article(session) {Title = "Some title", Content = "Some content"};
          session.SaveChanges(); // Ensures changes are flushed
          
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
          Assert.IsNull(session.Query.SingleOrDefault(article.Key));

          t.Complete();
        }
      }
    }

    private static Domain BuildDomain()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Simple).Assembly, typeof(Simple).Namespace);
      return Domain.Build(config);
    }
  }
}