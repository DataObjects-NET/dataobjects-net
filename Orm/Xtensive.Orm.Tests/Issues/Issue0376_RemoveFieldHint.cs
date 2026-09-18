// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using System.Reflection;
using System.Linq;
using M1 = Xtensive.Orm.Tests.Issues.Issue0376.Model1;
using M2 = Xtensive.Orm.Tests.Issues.Issue0376.Model2;
using M3 = Xtensive.Orm.Tests.Issues.Issue0376.Model3;

#region Models

namespace Xtensive.Orm.Tests.Issues.Issue0376.Model1
{
  [HierarchyRoot]
  public class Father : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    
    [Field]
    public string LastName { get; set; }
  }

  public class Son : Father
  {
    [Field]
    public string FirstName { get; set; }
    
    [Field]
    public string NickName { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.Issue0376.Model2
{
  [HierarchyRoot]
  public class Father : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public string FirstName { get; set; }
  }

  public class Son : Father
  {
    [Field]
    public string NickName { get; set; }
  }
  
  public class Upgrader : UpgradeHandler
  {
    private static bool isEnabled = false;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable()
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      return new Disposable(_ => {
        isEnabled = false;
      });
    }

    public override bool IsEnabled => isEnabled;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      var oldNamespace = "Xtensive.Orm.Tests.Issues.Issue0376.Model1";
      _ = hints.Add(new RenameTypeHint(oldNamespace + ".Father", typeof (Father)));
      _ = hints.Add(new RenameTypeHint(oldNamespace + ".Son", typeof (Son)));
      _ = hints.Add(new MoveFieldHint(oldNamespace + ".Son", "FirstName", typeof (Father)));
//      hintSet.Add(new CopyFieldHint(oldNamescpace + ".Son", "FirstName", typeof (Father)));
//      hintSet.Add(new RemoveFieldHint(oldNamescpace + ".Son", "FirstName"));
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.Issue0376.Model3
{
  [HierarchyRoot]
  public class Father : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public string FirstName { get; set; }
  }

  public class Upgrader : UpgradeHandler
  {
    private static bool isEnabled = false;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable()
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      return new Disposable(_ => {
        isEnabled = false;
      });
    }

    public override bool IsEnabled {
      get {
        return isEnabled;
      }
    }

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      var oldNamespace = "Xtensive.Orm.Tests.Issues.Issue0376.Model2";
      _ = hints.Add(new RenameTypeHint(oldNamespace + ".Father", typeof (Father)));
      _ = hints.Add(new RemoveTypeHint(oldNamespace + ".Son"));
    }
  }
}

#endregion


namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0376_RemoveFieldHint
  {
    [Test]
    public void MainTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.UpdateFrom);

      var config = DomainConfigurationFactory.Create();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof(M1.Son).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      Domain domain;
      using (domain = Domain.Build(config))
      using (var session = domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var son = new M1.Son { FirstName = "FirstName", LastName = "LastName", NickName = "NickName" };
        transactionScope.Complete();
      }

      // Test MoveFieldHint (RemoveFieldHint)
      config = DomainConfigurationFactory.Create();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof(M2.Son).Namespace);
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;

      using (M2.Upgrader.Enable()) {
        domain = Domain.Build(config);
      }
      using (domain)
      using (var session = domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var son = session.Query.All<M2.Son>().Single();
        Assert.That(son.FirstName, Is.EqualTo("FirstName"));
        Assert.That(son.LastName, Is.EqualTo("LastName"));
        Assert.That(son.NickName, Is.EqualTo("NickName"));
        transactionScope.Complete();
      }
      
      // Test RemoveTypeHint
      config = DomainConfigurationFactory.Create();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof(M3.Father).Namespace);
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;
      using (M3.Upgrader.Enable()) {
        domain = Domain.Build(config);
      }
      using (domain)
      using (var session = domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        Assert.That(session.Query.All<M3.Father>().Count() == 0, Is.True);
        transactionScope.Complete();
      }
    }
  }
}