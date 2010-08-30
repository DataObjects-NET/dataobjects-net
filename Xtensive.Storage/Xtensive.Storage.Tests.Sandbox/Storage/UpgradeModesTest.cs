// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Tests.Storage.UpgradeModesTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }
  }

  [Serializable]
  public class Author : Person
  {
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
      // Reverting...
      BuildStandardDomain(DomainUpgradeMode.Perform);

      BuildStandardDomain(DomainUpgradeMode.Validate);

      AssertEx.Throws<SchemaSynchronizationException>(() => {
        BuildMinimalDomain(DomainUpgradeMode.Validate);
      });

      AssertEx.Throws<SchemaSynchronizationException>(() => {
        BuildFullDomain(DomainUpgradeMode.Validate);
      });
    }

    [Test]
    public void SkipModeTest()
    {
      // Reverting...
      BuildStandardDomain(DomainUpgradeMode.Perform);

      BuildStandardDomain(DomainUpgradeMode.Skip);

      BuildMinimalDomain(DomainUpgradeMode.Skip);

      AssertEx.Throws<Exception>(() => {
        BuildFullDomain(DomainUpgradeMode.Skip);
      });
    }

    [Test]
    public void PerformModeTest()
    {
      BuildStandardDomain(DomainUpgradeMode.Perform);
    }

    [Test]
    public void RecreateModeTest()
    {
      BuildStandardDomain(DomainUpgradeMode.Recreate);
    }

    private void BuildFullDomain(DomainUpgradeMode upgradeMode)
    {
      registerBook  = true;
      registerPerson = true;
      registerAuthor = true;

      BuildDomain("Full", upgradeMode, 3);
    }

    private void BuildStandardDomain(DomainUpgradeMode upgradeMode)
    {
      registerBook  = true;
      registerPerson = true;
      registerAuthor = false;

      BuildDomain("Standard", upgradeMode, 2);
    }

    private void BuildMinimalDomain(DomainUpgradeMode upgradeMode)
    {
      registerBook  = true;
      registerPerson = false;
      registerAuthor = false;

      BuildDomain("Minimal", upgradeMode, 1);
    }

    private void BuildDomain(string type, DomainUpgradeMode upgradeMode, int typeCount)
    {
      Measurement m;
      var cfg = BuildConfiguration();
      cfg.UpgradeMode = upgradeMode;
      Console.WriteLine();
      Console.WriteLine("Building {0} Domain in {1} mode.", type, upgradeMode);
      try {
        using (m = new Measurement("metrics"))
          CheckTypeCount(typeCount, Domain.Build(cfg));
        Console.WriteLine("  Done, {0}", m);
      }
      catch (Exception e) {
        Console.WriteLine("  Failed with {0}.", e.GetType().GetShortName());
        throw;
      }
    }

    private void CheckTypeCount(int expectedTypeCount, Domain domain)
    {
      Console.WriteLine("  Types:");
      var types = domain.Model.Types.Where(type => type.IsEntity).ToList();
      foreach (var type in types)
        Console.WriteLine("    Id = {0,3}: {1}", type.TypeId, type.Name);
      Assert.AreEqual(expectedTypeCount + 3, types.Count);
    }
  }
}