// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.SnakesModel;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class TranslationTest : AutoBuildTest
  {
    const int snakesCount = 100;
    const int creaturesCount = 100;
    const int lizardsCount = 100;

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Xtensive.Storage.Domain result = base.BuildDomain(configuration);
      return result;
    }

    [TestFixtureSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < snakesCount; i++)
            new Snake {Name = ("Kaa" + i), Length = i};
          for (int j = 0; j < creaturesCount; j++)
            new Creature {Name = ("Creature" + j)};
          for (int i = 0; i < lizardsCount; i++)
            new Lizard {Name = ("Lizard" + i), Color = ("Color" + i)};
          t.Complete();
        }
      }
    }

    [Test]
    public void WhereTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var snakes = Session.Current.All<Snake>();
          foreach (var snake in snakes.Where(s => s.ID == 20)) {
            Console.Out.WriteLine(snake.Name);
          }
          foreach (var snake in snakes.Where(s => s.Name == "Kaa20")) {
            Console.Out.WriteLine(snake.Name);
          }
        }
      }
    }

    [Test]
    public void NativeLanguageTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var snakes = Session.Current.All<Snake>();
          var result = from s in snakes
                       where s.Name=="Kaa20"
                       select s;
          foreach (var snake in result) {
            Console.Out.WriteLine(snake.Name);
          }
        }
      }
    }

    [Test]
    public void SelectTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var snakes = Session.Current.All<Snake>();
          var result = from s in snakes
                       where s.Name=="Kaa20"
                       select new {s.ID, s.Name};
          foreach (var snake in result) {
            Console.Out.WriteLine(string.Format("ID:{0}; Name:{1}.", snake.ID, snake.Name));
          }
        }
      }
    }

    [Test]
    public void AggregateTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var snakes = Session.Current.All<Snake>();
          int result = snakes.Count();
          Assert.AreEqual(snakesCount, result);
          result = snakes.Count(snake => snake.Length == 10);
          Assert.AreEqual(1, result);
          var maxLen = snakes.Max(snake => snake.Length);
          Assert.AreEqual(snakesCount - 1, maxLen.Value);
        }
      }
    }
    
  }
}