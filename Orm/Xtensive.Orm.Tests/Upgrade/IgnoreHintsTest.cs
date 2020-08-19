// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.06.19

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;

using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Tests.Upgrade.IgnoreHints.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Building;
using System.Threading.Tasks;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class IgnoreHintsTest
  {
    [Test]
    public void MainTest()
    {
      var domain = BuildDomain(DomainUpgradeMode.Recreate);
      using (domain)
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          new A() {X = "1", Y = "1"};
          new B();
          transaction.Complete();
        }
      }

      using (IgnoreHintsUpgrader.Enable(
        new IgnoreHint("Tables/A/Columns/X"), new IgnoreHint("Tables/B"))) {
        domain = BuildDomain(DomainUpgradeMode.Perform);
      }
      using (domain)
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.AreEqual(1, session.Query.All<A>().Count());
          Assert.AreEqual("1", session.Query.All<A>().First().Y);
          transaction.Complete();
        }
      }

      domain = BuildDomain(DomainUpgradeMode.Validate);
      using (domain)
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.AreEqual(1, session.Query.All<A>().Count());
          Assert.AreEqual("1", session.Query.All<A>().First().X);
          Assert.AreEqual("1", session.Query.All<A>().First().Y);
          Assert.AreEqual(1, session.Query.All<B>().Count());
          transaction.Complete();
        }
      }
    }

    [Test]
    public async Task MainAsyncTest()
    {
      var domain = BuildDomain(DomainUpgradeMode.Recreate);
      using (domain)
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          new A() { X = "1", Y = "1" };
          new B();
          transaction.Complete();
        }
      }

      using (IgnoreHintsUpgrader.Enable(
        new IgnoreHint("Tables/A/Columns/X"), new IgnoreHint("Tables/B"))) {
        domain = await BuildDomainAsync(DomainUpgradeMode.Perform);
      }
      using (domain)
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.AreEqual(1, session.Query.All<A>().Count());
          Assert.AreEqual("1", session.Query.All<A>().First().Y);
          transaction.Complete();
        }
      }

      domain = await BuildDomainAsync(DomainUpgradeMode.Validate);
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.AreEqual(1, session.Query.All<A>().Count());
          Assert.AreEqual("1", session.Query.All<A>().First().X);
          Assert.AreEqual("1", session.Query.All<A>().First().Y);
          Assert.AreEqual(1, session.Query.All<B>().Count());
          transaction.Complete();
        }
      }
    }

    private Domain BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(typeof (A));
      configuration.Types.Register(typeof (B));
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(typeof(A));
      configuration.Types.Register(typeof(B));
      return Domain.BuildAsync(configuration);
    }

    private class IgnoreHintsUpgrader : UpgradeHandler, 
      IModule
    {
      public static bool isEnabled = false;
      private static IgnoreHint[] hints;

      public static IDisposable Enable(params IgnoreHint[] ignoreHints)
      {
        if (isEnabled) {
          throw new InvalidOperationException();
        }
        isEnabled = true;
        hints = ignoreHints;
        return new Disposable(_ => {
          isEnabled = false;
          hints = null;
        });
      }

      public override bool IsEnabled { get { return isEnabled; } }

      protected override string DetectAssemblyVersion() => "1";

      public override bool CanUpgradeFrom(string oldVersion) => true;

      public override void OnSchemaReady()
      {
        var context = UpgradeContext.Demand();
        hints.ForEach(context.SchemaHints.Add);
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
        if (!isEnabled || !model.Types.Contains("A")) {
          return;
        }

        var aType = model.Types["A"];
        var bType = model.Types["B"];
        var xField = aType.Fields["X"];

        _ = aType.Fields.Remove(xField);
        _ = model.Types.Remove(bType);
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.IgnoreHints.Model
{
  [Serializable]
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

  [Serializable]
  [HierarchyRoot]
  public class B : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
  }
}