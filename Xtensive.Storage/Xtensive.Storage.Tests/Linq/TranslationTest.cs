// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class TranslationTest : AutoBuildTest
  {
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
    public void SetUp()
    {
      const int snakesCount = 100;
      const int creaturesCount = 100;
      const int lizardsCount = 100;
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
    public void Test()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var snakes = Session.Current.All<Snake>();
          foreach (var snake in snakes) {
            
          }
        }
      }
    }
    
  }
}