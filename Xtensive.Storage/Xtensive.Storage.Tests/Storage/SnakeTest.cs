// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.28

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.SnakesModel;


namespace Xtensive.Storage.Tests.Storage.SnakesModel
{
  public enum Features
  {
    None = 0,
    CanCrawl = 1,
    CanJump = 2,
    CanFly = 3,
    CanWalk = 4,
  }

  public interface ICreature : IEntity
  {
    [Field(Length = 255)]
    string Name { get; set; }

    [Field(Length = 255)]
    string AlsoKnownAs { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'")]
  [Index("Name")]
  [Index("Name", "AlsoKnownAs")]
  [HierarchyRoot]
  [KeyGenerator(CacheSize = 16)]
  public class Creature : Entity,
    ICreature
  {
    [Field, Key]
    public int ID { get; private set; }
        
    public string Name { get; set; }

    public string AlsoKnownAs { get; set; }

    [Field]
    public Features? Features { get; set; }

    [Field]
    public TimeSpan LifeDuration { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Length = {Length}")]
  public class Snake : Creature
  {
    [Field]
    public int? Length { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Color = {Color}")]
  public class Lizard : Creature
  {
    [Field(Length = 20)]
    public string Color { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Length = {Length}")]
  [Index("Length", "Description")]
  public class ClearSnake : Creature
  {
    [Field]
    public int? Length { get; set; }

    [Field]
    public string Description { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class SnakesTest : AutoBuildTest
  {
    // Column names (they can be different for different storage providers)
    private string cID;
    private string cName;
    private string cLength;
    private string cFeatures;

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Index);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Xtensive.Storage.Domain result = base.BuildDomain(configuration);

      Xtensive.Storage.Model.FieldInfo field;
      field = result.Model.Types[typeof (Creature)].Fields["ID"];
      cID = field.Column.Name;
      field = result.Model.Types[typeof (Creature)].Fields["Name"];
      cName = field.Column.Name;
      field = result.Model.Types[typeof (Snake)].Fields["Length"];
      cLength = field.Column.Name;
      field = result.Model.Types[typeof (Snake)].Fields["Features"];
      cFeatures = field.Column.Name;
      return result;
    }

    [Test]
    public void GroupTest()
    {
      TestFixtureTearDown();
      TestFixtureSetUp();
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          new Snake {Name = "Kaa1", Length = 1, Features = Features.None};
          new Snake { Name = "Kaa2", Length = 2, Features = Features.None };
          new Snake { Name = "Kaa1", Length = 3, Features = Features.None };
          new Snake { Name = "Kaa2", Length = 2, Features = Features.None };
          new Snake { Name = "Kaa1", Length = 3, Features = Features.CanCrawl };
          new Snake { Name = "Kaa3", Length = 3, Features = Features.None };

          Session.Current.Persist();

          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          var rs = rsSnakePrimary.OrderBy(OrderBy.Asc(4)).OrderBy(OrderBy.Asc(2));

          rs.Count();
          rs = rs.Aggregate(
            new [] {4, 2},
            new AggregateColumnDescriptor("Count1", 0, AggregateType.Count),
            new AggregateColumnDescriptor("Min1", 0, AggregateType.Min),
            new AggregateColumnDescriptor("Max1", 0, AggregateType.Max),
            new AggregateColumnDescriptor("Sum1", 0, AggregateType.Sum),
            new AggregateColumnDescriptor("Avg1", 0, AggregateType.Avg));

          t.Complete();
          Assert.AreEqual(rs.Count(), 4);
        }
      }
    }

    [Test]
    public void ProviderTest()
    {
      const int snakesCount = 100;
      const int creaturesCount = 100;
      const int lizardsCount = 100;

      TestFixtureTearDown();
      TestFixtureSetUp();

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++)
            new Snake {Name = ("MyKaa" + i), Length = i};
          for (int j = 0; j < creaturesCount; j++)
            new Creature {Name = ("Creature" + j)};
          for (int i = 0; i < lizardsCount; i++)
            new Lizard {Name = ("Lizard" + i), Color = ("Color" + i)};

          Session.Current.Persist();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];

          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();

          var rsCalculated = rsSnakePrimary.Calculate(new CalculatedColumnDescriptor("FullName", typeof (string), (s) => (s.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(0, 2))),
            new CalculatedColumnDescriptor("FullName2", typeof (string), (s) => (s.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(0, 3))))
            .Take(10);

          Assert.AreEqual(10, rsCalculated.Count());

          foreach (var tuple in rsCalculated) {
            Assert.AreEqual("My", tuple.GetValue(rsCalculated.Header.Length - 2));
            Assert.AreEqual("MyK", tuple.GetValue(rsCalculated.Header.Length - 1));
          }
          rsSnakePrimary.Count();

          RecordSet aggregates = rsSnakePrimary.Aggregate(null,
            new AggregateColumnDescriptor("Count1", 0, AggregateType.Count),
            new AggregateColumnDescriptor("Min1", 0, AggregateType.Min),
            new AggregateColumnDescriptor("Max1", 0, AggregateType.Max),
            new AggregateColumnDescriptor("Sum1", 0, AggregateType.Sum),
            new AggregateColumnDescriptor("Avg1", 0, AggregateType.Avg),
            new AggregateColumnDescriptor("Count2", 2, AggregateType.Count),
            new AggregateColumnDescriptor("Min2", 2, AggregateType.Min),
            new AggregateColumnDescriptor("Max2", 2, AggregateType.Max),
            new AggregateColumnDescriptor("Count3", 3, AggregateType.Count),
            new AggregateColumnDescriptor("Max3", 3, AggregateType.Max));

          Assert.AreEqual(aggregates.Count(), 1);

          string name = "TestName";
          var scope = TemporaryDataScope.Global;
          RecordSet saved = rsSnakePrimary.
            Take(10).
            Take(5).
            Save(scope, name);

          saved.Count();
          var loaded = RecordSet.Load(saved.Header, scope, name);

          AssertEx.AreEqual(saved, loaded);
          t.Complete();
        }
      }
    }

    [Test]
    public void MainTest()
    {

      int i = (int) Convert.ChangeType("123", typeof (Int32));

      Key persistedKey = null;
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake snake = new Snake();
          Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
          persistedKey = snake.Key;

          Assert.IsNotNull(snake.Key);
          Assert.AreEqual((object) snake, Query.SingleOrDefault(snake.Key));

          Assert.AreEqual(null, snake.Length);
          Assert.AreEqual(null, snake.Name);
          Assert.AreEqual(null, snake.Features);

          snake.Name = "Kaa";
          snake.Features = Features.CanCrawl;
          Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          snake.Length = 32;
          Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
          Assert.AreEqual(32, snake.Length);
            
          Key key = Key.Create<Snake>(IncludeTypeIdModifier.IsEnabled
            ? Tuple.Create(snake.ID, snake.TypeId) : Tuple.Create(snake.ID));
          var keyString = key.Format();
          Assert.AreEqual(Key.Parse(keyString), key);
          
          Assert.IsTrue(snake.Key.Equals(key));
          Assert.AreEqual(snake.Key, key);

          Lizard lizard = new Lizard();
          lizard.Color = "#ff5544";
          lizard.Features = Features.CanWalk;

          Session.Current.Persist();
          t.Complete();
        }
      }

      using (new Measurement("Fetching..."))
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Creature snake = Query<Creature>.SingleOrDefault(persistedKey);
          Assert.AreEqual(PersistenceState.Synchronized, snake.PersistenceState);
          Assert.IsNotNull(snake);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          t.Complete();
        }
      }

