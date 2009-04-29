// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Upgrade.Hints;


namespace Xtensive.Storage.Tests.Upgrade.Model_1
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

namespace Xtensive.Storage.Tests.Upgrade.Model_2
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
    public Recycled_2.Address Address { get; set;}
  }

  public class UpgraderToVersion_2 : IUpgrader
  {


    public string GetAssemblyName()
    {
      return "Xtensive.Storage.Tests.Upgrade.Model";
    }

    public bool CanUpgradeFrom(string schemaVersion)
    {
      return schemaVersion == "1";
    }
    
    public string ResultVersion
    {
      get { return "2"; }
    }

    public void OnBeforeUpgrade()
    {
      UpgradeContext.Current.Hints.Add(
        new RenameFieldHint(typeof(Person), "FullName", "Name"));
    }

    public void OnUpgrade()
    {
      foreach (Person person in Query<Person>.All) {
        person.City = person.Address.City;
      }
    }

    public bool IsAvailable(Type type)
    {
      return true;
    }
  }
}

namespace Xtensive.Storage.Tests.Upgrade.Recycled_2
{
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
  [TestFixture]
  public class SchemaUpgradeTest
  {
    private int assemblyTypeId;
    private int persionTypeId;

    public DomainConfiguration GetConfiguration(Type persistentType)
    {
      DomainConfiguration configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(Assembly.GetExecutingAssembly(), persistentType.Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      BuildFirstModel();
      BuildSecondDomain();
    }

    private void BuildFirstModel()
    {
      DomainConfiguration configuration = GetConfiguration(typeof(Model_1.Person));
      configuration.BuildMode = DomainBuildMode.Recreate;
      Domain domain = Domain.Build(configuration);
      using (domain.OpenSession()) {        
        using (var transactionScope = Transaction.Open()) {

          assemblyTypeId = domain.Model.Types[typeof (Metadata.Assembly)].TypeId;
          persionTypeId = domain.Model.Types[typeof (Model_1.Person)].TypeId;

          new Model_1.Person
            {
              Address = new Model_1.Address {City = "Mumbai"},
              FullName = "Gaurav"
            };
          new Model_1.Person
            {
              Address = new Model_1.Address {City = "Delhi"},
              FullName = "Mihir"
            };
          transactionScope.Complete();
        }
      }
    }

    private void BuildSecondDomain()
    {
      DomainConfiguration configuration = GetConfiguration(typeof(Model_2.Person));
      configuration.BuildMode = DomainBuildMode.PerformStrict;
      Domain domain = Domain.Build(configuration);
      using (domain.OpenSession()) {
        using (Transaction.Open()) {

          Assert.AreEqual(assemblyTypeId, domain.Model.Types[typeof (Metadata.Assembly)].TypeId);
          Assert.AreEqual(persionTypeId, domain.Model.Types[typeof (Model_2.Person)].TypeId);

          foreach (var person in Query<Model_2.Person>.All) {
            if (person.Name=="Gauvar")
              Assert.AreEqual("Mumbai", person.City);
            else
              Assert.AreEqual("Delhi", person.City);
          }
        }
      }
    }
  }
}
