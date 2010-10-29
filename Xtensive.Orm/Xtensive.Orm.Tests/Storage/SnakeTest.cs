// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Helpers;
using Xtensive.Parameters;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse;
using Xtensive.Orm.Tests.Storage.SnakesModel;


namespace Xtensive.Orm.Tests.Storage.SnakesModel
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

  [Serializable]
  [DebuggerDisplay("Name = '{Name}'")]
  [Index("Name")]
  [Index("Name", "AlsoKnownAs")]
  [HierarchyRoot]
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

  [Serializable]
  [DebuggerDisplay("Name = '{Name}'; Length = {Length}")]
  public class Snake : Creature
  {
    [Field]
    public int? Length { get; set; }
  }

  [Serializable]
  [DebuggerDisplay("Name = '{Name}'; Color = {Color}")]
  public class Lizard : Creature
  {
    [Field(Length = 20)]
    public string Color { get; set; }
  }

  [Serializable]
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

namespace Xtensive.Orm.Tests.Storage
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
      Require.ProviderIs(StorageProvider.Index);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.SnakesModel");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var result = base.BuildDomain(configuration);

      Xtensive.Orm.Model.FieldInfo field;
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new Snake {Name = "Kaa1", Length = 1, Features = Features.None};
          new Snake { Name = "Kaa2", Length = 2, Features = Features.None };
          new Snake { Name = "Kaa1", Length = 3, Features = Features.None };
          new Snake { Name = "Kaa2", Length = 2, Features = Features.None };
          new Snake { Name = "Kaa1", Length = 3, Features = Features.CanCrawl };
          new Snake { Name = "Kaa3", Length = 3, Features = Features.None };

          Session.Current.SaveChanges();

          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          RecordQuery rqSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();
          var recordQuery = rqSnakePrimary.OrderBy(OrderBy.Asc(4)).OrderBy(OrderBy.Asc(2));

          recordQuery.Count(session);
          recordQuery = recordQuery.Aggregate(
            new [] {4, 2},
            new AggregateColumnDescriptor("Count1", 0, AggregateType.Count),
            new AggregateColumnDescriptor("Min1", 0, AggregateType.Min),
            new AggregateColumnDescriptor("Max1", 0, AggregateType.Max),
            new AggregateColumnDescriptor("Sum1", 0, AggregateType.Sum),
            new AggregateColumnDescriptor("Avg1", 0, AggregateType.Avg));

          t.Complete();
          Assert.AreEqual(recordQuery.Count(session), 4);
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < snakesCount; i++)
            new Snake {Name = ("MyKaa" + i), Length = i};
          for (int j = 0; j < creaturesCount; j++)
            new Creature {Name = ("Creature" + j)};
          for (int i = 0; i < lizardsCount; i++)
            new Lizard {Name = ("Lizard" + i), Color = ("Color" + i)};

          session.SaveChanges();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];

          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();

          var rsCalculated = rsSnakePrimary.Calculate(new CalculatedColumnDescriptor("FullName", typeof (string), (s) => (s.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(0, 2))),
            new CalculatedColumnDescriptor("FullName2", typeof (string), (s) => (s.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(0, 3))))
            .Take(10);

          Assert.AreEqual(10, rsCalculated.Count(session));

          foreach (var tuple in rsCalculated.ToRecordSet(session)) {
            Assert.AreEqual("My", tuple.GetValue(rsCalculated.Header.Length - 2));
            Assert.AreEqual("MyK", tuple.GetValue(rsCalculated.Header.Length - 1));
          }
          rsSnakePrimary.Count(session);

          RecordQuery aggregates = rsSnakePrimary.Aggregate(null,
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

          Assert.AreEqual(aggregates.Count(session), 1);

          string name = "TestName";
          var scope = TemporaryDataScope.Global;
          RecordQuery saved = rsSnakePrimary.
            Take(10).
            Take(5).
            Save(scope, name);

          saved.Count(session);
          var loaded = RecordQuery.Load(saved.Header, scope, name);

          AssertEx.AreEqual(saved.ToRecordSet(session), loaded.ToRecordSet(session));
          t.Complete();
        }
      }
    }

    [Test]
    public void MainTest()
    {

      int i = (int) Convert.ChangeType("123", typeof (Int32));

      Key persistedKey = null;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake snake = new Snake();
          Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
          persistedKey = snake.Key;

          Assert.IsNotNull(snake.Key);
          Assert.AreEqual((object) snake, session.Query.SingleOrDefault(snake.Key));

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

          Session.Current.SaveChanges();
          t.Complete();
        }
      }

      using (new Measurement("Fetching..."))
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Creature snake = session.Query.SingleOrDefault<Creature>(persistedKey);
          Assert.AreEqual(PersistenceState.Synchronized, snake.PersistenceState);
          Assert.IsNotNull(snake);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          t.Complete();
        }
      }

      using (new Measurement("Fetching..."))
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake snake = session.Query.SingleOrDefault<Snake>(persistedKey);
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake s = new Snake();
          key = s.Key;
          s.Name = "Kaa";
          Assert.AreEqual(PersistenceState.New, s.PersistenceState);
          Session.Current.SaveChanges();

          Assert.AreEqual("Kaa", s.Name);
          Assert.AreEqual(PersistenceState.Synchronized, s.PersistenceState);

          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake s = session.Query.SingleOrDefault<Snake>(key);
          Assert.AreEqual(PersistenceState.Synchronized, s.PersistenceState);
          Assert.AreEqual("Kaa", s.Name);
          s.Length = 32;
          Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
          Session.Current.SaveChanges();

          Assert.AreEqual(32, s.Length);
          Assert.AreEqual(PersistenceState.Synchronized, s.PersistenceState);
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake s = session.Query.SingleOrDefault<Snake>(key);
          s.Name = "Snake";
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake s = session.Query.SingleOrDefault<Snake>(key);
          s.Length = 16;
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake s = session.Query.SingleOrDefault<Snake>(key);
          s.Name = "Kaa";
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake s = session.Query.SingleOrDefault<Snake>(key);
          s.Length = 32;
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Snake s = session.Query.SingleOrDefault<Snake>(key);
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          try {
            session.Query.SingleOrDefault<Snake>(key);
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
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

          session.SaveChanges();

          var pID = new Parameter<Range<Entire<Tuple>>>();
          var pName = new Parameter<Range<Entire<Tuple>>>();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];
          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();
          RecordQuery rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordQuery();

          RecordQuery result = rsSnakePrimary
            .Range(() => pID.Value)
            .Join(rsSnakeName
              .Range(() => pName.Value)
              .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
              .Alias("NameIndex"), rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf(cID));
          
          using (new ParameterContext().Activate()) {
            pID.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(21)),
              new Entire<Tuple>(Tuple.Create(120), Direction.Positive));
            pName.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create("Kaa")), new Entire<Tuple>(Tuple.Create("Kaa900")));
            var count = result.Count(session);
            Assert.AreEqual(91, count);
          }

          result = rsSnakeName
            .OrderBy(OrderBy.Desc(rsSnakeName.Header.IndexOf(cName)), true)
            .Like(Tuple.Create("Kaa" + 10));

          var cc = result.Count(session);
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }

          session.SaveChanges();

          var pID = new Parameter<RangeSet<Entire<Tuple>>>();
          var pName = new Parameter<RangeSet<Entire<Tuple>>>();

          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();
          RecordQuery rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordQuery();

          RecordQuery result = rsSnakePrimary
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
            var count = result.Count(session);
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
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
          session.SaveChanges();

          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          TypeInfo lizardType = Domain.Model.Types[typeof(Lizard)];
          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();
          RecordQuery rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordQuery();
          RecordQuery rsLizardName = lizardType.Indexes.GetIndex("Name").ToRecordQuery();


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
 
          var count = snakes1.Intersect(snakes2).Count(session);
          Assert.AreEqual(count, snakesCount / 5);
          count = snakes1.Except(snakes2).Count(session);
          Assert.AreEqual(count, snakesCount / 2 - snakesCount / 5);
          count = snakes1.Concat(snakes2).Count(session);
          Assert.AreEqual(count, snakesCount / 2 + snakesCount / 5);
          count = snakes1.Union(snakes2).Count(session);
          Assert.AreEqual(count, snakesCount / 2);

          count = rsSnakeName.Except(rsLizardName).Count(session);
          Assert.AreEqual(count, snakesCount);
          count = rsSnakeName.Intersect(rsLizardName).Count(session);
          Assert.AreEqual(count, 0);
          count = rsSnakeName.Concat(rsLizardName).Count(session);
          Assert.AreEqual(count, snakesCount + lizardsCount);
          count = rsSnakeName.Union(rsLizardName).Count(session);
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

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

          session.SaveChanges();

          TypeInfo snakeType = Domain.Model.Types[typeof(ClearSnake)];
          RecordQuery rsSnake = snakeType.Indexes.GetIndex(cLength, "Description").ToRecordQuery();

          RecordQuery result = rsSnake
            .Like(Tuple.Create(1, "KkK"));

          var c = result.Count(session);
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();
          string name = "90";

          RecordQuery result = rsSnakePrimary.Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Contains(name));
          Assert.Greater(result.Count(session), 0);
          
          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).EndsWith(name));
          Assert.Greater(result.Count(session), 0);

          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > 10);
          Assert.Greater(result.Count(session), 0);

          int len = 10;
          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > len);
          Assert.Greater(result.Count(session), 0);

          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) * 2 * tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > len);
          Assert.Greater(result.Count(session), 0);

          var pLen = new Parameter<int>();
          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > pLen.Value);
          using (new ParameterContext().Activate()) {
            pLen.Value = 10;
            Assert.Greater(result.Count(session), 0);
          }

          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(3, 1) == "9");
          Assert.Greater(result.Count(session), 0);

          name = "Kaa90";
          var keyColumns = rsSnakePrimary.Header.ColumnGroups[0].Keys.ToArray();
          var nameFieldIndex = rsSnakePrimary.Header.IndexOf(cName);
          result = rsSnakePrimary
            .Filter(tuple => tuple.GetValue<string>(nameFieldIndex).GreaterThan(name));
          Assert.Greater(result.Count(session), 0);
          Assert.Greater(result.ToRecordSet(session).ToEntities<Snake>(0)
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordQuery rsPrimary = type.Indexes.PrimaryIndex.ToRecordQuery();
          foreach (var entity in rsPrimary.ToRecordSet(session).ToEntities<ICreature>(0).ToList())
            entity.Remove();
          t.Complete(); 
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
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

          session.SaveChanges();
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordQuery rsPrimary = type.Indexes.PrimaryIndex.ToRecordQuery();
          foreach (var entity in rsPrimary.ToRecordSet(session).ToEntities<ICreature>(0))
            Assert.IsNotNull(entity.Name);
          t.Complete();
        }
      }
    }

    [Test]
    public void RemovalTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < 10; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          Session.Current.SaveChanges();
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordQuery rs = type.Indexes.PrimaryIndex.ToRecordQuery();
          foreach (var entity in rs.ToRecordSet(session).ToEntities<ICreature>(0).ToList())
            entity.Remove();
          Session.Current.SaveChanges();
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Assert.AreEqual(0, session.Query.All<Snake>().Count());
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
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
          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();
          
          // Test for SQL generation
          var rsSkipTest = rsSnakePrimary.Skip(2);
          var skipCount = rsSkipTest.Count(session); // Tests for specific case.
          Assert.AreEqual(rsSnakePrimary.Count(session), skipCount + 2);

          var rsTwoFieldIndex = snakeType.Indexes.GetIndex("Name", "AlsoKnownAs").ToRecordQuery();
          var twoFieldIndexCount = rsTwoFieldIndex.Skip(3).Count(session); // Tests for specific case.
          Assert.AreEqual(rsSnakePrimary.Count(session), twoFieldIndexCount + 3);

          using (new Measurement("Query performance")) {
            RecordQuery rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordQuery();
            rsSnakeName = rsSnakeName
              .Range(fromName, toName)
              .OrderBy(OrderBy.Asc(rsSnakeName.Header.IndexOf(cID)))
              .Alias("NameIndex");

            RecordQuery range = rsSnakePrimary.Range(from, to);
            RecordQuery join = range.Join(rsSnakeName, new Pair<int>(rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf("NameIndex."+cID)));
            RecordQuery where = join.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) >= 100);
            RecordQuery orderBy = where.OrderBy(OrderBy.Desc(rsSnakePrimary.Header.IndexOf(cName)));
            RecordQuery skip = orderBy.Skip(5);
            RecordQuery take = skip.Take(50);
            RecordQuery skip2 = take.Skip(7);
            var snakesRse = take.ToRecordSet(session).ToEntities<Snake>(0);
            t.Complete();
            foreach (Snake snake in snakesRse) {
              Console.WriteLine(snake.Key);
            }
            Assert.AreEqual(15, snakesRse.Count());
            Assert.AreEqual(8, skip2.Count(session));
            // Row Number
            RecordQuery rsRowNumber1 = skip2.RowNumber("RowNumber1");
            Assert.AreEqual(skip2.Count(session), rsRowNumber1.Count(session));
            int rowNumber = 1;
            foreach (var tuple in rsRowNumber1.ToRecordSet(session))
            {
              Assert.AreEqual(rowNumber++, tuple.GetValueOrDefault(rsRowNumber1.Header.Columns["RowNumber1"].Index));
            }
          }
          
          IEnumerable<Snake> snakes = session.Query.All<Snake>();
          Assert.AreEqual(snakesCount, snakes.Count());
          IEnumerable<Creature> creatures = session.Query.All<Creature>();
          Assert.AreEqual(creaturesCount + snakesCount + lizardsCount, creatures.Count());

          Snake snakeKaa53 = session.Query.All<Snake>()
            .Where(snake => snake.Name=="Kaa53")
            .First();
          Assert.AreEqual("Kaa53", snakeKaa53.Name);

          var result = from s in session.Query.All<Snake>()
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

      Require.ProviderIsNot(StorageProvider.Memory);

      TestFixtureTearDown();
      TestFixtureSetUp();

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
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
          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();

          using (new Measurement("Query performance")) {
            RecordQuery rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordQuery();
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

            Assert.AreEqual(15, snakesRse.Count(session));
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

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          for (int i = 0; i < snakesCount; i++)
            new Snake {Name = ("Kaa" + i), Length = i};
          for (int j = 0; j < creaturesCount; j++)
            new Creature {Name = ("Creature" + j)};
          for (int i = 0; i < lizardsCount; i++)
            new Lizard {Name = ("Lizard" + i), Color = ("Color" + i)};

          var pID = new Parameter<Range<Entire<Tuple>>>();
          var pName = new Parameter<Range<Entire<Tuple>>>();
          var pLength = new Parameter<int>();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];
          RecordQuery rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordQuery();
          RecordQuery rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordQuery();

          RecordQuery result = rsSnakePrimary
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
            Assert.AreEqual(15, result.Count(session));
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < snakesCount; i++) {
            var snake = new Snake {Name = ("Name_" + i), Length = (i % 11 + 2)};
            snakesDTO.Add(Tuple.Create(snake.Key, snake.Name, snake.Length));
          }
          t.Complete();
        }
      }

      using (new Measurement("Fetching...", snakesCount))
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake persistedSnake = session.Query.SingleOrDefault<Snake>(snakesDTO[i].GetValue<Key>(0));
            Assert.IsNotNull(persistedSnake);
            Assert.AreEqual(snakesDTO[i].GetValue<string>(1), persistedSnake.Name);
            Assert.AreEqual(snakesDTO[i].GetValue<int?>(2), persistedSnake.Length);
          }
          Session.Current.SaveChanges();
          t.Complete();
        }
      }
    }

    [Test]
    public void SetValuePerformance()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var recordSet = Domain.Model.Types[typeof (Creature)].Indexes.PrimaryIndex.ToRecordQuery();
        var column = recordSet.Header.IndexOf("Name");
        var descriptor = new CalculatedColumnDescriptor("WowName", typeof (string), t => ((string) t.GetValue(column)) + "!!!");
        recordSet.Calculate(descriptor).Alias("lalala").ToRecordSet(session).ToList();
      }
    }

    [Test]
    public void LinqQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var lizardsDirectly = (
          from lizard in session.Query.All<Lizard>().AsEnumerable()
          where lizard.Color.StartsWith("G") || lizard.Color.StartsWith("g")
          select lizard
          ).ToArray();
        var lizards = (
          from lizard in session.Query.All<Lizard>()
          where lizard.Color.StartsWith("G") || lizard.Color.StartsWith("g")
          select lizard
          ).ToArray();
        AssertEx.AreEqual(lizardsDirectly, lizards);
      }
    }
  }
}
