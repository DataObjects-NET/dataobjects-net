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

namespace Xtensive.Orm.Tests.Issues.IssueJira0746_MethodLikeDifferentBehaviorInSQLAndMemoryModel
{
  [HierarchyRoot]
  public class SimpleEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0746_LikeBehaviorDifferentOnClientSide : AutoBuildTest
  {
    #region Test cases and searching phrases

    public readonly string[] TestCases =
    {
      "&%nameazazazaza",
      "azazazaza&%name",
      "azazazaza&%nameazazazaza",
      "azazazaza&&%name",
      "&&%nameazazazaza",
      "azazazaza&&%nameazazazaza",
      "&&%name",
      "&%name",
      "%name",
      "name%",
      "&name",
      "name&",
      "name",
    };

    public readonly string[] HitSearchStrings = new[]
    {
      "&&&%name",
      "&&%name",
      "name",

      "%&&&%name",
      "%&&%name",
      "%name",

      "&&&%name%",
      "&&%name%",
      "name%",

      "%&&&%name%",
      "%&&%name%",
      "%name%",
    };

    public readonly string[] MissSearchStrings = new[]
    {
      "&&&%abc",
      "&&%abc",
      "abc",
      "%&&&%abc",
      "%&&%abc",
      "%abc",
      "&&&%abc%",
      "&&%abc%",
      "abc%",
      "%&&&%abc%",
      "%&&%abc%",
      "%abc%",
    };

    #endregion

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (SimpleEntity).Assembly, typeof (SimpleEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var testCase in TestCases)
          new SimpleEntity {Name = testCase};

        transaction.Complete();
      }
    }

    [Test]
    [TestCaseSource("HitSearchStrings")]
    public void HitTest(string searchPattern)
    {
      var session = Session.Current;
      var local = session.Query.All<SimpleEntity>().ToArray().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e=> e.Id).ToArray();
      var server = session.Query.All<SimpleEntity>().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e => e.Id).ToArray();

      Assert.That(server.Length, Is.Not.EqualTo(0));
      Assert.That(local.Length, Is.EqualTo(server.Length));
      Assert.That(local.SequenceEqual(server), Is.True);
      
    }

    [Test]
    [TestCaseSource("MissSearchStrings")]
    public void MissTest(string searchPattern)
    {
      var session = Session.Current;
      var local = session.Query.All<SimpleEntity>().ToArray().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e => e.Id).ToArray();
      var server = session.Query.All<SimpleEntity>().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e => e.Id).ToArray();

      Assert.That(server.Length, Is.EqualTo(0));
      Assert.That(local.Length, Is.EqualTo(server.Length));
    }

    [Test]
    public void LocalLikeConsistencyTest01()
    {
      var searchingWord = "name";
      var positiveTestPhrases = new[] {
        "name",
      };

      var negativeTestPhrases = new[] {
        "This is how we do",
        "There is no searching phrase in the phrase",
        string.Empty,
        "name is only what she knew about him",
        "his name was realy strage so it popped out",
        "And he asked for her name",
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

    [Test]
    public void LocalLikeConsistencyTest02()
    {
      var searchingWord = "%name%";
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

    [Test]
    public void LocalLikeConsistencyTest03()
    {
      var searchingWord = "name%";
      var positiveTestPhrases = new[] {
        "name is only what she knew about him",
        "name",
      };

      var negativeTestPhrases = new[] {
        "This is how we do",
        "There is no searching phrase in the phrase",
        string.Empty,
        "his name was realy strage so it popped out",
        "And he asked for her name",
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

    [Test]
    public void LocalLikeConsistencyTest04()
    {
      var searchingWord = "%name";
      var positiveTestPhrases = new[] {
        "And he asked for her name",
        "name",
      };

      var negativeTestPhrases = new[] {
        "This is how we do",
        "There is no searching phrase in the phrase",
        string.Empty,
        "name is only what she knew about him",
        "his name was realy strage so it popped out",
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
  }
}
