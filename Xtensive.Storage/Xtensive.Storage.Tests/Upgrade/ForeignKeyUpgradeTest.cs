// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.29

using System;
using NUnit.Framework;
using System.Linq;
using Xtensive.Core.Disposing;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.Model.Version1;
using Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.Model.Version2;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.Model.Version1
{
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public string Number { get; set; }

    [Field]
    public Person Consumer { get; set; }
  }

  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.Model.Version2
{
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public string Number { get; set; }

    [Field]
    public Company Consumer { get; set; }
  }

  [HierarchyRoot]
  public class Company : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade
{
  public class TestUpgradeHandler : UpgradeHandler
  {
    private static bool isEnabled = false;
    private static string runningVersion;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(string version)
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      runningVersion = version;
      return new Disposable(_ => {
        isEnabled = false;
        runningVersion = null;
      });
    }

    public override bool IsEnabled
    {
      get { return isEnabled; }
    }

    protected override string DetectAssemblyVersion()
    {
      return runningVersion;
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion == null || double.Parse(oldVersion) <= double.Parse(runningVersion);
    }

    public override void OnUpgrade()
    {
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Version" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace != originalNamespace
        && base.IsTypeAvailable(type, upgradeStage);
    }

    public override string GetTypeName(Type type)
    {
      string suffix = ".Version" + runningVersion;
      var nameSpace = type.Namespace.TryCutSuffix(suffix);
      return nameSpace + type.Name;
    }
  }
}

namespace Xtensive.Storage.Tests.Upgrade
{
  using OldOrder = ForeignKeyUpgrade.Model.Version1.Order;
  using NewOrder = ForeignKeyUpgrade.Model.Version2.Order;
  using System.Reflection;
  
  [TestFixture]
  public class ForeignKeyUpgradeTest
  {
    private Domain BuildDomain(string @namespace, string version, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(Assembly.GetExecutingAssembly(), @namespace);
      configuration.UpgradeMode = mode;
      Domain domain;
      using (ForeignKeyUpgrade.TestUpgradeHandler.Enable(version)) {
        domain = Domain.Build(configuration);
      }
      return domain;
    }
    
    [Test]
    public void SetReferencingFieldToDefaultTest()
    {
      var domain = BuildDomain(typeof (OldOrder).Namespace, "1", DomainUpgradeMode.Recreate);
      using (domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          var person = new Person {Name = "Person1"};
          new OldOrder {Consumer = person};
          new OldOrder {Consumer = person};
          transactionScope.Complete();
        }
      }
      domain.DisposeSafely();

      domain = BuildDomain(typeof(NewOrder).Namespace, "2", DomainUpgradeMode.Perform);
      using (domain.OpenSession()) {
        using (Transaction.Open()) {          
          Assert.AreEqual(2, Query<NewOrder>.All.Count());          
          foreach (var order in Query<NewOrder>.All) 
            Assert.IsNull(order.Consumer);
        }
      }
      domain.DisposeSafely();
    }
  }
}