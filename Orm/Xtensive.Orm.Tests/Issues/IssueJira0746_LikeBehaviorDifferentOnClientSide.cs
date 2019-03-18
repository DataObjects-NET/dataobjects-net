// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.25.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0746_MethodLikeDifferentBehaviorInSQLAndMemoryModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0746_LikeBehaviorDifferentOnClientSide : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(SimpleEntity).Assembly, typeof(SimpleEntity).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SimpleEntity {Name = "&&%name"};
        new SimpleEntity {Name = "&%name"};

        AreEqual(session, q => q.Single(z => z.Name.Like("&&&%name", '&')));
      }
    }

    [Test]
    public void Case1Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SimpleEntity {Name = "azaza&%nameazaza"};
        new SimpleEntity {Name = "&%name"};
        new SimpleEntity {Name = "%name"};

        AreEqual(session, q => q.Single(z => z.Name.Like("&&%name", '&')));
        AreEqual(session, q => q.Single(z => z.Name.Like("&%name%", '&')));
      }
    }

    [Test]
    public void Case2Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SimpleEntity {Name = "azaza&%nameazaza"};
        new SimpleEntity {Name = "&%name"};
        new SimpleEntity {Name = "%name"};
        new SimpleEntity {Name = "name%"};

        Assert.Throws<InvalidOperationException>(() => session.Query.All<SimpleEntity>().ToArray().Single(z => z.Name.Like("name", '&')));
        Assert.Throws<InvalidOperationException>(() => session.Query.All<SimpleEntity>().Single(z => z.Name.Like("name", '&')));
      }
    }

    [Test]
    public void Case3Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SimpleEntity {Name = "azaza&%nameazaza"};
        new SimpleEntity {Name = "&%name"};
        new SimpleEntity {Name = "%name"};
        new SimpleEntity {Name = "name%"};
        new SimpleEntity {Name = "&name"};
        new SimpleEntity {Name = "name&"};
        new SimpleEntity {Name = "name"};

        session.Query.All<SimpleEntity>().ToArray().Single(z => z.Name.Like("name", '&'));
        session.Query.All<SimpleEntity>().Single(z => z.Name.Like("name", '&'));
      }
    }

    [Test]
    public void Case4Test()
    {
      var searchingWord = "name";
      var positiveTestPhrases = new[] {
        "name is only what she knew about him",
        "his name was realy strage so it popped out",
        "And he asked for her name",
        "name",
      };

      var negativeTestPhrases = new[] {
        "This is how we do",
        "There is no searching phrase in the phrase",
        string.Empty,
      };

      foreach (var positiveTestPhrase in positiveTestPhrases) {
        Assert.That(positiveTestPhrase.Like(searchingWord), Is.True);
        Assert.That(positiveTestPhrase.Like(searchingWord, '&'), Is.True);
      }

      foreach (var negativeTestPhrase in negativeTestPhrases) {
        Assert.That(negativeTestPhrase.Like(searchingWord), Is.False);
        Assert.That(negativeTestPhrase.Like(searchingWord, '&'), Is.False);
      }
    }

    private void AreEqual(Session session, Func<IQueryable<SimpleEntity>, SimpleEntity> selectClause)
    {
      var queryable = session.Query.All<SimpleEntity>();
      var ent1 = selectClause(queryable);
      var ent2 = selectClause(queryable.ToArray().AsQueryable());
      Assert.AreEqual(ent1, ent2);
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0746_MethodLikeDifferentBehaviorInSQLAndMemoryModel
{
  [HierarchyRoot]
  public class SimpleEntity : Entity
  {
    [Field,Key]
    public Guid Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}
