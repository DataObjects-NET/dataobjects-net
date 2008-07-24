// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Generators;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.SnakeModel;
using FieldAttributes=Xtensive.Storage.Model.FieldAttributes;

namespace Xtensive.Storage.Tests.SnakeModel
{
  public enum Features
  {
    None = 0,
    CanCrawl = 1,
    CanJump = 2,
    CanFly = 3,
    CanWalk = 4,
  }

  [DebuggerDisplay("Name = '{Name}'")]
  [Index("Name")]
  [HierarchyRoot(typeof (IncrementalGenerator), "ID")]
  public class Creature : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Features Features { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Length = {Length}")]
  public class Snake : Creature
  {
    [Field]
    public int Length { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Color = {Color}")]
  public class Lizard : Creature
  {
    [Field]
    public string Color { get; set; }
  }
}

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public class SnakeTests
  {
    private Domain domain;
    private DomainConfiguration config;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      config = new DomainConfiguration("memory://localhost/Snakes");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.SnakeModel");
      domain = Domain.Build(config);
      domain.Model.Dump();
    }

    [Test]
    public void MainTest()
    {
      Key persistedKey = null;
      using (domain.OpenSession()) {
        Snake snake = new Snake();
        Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
        persistedKey = snake.Key;

        Assert.IsNotNull(snake.Key);
        Assert.AreEqual(snake, snake.Key.Resolve());

        Assert.AreEqual(0, snake.Length);
        Assert.AreEqual(null, snake.Name);
        Assert.AreEqual(Features.None, snake.Features);

        snake.Name = "Kaa";
        snake.Features = Features.CanCrawl;
        Assert.AreEqual(PersistenceState.Modified, snake.PersistenceState);
        Assert.AreEqual("Kaa", snake.Name);
        Assert.AreEqual(Features.CanCrawl, snake.Features);
        snake.Length = 32;
        Assert.AreEqual(PersistenceState.Modified, snake.PersistenceState);
        Assert.AreEqual(32, snake.Length);

        Key key = Key.Get<Snake>(Tuple.Create(1));
        Assert.IsTrue(snake.Key.Equals(key));
        Assert.IsTrue(ReferenceEquals(snake.Key, key));

        Lizard lizard = new Lizard();
        lizard.Color = "#ff5544";
        lizard.Features = Features.CanWalk;
      }

      using (new Measurement("Fetching..."))
      using (domain.OpenSession()) {
        Creature snake = persistedKey.Resolve<Creature>();
        Assert.AreEqual(PersistenceState.Persisted, snake.PersistenceState);
        Assert.IsNotNull(snake);
        Assert.AreEqual("Kaa", snake.Name);
        Assert.AreEqual(Features.CanCrawl, snake.Features);
      }

      using (new Measurement("Fetching..."))
      using (domain.OpenSession()) {
        Snake snake = persistedKey.Resolve<Snake>();
        Assert.IsNotNull(snake);
        Assert.AreEqual("Kaa", snake.Name);
        Assert.AreEqual(32, snake.Length);
        Assert.AreEqual(Features.CanCrawl, snake.Features);
      }
    }

    [Test]
    public void UpdateTest()
    {
      Key key;
      using (domain.OpenSession()) {
        Snake s = new Snake();
        key = s.Key;
        s.Name = "Kaa";
        Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
        SessionScope.Current.Session.Persist();
        Assert.AreEqual("Kaa", s.Name);
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
      }

      using (domain.OpenSession()) {
        Snake s = key.Resolve<Snake>();
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
        Assert.AreEqual("Kaa", s.Name);
        s.Length = 32;
        Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
        SessionScope.Current.Session.Persist();
        Assert.AreEqual(32, s.Length);
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
      }

      using (domain.OpenSession()) {
        Snake s = key.Resolve<Snake>();
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
        Assert.AreEqual("Kaa", s.Name);
        Assert.AreEqual(32, s.Length);
        s.Remove();
        Assert.AreEqual(PersistenceState.Removed, s.PersistenceState);
        SessionScope.Current.Session.Persist();
        Assert.AreEqual(PersistenceState.Removed, s.PersistenceState);
      }

      using (domain.OpenSession()) {
        try {
          key.Resolve<Snake>();
        }
        catch (InvalidOperationException) {
        }
      }
    }

