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
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;
using Address=Xtensive.Storage.Tests.Upgrade.Model.Version1.Address;

namespace Xtensive.Storage.Tests.Upgrade.Model.Version1
{
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Person : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Name  { get; set;}

    [Field]
    public Address Address { get; set; }
  }

  [HierarchyRoot("Person")]
  public class Address : Entity
  {
    [Field]
    public Person Person { get; private set; }

    [Field]
    public string City { get; set;}

    public Address(Person person)
      : base(Tuple.Create(person.Id))
    {
    }
  }
}

namespace Xtensive.Storage.Tests.Upgrade.Model.Version2
{
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Contact : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string FullName  { get; set;}

    [Field]
    public Address Address { get; set; }
  }

  [HierarchyRoot("Contact")]
  public class Address : Entity
  {
    [Field]
    public Contact Contact { get; private set; }

    [Field]
    public string City { get; set;}

    public Address(Contact contact)
      : base(Tuple.Create(contact.Id))
    {
    }
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
      base.AddUpgradeHints();
      var context = UpgradeContext.Current;
      if (runningVersion!="2")
        return;
      // TODO: Uncomment this when rename hints will work
      context.Hints.Add(new RenameTypeHint(
        typeof (Model.Version2.Contact), 
        "Xtensive.Storage.Tests.Upgrade.ModelPerson"));
      context.Hints.Add(new RenameNodeHint(
        "Tables/Contact",
        "Tables/Person"));
      context.Hints.Add(new RenameNodeHint(
        "Tables/Contact/Columns/FullName",
        "Tables/Person/Columns/Name"));
      context.Hints.Add(new RenameNodeHint(
        "Tables/Contact/Columns/Address.Contact.Id",
        "Tables/Person/Columns/Address.Person.Id"));
      context.Hints.Add(new RenameNodeHint(
        "Tables/Address/Columns/Contact.Id",
        "Tables/Address/Columns/Person.Id"));
    }

    public override void OnUpgrade()
    {
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Version" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace!=originalNamespace 
        && base.IsTypeAvailable(type, upgradeStage);
    }

    public override string GetTypeName(Type type)
    {
      string suffix = ".Version" + runningVersion;
      var nameSpace = type.Namespace.TryCutSuffix(suffix);
      return nameSpace + type.Name;
    }
  }

  [TestFixture]
  public class SchemaUpgradeTest
  {
    private int assemblyTypeId;
    private int personTypeId;

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
      var dc = GetConfiguration(typeof(Model.Version1.Person));
      dc.UpgradeMode = DomainUpgradeMode.Recreate;
      Domain domain;
      using (TestUpgradeHandler.Enable("1")) {
        domain = Domain.Build(dc);
      }
      using (domain.OpenSession()) {        
        using (var ts = Transaction.Open()) {
          assemblyTypeId = domain.Model.Types[typeof (Metadata.Assembly)].TypeId;
          personTypeId = domain.Model.Types[typeof (Model.Version1.Person)].TypeId;
          var p1 = new Model.Version1.Person {Name = "Gaurav"};
          p1.Address = new Address(p1) {City = "Mumbai"};
          var p2 = new Model.Version1.Person {Name = "Mihir"};
          p2.Address = new Address(p2) {City = "Delhi"};
          ts.Complete();
        }
      }
    }

    private void BuildSecondDomain()
    {
      var dc = GetConfiguration(typeof(Model.Version2.Contact));
      dc.UpgradeMode = DomainUpgradeMode.PerformSafely;
      Domain domain;
      using (TestUpgradeHandler.Enable("2")) {
        domain = Domain.Build(dc);
      }
      using (domain.OpenSession()) {
        using (var ts = Transaction.Open()) {
          Assert.AreEqual(assemblyTypeId, domain.Model.Types[typeof (Metadata.Assembly)].TypeId);
          Assert.AreEqual(personTypeId, domain.Model.Types[typeof (Model.Version2.Contact)].TypeId);
          Assert.IsFalse(domain.Model.Types.Contains(typeof (Model.Version1.Address)));

          foreach (var person in Query<Model.Version2.Contact>.All) {
            if (person.FullName=="Gaurav")
              Assert.AreEqual("Mumbai", person.Address.City);
            else
              Assert.AreEqual("Delhi", person.Address.City);
            AssertEx.Throws<Exception>(() => {
              var a = person.Address;
            });
          }

          var c1 = new Model.Version2.Contact {FullName = "Some contact"};
          c1.Address = new Model.Version2.Address(c1) {City = "Delhi"};
          ts.Complete();
        }
      }
    }
  }
}
