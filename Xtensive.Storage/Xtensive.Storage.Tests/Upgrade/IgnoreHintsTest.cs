// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.19

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Tests.Upgrade.IgnoreHints.Model;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Building;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class IgnoreHintsTest
  {
    private Domain domain;

    [Test]
    public void MainTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate);
      using (Session.Open(domain)) {
        using (var transaction = Transaction.Open()) {
          new A() {X = "1", Y = "1"};
          new B();
          transaction.Complete();
        }
      }

      using (IgnoreHintsUpgrader.Enable(
        new IgnoreHint("Tables/A/Columns/X"), new IgnoreHint("Tables/B"))) {
        BuildDomain(DomainUpgradeMode.Perform);
      }
      using (Session.Open(domain)) {
        using (var transaction = Transaction.Open()) {
          Assert.AreEqual(1, Query<A>.All.Count());
          Assert.AreEqual("1", Query<A>.All.First().Y);
          transaction.Complete();
        }
      }

      BuildDomain(DomainUpgradeMode.Validate);
      using (Session.Open(domain)) {
        using (var transaction = Transaction.Open()) {
          Assert.AreEqual(1, Query<A>.All.Count());
          Assert.AreEqual("1", Query<A>.All.First().X);
          Assert.AreEqual("1", Query<A>.All.First().Y);
          Assert.AreEqual(1, Query<B>.All.Count());
          transaction.Complete();
        }
      }
    }

    private void BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(typeof (A));
      configuration.Types.Register(typeof (B));
      domain = Domain.Build(configuration);
    }

    private class IgnoreHintsUpgrader : UpgradeHandler, 
      IModule
    {
      public static bool isEnabled = false;
      private static IgnoreHint[] hints;

      public static IDisposable Enable(params IgnoreHint[] ignoreHints)
      {
        if (isEnabled)
          throw new InvalidOperationException();
        isEnabled = true;
        hints = ignoreHints;
        return new Disposable(_ => {
          isEnabled = false;
          hints = null;
        });
      }

      public override bool IsEnabled { get { return isEnabled; } }

      protected override string DetectAssemblyVersion()
      {
        return "1";
      }

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnSchemaReady()
      {
        var context = UpgradeContext.Current;
        hints.Apply(context.SchemaHints.Add);
      }

      public override void OnUpgrade()
      {
      }

      public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
      {
        return true;
      }

      public void OnBuilt(Domain domain)
      {
      }

      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        if (!isEnabled || !model.Types.Contains("A"))
          return;

        var aType = model.Types["A"];
        var bType = model.Types["B"];
        var xField = aType.Fields["X"];

        aType.Fields.Remove(xField);
        model.Types.Remove(bType);
      }
    }
  }
}

namespace Xtensive.Storage.Tests.Upgrade.IgnoreHints.Model
{
  [HierarchyRoot]
  public class A : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string X { get; set; }

    [Field]
    public string Y { get; set; }
  }

  [HierarchyRoot]
  public class B : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
  }

}