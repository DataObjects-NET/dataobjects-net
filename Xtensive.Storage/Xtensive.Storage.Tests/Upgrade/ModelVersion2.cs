// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Upgrade.Model.Version2
{
  // BusinessContact renamed to Person
  // Person renamed to BusinessContact
  // Fields LastName and FirstName moved from Employee to BusinessContact (old Person)
  // Field Order.ProcessingTime removed

  // Product.Name renamed to Product.Title
  // Category renamed to ProductGroup
  // Product.Category renamed to Product.Group
  // Category.Id renamed to ProductGroup.GroupId

  // Boy.FriendlyGirls renamed to Boy.MeetWith
  // Girl.FrenldyBoys renamed to Girl.MeetWith

  // EntityX.Id renamed to EntityX.Code
  // StructureX.EX renamed to StructureX.MyEX
  
  #region Address, BusinessContact, Person, Employee, Order

  public class Address : Structure
  {
    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }
  }

  [Index("FirstName")]
  [HierarchyRoot]
  public class BusinessContact : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public Address Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 20)]
    public string LastName { get; set; }

    [Field(Length = 10)]
    public string FirstName { get; set; }
  }
  
  [Index("CompanyName")]
  public class Person : BusinessContact
  {
    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }
  }

  [Index("HireDate")]
  public class Employee : BusinessContact
  {
    [Field]
    public DateTime? HireDate { get; set; }

    [Field]
    public Employee ReportsTo { get; set; }

    [Field(PairTo = "ReportsTo")]
    public EntitySet<Employee> ReportingEmployees { get; set; }

    [Field(PairTo = "Employee")]
    public EntitySet<Order> Orders { get; private set; }

    public string FullName { get { return FirstName + " " + LastName; } }
  }

  [Index("OrderDate")]
  [Index("Freight")]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public Person Customer { get; set; }

    [Field]
    public DateTime OrderDate { get; set; }

    [Field(Length = 128)]
    public string ProductName { get; set; }

    [Field]
    public decimal? Freight { get; set; }

    [Field]
    public Address ShippingAddress { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("OrderId: {0}; OrderDate: {1}.", Id, OrderDate);
    }
  }

  #endregion

  #region ProductGroup, Product

  [HierarchyRoot]
  public class ProductGroup : Entity
  {
    [KeyField, Field]
    public int GroupId { get; private set; }

    [Field(Length = 100)]
    public string Name { get; set; }

    [Field(PairTo = "Group")]
    public EntitySet<Product> Products { get; private set; }
  }

  [HierarchyRoot]
  public class Product : Entity
  {
    [KeyField, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Title { get; set; }

    [Field]
    public bool IsActive { get; set; }

    [Field]
    public ProductGroup Group { get; set; }
  }

  #endregion

  #region Boy, Girl

  [HierarchyRoot]
  [KeyGenerator(null)]
  public class Boy : Entity
  {
    [KeyField, Field(Length = 20)]
    public string Name { get; private set; }

    [Field(PairTo = "MeetWith")]
    public EntitySet<Girl> MeetWith { get; private set; }

    public Boy(string name)
      : base(name)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(null)]
  public class Girl : Entity
  {
    [KeyField, Field(Length = 20)]
    public string Name { get; private set; }

    [Field]
    public EntitySet<Boy> MeetWith { get; private set; }

    public Girl(string name)
      : base(name)
    {
    }
  }

  #endregion

  
  #region Crazy association nesting
  
  [HierarchyRoot]
  [KeyGenerator(null)]
  public class Entity1 : Entity
  {
    [KeyField, Field]
    public int Code { get; private set; }

    public Entity1(int id)
      : base(id)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(null)]
  public class Entity2 : Entity
  {
    [KeyField(0), Field]
    public int Code { get; private set; }

    [KeyField(1), Field]
    public Entity1 E1 { get; private set; }

    public Entity2(int id, Entity1 e1)
      : base(id, e1)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(null)]
  public class Entity3 : Entity
  {
    [KeyField(0), Field]
    public int Code { get; private set; }

    [KeyField(1), Field]
    public Entity2 E2 { get; private set; }

    public Entity3(int id, Entity2 e2)
      : base(id, e2)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(null)]
  public class Entity4 : Entity
  {
    [KeyField(0), Field]
    public int Code { get; private set; }

    [KeyField(1), Field]
    public Entity3 E3 { get; private set; }

    public Entity4(int id, Entity3 e3)
      : base(id, e3)
    {
    }
  }
  
  public class Structure1 : Structure
  {
    [Field]
    public Entity1 MyE1 { get; set; }
  }

  public class Structure2 : Structure
  {
    [Field]
    public Structure1 S1 { get; set; }

    [Field]
    public Entity2 MyE2 { get; set; }
  }

  public class Structure3 : Structure
  {
    [Field]
    public Structure2 S2 { get; set; }

    [Field]
    public Entity3 MyE3 { get; set; }
  }

  public class Structure4 : Structure
  {
    [Field]
    public Structure3 S3 { get; set; }

    [Field]
    public Entity4 MyE4 { get; set; }
  }
  
  [HierarchyRoot]
  public class StructureContainer1 : Entity
  {
    [KeyField, Field]
    public int Id { get; private set; }

    [Field]
    public Structure1 S1 { get; set; }
  }

  [HierarchyRoot]
  public class StructureContainer2 : Entity
  {
    [KeyField, Field]
    public int Id { get; private set; }

    [Field]
    public Structure2 S2 { get; set; }
  }

  [HierarchyRoot]
  public class StructureContainer3 : Entity
  {
    [KeyField, Field]
    public int Id { get; private set; }

    [Field]
    public Structure3 S3 { get; set; }
  }

  [HierarchyRoot]
  public class StructureContainer4 : Entity
  {
    [KeyField, Field]
    public int Id { get; private set; }

    [Field]
    public Structure4 S4 { get; set; }
  }

  #endregion
}