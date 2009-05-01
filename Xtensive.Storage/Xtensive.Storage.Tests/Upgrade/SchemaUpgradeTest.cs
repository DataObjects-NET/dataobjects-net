// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Upgrade.Model2;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Tests.Upgrade.Model1
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Person : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string FullName  { get; set;}

    [Field] 
    public Address Address { get; set;}
  }

  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Address : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string City { get; set;}
  }
}

namespace Xtensive.Storage.Tests.Upgrade.Model2
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Person : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string Name  { get; set;}

    [Field] 
    public string City { get; set;}

    [Field]
    [Obsolete]
    public Address Address { get; set;}
  }

  [Recycled]
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Address : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string City { get; set;}
  }
}

namespace Xtensive.Storage.Tests.Upgrade
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

    public override bool IsEnabled {
      get {
        return isEnabled;
      }
    }
    
    protected override string DetectAssemblyVersion()
    {
      return runningVersion;
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion==null || double.Parse(oldVersion) <= double.Parse(runningVersion);
    }
    
    protected override void AddUpgradeHints()
    {
      var context = UpgradeContext.Current;
      base.AddUpgradeHints();
      if (runningVersion!="2")
        return;
      context.Hints.Add(new RenameNodeHint(
        "Tables/Person/Columns/Name", 
        "Tables/Person/Columns/FullName"));
    }

    public override void OnUpgrade()
    {
      var context = UpgradeContext.Current;
      if (runningVersion!="2")
        return;
      foreach (var person in Query<Person>.All)
        person.City = person.Address.City;
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Model" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace!=originalNamespace 
        && base.IsTypeAvailable(type, upgradeStage);
    }

    public override string GetTypeName(Type type)
    {
      string suffix = "Model" + runningVersion;
      var ns = type.Namespace.TryCutSuffix(suffix);
      return ns + type.Name;
    }
  }

  [TestFixture]
  public class SchemaUpgradeTest
  {
    private int assemblyTypeId;
    private int persionTypeId;

    public DomainConfiguration GetConfiguration(Type persistentType)
    {
      var dc = DomainConfigurationFactory.Create();
      var ns = persistentType.Namespace;
      dc.Types.Register(Assembly.GetExecutingAssembly(), ns);
      return dc;
    }

    [Test]
    public void MainTest()
    {
      BuildFirstModel();
      BuildSecondDomain();
    }

    private void BuildFirstModel()
    {
      var dc = GetConfiguration(typeof(Model1.Person));
      dc.UpgradeMode = DomainUpgradeMode.Recreate;
      Domain domain;
      using (TestUpgradeHandler.Enable("1")) {
        domain = Domain.Build(dc);
      }
      using (domain.OpenSession()) {        
        using (var ts = Transaction.Open()) {
          assemblyTypeId = domain.Model.Types[typeof (Metadata.Assembly)].TypeId;
          persionTypeId = domain.Model.Types[typeof (Model1.Person)].TypeId;

          new Model1.Person {
            Address = new Model1.Address {City = "Mumbai"},
            FullName = "Gaurav"
          };
          new Model1.Person {
            Address = new Model1.Address {City = "Delhi"},
            FullName = "Mihir"
          };
          ts.Complete();
        }
      }
    }

    private void BuildSecondDomain()
    {
      var dc = GetConfiguration(typeof(Model2.Person));
      dc.UpgradeMode = DomainUpgradeMode.Perform;
      Domain domain;
      using (TestUpgradeHandler.Enable("2")) {
        domain = Domain.Build(dc);
      }
      using (domain.OpenSession()) {
        using (var ts = Transaction.Open()) {
          Assert.AreEqual(assemblyTypeId, domain.Model.Types[typeof (Metadata.Assembly)].TypeId);
          Assert.AreEqual(persionTypeId, domain.Model.Types[typeof (Model2.Person)].TypeId);

          foreach (var person in Query<Model2.Person>.All) {
            if (person.Name=="Gauvar")
              Assert.AreEqual("Mumbai", person.City);
            else
              Assert.AreEqual("Delhi", person.City);
          }
          ts.Complete();
        }
      }
    }
  }
}
