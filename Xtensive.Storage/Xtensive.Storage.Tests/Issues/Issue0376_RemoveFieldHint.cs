// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.09

using System;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Upgrade;
using System.Reflection;
using System.Linq;

#region Models

namespace Xtensive.Storage.Tests.Issues.Issue0376.Model1
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

namespace Xtensive.Storage.Tests.Issues.Issue0376.Model2
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

    protected override void AddUpgradeHints()
    {
      var hintSet = UpgradeContext.Demand().Hints;
      var oldNamescpace = "Xtensive.Storage.Tests.Issues.Issue0376.Model1";
      hintSet.Add(new RenameTypeHint(oldNamescpace + ".Father", typeof (Father)));
      hintSet.Add(new RenameTypeHint(oldNamescpace + ".Son", typeof (Son)));

      hintSet.Add(new MoveFieldHint(oldNamescpace + ".Son", "FirstName", typeof (Father)));
//      hintSet.Add(new CopyFieldHint(oldNamescpace + ".Son", "FirstName", typeof (Father)));
//      hintSet.Add(new RemoveFieldHint(oldNamescpace + ".Son", "FirstName"));
    }
  }
}

namespace Xtensive.Storage.Tests.Issues.Issue0376.Model3
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

    protected override void AddUpgradeHints()
    {
      var hintSet = UpgradeContext.Demand().Hints;
      var oldNamescpace = "Xtensive.Storage.Tests.Issues.Issue0376.Model2";
      hintSet.Add(new RenameTypeHint(oldNamescpace + ".Father", typeof (Father)));

      hintSet.Add(new RemoveTypeHint(oldNamescpace + ".Son"));
    }
  }
}

#endregion


namespace Xtensive.Storage.Tests.Issues
{
  using M1 = Issue0376.Model1;
  using M2 = Issue0376.Model2;
  using M3 = Issue0376.Model3;
  
  [TestFixture]
  public class Issue0376_RemoveFieldHint : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Issues.Issue0376.Model1");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override Domain BuildDomain(Xtensive.Storage.Configuration.DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          var son = new M1.Son {FirstName = "FirstName", LastName = "LastName", NickName = "NickName"};
          transactionScope.Complete();
        }
      }
      return domain;
    }

    [Test]
    public void BaseTest()
    {
      // Test MoveFieldHint (RemoveFieldHint)
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Issues.Issue0376.Model2");
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;
      Domain domain;
      using (M2.Upgrader.Enable()) {
        domain = Domain.Build(config);
      }
      using (var session = Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          var son = Query.All<M2.Son>().Single();
          Assert.AreEqual("FirstName", son.FirstName);
          Assert.AreEqual("LastName", son.LastName);
          Assert.AreEqual("NickName", son.NickName);
          transactionScope.Complete();
        }
      }
      
      // Test RemoveTypeHint
      config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Issues.Issue0376.Model3");
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;
      using (M3.Upgrader.Enable()) {
        domain = Domain.Build(config);
      }
      using (var session = Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          Assert.IsTrue(Query.All<M3.Father>().Count()==0);
          transactionScope.Complete();
        }
      }
    }
  }
}