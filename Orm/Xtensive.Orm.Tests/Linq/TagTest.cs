// Copyright (C) 2021-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Edgar Isajanyan
// Created:    2021.09.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string FullName { get; set; }

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
    private Func<string, string> CursorCutter;

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
      DefineCursorCutter();
      CreateSessionAndTransaction();
    }

    private void DefineCursorCutter()
    {
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Oracle)) {
        CursorCutter = (s) => {
          var index = s.IndexOf("FOR");
          return s.Substring(index + 4);
        };
      }
      else {
        CursorCutter = (s) => s;
      }
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
      using (var tagScope = session.Tag("sessionTag"))
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Book>().Tag("simpleTag");
        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CheckTag(CursorCutter(queryString), "/*simpleTag sessionTag*/", tagsLocation));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void LatestTagWins()
    {
      var session = Session.Demand();

      using (var tagScope = session.Tag("sessionTag"))
      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var query = session.Query.All<Book>()
          .Tag("firstTag")
          .OrderBy(x => x.Id)
          .Tag("secondTag");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CursorCutter(queryString).StartsWith("/*firstTag secondTag sessionTag*/"));
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

        Assert.IsTrue(CursorCutter(queryString).StartsWith($"/*{tagText}*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInSubquery()
    {
      var session = Session.Demand();

      using (var tagScope = session.Tag("sessionTag"))
      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var query = session.Query.All<Author>()
        .Tag("superCoolTag")
        .Where(author => author.Books.Tag("evenCoolerTag").Any(book => book.Name.Equals("something")));

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CursorCutter(queryString).StartsWith("/*superCoolTag sessionTag evenCoolerTag*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInJoin()
    {
      var session = Session.Demand();

      using (var tagScope = session.Tag("sessionTag"))
      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var inner = session.Query.All<BusinessUnit>().Tag("inner");
        var outer = session.Query.All<Property>().Tag("outer");

        var query = outer.LeftJoin(inner, o => o.Owner.Id, i => i.Id, (i, o) => new { i, o });

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CursorCutter(queryString).StartsWith("/*outer inner sessionTag*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInUnion()
    {
      var session = Session.Demand();

      using (var tagScope = session.Tag("sessionTag"))
      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Union(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CursorCutter(queryString).StartsWith("/*final sessionTag left right*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInConcat()
    {
      var session = Session.Demand();

      using (var tagScope = session.Tag("sessionTag"))
      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Union(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CursorCutter(queryString).StartsWith("/*final sessionTag left right*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInExcept()
    {
      Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Firebird);

      var session = Session.Demand();

      using (var tagScope = session.Tag("sessionTag"))
      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Except(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CursorCutter(queryString).StartsWith("/*final sessionTag left right*/"));
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void TagInIntersect()
    {
      Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Firebird);
      var session = Session.Demand();

      using (var tagScope = session.Tag("sessionTag"))
      using (var innerTx = session.OpenTransaction(TransactionOpenMode.New)) {
        var left = session.Query.All<BusinessUnit>().Tag("left");
        var right = session.Query.All<BusinessUnit>().Tag("right");

        var query = left.Intersect(right).Tag("final");

        var queryFormatter = session.Services.Demand<QueryFormatter>();
        var queryString = queryFormatter.ToSqlString(query);
        Console.WriteLine(queryString);

        Assert.IsTrue(CursorCutter(queryString).StartsWith("/*final sessionTag left right*/"));
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

        var noCursorString = CursorCutter(queryString);
        Assert.IsTrue(noCursorString.StartsWith("/*BU000"));
        Assert.IsTrue(noCursorString.Contains("BU0003"));
        Assert.IsTrue(noCursorString.Contains("BU0002"));
        Assert.DoesNotThrow(() => tagLookup.Run());
      }
    }


    [Test]
    public void TagInGrouping()
    {
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

        using (var tagScope = session.Tag("sessionTag1")) {
          session.Events.DbCommandExecuting += SqlCapturer;
          foreach (var group in query)
            foreach (var groupItem in group.Items)
              ;
          session.Events.DbCommandExecuting -= SqlCapturer;
        }

        PrintList(allCommands);
        Assert.That(CursorCutter(allCommands[0]).StartsWith("/*BeforeGroupBy sessionTag1*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => CursorCutter(command).StartsWith("/*BeforeGroupBy (Root query tags -> BeforeGroupBy sessionTag1)*/")));

        allCommands.Clear();

        query = session.Query.All<BusinessUnit>()
          .GroupBy(b => b.Active)
          .Tag("AfterGroupBy")
          .Select(g => new { g.Key, Items = g });

        using (var tagScope = session.Tag("sessionTag2")) {
          session.Events.DbCommandExecuting += SqlCapturer;
          foreach (var group in query)
            foreach (var groupItem in group.Items)
              ;
          session.Events.DbCommandExecuting -= SqlCapturer;
        }

        PrintList(allCommands);
        Assert.That(CursorCutter(allCommands[0]).StartsWith("/*AfterGroupBy sessionTag2*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => CursorCutter(command).StartsWith("/*(Root query tags -> AfterGroupBy sessionTag2)*/")));

        allCommands.Clear();

        query = session.Query.All<BusinessUnit>().Tag("BeforeGrouping")
          .GroupBy(b => b.Active)
          .Tag("AfterGrouping")
          .Select(g => new { g.Key, Items = g });

        using (var sessionTag = session.Tag("sessionTag3")) {
          session.Events.DbCommandExecuting += SqlCapturer;
          foreach (var group in query)
            foreach (var groupItem in group.Items)
              ;
          session.Events.DbCommandExecuting -= SqlCapturer;
        }

        PrintList(allCommands);
        Assert.That(CursorCutter(allCommands[0]).StartsWith("/*BeforeGrouping AfterGrouping sessionTag3*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => CursorCutter(command).StartsWith("/*BeforeGrouping (Root query tags -> BeforeGrouping AfterGrouping sessionTag3)*/")));

        allCommands.Clear();

        query = session.Query.All<BusinessUnit>()
          .GroupBy(b => b.Active)
          .Tag("AfterGrouping")
          .Select(g => new { g.Key, Items = g })
          .Tag("AtTheEnd");

        using (var sessionTag = session.Tag("sessionTag4")) {
          session.Events.DbCommandExecuting += SqlCapturer;
          foreach (var group in query)
            foreach (var groupItem in group.Items)
              ;
          session.Events.DbCommandExecuting -= SqlCapturer;
        }

        PrintList(allCommands);
        Assert.That(CursorCutter(allCommands[0]).StartsWith("/*AfterGrouping AtTheEnd sessionTag4*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => CursorCutter(command).StartsWith("/*(Root query tags -> AfterGrouping AtTheEnd sessionTag4)*/")));

        allCommands.Clear();

        query = session.Query.All<BusinessUnit>().Tag("BeforeGrouping")
          .GroupBy(b => b.Active)
          .Tag("AfterGrouping")
          .Select(g => new { g.Key, Items = g })
          .Tag("AtTheEnd");

        using (var tagScope = session.Tag("sessionTag5")) {
          session.Events.DbCommandExecuting += SqlCapturer;
          foreach (var group in query)
            foreach (var groupItem in group.Items)
              ;
          session.Events.DbCommandExecuting -= SqlCapturer;
        }

        PrintList(allCommands);
        Assert.That(CursorCutter(allCommands[0]).StartsWith("/*BeforeGrouping AfterGrouping AtTheEnd sessionTag5*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => CursorCutter(command).StartsWith("/*BeforeGrouping (Root query tags -> BeforeGrouping AfterGrouping AtTheEnd sessionTag5)*/")));

        allCommands.Clear();

        var query1 = session.Query.All<Property>()
          .GroupBy(b => b.Owner.Id)
          .Tag("AfterGroup")
          .Select(g => new { g.Key, Items = g })
          .Tag("AfterSelect")
          .Where(g => g.Items.Count() >= 0)
          .Tag("AfterWhere")
          .LeftJoin(session.Query.All<BusinessUnit>().Tag("WithinJoin"), g => g.Key, bu => bu.Id, (g, bu) => new { Key = bu, Items = g.Items });

        using (var tagScope = session.Tag("sessionTag6")) {
          session.Events.DbCommandExecuting += SqlCapturer;
          foreach (var group in query1)
            foreach (var groupItem in group.Items)
              ;
          session.Events.DbCommandExecuting -= SqlCapturer;
        }

        PrintList(allCommands);
        Assert.That(CursorCutter(allCommands[0]).StartsWith("/*AfterGroup AfterSelect AfterWhere WithinJoin sessionTag6*/"));
        Assert.That(allCommands.Skip(1)
          .All(command => CursorCutter(command).StartsWith("/*(Root query tags -> AfterGroup AfterSelect AfterWhere WithinJoin sessionTag6)*/")));

        allCommands.Clear();
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    [Test]
    public void SessionTagInlineQuery()
    {
      var session = Session.Demand();
      var allCommands = new List<string>();

      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += SqlCapturer;
        session.Query.All<BusinessUnit>().Run();

        Assert.That(allCommands.Count, Is.EqualTo(1));
        Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost*/"));

        allCommands.Clear();

        _ = session.Query.All<BusinessUnit>().ToArray();
        session.Events.DbCommandExecuting -= SqlCapturer;

        Assert.That(allCommands.Count, Is.EqualTo(1));
        Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost*/"));

        allCommands.Clear();
      }

      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        using (var inBetween = session.Tag("in-between")) {

          session.Events.DbCommandExecuting += SqlCapturer;
          session.Query.All<BusinessUnit>().Run();

          Assert.That(allCommands.Count, Is.EqualTo(1));
          Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost in-between*/"));

          allCommands.Clear();

          _ = session.Query.All<BusinessUnit>().ToArray();
          session.Events.DbCommandExecuting -= SqlCapturer;

          Assert.That(allCommands.Count, Is.EqualTo(1));
          Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost in-between*/"));

          allCommands.Clear();
        }
      }

      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {
          using (var deepest = session.Tag("deepest")) {

            session.Events.DbCommandExecuting += SqlCapturer;
            session.Query.All<BusinessUnit>().Run();

            Assert.That(allCommands.Count, Is.EqualTo(1));
            Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost in-between deepest*/"));

            allCommands.Clear();

            _ = session.Query.All<BusinessUnit>().ToArray();
            session.Events.DbCommandExecuting -= SqlCapturer;

            Assert.That(allCommands.Count, Is.EqualTo(1));
            Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost in-between deepest*/"));

            allCommands.Clear();
          }
        }
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    [Test]
    public void SessionTagCachingQuery()
    {
      var session = Session.Demand();
      var allCommands = new List<string>();

      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += SqlCapturer;
        _ = session.Query.Execute(q => q.All<BusinessUnit>()).ToArray();
        session.Events.DbCommandExecuting -= SqlCapturer;

        Assert.That(allCommands.Count, Is.EqualTo(1));
        Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost*/"));

        allCommands.Clear();
      }

      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        using (var inBetween = session.Tag("in-between")) {

          session.Events.DbCommandExecuting += SqlCapturer;
          _ = session.Query.Execute(q => q.All<BusinessUnit>()).ToArray();
          session.Events.DbCommandExecuting -= SqlCapturer;

          Assert.That(allCommands.Count, Is.EqualTo(1));
          Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost in-between*/"));

          allCommands.Clear();
        }
      }

      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {
          using (var deepest = session.Tag("deepest")) {

            session.Events.DbCommandExecuting += SqlCapturer;
            _ = session.Query.Execute(q => q.All<BusinessUnit>()).ToArray();
            session.Events.DbCommandExecuting -= SqlCapturer;

            Assert.That(allCommands.Count, Is.EqualTo(1));
            Assert.IsTrue(CursorCutter(allCommands[0]).Contains("/*outermost in-between deepest*/"));

            allCommands.Clear();
          }
        }
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    [Test]
    public void SessionTagSingle()
    {
      var allCommands = new List<string>();

      var id = 0L;

      using (var populateSession = Domain.OpenSession())
      using (var tx = populateSession.OpenTransaction()) {
        id = new BusinessUnit() { Name = "Single #1" }.Id;

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += SqlCapturer;
        _ = session.Query.Single<BusinessUnit>(id);
        session.Events.DbCommandExecuting -= SqlCapturer;

        Assert.That(allCommands.Count, Is.EqualTo(1));
        Assert.IsTrue(CursorCutter(allCommands[0]).StartsWith("/*outermost*/"));

        allCommands.Clear();
      }


      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {

          Assert.That(session.Tags.Count, Is.EqualTo(2));
          Assert.That(session.Tags[0], Is.EqualTo("outermost"));
          Assert.That(session.Tags[1], Is.EqualTo("in-between"));

          session.Events.DbCommandExecuting += SqlCapturer;
          _ = session.Query.Single<BusinessUnit>(id);
          session.Events.DbCommandExecuting -= SqlCapturer;

          Assert.That(allCommands.Count, Is.EqualTo(1));
          Assert.IsTrue(CursorCutter(allCommands[0]).StartsWith("/*outermost in-between*/"));

          allCommands.Clear();
        }
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {
          using (var dippest = session.Tag("deepest")) {

            session.Events.DbCommandExecuting += SqlCapturer;
            _ = session.Query.Single<BusinessUnit>(id);
            session.Events.DbCommandExecuting -= SqlCapturer;

            Assert.That(allCommands.Count, Is.EqualTo(1));
            Assert.IsTrue(CursorCutter(allCommands[0]).StartsWith("/*outermost in-between deepest*/"));

            allCommands.Clear();
          }
        }
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    [Test]
    public void SessionTagSingleOrDefault()
    {
      var allCommands = new List<string>();
      var id = 0L;

      using (var populateSession = Domain.OpenSession())
      using (var tx = populateSession.OpenTransaction()) {
        id = new BusinessUnit() { Name = "SingleOrDefault #1" }.Id;

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += SqlCapturer;
        _ = session.Query.SingleOrDefault<BusinessUnit>(id);
        session.Events.DbCommandExecuting -= SqlCapturer;

        Assert.That(allCommands.Count, Is.EqualTo(1));
        Assert.IsTrue(CursorCutter(allCommands[0]).StartsWith("/*outermost*/"));

        allCommands.Clear();
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {

          Assert.That(session.Tags.Count, Is.EqualTo(2));
          Assert.That(session.Tags[0], Is.EqualTo("outermost"));
          Assert.That(session.Tags[1], Is.EqualTo("in-between"));

          session.Events.DbCommandExecuting += SqlCapturer;
          _ = session.Query.SingleOrDefault<BusinessUnit>(id);
          session.Events.DbCommandExecuting -= SqlCapturer;

          Assert.That(allCommands.Count, Is.EqualTo(1));
          Assert.IsTrue(CursorCutter(allCommands[0]).StartsWith("/*outermost in-between*/"));

          allCommands.Clear();
        }
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {
          using (var dippest = session.Tag("deepest")) {

            session.Events.DbCommandExecuting += SqlCapturer;
            _ = session.Query.SingleOrDefault<BusinessUnit>(id);
            session.Events.DbCommandExecuting -= SqlCapturer;

            Assert.That(allCommands.Count, Is.EqualTo(1));
            Assert.IsTrue(CursorCutter(allCommands[0]).StartsWith("/*outermost in-between deepest*/"));

            allCommands.Clear();
          }
        }
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    [Test]
    public void SessionTagPrefetchEntity()
    {
      var allCommands = new List<string>();

      using (var populateSession = Domain.OpenSession())
      using (var tx = populateSession.OpenTransaction()) {
        var businessUnit = new BusinessUnit() { Name = "Prefetch Entity #1" };
        var property1 = new Property() { Owner = businessUnit, Name = "Prefetch Entity #1" };
        var property2 = new Property() { Owner = businessUnit, Name = "Prefetch Entity #3" };

        businessUnit = new BusinessUnit() { Name = "Prefetch Entity #2" };
        property1 = new Property() { Owner = businessUnit, Name = "Prefetch Entity #3" };
        property2 = new Property() { Owner = businessUnit, Name = "Prefetch Entity #3" };

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += SqlCapturer;
        _ = session.Query.All<Property>().Prefetch(p => p.Owner).ToArray();
        session.Events.DbCommandExecuting -= SqlCapturer;

        Assert.That(allCommands.Count, Is.EqualTo(2));
        Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost*/"));

        allCommands.Clear();
      }


      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {

          Assert.That(session.Tags.Count, Is.EqualTo(2));
          Assert.That(session.Tags[0], Is.EqualTo("outermost"));
          Assert.That(session.Tags[1], Is.EqualTo("in-between"));

          session.Events.DbCommandExecuting += SqlCapturer;
          _ = session.Query.All<Property>().Prefetch(p => p.Owner).ToArray();
          session.Events.DbCommandExecuting -= SqlCapturer;

          Assert.That(allCommands.Count, Is.EqualTo(2));
          Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost in-between*/"));

          allCommands.Clear();
        }
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {
          using (var dippest = session.Tag("deepest")) {

            session.Events.DbCommandExecuting += SqlCapturer;
            _ = session.Query.All<Property>().Prefetch(p => p.Owner).ToArray();
            session.Events.DbCommandExecuting -= SqlCapturer;

            Assert.That(allCommands.Count, Is.EqualTo(2));
            Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost in-between deepest*/"));

            allCommands.Clear();
          }
        }
      }

      void SqlCapturer(object sender, DbCommandEventArgs args)
      {
        allCommands.Add(args.Command.CommandText);
      }
    }

    [Test]
    public void SessionTagPrefetchEntitySet()
    {
      var allCommands = new List<string>();
      var batchesSupported = Domain.Handlers.ProviderInfo.Supports(Providers.ProviderFeatures.DmlBatches);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var book = new Author();
        _ = book.Books.Add(new Book() { Name = "Prefetch EntitySet Book #1" });
        _ = book.Books.Add(new Book() { Name = "Prefetch EntitySet Book #2" });

        book = new Author();
        _ = book.Books.Add(new Book() { Name = "Prefetch EntitySet Book #1" });
        _ = book.Books.Add(new Book() { Name = "Prefetch EntitySet Book #2" });

        tx.Complete();
      }


      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += SqlCapturer;
        _ = session.Query.All<Author>().Prefetch(a => a.Books).ToArray();
        session.Events.DbCommandExecuting -= SqlCapturer;

        if (batchesSupported) {
          Assert.That(allCommands.Count, Is.EqualTo(2));
          Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost*/"));
        }
        else {
          Assert.That(allCommands.Count, Is.EqualTo(3));
          Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost*/"));
          Assert.IsTrue(CursorCutter(allCommands[2]).StartsWith("/*outermost*/"));
        }

        allCommands.Clear();
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {

          session.Events.DbCommandExecuting += SqlCapturer;
          _ = session.Query.All<Author>().Prefetch(a => a.Books).ToArray();
          session.Events.DbCommandExecuting -= SqlCapturer;

          if (batchesSupported) {
            Assert.That(allCommands.Count, Is.EqualTo(2));
            Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost in-between*/"));
          }
          else {
            Assert.That(allCommands.Count, Is.EqualTo(3));
            Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost in-between*/"));
            Assert.IsTrue(CursorCutter(allCommands[2]).StartsWith("/*outermost in-between*/"));
          }

          allCommands.Clear();
        }
      }

      using (var session = Domain.OpenSession())
      using (var outermost = session.Tag("outermost"))
      using (var tx = session.OpenTransaction()) {
        using (var inBetween = session.Tag("in-between")) {
          using (var dippest = session.Tag("deepest")) {

            session.Events.DbCommandExecuting += SqlCapturer;
            _ = session.Query.All<Author>().Prefetch(a => a.Books).ToArray();
            session.Events.DbCommandExecuting -= SqlCapturer;

            if (batchesSupported) {
              Assert.That(allCommands.Count, Is.EqualTo(2));
              Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost in-between deepest*/"));
            }
            else {
              Assert.That(allCommands.Count, Is.EqualTo(3));
              Assert.IsTrue(CursorCutter(allCommands[1]).StartsWith("/*outermost in-between deepest*/"));
              Assert.IsTrue(CursorCutter(allCommands[2]).StartsWith("/*outermost in-between deepest*/"));
            }

            allCommands.Clear();
          }
        }
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