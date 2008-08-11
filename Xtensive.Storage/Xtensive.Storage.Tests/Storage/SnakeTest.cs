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
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
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
    string Name { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'")]
  [Index("Name")]
  [HierarchyRoot(typeof (DefaultGenerator), "ID")]
  public class Creature : Entity,
    ICreature
  {
    [Field]
    public int ID { get; set; }

    [Field(Length = 255,IsNullable = true)]
    public string Name { get; set; }

    [Field(IsNullable = true)]
    public Features Features { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Length = {Length}")]
  public class Snake : Creature
  {
    [Field(IsNullable = true)]
    public int Length { get; set; }
  }

  [DebuggerDisplay("Name = '{Name}'; Color = {Color}")]
  public class Lizard : Creature
  {
    [Field(Length = 7)]
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
      Domain.Model.Dump();
      Key persistedKey = null; 
      using (Domain.OpenSession())
      {
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
      using (Domain.OpenSession())
      {
        Creature snake = persistedKey.Resolve<Creature>();
        Assert.AreEqual(PersistenceState.Persisted, snake.PersistenceState);
        Assert.IsNotNull(snake);
        Assert.AreEqual("Kaa", snake.Name);
        Assert.AreEqual(Features.CanCrawl, snake.Features);
      }

      using (new Measurement("Fetching..."))
      using (Domain.OpenSession())
      {
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
      using (Domain.OpenSession())
      {
        Snake s = new Snake();
        key = s.Key;
        s.Name = "Kaa";
        Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
        SessionScope.Current.Session.Persist();
        Assert.AreEqual("Kaa", s.Name);
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
      }

      using (Domain.OpenSession())
      {
        Snake s = key.Resolve<Snake>();
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
        Assert.AreEqual("Kaa", s.Name);
        s.Length = 32;
        Assert.AreEqual(PersistenceState.Modified, s.PersistenceState);
        SessionScope.Current.Session.Persist();
        Assert.AreEqual(32, s.Length);
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
      }

      using (Domain.OpenSession())
      {
        Snake s = key.Resolve<Snake>();
        Assert.AreEqual(PersistenceState.Persisted, s.PersistenceState);
        Assert.AreEqual("Kaa", s.Name);
        Assert.AreEqual(32, s.Length);
        s.Remove();
        Assert.AreEqual(PersistenceState.Removed, s.PersistenceState);
        SessionScope.Current.Session.Persist();
        Assert.AreEqual(PersistenceState.Removed, s.PersistenceState);
      }

      using (Domain.OpenSession())
      {
        try
        {
          key.Resolve<Snake>();
        }
        catch (InvalidOperationException)
        {
        }
      }
    }

    [Test]
    public void QueryTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      using (Domain.OpenSession())
      {
        Session session = SessionScope.Current.Session;
        for (int i = 0; i < snakesCount; i++)
        {
          Snake s = new Snake();
          s.Name = "Kaa" + i;
          s.Length = i;
        }
        for (int j = 0; j < creaturesCount; j++)
        {
          Creature c = new Creature();
          c.Name = "Creature" + j;
        }
        for (int i = 0; i < lizardsCount; i++)
        {
          Lizard l = new Lizard();
          l.Name = "Lizard" + i;
          l.Color = "Color" + i;
        }

        Tuple from = Tuple.Create(21);
        Tuple to = Tuple.Create(120);
        Tuple fromName = Tuple.Create("Kaa");
        Tuple toName = Tuple.Create("Kaa900");
        TypeInfo snakeType = session.Domain.Model.Types[typeof(Snake)];
        RecordSet rsSnakePrimary = session.Select(snakeType.Indexes.GetIndex("ID"));
        RecordSet a = rsSnakePrimary.Alias("a");
        RecordSet js = rsSnakePrimary.Join(a, new Pair<int>(rsSnakePrimary.IndexOf("ID"), a.IndexOf("a.ID")));
        js.Parse();

        using (new Measurement("Query performance"))
        {
          RecordSet rsSnakeName = session.Select(snakeType.Indexes.GetIndex("Name"));
          rsSnakeName = rsSnakeName
            .Range(fromName, toName)
            .IndexBy(OrderBy.Asc(rsSnakeName.IndexOf("ID")))
            .Alias("NameIndex");

          RecordSet range = rsSnakePrimary.Range(from, to);
          RecordSet join = range.Join(rsSnakeName, new Pair<int>(rsSnakePrimary.IndexOf("ID"), rsSnakeName.IndexOf("NameIndex.ID")));
          RecordSet where = join.Where(tuple => tuple.GetValue<int>(rsSnakePrimary.IndexOf("Length")) >= 100);
          RecordSet orderBy = where.OrderBy(OrderBy.Desc(rsSnakePrimary.IndexOf("Name")));
          RecordSet skip = orderBy.Skip(5);
          RecordSet take = skip.Take(50);
          var snakesRse = take.AsEntities<Snake>();

          /*// debug
          long rsSnakePrimaryCount = rsSnakePrimary.Provider.GetService<ICountable>(true).Count;
          long joinCount = join.Provider.Count();
          long whereCount = where.Provider.Count();
          long orderByCount = orderBy.Provider.Count();*/

          Assert.AreEqual(15, snakesRse.Count());
        }


        //        RecordSet r = session.Indexes[...];
        //        r = r.Range(...).Where(...);
        //        var snakes = r.ToEntities<Snake>("Id");

        IEnumerable<Snake> snakes = session.All<Snake>();
        Assert.AreEqual(snakesCount, snakes.Count());
        IEnumerable<Creature> creatures = session.All<Creature>();
        Assert.AreEqual(creaturesCount + snakesCount + lizardsCount, creatures.Count());

        Snake snakeKaa53 = session.All<Snake>()
          .Where(snake => snake.Name == "Kaa53")
          .First();
        Assert.AreEqual("Kaa53", snakeKaa53.Name);

        var result = from s in session.All<Snake>()
                     where s.Length >= 500
                     select s;
        Assert.AreEqual(500, result.Count());
      }
    }

    [Test]
    public void InterfaceTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      using (Domain.OpenSession()) {
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
        TypeInfo type = session.Domain.Model.Types[typeof(ICreature)];
        RecordSet rsPrimary = session.Select(type.Indexes.PrimaryIndex);
        foreach (var entity in rsPrimary.AsEntities<ICreature>())
          Assert.IsNotNull(entity.Name);
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void PerformanceTest()
    {
      const int snakesCount = 10000;
      List<Snake> snakes = new List<Snake>();

      using (new Measurement("Persisting...", snakesCount))
      using (Domain.OpenSession())
      {
        for (int i = 0; i < snakesCount; i++)
        {
          Snake snake = new Snake { Name = ("Name_" + i), Length = (i % 11 + 2) };
          snakes.Add(snake);
        }
      }

      using (new Measurement("Fetching...", snakesCount))
      using (Domain.OpenSession())
      {
        for (int i = 0; i < snakesCount; i++)
        {
          Snake snake = snakes[i];
          Snake persistedSnake = snake.Key.Resolve<Snake>();
          Assert.IsNotNull(persistedSnake);
          Assert.AreEqual(snake.Name, persistedSnake.Name);
          Assert.AreEqual(snake.Length, persistedSnake.Length);
        }
      }
    }

    [Test]
    [Explicit]
    public void HierarchyInfoTest()
    {
      foreach (HierarchyInfo hierarchyInfo in Domain.Model.Hierarchies) {
        Assert.AreEqual(hierarchyInfo.Columns.Count, hierarchyInfo.Root.Indexes.PrimaryIndex.KeyColumns.Count);
        for (int i = 0; i < hierarchyInfo.Columns.Count; i++)
          Assert.AreEqual((object)hierarchyInfo.Columns[i], hierarchyInfo.Root.Indexes.PrimaryIndex.KeyColumns[i].Key);
      }
    }
  }
}