    [Test]
    public void QueryTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      domain = Domain.Build(config);

      using (domain.OpenSession()) {
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

        Tuple from = Tuple.Create(21);
        Tuple to = Tuple.Create(120);
        Tuple fromName = Tuple.Create("Kaa");
        Tuple toName = Tuple.Create("Kaa900");
        TypeInfo snakeType = session.Domain.Model.Types[typeof (Snake)];
        RecordSet rsSnakePrimary = session.Select(snakeType.Indexes.GetIndex("ID"));

        using (new Measurement("Query performance")) {
          RecordSet rsSnakeName = session.Select(snakeType.Indexes.GetIndex("Name"));
          rsSnakeName = rsSnakeName
            .Range(fromName, toName)
            .IndexBy(OrderBy.Asc(rsSnakeName.Map("ID")))
            .Alias("NameIndex");

          RecordSet range = rsSnakePrimary.Range(from, to);
          RecordSet join = range.Join(rsSnakeName, new Pair<int>(rsSnakePrimary.Map("ID"), rsSnakeName.Map("NameIndex.ID")));
          RecordSet where = join.Where(tuple => tuple.GetValue<int>(rsSnakePrimary.Map("Length")) >= 100);
          RecordSet orderBy = where.OrderBy(OrderBy.Desc(rsSnakePrimary.Map("Name")));
          var snakesRse = orderBy.AsEntities<Snake>();

          /*// debug
          long rsSnakePrimaryCount = rsSnakePrimary.Provider.GetService<ICountable>(true).Count;
          long joinCount = join.Provider.Count();
          long whereCount = where.Provider.Count();
          long orderByCount = orderBy.Provider.Count();*/

          Assert.AreEqual(20, snakesRse.Count());
        }


        //        RecordSet r = session.Indexes[...];
        //        r = r.Range(...).Where(...);
        //        var snakes = r.ToEntities<Snake>("Id");

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
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void PerformanceTest()
    {
      const int snakesCount = 10000;

      DomainConfiguration config = new DomainConfiguration("memory://localhost/Snakes");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.SnakeModel");
      Domain domain = Domain.Build(config);

      List<Snake> snakes = new List<Snake>();

      using (new Measurement("Persisting...", snakesCount))
      using (domain.OpenSession()) {
        for (int i = 0; i < snakesCount; i++) {
          Snake snake = new Snake {Name = ("Name_" + i), Length = (i % 11 + 2)};
          snakes.Add(snake);
        }
      }

      using (new Measurement("Fetching...", snakesCount))
      using (domain.OpenSession()) {
        for (int i = 0; i < snakesCount; i++) {
          Snake snake = snakes[i];
          Snake persistedSnake = snake.Key.Resolve<Snake>();
          Assert.IsNotNull(persistedSnake);
          Assert.AreEqual(snake.Name, persistedSnake.Name);
          Assert.AreEqual(snake.Length, persistedSnake.Length);
        }
      }
    }

    [Test]
    public void HierarchyInfoTest()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/Snakes");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.SnakeModel");
      Domain domain = Domain.Build(config);
      foreach (HierarchyInfo hierarchyInfo in domain.Model.Hierarchies) {
        Assert.AreEqual(hierarchyInfo.Columns.Count, hierarchyInfo.Root.Indexes.PrimaryIndex.KeyColumns.Count);
        for (int i = 0; i < hierarchyInfo.Columns.Count; i++)
          Assert.AreEqual((object) hierarchyInfo.Columns[i], hierarchyInfo.Root.Indexes.PrimaryIndex.KeyColumns[i].Key);
      }
    }
  }
}
