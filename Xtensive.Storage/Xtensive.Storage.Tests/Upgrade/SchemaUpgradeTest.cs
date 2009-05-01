// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
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
    public static string RunningVersion = "1";

    public override bool IsEnabled {
      get {
        var context = UpgradeContext.Current;
        return (
          from type in context.OriginalConfiguration.Types
          where type.Namespace.StartsWith(GetType().Namespace)
          select type
          ).Any();
      }
    }
    
    protected override string DetectAssemblyVersion()
    {
      return RunningVersion;
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion==null || new Version(oldVersion) < new Version(RunningVersion);
    }
    
    protected override void AddUpgradeHints()
    {
      var context = UpgradeContext.Current;
      base.AddUpgradeHints();
      if (RunningVersion!="2")
        return;
      context.Hints.Add(new RenameNodeHint(
        "/Tables/Person/Columns/FullName", 
        "/Tables/Person/Columns/Name"));
    }

    public override void OnUpgrade()
    {
      var context = UpgradeContext.Current;
      if (RunningVersion!="2")
        return;
      foreach (var person in Query<Person>.All)
        person.City = person.Address.City;
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Model" + RunningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace!=originalNamespace 
        && base.IsTypeAvailable(type, upgradeStage);
    }

    public override string GetTypeName(Type type)
    {
      string suffix = "Model" + RunningVersion;
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
      TestUpgradeHandler.RunningVersion = ns.Substring(ns.Length - 1);
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
      var domain = Domain.Build(dc);
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
      var domain = Domain.Build(dc);
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
