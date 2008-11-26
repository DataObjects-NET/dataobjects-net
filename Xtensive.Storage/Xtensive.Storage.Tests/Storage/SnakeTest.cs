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
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.Storage.SnakesModel;
using Xtensive.Storage;


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
  [HierarchyRoot(typeof(KeyGenerator), "ID", KeyGeneratorCacheSize = 16)]
  public class Creature : Entity,
    ICreature
  {
    [Field]
    public int ID { get; private set; }
        
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

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
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

      return result;
    }

    [Test]
    public void GroupTest()
    {
      TestFixtureTearDown();
      TestFixtureSetUp();
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          new Snake {Name = "Kaa1", Length = 1};
          new Snake { Name = "Kaa2", Length = 2 };
          new Snake { Name = "Kaa1", Length = 3 };
          new Snake { Name = "Kaa2", Length = 2 };
          new Snake { Name = "Kaa1", Length = 3 };
          new Snake { Name = "Kaa3", Length = 3 };

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

          Assert.AreEqual(rs.Count(),4);
          t.Complete();
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

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
      for (int i = 0; i < snakesCount; i++)
            new Snake { Name = ("Kaa" + i), Length = i };
          for (int j = 0; j < creaturesCount; j++)
            new Creature { Name = ("Creature" + j) };
          for (int i = 0; i < lizardsCount; i++)
            new Lizard { Name = ("Lizard" + i), Color = ("Color" + i) };

          Session.Current.Persist();

          TypeInfo snakeType = Domain.Model.Types[typeof(Snake)];
          
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();

          var rsCalculated = rsSnakePrimary.Calculate(new CalculatedColumnDescriptor("FullName", typeof(string), (s) => (s.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(0, 2))),
          new CalculatedColumnDescriptor("FullName2", typeof(string), (s) => (s.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(0, 3))))
            .Take(10);

          Assert.AreEqual(10, rsCalculated.Count());

          foreach (var tuple in rsCalculated) {
            Assert.AreEqual("Ka", tuple.GetValue(5));
            Assert.AreEqual("Kaa", tuple.GetValue(6));
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

          using (EnumerationScope.Open()) {
            Assert.AreEqual(name, saved.Provider.GetService<IHasNamedResult>().Name);
            Assert.AreEqual(scope, saved.Provider.GetService<IHasNamedResult>().Scope);
          }

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
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
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
          Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          snake.Length = 32;
          Assert.AreEqual(PersistenceState.New, snake.PersistenceState);
          Assert.AreEqual(32, snake.Length);
            
          Key key = Key.Create<Snake>(Tuple.Create(snake.ID));
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
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Creature snake = persistedKey.Resolve<Creature>();
          Assert.AreEqual(PersistenceState.Synchronized, snake.PersistenceState);
          Assert.IsNotNull(snake);
          Assert.AreEqual("Kaa", snake.Name);
          Assert.AreEqual(Features.CanCrawl, snake.Features);
          t.Complete();
        }
      }

      using (new Measurement("Fetching..."))
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
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

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Snake s = key.Resolve<Snake>();
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

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Snake s = key.Resolve<Snake>();
          s.Name = "Snake";
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Snake s = key.Resolve<Snake>();
          s.Length = 16;
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Snake s = key.Resolve<Snake>();
          s.Name = "Kaa";
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Snake s = key.Resolve<Snake>();
          s.Length = 32;
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Snake s = key.Resolve<Snake>();
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

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
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
    [Ignore]
    public void StringComparisonTest()
    {
      //var stringList = new string[] {"k", "K", "kK","kk!", "Kk", "KKy", "kkx", "KKK"+'\u00FF', "KKKK", "kkjkllkj", string.Concat('\u0920','\u00FF'), string.Concat('\u0920','\u0920')};
      var stringList = new string[] { "KAA10" + '\u00FF', "Kaa1000" };
      var signList = new List<string>();
      
      for(int i = 0; i<stringList.Length-1; i++) {
        if (stringList[i].CompareTo(stringList[i + 1]) < 0) {
          signList.Add("<");
          continue;
        }
        if (stringList[i].CompareTo(stringList[i + 1]) > 0) {
          signList.Add(">");
          continue;
        }
        signList.Add("=");
      }

      var result = new StringBuilder(stringList[0] + " ");
      for (int i = 0; i < signList.Count; i++)
        result.Append(signList[i]).Append(" ").Append(stringList[i + 1]).Append(" ");
      
      Log.Info(result.ToString());
      Log.Info('K'.CompareTo(char.MinValue).ToString());

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
          
          using(new ParameterScope()) {
            pID.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(21)), new Entire<Tuple>(Tuple.Create(120)));
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
    public void LikeTest()
    {
      TestFixtureTearDown();
      TestFixtureSetUp();
      using (Domain.OpenSession()) {
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

      using(Domain.OpenSession()) {
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

      using (Domain.OpenSession()) {
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
          using (new ParameterScope()) {
            pLen.Value = 10;
            Assert.Greater(result.Count(), 0);
          }

          result = rsSnakePrimary.Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Substring(3, 1) == "9");
          Assert.Greater(result.Count(), 0);

          name = "Kaa90";
          result = rsSnakePrimary
            .Filter(tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).StartsWith(name))
            .Aggregate(new[] { 0 }, new AggregateColumnDescriptor("Count", 1, AggregateType.Count))
            .Select(0,1)
            .Filter(tuple => tuple.GetValue<long>(1) > 0);
          Assert.Greater(result.Count(), 0);
          Assert.IsTrue(result.ToEntities<Snake>()
            .Where(s => s != null && s.Name.StartsWith(name)).Count() > 1);

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
        using (var t = Transaction.Open()) {
          var session = Session.Current;
          TypeInfo type = session.Domain.Model.Types[typeof (ICreature)];
          RecordSet rsPrimary = type.Indexes.PrimaryIndex.ToRecordSet();
          foreach (var entity in rsPrimary.ToEntities<ICreature>().ToList())
            entity.Remove();
          t.Complete(); 
        }
      }

      using (Domain.OpenSession()) {
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

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
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
        using (var t = Transaction.Open()) {
          Assert.AreEqual(0, Session.Current.All<ICreature>().Count());
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

            RecordSet range = rsSnakePrimary.Range(from, to);
            RecordSet join = range.Join(rsSnakeName, new Pair<int>(rsSnakePrimary.Header.IndexOf(cID), rsSnakeName.Header.IndexOf("NameIndex."+cID)));
            RecordSet where = join.Filter(tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) >= 100);
            RecordSet orderBy = where.OrderBy(OrderBy.Desc(rsSnakePrimary.Header.IndexOf(cName)));
            RecordSet skip = orderBy.Skip(5);
            RecordSet take = skip.Take(50);
            var snakesRse = take.ToEntities<Snake>();
            t.Complete();
            foreach (Snake snake in snakesRse) {
              Console.WriteLine(snake.Key);
            }
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
    public void ExecutionSiteOptionsTest()
    {
      const int snakesCount = 1000;
      const int creaturesCount = 1000;
      const int lizardsCount = 1000;

      TestFixtureTearDown();
      TestFixtureSetUp();

      using (Domain.OpenSession()) {
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
              .ExecuteAt(ExecutionOptions.Client)
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

      using (Domain.OpenSession()) {
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

          using (new ParameterScope()) {
            pID.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(21)), new Entire<Tuple>(Tuple.Create(120)));
            pName.Value = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create("Kaa")), new Entire<Tuple>(Tuple.Create("Kaa900")));
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
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            var snake = new Snake {Name = ("Name_" + i), Length = (i % 11 + 2)};
            snakesDTO.Add(Tuple.Create(snake.Key, snake.Name, snake.Length));
          }
          t.Complete();
        }
      }

      using (new Measurement("Fetching...", snakesCount))
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++) {
            Snake persistedSnake = snakesDTO[i].GetValue<Key>(0).Resolve<Snake>();
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
      using (Domain.OpenSession()) {
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
      using (Domain.OpenSession()) {
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
  }
}
