// Copyright (C) 2018-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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

    public SimpleEntity(Session session)
       : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0746_LikeBehaviorDifferentOnClientSide : AutoBuildTest
  {
    #region Test cases and searching phrases

    public static string[] TestCases = new []
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

    public static string[] HitSearchStrings = new[]
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

    public static string[] MissSearchStrings = new[]
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

    protected override bool InitGlobalSession => true;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof(SimpleEntity).Assembly, typeof(SimpleEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      foreach (var testCase in TestCases) {
        _ = new SimpleEntity(GlobalSession) { Name = testCase };
      }

      GlobalSession.SaveChanges();
    }

    [Test]
    [TestCaseSource(nameof(HitSearchStrings))]
    public void HitTest(string searchPattern)
    {
      var local = GlobalSession.Query.All<SimpleEntity>().AsEnumerable().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e => e.Id).ToArray();
      var server = GlobalSession.Query.All<SimpleEntity>().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e => e.Id).ToArray();

      Assert.That(server.Length, Is.Not.EqualTo(0));
      Assert.That(local.Length, Is.EqualTo(server.Length));
      Assert.That(local.SequenceEqual(server), Is.True);

    }

    [Test]
    [TestCaseSource(nameof(MissSearchStrings))]
    public void MissTest(string searchPattern)
    {
      var local = GlobalSession.Query.All<SimpleEntity>().AsEnumerable().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e => e.Id).ToArray();
      var server = GlobalSession.Query.All<SimpleEntity>().Where(z => z.Name.Like(searchPattern, '&')).OrderBy(e => e.Id).ToArray();

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