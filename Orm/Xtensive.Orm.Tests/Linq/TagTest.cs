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

  [HierarchyRoot]
  public class Property : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public BusinessUnit Owner { get; set; }

    [Field]
    public string Name { get; set; }
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
      configuration.Types.Register(typeof(Book));
      configuration.Types.Register(typeof(Author));
      configuration.Types.Register(typeof(TagType));
      configuration.Types.Register(typeof(BusinessUnit));
      configuration.Types.Register(typeof(Property));
      configuration.Types.Register(typeof(Tag));
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
    [TestCase(TagsLocation.BeforeStatement, TestName = nameof(TagsLocation.BeforeStatement))]
    [TestCase(TagsLocation.WithinStatement, TestName = nameof(TagsLocation.WithinStatement))]
    [TestCase(TagsLocation.AfterStatement, TestName = nameof(TagsLocation.AfterStatement))]
    public void VariousPlacements(TagsLocation tagsLocation)
    {
      Require.ProviderIsNot(StorageProvider.Sqlite | StorageProvider.Firebird);
      var config = Domain.Configuration.Clone();
      config.TagsLocation = tagsLocation;
      config.UpgradeMode = DomainUpgradeMode.Skip;

      using (var domain = Domain.Build(config))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Book>().Tag("simpleTag");
        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CheckTag(queryString, $"/*simpleTag*/", tagsLocation));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

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

        Assert.IsTrue(queryString.StartsWith("/*firstTag secondTag*/"));
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

        Assert.IsTrue(queryString.StartsWith($"/*{tagText}*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInSubquery()
    {
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var query = session.Query.All<Author>()
        .Tag("superCoolTag")
        .Where(author => author.Books.Tag("evenCoolerTag").Any(book => book.Name.Equals("something")));

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.StartsWith("/*superCoolTag evenCoolerTag*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInJoin()
    {
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var inner = session.Query.All<BusinessUnit>().Tag("inner");
        var outer = session.Query.All<Property>().Tag("outer");

        var query = outer.LeftJoin(inner, o => o.Owner.Id, i => i.Id, (i, o) => new { i, o });

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.StartsWith("/*outer inner*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInUnion()
    {
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Union(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.StartsWith("/*final left right*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInConcat()
    {
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Union(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.StartsWith("/*final left right*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInExcept()
    {
      Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Firebird);

      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Except(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.StartsWith("/*final left right*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInIntersect()
    {
      Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Firebird);
      var session = Session.Demand();

      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Intersect(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(queryString.StartsWith("/*final left right*/"));
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

        Assert.IsTrue(queryString.StartsWith("/*BU000"));
        Assert.IsTrue(queryString.Contains("BU0003"));
        Assert.IsTrue(queryString.Contains("BU0002"));
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

        var bu = new BusinessUnit() { Name = "Active#1", Active = true };
        _ = new Property() { Name = "Prop#1", Owner = bu };
        _ = new Property() { Name = "Prop#2", Owner = bu };
        _ = new Property() { Name = "Prop#3", Owner = bu };
        bu = new BusinessUnit() { Name = "Active#2", Active = true };
        _ = new Property() { Name = "Prop#4", Owner = bu };
        _ = new Property() { Name = "Prop#5", Owner = bu };
        _ = new Property() { Name = "Prop#6", Owner = bu };
        bu = new BusinessUnit() { Name = "Disabled#1", Active = false };
        _ = new Property() { Name = "Prop#1", Owner = bu };
        _ = new Property() { Name = "Prop#2", Owner = bu };
        _ = new Property() { Name = "Prop#3", Owner = bu };

        session.SaveChanges();

        var query = session.Query.All<BusinessUnit>().Tag("BeforeGroupBy")
          .GroupBy(b => b.Active)
          .Select(g => new { g.Key, Items = g });

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        Assert.That(allCommands[0].StartsWith("/*BeforeGroupBy*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => command.StartsWith("/*BeforeGroupBy Root query tags -> BeforeGroupBy*/")));

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
        Assert.That(allCommands[0].StartsWith("/*AfterGroupBy*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => command.StartsWith("/*Root query tags -> AfterGroupBy*/")));

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
        Assert.That(allCommands[0].StartsWith("/*BeforeGrouping AfterGrouping*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => command.StartsWith("/*BeforeGrouping Root query tags -> BeforeGrouping AfterGrouping*/")));

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
        Assert.That(allCommands[0].StartsWith("/*AfterGrouping AtTheEnd*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => command.StartsWith("/*Root query tags -> AfterGrouping AtTheEnd*/")));

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
        Assert.That(allCommands[0].StartsWith("/*BeforeGrouping AfterGrouping AtTheEnd*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => command.StartsWith("/*BeforeGrouping Root query tags -> BeforeGrouping AfterGrouping AtTheEnd*/")));

        allCommands.Clear();

        var query1 = session.Query.All<Property>()
          .GroupBy(b => b.Owner.Id)
          .Tag("AfterGroup")
          .Select(g => new { g.Key, Items = g })
          .Tag("AfterSelect")
          .Where(g => g.Items.Count() >= 0)
          .Tag("AfterWhere")
          .LeftJoin(session.Query.All<BusinessUnit>().Tag("WithinJoin"), g => g.Key, bu => bu.Id, (g, bu) => new { Key = bu, Items = g.Items });

        session.Events.DbCommandExecuting += SqlCapturer;
        foreach (var group in query1)
          foreach (var groupItem in group.Items);
        session.Events.DbCommandExecuting -= SqlCapturer;

        PrintList(allCommands);
        Assert.That(allCommands[0].StartsWith("/*AfterGroup AfterSelect AfterWhere WithinJoin*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => command.StartsWith("/*Root query tags -> AfterGroup AfterSelect AfterWhere WithinJoin*/")));

        allCommands.Clear();
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    private static bool CheckTag(string query, string expectedComment, TagsLocation place)
    {
      return place switch {
        TagsLocation.BeforeStatement => query.StartsWith(expectedComment),
        TagsLocation.WithinStatement => query.Contains(expectedComment),
        TagsLocation.AfterStatement => query.EndsWith(expectedComment),
        _ => throw new NotImplementedException()
      };
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