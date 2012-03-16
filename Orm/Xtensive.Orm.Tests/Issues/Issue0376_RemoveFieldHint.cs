// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.09

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using System.Reflection;
using System.Linq;

#region Models

namespace Xtensive.Orm.Tests.Issues.Issue0376.Model1
{
  [Serializable]
  [HierarchyRoot]
  public class Father : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    
    [Field]
    public string LastName { get; set; }
  }

  [Serializable]
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
  [Serializable]
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

  [Serializable]
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

    public override bool IsEnabled {
      get {
        return isEnabled;
      }
    }

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      var oldNamespace = "Xtensive.Orm.Tests.Issues.Issue0376.Model1";
      hints.Add(new RenameTypeHint(oldNamespace + ".Father", typeof (Father)));
      hints.Add(new RenameTypeHint(oldNamespace + ".Son", typeof (Son)));
      hints.Add(new MoveFieldHint(oldNamespace + ".Son", "FirstName", typeof (Father)));
//      hintSet.Add(new CopyFieldHint(oldNamescpace + ".Son", "FirstName", typeof (Father)));
//      hintSet.Add(new RemoveFieldHint(oldNamescpace + ".Son", "FirstName"));
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.Issue0376.Model3
{
  [Serializable]
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
      hints.Add(new RenameTypeHint(oldNamespace + ".Father", typeof (Father)));
      hints.Add(new RemoveTypeHint(oldNamespace + ".Son"));
    }
  }
}

#endregion


namespace Xtensive.Orm.Tests.Issues
{
  using M1 = Issue0376.Model1;
  using M2 = Issue0376.Model2;
  using M3 = Issue0376.Model3;
  
  [TestFixture]
  public class Issue0376_RemoveFieldHint : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Issues.Issue0376.Model1");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var son = new M1.Son {FirstName = "FirstName", LastName = "LastName", NickName = "NickName"};
          transactionScope.Complete();
        }
      }
      return domain;
    }

    [Test]
    public void BaseTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.UpdateFrom);
      // Test MoveFieldHint (RemoveFieldHint)
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Issues.Issue0376.Model2");
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;
      Domain domain;
      using (M2.Upgrader.Enable()) {
        domain = Domain.Build(config);
      }
      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var son = session.Query.All<M2.Son>().Single();
          Assert.AreEqual("FirstName", son.FirstName);
          Assert.AreEqual("LastName", son.LastName);
          Assert.AreEqual("NickName", son.NickName);
          transactionScope.Complete();
        }
      }
      
      // Test RemoveTypeHint
      config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Issues.Issue0376.Model3");
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;
      using (M3.Upgrader.Enable()) {
        domain = Domain.Build(config);
      }
      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          Assert.IsTrue(session.Query.All<M3.Father>().Count()==0);
          transactionScope.Complete();
        }
      }
    }
  }
}