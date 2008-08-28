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
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Index;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.Storage.SnakesModel;
using SeekResultType=Xtensive.Indexing.SeekResultType;

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
  }

  [DebuggerDisplay("Name = '{Name}'")]
  [Index("Name")]
  [HierarchyRoot(typeof (Generator), "ID")]
  public class Creature : Entity,
    ICreature
  {
    [Field]
    public int ID { get; set; }

    public string Name { get; set; }

    [Field]
    public Features? Features { get; set; }
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
}

namespace Xtensive.Storage.Tests.Storage
{
  public class SnakesTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    [Test]
    public void MainTest()
    {
      Key persistedKey = null;
      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Snake snake = new Snake();
          Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
          persistedKey = snake.Key;

          Assert.IsNotNull(snake.Key);
          Assert.AreEqual((object) snake, snake.Key.Resolve());

          Assert.AreEqual(null, snake.Length);
          Assert.AreEqual(null, snake.Name);
          Assert.AreEqual(null, snake.Features);

          snake.Name = "Kaa";
          snake.Features = Features.CanCrawl;
          Assert.AreEqual(PersistenceState.Modified, snake.PersistenceState);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          snake.Length = 32;
          Assert.AreEqual(PersistenceState.Modified, snake.PersistenceState);
          Assert.AreEqual(32, snake.Length);

          Key key = Key.Get<Snake, int>(1);
          Assert.IsTrue(snake.Key.Equals(key));
          Assert.IsTrue(ReferenceEquals(snake.Key, key));

          Lizard lizard = new Lizard();
          lizard.Color = "#ff5544";
          lizard.Features = Features.CanWalk;

          Session.Current.Persist();
          t.Complete();
        }
      }

      using (new Measurement("Fetching..."))
      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Creature snake = persistedKey.Resolve<Creature>();
          Assert.AreEqual(PersistenceState.Persisted, snake.PersistenceState);
          Assert.IsNotNull(snake);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          t.Complete();
        }
      }

      using (new Measurement("Fetching..."))
      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Snake snake = persistedKey.Resolve<Snake>();
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
      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Snake s = new Snake();
          key = s.Key;
          s.Name = "Kaa";
          Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
          Session.Current.Persist();

          Assert.AreEqual("Kaa", s.Name);
          Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);

          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Snake s = key.Resolve<Snake>();
          Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
          Assert.AreEqual("Kaa", s.Name);
          s.Length = 32;
          Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
          Session.Current.Persist();

          Assert.AreEqual(32, s.Length);
          Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Snake s = key.Resolve<Snake>();
          Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
          Assert.AreEqual("Kaa", s.Name);
          Assert.AreEqual(32, s.Length);
          s.Remove();
          Assert.AreEqual(PersistenceState.Removed, s.PersistenceState);
          Session.Current.Persist();

          Assert.AreEqual(PersistenceState.Removed, s.PersistenceState);
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          try {
            key.Resolve<Snake>();
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

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
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

          var pID = new Parameter<Range<IEntire<Tuple>>>();
          var pName = new Parameter<Range<IEntire<Tuple>>>();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();

          RecordSet result = rsSnakePrimary
            .Range(() => pID.Value)
            .Join(rsSnakeName
              .Range(() => pName.Value)
              .OrderBy(OrderBy.Asc(rsSnakeName.IndexOf("ID")))
              .Alias("NameIndex"), rsSnakePrimary.IndexOf("ID"), rsSnakeName.IndexOf("ID"));

          using(new ParameterScope()) {
            pID.Value = new Range<IEntire<Tuple>>(Entire<Tuple>.Create(Tuple.Create(21)), Entire<Tuple>.Create(Tuple.Create(120)));
            pName.Value = new Range<IEntire<Tuple>>(Entire<Tuple>.Create(Tuple.Create("Kaa")), Entire<Tuple>.Create(Tuple.Create("Kaa900")));
            Assert.AreEqual(100, result.Count());
          }

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

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Session session = SessionScope.Current.Session;
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
        return;
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
              .OrderBy(OrderBy.Asc(rsSnakeName.IndexOf("ID")), true)
              .Alias("NameIndex");

            RecordSet range = rsSnakePrimary.Range(from, to);
            RecordSet join = range.Join(rsSnakeName, new Pair<int>(rsSnakePrimary.IndexOf("ID"), rsSnakeName.IndexOf("NameIndex.ID")));
            RecordSet where = join.Where(tuple => tuple.GetValue<int>(rsSnakePrimary.IndexOf("Length")) >= 100);
            RecordSet orderBy = where.OrderBy(OrderBy.Desc(rsSnakePrimary.IndexOf("Name")));
            RecordSet skip = orderBy.Skip(5);
            RecordSet take = skip.Take(50);
            var snakesRse = take.ToEntities<Snake>();

            Assert.AreEqual(15, snakesRse.Count());
          }

          IEnumerable<Snake> snakes = session.All<Snake>();
          Assert.AreEqual(snakesCount, snakes.Count());
          IEnumerable<Creature> creatures = session.All<Creature>();
          Assert.AreEqual(creaturesCount + snakesCount + lizardsCount, creatures.Count());

          Snake snakeKaa53 = session.All<Snake>()
            .Where(snake => snake.Name=="Kaa53")
            .First();
          Assert.AreEqual("Kaa53", snakeKaa53.Name);

          var result = from s in session.All<Snake>()
          where s.Length >= 500
          select s;
          Assert.AreEqual(500, result.Count());

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

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {

          for (int i = 0; i < snakesCount; i++)
            new Snake {Name = ("Kaa" + i), Length = i};
          for (int j = 0; j < creaturesCount; j++)
            new Creature {Name = ("Creature" + j)};
          for (int i = 0; i < lizardsCount; i++)
            new Lizard {Name = ("Lizard" + i), Color = ("Color" + i)};

          Session.Current.Persist();


          var pID = new Parameter<Range<IEntire<Tuple>>>();
          var pName = new Parameter<Range<IEntire<Tuple>>>();
          var pLength = new Parameter<int>();

          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          RecordSet rsSnakeName = snakeType.Indexes.GetIndex("Name").ToRecordSet();

          RecordSet result = rsSnakePrimary
            .Range(() => pID.Value)
            .Join(rsSnakeName
              .Range(() => pName.Value)
              .OrderBy(OrderBy.Asc(rsSnakeName.IndexOf("ID")))
              .Alias("NameIndex"), rsSnakePrimary.IndexOf("ID"), rsSnakeName.IndexOf("ID"))
            .Where(tuple => tuple.GetValue<int>(rsSnakePrimary.IndexOf("Length")) >= pLength.Value)
            .OrderBy(OrderBy.Desc(rsSnakePrimary.IndexOf("Name")))
            .Skip(5)
            .Take(50);
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

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          var session = Session.Current;
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordSet rsPrimary = type.Indexes.PrimaryIndex.ToRecordSet();
          foreach (var entity in rsPrimary.ToEntities<ICreature>().ToList())
            entity.Remove();
          t.Complete(); 
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Session session = SessionScope.Current.Session;
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
          foreach (var entity in rsPrimary.ToEntities<ICreature>())
            Assert.IsNotNull(entity.Name);
          t.Complete();
        }
      }
    }

    [Test]
    public void RemovalTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          for (int i = 0; i < 10; i++) {
            Snake s = new Snake();
            s.Name = "Kaa" + i;
            s.Length = i;
          }
          Session.Current.Persist();
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          var session = Session.Current;
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordSet rs = type.Indexes.PrimaryIndex.ToRecordSet();
          foreach (var entity in rs.ToEntities<ICreature>().ToList())
            entity.Remove();
          Session.Current.Persist();
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          Assert.AreEqual(0, Session.Current.All<ICreature>().Count());
          t.Complete();
        }
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void PerformanceTest()
    {
      const int snakesCount = 1000;
      List<Snake> snakes = new List<Snake>();

      using (new Measurement("Persisting...", snakesCount))
      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake snake = new Snake {Name = ("Name_" + i), Length = (i % 11 + 2)};
            snakes.Add(snake);
          }
          Session.Current.Persist();
          t.Complete();
        }
      }

      using (new Measurement("Fetching...", snakesCount))
      using (Domain.OpenSession()) {
        using (var t = Session.Current.BeginTransaction()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake snake = snakes[i];
            Snake persistedSnake = snake.Key.Resolve<Snake>();
            Assert.IsNotNull(persistedSnake);
            Assert.AreEqual(snake.Name, persistedSnake.Name);
            Assert.AreEqual(snake.Length, persistedSnake.Length);
          }
          Session.Current.Persist();
          t.Complete();
        }
      }
    }
  }
}