      using (new Measurement("Fetching..."))
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake snake = Query<Snake>.SingleOrDefault(persistedKey);
          Assert.IsNotNull(snake);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(32, snake.Length);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          t.Complete();
        }
      }
    }

    [Test]
    public void UpdateTest()
    {
      Key key;
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake s = new Snake();
          key = s.Key;
          s.Name = "Kaa";
          Assert.AreEqual(PersistenceState.New, s.PersistenceState);
          Session.Current.Persist();

          Assert.AreEqual("Kaa", s.Name);
          Assert.AreEqual(PersistenceState.Synchronized, s.PersistenceState);

          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake s = Query<Snake>.SingleOrDefault(key);
          Assert.AreEqual(PersistenceState.Synchronized, s.PersistenceState);
          Assert.AreEqual("Kaa", s.Name);
          s.Length = 32;
          Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
          Session.Current.Persist();

          Assert.AreEqual(32, s.Length);
          Assert.AreEqual(PersistenceState.Synchronized, s.PersistenceState);
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake s = Query<Snake>.SingleOrDefault(key);
          s.Name = "Snake";
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake s = Query<Snake>.SingleOrDefault(key);
          s.Length = 16;
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake s = Query<Snake>.SingleOrDefault(key);
          s.Name = "Kaa";
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake s = Query<Snake>.SingleOrDefault(key);
          s.Length = 32;
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Snake s = Query<Snake>.SingleOrDefault(key);
          Assert.AreEqual(PersistenceState.Synchronized, s.PersistenceState);
          Assert.AreEqual("Kaa", s.Name);
          Assert.AreEqual(32, s.Length);
          Assert.IsFalse(s.IsRemoved);
          s.Remove();
          Assert.AreEqual(PersistenceState.Removed, s.PersistenceState);
          Assert.IsTrue(s.IsRemoved);
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          try {
            Query<Snake>.SingleOrDefault(key);
          }
          catch (InvalidOperationException) {
          }
          t.Complete();
        }
      }
    }

    [Test]
    public void RangeTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      TestFixtureTearDown();
      TestFixtureSetUp();

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          for (int j = 0; j < creaturesCount; j++) {
            Creature c = new Creature();
            c.Name = "Creature" + j;
          }
          for (int i = 0; i < lizardsCount; i++) {
            Lizard l = new Lizard();
            l.Name = "Lizard" + i;
            l.Color = "Color" + i;
          }

          Session.Current.Persist();

          var pID = new Parameter<Range<Entire<Tuple>>>();
          var pName = new Parameter<Range<Entire<Tuple>>>();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();

          RecordSet result = rsSnakePrimary
            .Range(() => pID.Value)
            .Join(rsSnakeName
              .Range(() => pName.Value)
              .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
              .Alias("NameIndex"), rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf(cID));
          
          using (new ParameterContext().Activate()) {
            pID.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(21)),
              new Entire<Tuple>(Tuple.Create(120), Direction.Positive));
            pName.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create("Kaa")), new Entire<Tuple>(Tuple.Create("Kaa900")));
            var count = result.Count();
            Assert.AreEqual(91, count);
          }

          result = rsSnakeName
            .OrderBy(OrderBy.Desc(rsSnakeName.Header.IndexOf(cName)), true)
            .Like(Tuple.Create("Kaa" + 10));

          var cc = result.Count();
          Assert.AreEqual(cc, 11);
          
          t.Complete();
        }
      }
    }

    [Test]
    public void RangeSetTest()
    {
      const int snakesCount = 1000;

      TestFixtureTearDown();
      TestFixtureSetUp();

      if (Domain.Configuration.Name == "memory")
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }

          Session.Current.Persist();

          var pID = new Parameter<RangeSet<Entire<Tuple>>>();
          var pName = new Parameter<RangeSet<Entire<Tuple>>>();

          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();

          RecordSet result = rsSnakePrimary
          .RangeSet(() => pID.Value)
          .Join(rsSnakeName
          .RangeSet(() => pName.Value)
          .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
          .Alias("NameIndex"), rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf(cID));

          var idRange = new RangeSet<Entire<Tuple>>(new Range<Entire<Tuple>>(
            new Entire<Tuple>(Tuple.Create(21)), new Entire<Tuple>(Tuple.Create(120), Direction.Positive)),
            AdvancedComparer<Entire<Tuple>>.Default);
          idRange = idRange.Unite(new RangeSet<Entire<Tuple>>(new Range<Entire<Tuple>>(
            new Entire<Tuple>(Tuple.Create(221)), new Entire<Tuple>(Tuple.Create(320), Direction.Positive)),
            AdvancedComparer<Entire<Tuple>>.Default));
          var nameRange = new RangeSet<Entire<Tuple>>(new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create("Kaa")), new Entire<Tuple>(Tuple.Create("Kaa900"))), AdvancedComparer<Entire<Tuple>>.Default);
          
          using (new ParameterContext().Activate()) {
            pID.Value = idRange;
            pName.Value = nameRange;
            var count = result.Count();
            Assert.AreEqual(191, count);
          }
          t.Complete();
        }
      }
      else
        Assert.Ignore();
    }

    [Test]
    public void SetOperationsTest()
    {
      const int snakesCount = 10;
      const int lizardsCount = 10;

      TestFixtureTearDown();
      TestFixtureSetUp();
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          for (int i = 0; i < lizardsCount; i++) {
            Lizard l = new Lizard();
            l.Name = "Lizard" + i;
            l.Color = "Color" + i;
          }
          Session.Current.Persist();

          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          TypeInfo lizardType = Domain.Model.Types[typeof(Lizard)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();
          RecordSet rsLizardName = lizardType.Indexes.GetIndex("Name").ToRecordSet();


          var snakes1 = rsSnakePrimary
            .Join(rsSnakeName
            .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
            .Alias("NameIndex"),rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf(cID))
            .Filter(sId => sId.GetValue<int>(0) <= snakesCount / 2);

          var snakes2 = rsSnakePrimary
            .Join(rsSnakeName
            .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
            .Alias("NameIndex"), rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf(cID))
            .Filter(sId => sId.GetValue<int>(0) <= snakesCount / 5);
 
          var count = snakes1.Intersect(snakes2).Count();
          Assert.AreEqual(count, snakesCount / 5);
          count = snakes1.Except(snakes2).Count();
          Assert.AreEqual(count, snakesCount / 2 - snakesCount / 5);
          count = snakes1.Concat(snakes2).Count();
          Assert.AreEqual(count, snakesCount / 2 + snakesCount / 5);
          count = snakes1.Union(snakes2).Count();
          Assert.AreEqual(count, snakesCount / 2);

          count = rsSnakeName.Except(rsLizardName).Count();
          Assert.AreEqual(count, snakesCount);
          count = rsSnakeName.Intersect(rsLizardName).Count();
          Assert.AreEqual(count, 0);
          count = rsSnakeName.Concat(rsLizardName).Count();
          Assert.AreEqual(count, snakesCount + lizardsCount);
          count = rsSnakeName.Union(rsLizardName).Count();
          Assert.AreEqual(count, snakesCount + lizardsCount);

          AssertEx.Throws<InvalidOperationException>(() => rsSnakeName.Intersect(rsSnakePrimary));
          AssertEx.Throws<InvalidOperationException>(() => rsSnakeName.Except(rsSnakePrimary));
          AssertEx.Throws<InvalidOperationException>(() => rsSnakeName.Concat(rsSnakePrimary));
          AssertEx.Throws<InvalidOperationException>(() => rsSnakeName.Union(rsSnakePrimary));

          t.Complete();
        } 
      }
    }

    [Test]
    public void LikeTest()
    {
      TestFixtureTearDown();
      TestFixtureSetUp();
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {

          ClearSnake s = new ClearSnake();
          s.Description = "kkKklm";
          s.Length = 1;

          ClearSnake s1 = new ClearSnake();
          s1.Description = "kKklm";
          s1.Length = 3;

          ClearSnake s2 = new ClearSnake();
          s2.Description = "KKK";
          s2.Length = 1;

          ClearSnake s3 = new ClearSnake();
          s3.Description = "Kkl";
          s3.Length = 1;

          Session.Current.Persist();

          TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
          RecordSet rsSnake = snakeType.Indexes.GetIndex(cLength, "Description").ToRecordSet();

          RecordSet result = rsSnake
            .Like(Tuple.Create(1, "KkK"));

          var c = result.Count();
          Assert.AreEqual(c, 2);
          
          t.Complete();
        }
      }
    }

    [Test]
    public void FilterTest()
    {
      TestFixtureTearDown();
      TestFixtureSetUp();

      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          for (int j = 0; j < creaturesCount; j++) {
            Creature c = new Creature();
            c.Name = "Creature" + j;
          }
          for (int i = 0; i < lizardsCount; i++) {
            Lizard l = new Lizard();
            l.Name = "Lizard" + i;
            l.Color = "Color" + i;
          }
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          string name = "90";

          RecordSet result = rsSnakePrimary.Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Contains(name));
          Assert.Greater(result.Count(), 0);
          
          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).EndsWith(name));
          Assert.Greater(result.Count(), 0);

          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > 10);
          Assert.Greater(result.Count(), 0);

          int len = 10;
          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > len);
          Assert.Greater(result.Count(), 0);

          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) * 2 * tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > len);
          Assert.Greater(result.Count(), 0);

          var pLen = new Parameter<int>();
          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > pLen.Value);
          using (new ParameterContext().Activate()) {
            pLen.Value = 10;
            Assert.Greater(result.Count(), 0);
          }

          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(3, 1) == "9");
          Assert.Greater(result.Count(), 0);

          name = "Kaa90";
          var keyColumns = rsSnakePrimary.Header.ColumnGroups[0].Keys.ToArray();
          var nameFieldIndex = rsSnakePrimary.Header.IndexOf(cName);
          result = rsSnakePrimary
            .Filter(tuple => tuple.GetValue<string>(nameFieldIndex).GreaterThan(name));
          Assert.Greater(result.Count(), 0);
          Assert.Greater(result.ToEntities<Snake>(0)
            .Where(s => s != null && s.Name.GreaterThan(name)).Count(), 1);

          t.Complete();
        }
      }
    }

    [Test]
    public void InterfaceTest()
    {
      const int snakesCount = 10;
      const int creaturesCount = 10;
      const int lizardsCount = 10;

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var session = Session.Current;
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordSet rsPrimary = type.Indexes.PrimaryIndex.ToRecordSet();
          foreach (var entity in rsPrimary.ToEntities<ICreature>(0).ToList())
            entity.Remove();
          t.Complete(); 
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var session = Session.Current;
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          for (int j = 0; j < creaturesCount; j++) {
            Creature c = new Creature();
            c.Name = "Creature" + j;
          }
          for (int i = 0; i < lizardsCount; i++) {
            Lizard l = new Lizard();
            l.Name = "Lizard" + i;
            l.Color = "Color" + i;
          }

          session.Persist();
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordSet rsPrimary = type.Indexes.PrimaryIndex.ToRecordSet();
          foreach (var entity in rsPrimary.ToEntities<ICreature>(0))
            Assert.IsNotNull(entity.Name);
          t.Complete();
        }
      }
    }

    [Test]
    public void RemovalTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < 10; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          Session.Current.Persist();
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var session = Session.Current;
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordSet rs = type.Indexes.PrimaryIndex.ToRecordSet();
          foreach (var entity in rs.ToEntities<ICreature>(0).ToList())
            entity.Remove();
          Session.Current.Persist();
          t.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Assert.AreEqual(0, Query<Snake>.All.Count());
          t.Complete();
        }
      }
    }

    [Test]
    public void QueryTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      TestFixtureTearDown();
      TestFixtureSetUp();

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var session = Session.Current;
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          for (int j = 0; j < creaturesCount; j++) {
            Creature c = new Creature();
            c.Name = "Creature" + j;
          }
          for (int i = 0; i < lizardsCount; i++) {
            Lizard l = new Lizard();
            l.Name = "Lizard" + i;
            l.Color = "Color" + i;
          }

          Tuple from = Tuple.Create(21);
          Tuple to = Tuple.Create(120);
          Tuple fromName = Tuple.Create("Kaa");
          Tuple toName = Tuple.Create("Kaa900");
          TypeInfo snakeType = session.Domain.Model.Types[typeof (Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          
          // Test for SQL generation
          var rsSkipTest = rsSnakePrimary.Skip(2);
          var skipCount = rsSkipTest.Count(); // Tests for specific case.
          Assert.AreEqual(rsSnakePrimary.Count(), skipCount + 2);

          var rsTwoFieldIndex = snakeType.Indexes.GetIndex("Name", "AlsoKnownAs").ToRecordSet();
          var twoFieldIndexCount = rsTwoFieldIndex.Skip(3).Count(); // Tests for specific case.
          Assert.AreEqual(rsSnakePrimary.Count(), twoFieldIndexCount + 3);

          using (new Measurement("Query performance")) {
            RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();
            rsSnakeName = rsSnakeName
              .Range(fromName, toName)
              .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
              .Alias("NameIndex");

            RecordSet range = rsSnakePrimary.Range(from, to);
            RecordSet join = range.Join(rsSnakeName, new Pair<int>(rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf("NameIndex."+cID)));
            RecordSet where = join.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) >= 100);
            RecordSet orderBy = where.OrderBy(OrderBy.Desc(rsSnakePrimary.Header.IndexOf(cName)));
            RecordSet skip = orderBy.Skip(5);
            RecordSet take = skip.Take(50);
            RecordSet skip2 = take.Skip(7);
            var snakesRse = take.ToEntities<Snake>(0);
            t.Complete();
            foreach (Snake snake in snakesRse) {
              Console.WriteLine(snake.Key);
            }
            Assert.AreEqual(15, snakesRse.Count());
            Assert.AreEqual(8, skip2.Count());
            // Row Number
            RecordSet rsRowNumber1 = skip2.RowNumber("RowNumber1");
            Assert.AreEqual(skip2.Count(), rsRowNumber1.Count());
            int rowNumber = 1;
            foreach (var tuple in rsRowNumber1)
            {
              Assert.AreEqual(rowNumber++, tuple[rsRowNumber1.Header.Columns["RowNumber1"].Index]);
            }
          }
          
          IEnumerable<Snake> snakes = Query<Snake>.All;
          Assert.AreEqual(snakesCount, snakes.Count());
          IEnumerable<Creature> creatures = Query<Creature>.All;
          Assert.AreEqual(creaturesCount + snakesCount + lizardsCount, creatures.Count());

          Snake snakeKaa53 = Query<Snake>.All
            .Where(snake => snake.Name=="Kaa53")
            .First();
          Assert.AreEqual("Kaa53", snakeKaa53.Name);

          var result = from s in Query<Snake>.All
          where s.Length >= 500
          select s;
          Assert.AreEqual(500, result.Count());

          t.Complete();
        }        
      }
    }


    [Test]
    public void TransferTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      TestFixtureTearDown();
      TestFixtureSetUp();

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var session = Session.Current;
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          for (int j = 0; j < creaturesCount; j++) {
            Creature c = new Creature();
            c.Name = "Creature" + j;
          }
          for (int i = 0; i < lizardsCount; i++) {
            Lizard l = new Lizard();
            l.Name = "Lizard" + i;
            l.Color = "Color" + i;
          }

          Tuple from = Tuple.Create(21);
          Tuple to = Tuple.Create(120);
          Tuple fromName = Tuple.Create("Kaa");
          Tuple toName = Tuple.Create("Kaa900");
          TypeInfo snakeType = session.Domain.Model.Types[typeof (Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();

          using (new Measurement("Query performance")) {
            RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();
            rsSnakeName = rsSnakeName
              .Range(fromName, toName)
              .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
              .Alias("NameIndex");

            var snakesRse = rsSnakePrimary
              .Range(from, to)
              .Join(rsSnakeName, new Pair<int>(rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf("NameIndex."+cID)))
              .ExecuteAt(TransferType.Client)
              .Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) >= 100)
              .OrderBy(OrderBy.Desc(rsSnakePrimary.Header.IndexOf(cName)))
              .Skip(5)
              .Take(50)
              //.ToEntities<Snake>()
              ;

            Assert.AreEqual(15, snakesRse.Count());
          }
          t.Complete();
        }        
      }
    }


    [Test]
    public void CachedQueryTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      TestFixtureTearDown();
      TestFixtureSetUp();

      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {

          for (int i = 0; i < snakesCount; i++)
            new Snake {Name = ("Kaa" + i), Length = i};
          for (int j = 0; j < creaturesCount; j++)
            new Creature {Name = ("Creature" + j)};
          for (int i = 0; i < lizardsCount; i++)
            new Lizard {Name = ("Lizard" + i), Color = ("Color" + i)};

          Session.Current.Persist();


          var pID = new Parameter<Range<Entire<Tuple>>>();
          var pName = new Parameter<Range<Entire<Tuple>>>();
          var pLength = new Parameter<int>();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();

          RecordSet result = rsSnakePrimary
            .Range(() => pID.Value)
            .Join(rsSnakeName
              .Range(() => pName.Value)
              .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
              .Alias("NameIndex"), rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf(cID))
            .Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) >= pLength.Value)
            .OrderBy(OrderBy.Desc(rsSnakePrimary.Header.IndexOf(cName)))
            .Skip(5)
            .Take(50);

          using (new ParameterContext().Activate()) {
            pID.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(21)),
              new Entire<Tuple>(Tuple.Create(120), Direction.Positive));
            pName.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create("Kaa")),
              new Entire<Tuple>(Tuple.Create("Kaa900")));
            pLength.Value = 100;
            Assert.AreEqual(15, result.Count());
          }

          t.Complete();
        }
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void PerformanceTest()
    {
      const int snakesCount = 1024;
      var snakesDTO = new List<Tuple>();

      using (new Measurement("Persisting...", snakesCount))
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            var snake = new Snake {Name = ("Name_" + i), Length = (i % 11 + 2)};
            snakesDTO.Add(Tuple.Create(snake.Key, snake.Name, snake.Length));
          }
          t.Complete();
        }
      }

      using (new Measurement("Fetching...", snakesCount))
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake persistedSnake = Query<Snake>.SingleOrDefault(snakesDTO[i].GetValue<Key>(0));
            Assert.IsNotNull(persistedSnake);
            Assert.AreEqual(snakesDTO[i].GetValue<string>(1), persistedSnake.Name);
            Assert.AreEqual(snakesDTO[i].GetValue<int?>(2), persistedSnake.Length);
          }
          Session.Current.Persist();
          t.Complete();
        }
      }
    }

    [Test]
    public void SetValuePerformance()
    {
      using (Session.Open(Domain)) {
        using (Transaction.Open()) {
          Snake s = new Snake();          
          const int count = 100000;
          using (new Measurement("Setting value...", count)) {
            for (int i = 0; i < count; i+=10) {
              s.Name = "Aaa";
              s.Name = "Bbb";
              s.Name = "Ccc";
              s.Name = "Ddd";
              s.Name = "Eee";
              s.Name = "Fff";
              s.Name = "Ggg";
              s.Name = "Hhh";
              s.Name = "Iii";
              s.Name = "Jjj";
            }
          }
        }
      }
    }

    [Test]
    public void GetValuePerformance()
    {
      using (Session.Open(Domain)) {
        using (Transaction.Open()) {
          Snake s = new Snake();
          s.Name = "SuperSnake";
          string value;
          const int count = 100000;
          using (new Measurement("Getting value...", count)) {
            for (int i = 0; i < count; i+=10) {
              value = s.Name;
              value = s.Name;
              value = s.Name;
              value = s.Name;
              value = s.Name;
              value = s.Name;
              value = s.Name;
              value = s.Name;
              value = s.Name;
              value = s.Name;
            }
          }
        }
      }
    }

    [Test]
    public void AliasAfterCalculateTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var recordSet = Domain.Model.Types[typeof (Creature)].Indexes.PrimaryIndex.ToRecordSet();
        var column = recordSet.Header.IndexOf("Name");
        var descriptor = new CalculatedColumnDescriptor("WowName", typeof (string), t => ((string) t.GetValue(column)) + "!!!");
        recordSet.Calculate(descriptor).Alias("lalala").ToList();
      }
    }

  }
}
