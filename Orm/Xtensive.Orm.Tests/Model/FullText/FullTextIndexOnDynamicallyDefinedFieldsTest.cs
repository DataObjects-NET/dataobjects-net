// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2016.11.17

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Model.FullTextIndexOnDynamicallyDefinedFieldsTestModel;

namespace Xtensive.Orm.Tests.Model.FullTextIndexOnDynamicallyDefinedFieldsTestModel
{
  [HierarchyRoot]
  public class Store : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    //Will be dynamically defined
    //[Field]
    //public Address Address { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    // index will be added dynamically
    //[FullText("English")]
    public string FirstName { get; set; }

    [Field]
    // index will be added dynamically
    //[FullText("English")]
    public string LastName { get; set; }

    [Field]
    public DateTime DateOfBirth { get; set; }

    //Field will be added dynamically
    // and added to FullText
    //[Field]
    //public string ManagerSpecialComment { get; set; }

    [Field]
    public Address Address { get; set; }

    //Field will be added dynamically
    // and added to FullText
    //[Field]
    //public Address OptionalAddress { get; set; }
  }

  public class Address : Structure
  {
    [Field]
    public string City { get; set; }

    [Field]
    public string Country { get; set; }

    // Will be defided dinamically
    // also will be included to FT index
    //[Field]
    //public string Region { get; set; }

    [Field]
    public string Street { get; set; }

    [Field]
    public string BuildingNumber { get; set; }
  }

  public class Module : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      DefineDynamicFields(model);
      DefineFullTextIndex(model);
    }

    private void DefineDynamicFields(DomainModelDef model)
    {
      var addressType = model.Types[typeof(Address)];
      var region = addressType.DefineField("Region", typeof(string));
      region.Length = 200;

      var customerType = model.Types[typeof(Customer)];
      var commentField = customerType.DefineField("ManagerSpecialComment", typeof(string));
      commentField.Length = 250;

      customerType.DefineField("OptionalAddress", typeof(Address));

      var storeType = model.Types[typeof(Store)];
      storeType.DefineField("Address", typeof(Address));
    }

    private void DefineFullTextIndex(DomainModelDef model)
    {
      var customerType = model.Types[typeof(Customer)];
      var fieldsToDefineIndex = customerType.Fields
        .Where(f => f.Name.In("FirstName", "LastName"))
        .Select(f => new FullTextFieldDef(f.Name, true) { Configuration = "English" });
      var index = new FullTextIndexDef(customerType);
      index.Fields.AddRange(fieldsToDefineIndex);

      model.FullTextIndexes.Add(index);

      var addressType = model.Types[typeof(Address)];
      fieldsToDefineIndex = addressType.Fields
        .Where(f => f.Name.In("Country", "Region", "City", "Street"))
        .Select(f => new FullTextFieldDef(f.Name, true) { Configuration = "English" });
      index = new FullTextIndexDef(addressType); 
      index.Fields.AddRange(fieldsToDefineIndex);
      model.FullTextIndexes.Add(index);
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class FullTextIndexOnDynamicallyDefinedFieldsTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    [Test]
    public void FullTextIndexExistanceTest()
    {
      var customer = Domain.Model.Types[typeof(Customer)];
      var ftIndex = customer.FullTextIndex;
      Assert.That(customer.FullTextIndex, Is.Not.Null);
      Assert.That(ftIndex.Columns.Count, Is.EqualTo(10));

      var store = Domain.Model.Types[typeof(Store)];
      ftIndex = store.FullTextIndex;
      Assert.That(store.FullTextIndex, Is.Not.Null);
      Assert.That(ftIndex.Columns.Count, Is.EqualTo(4));
    }

    [Test]
    public void DynamicallyDefinedEntityIndexTest()
    {
      var customer = Domain.Model.Types[typeof(Customer)];
      var ftIndex = customer.FullTextIndex;

      var firstNameField = customer.Fields["FirstName"];
      Assert.That(ftIndex.Columns.Contains(firstNameField.Column.Name));

      var lastNameField = customer.Fields["LastName"];
      Assert.IsTrue(ftIndex.Columns.Contains(lastNameField.Column.Name));
    }

    [Test]
    public void DynamicallyDefinedStructureIndexTest()
    {
      var customer = Domain.Model.Types[typeof(Customer)];
      var ftIndex = customer.FullTextIndex;

      var addressField = customer.Fields["Address"];
      var indexedColumns = addressField.Fields
        .Where(f => f.Name.In("Country", "Region", "City", "Street"))
        .Select(f => f.Column);
      Assert.IsTrue(indexedColumns.All(c => ftIndex.Columns.Contains(c.Name)));
    }

    [Test]
    public void DynamicallyDefinedBothEntityAndStrucureFieldsTest()
    {
      var customer = Domain.Model.Types[typeof(Customer)];
      var ftIndex = customer.FullTextIndex;

      var optionalAddressField = customer.Fields["OptionalAddress"];
      var indexedColumns = optionalAddressField.Fields
        .Where(f => f.Name.In("Country", "Region", "City", "Street"))
        .Select(f => f.Column);
      Assert.IsTrue(indexedColumns.All(c => ftIndex.Columns.Contains(c.Name)));

      var store = Domain.Model.Types[typeof(Store)];
      ftIndex = store.FullTextIndex;

      var addressField = customer.Fields["Address"];
      indexedColumns = addressField.Fields
        .Where(f => f.Name.In("Country", "Region", "City", "Street"))
        .Select(f => f.Column);
      Assert.IsTrue(indexedColumns.All(c => ftIndex.Columns.Contains(c.Name)));
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Customer).Assembly, typeof(Customer).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
