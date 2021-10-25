// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Edgar Isajanyan
// Created:    2021.09.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Linq.TagTestModel;

namespace Xtensive.Orm.Tests.Linq.TagTestModel
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; private set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string FullName { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [HierarchyRoot]
  public class TagType : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 55, Nullable = false)]
    public string Name { get; set; }

    [Field] 
    public bool Active { get; set; }
  }

  [HierarchyRoot]
  [Index(nameof(Tag.ModifiedOn))]
  [Index(nameof(Tag.RemovedOn))]
  public class Tag : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 255, Nullable = false)]
    public string Name { get; set; }

    [Field(Nullable = false)]
    public TagType Type { get; set; }

    [Field]
    public string Memo { get; set; }

    [Field]
    public DateTime? RemovedOn { get; set; }

    [Field]
    public DateTime ModifiedOn { get; set; }

    [Field]
    public bool Active { get; set; }
  }

  [HierarchyRoot()]
  [Index("Name", Unique = true)]
  public class BusinessUnit : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 255, Nullable = false)]
    public string Name { get; set; }

    [Field]
    public bool Active { get; set; }
  }

  public class TagTypePair
  {
    public Tag Tag { get; set; }
    public TagType Type { get; set; }
  }

  public class TagModel
  {
    public string Memo { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class TagTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Book));
      configuration.Types.Register(typeof (Author));
      configuration.Types.Register(typeof (TagType));
      configuration.Types.Register(typeof (BusinessUnit));
      configuration.Types.Register(typeof (Tag));
      return configuration;
    }
    
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      CreateSessionAndTransaction();
    }

    protected override void CheckRequirements() =>
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Savepoints);

    [Test]
    public void LatestTagWins()
    {
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var query = session.Query.All<Book>()
          .Tag("firstTag")
          .OrderBy(x => x.Id)
          .Tag("secondTag");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsFalse(queryString.Contains("/*firstTag*/"));
        Assert.IsTrue(queryString.Contains("/*secondTag*/"));

        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    [TestCase("simpleTag", TestName = "OneLineTag")]
    [TestCase("A long time ago in a galaxy far,\t\rfar away...", TestName = "MultilineTag")]
    public void SingleTag(string tagText)
    {
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var query = session.Query.All<Book>()
        .Tag(tagText);

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.Contains($"/*{tagText}*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInJoin()
    {
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var query = session.Query.All<Author>()
        .Tag("superCoolTag")
        .Where(author => author.Books.Any(book => book.Name.Equals("something")));

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.Contains("/*superCoolTag*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }
    
    [Test]
    public void TagInPredicateJoin()
    {
      var realSession = Session.Demand();

      using (var innerTx = realSession.OpenTransaction(TransactionOpenMode.New)) {
        var tagLookup = (
          from tag in realSession.Query.All<Tag>().Tag("BU0001")
          from tagType in realSession.Query.All<TagType>().Tag("BU0002")
            .Where(tagType => tagType == tag.Type && tagType.Active == true)
          where realSession.Query.All<BusinessUnit>().Tag("BU0003")
            .Any(bu => bu.Active == true && bu.Active == tag.Active)
          select new TagTypePair { Tag = tag, Type = tagType })
        .Select(pair => new TagModel { Memo = pair.Tag.Memo });

        var queryFormatter = realSession.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(tagLookup);
        Console.WriteLine(queryString);

        // Currently we don't enforce which tag should be in resulting query
        // when there are many of them in sqlexpression tree
        Assert.IsTrue(queryString.Contains("/*BU000"));
        Assert.DoesNotThrow(() => tagLookup.Run());
      }
    }

    [Test]
    public void TagInGrouping()
    {
      // no checks so far, something will change probably
      // for now it is just to see who it goes.

      var session = Session.Demand();
      var allCommands = new List<string>();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {

        _ = new BusinessUnit() { Name = "Active#1", Active = true };
        _ = new BusinessUnit() { Name = "Active#2", Active = true };
        _ = new BusinessUnit() { Name = "Disabled#1", Active = false };

        session.SaveChanges();

        var query = session.Query.All<BusinessUnit>().Tag("BeforeGroupBy")
          .GroupBy(b => b.Active)
          .Select(g => new { g.Key, Items = g });

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        allCommands.Clear();

        query = session.Query.All<BusinessUnit>()
          .GroupBy(b => b.Active)
          .Tag("AfterGroupBy")
          .Select(g => new { g.Key, Items = g });

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        allCommands.Clear();

        query = session.Query.All<BusinessUnit>()
          .GroupBy(b => b.Active)
          .Select(g => new { g.Key, Items = g })
          .Tag("AtTheEnd");

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        allCommands.Clear();

        query = session.Query.All<BusinessUnit>().Tag("BeforeGrouping")
          .GroupBy(b => b.Active)
          .Tag("AfterGrouping")
          .Select(g => new { g.Key, Items = g });

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        allCommands.Clear();

        query = session.Query.All<BusinessUnit>()
          .GroupBy(b => b.Active)
          .Tag("AfterGrouping")
          .Select(g => new { g.Key, Items = g })
          .Tag("AtTheEnd");

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        allCommands.Clear();

        query = session.Query.All<BusinessUnit>().Tag("BeforeGrouping")
          .GroupBy(b => b.Active)
          .Tag("AfterGrouping")
          .Select(g => new { g.Key, Items = g })
          .Tag("AtTheEnd");

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        allCommands.Clear();
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    private void PrintList(List<string> list)
    {
      Console.WriteLine("-------------");
      foreach (var s in list) {
        Console.WriteLine(s);
      }
      Console.WriteLine("=============");
    }
  }
}