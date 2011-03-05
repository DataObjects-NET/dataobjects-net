// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Storage.Rse;

namespace Xtensive.Orm.Tests.Storage.UpgradeModesTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }
  }

  [Serializable]
  public class Author : Person
  {
    // Just to validate how model builder handles [Field(Nullable = ...)]
    [Field(Nullable = true)]
    public int? NullableInt { get; set; }

    // Just to validate how model builder handles [Field(Nullable = ...)]
    [Field(Nullable = false)]
    public int NonNullableInt { get; set; }
  }

  [TestFixture]
  public class UpgradeModesTest : AutoBuildTest
  {
    public bool registerBook = true;
    public bool registerPerson = true;
    public bool registerAuthor = false;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      if (registerBook)
        configuration.Types.Register(typeof(Book));
      if (registerPerson)
        configuration.Types.Register(typeof(Person));
      if (registerAuthor)
        configuration.Types.Register(typeof(Author));
      return configuration;
    }

    [Test]
    public void ValidateModeTest()
    {
      BuildFullDomainWithNonDefaultTypeIds();
      var types = BuildBookPersonDomain(DomainUpgradeMode.Perform);
      int bookTypeId = types[typeof (Book)];
      int personTypeId = types[typeof (Person)];

      types = BuildBookPersonDomain(DomainUpgradeMode.Validate);
      Assert.AreEqual(personTypeId, types[typeof (Person)]);
      Assert.AreEqual(bookTypeId, types[typeof (Book)]);

      AssertEx.Throws<SchemaSynchronizationException>(() => {
        BuildPersonDomain(DomainUpgradeMode.Validate);
      });

      AssertEx.Throws<SchemaSynchronizationException>(() => {
        BuildBookDomain(DomainUpgradeMode.Validate);
      });

      AssertEx.Throws<SchemaSynchronizationException>(() => {
        BuildFullDomain(DomainUpgradeMode.Validate);
      });

      types = BuildBookPersonDomain(DomainUpgradeMode.Validate);
      Assert.AreEqual(personTypeId, types[typeof (Person)]);
      Assert.AreEqual(bookTypeId, types[typeof (Book)]);
    }

    [Test]
    public void SkipModeTest()
    {
      BuildFullDomainWithNonDefaultTypeIds();
      var types = BuildBookPersonDomain(DomainUpgradeMode.Perform);
      int bookTypeId = types[typeof (Book)];
      int personTypeId = types[typeof (Person)];

      types = BuildBookPersonDomain(DomainUpgradeMode.Skip);
      Assert.AreEqual(personTypeId, types[typeof (Person)]);
      Assert.AreEqual(bookTypeId, types[typeof (Book)]);

      types = BuildPersonDomain(DomainUpgradeMode.Skip);
      Assert.AreEqual(personTypeId, types[typeof (Person)]);

      types = BuildBookDomain(DomainUpgradeMode.Skip);
      Assert.AreEqual(bookTypeId, types[typeof (Book)]);

      types = BuildBookPersonDomain(DomainUpgradeMode.Skip);
      Assert.AreEqual(personTypeId, types[typeof (Person)]);
      Assert.AreEqual(bookTypeId, types[typeof (Book)]);

      AssertEx.Throws<Exception>(() => {
        BuildFullDomain(DomainUpgradeMode.Skip);
      });
    }

    [Test]
    public void PerformModeTest()
    {
      var types = BuildFullDomain(DomainUpgradeMode.Recreate);
      int personTypeId = types[typeof (Person)];

      types = BuildFullDomain(DomainUpgradeMode.Perform);
      Assert.AreEqual(personTypeId, types[typeof (Person)]);
      AssertEx.AreEqual(new [] {101,102,103}, types.Values.OrderBy(id => id));

      types = BuildPersonDomain(DomainUpgradeMode.Perform);
      Assert.AreEqual(personTypeId, types[typeof (Person)]);
      AssertEx.AreEqual(new [] {personTypeId}, types.Values.OrderBy(id => id));
      int maxTypeId = types.Values.Max();

      types = BuildFullDomain(DomainUpgradeMode.Perform);
      int bookTypeId = types[typeof (Book)];
      Assert.AreEqual(personTypeId, types[typeof (Person)]);
      Assert.Less(maxTypeId, types[typeof (Book)]);
      Assert.Less(maxTypeId, types[typeof (Author)]);

      types = BuildBookDomain(DomainUpgradeMode.Perform);
      Assert.AreEqual(bookTypeId, types[typeof (Book)]);
      AssertEx.AreEqual(new [] {bookTypeId}, types.Values.OrderBy(id => id));
      maxTypeId = types.Values.Max();

      types = BuildFullDomain(DomainUpgradeMode.Perform);
      Assert.AreEqual(bookTypeId, types[typeof (Book)]);
      Assert.AreNotEqual(personTypeId, types[typeof (Person)]);
      Assert.Less(maxTypeId, types[typeof (Person)]);
      Assert.Less(maxTypeId, types[typeof (Author)]);
    }

    [Test]
    public void RecreateModeTest()
    {
      var types = BuildPersonDomain(DomainUpgradeMode.Recreate);
      AssertEx.AreEqual(new [] {101}, types.Values.OrderBy(id => id));
      types = BuildFullDomain(DomainUpgradeMode.Recreate);
      AssertEx.AreEqual(new [] {101,102,103}, types.Values.OrderBy(id => id));
      types = BuildBookPersonDomain(DomainUpgradeMode.Recreate);
      AssertEx.AreEqual(new [] {101,102}, types.Values.OrderBy(id => id));
      types = BuildBookDomain(DomainUpgradeMode.Recreate);
      AssertEx.AreEqual(new [] {101}, types.Values.OrderBy(id => id));
    }

    private Dictionary<Type, int> BuildFullDomainWithNonDefaultTypeIds()
    {
      BuildFullDomain(DomainUpgradeMode.Recreate);
      BuildPersonDomain(DomainUpgradeMode.Perform);
      return BuildFullDomain(DomainUpgradeMode.Perform);
    }

    private Dictionary<Type, int> BuildFullDomain(DomainUpgradeMode upgradeMode)
    {
      registerBook  = true;
      registerPerson = true;
      registerAuthor = true;

      return BuildDomain("Full", upgradeMode, 3);
    }

    private Dictionary<Type, int> BuildBookPersonDomain(DomainUpgradeMode upgradeMode)
    {
      registerBook  = true;
      registerPerson = true;
      registerAuthor = false;

      return BuildDomain("Book+Person", upgradeMode, 2);
    }

    private Dictionary<Type, int> BuildPersonDomain(DomainUpgradeMode upgradeMode)
    {
      registerBook  = false;
      registerPerson = true;
      registerAuthor = false;

      return BuildDomain("Person", upgradeMode, 1);
    }

    private Dictionary<Type, int> BuildBookDomain(DomainUpgradeMode upgradeMode)
    {
      registerBook  = true;
      registerPerson = false;
      registerAuthor = false;

      return BuildDomain("Book", upgradeMode, 1);
    }

    private Dictionary<Type, int> BuildDomain(string type, DomainUpgradeMode upgradeMode, int typeCount)
    {
      Measurement m;
      var cfg = BuildConfiguration();
      cfg.UpgradeMode = upgradeMode;
      Console.WriteLine();
      Console.WriteLine("Building {0} Domain in {1} mode.", type, upgradeMode);
      try {
        Dictionary<Type, int> result = new Dictionary<Type, int>();
        using (m = new Measurement("metrics"))
          result = CheckTypeCount(typeCount, Domain.Build(cfg));
        Console.WriteLine("  Done, {0}", m);
        return result;
      }
      catch (Exception e) {
        Console.WriteLine("  Failed with {0}.", e.GetType().GetShortName());
        throw;
      }
    }

    private Dictionary<Type, int> CheckTypeCount(int expectedTypeCount, Domain domain)
    {
      Console.WriteLine("  Types:");
      var types = (
        from type in domain.Model.Types
        where type.IsEntity && !type.IsSystem
        orderby type.TypeId
        select type
        ).ToList();
      foreach (var type in types)
        Console.WriteLine("    Id = {0,3}: {1}", type.TypeId, type.Name);
      Assert.AreEqual(expectedTypeCount, types.Count);
      return types.ToDictionary(type => type.UnderlyingType, type => type.TypeId);
    }
  }
}