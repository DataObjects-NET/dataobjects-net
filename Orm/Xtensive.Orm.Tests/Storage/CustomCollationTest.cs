// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.05

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.CustomCollationTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace CustomCollationTestModel
  {
    [HierarchyRoot]
    public class Collated : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field(Indexed = true)]
      public string Name { get; set; }
    }
  }

  public class CustomCollationTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sqlite, "SQLite specific collation used");
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Collation = "StringComparer_InvariantCulture_IgnoreCase";
      configuration.Types.Register(typeof (Collated));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) 
      using (session.OpenTransaction()) {
        CreateEntities();

        var expected = new[] {
          "bye", "bye", "hello", "hello",
          "пока", "пока", "привет", "привет"
        };

        var actual = session.Query.All<Collated>()
          .OrderBy(c => c.Name)
          .AsEnumerable()
          .Select(c => c.Name.ToLowerInvariant())
          .ToArray();

        Assert.That(actual.SequenceEqual(expected));
      }
    }

    private static void CreateEntities()
    {
      new Collated {Name = "hello"};
      new Collated {Name = "привет"};
      new Collated {Name = "bye"};
      new Collated {Name = "пока"};

      new Collated {Name = "Hello"};
      new Collated {Name = "Привет"};
      new Collated {Name = "Bye"};
      new Collated {Name = "Пока"};
    }
  }
}