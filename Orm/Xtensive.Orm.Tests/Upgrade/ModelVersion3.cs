// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20


namespace Xtensive.Orm.Tests.Upgrade.Model.Version3
{
  // BusinessContact replaced with Contact
  // Order.Customer field type changed to Contact
  // Employee removed

  [Serializable]
  public class Address : Structure
  {
    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Address Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }
  }

  [Serializable]
  [Index("CompanyName")]
  [HierarchyRoot]
  public class Contact : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }
  }

  [Serializable]
  [Index("OrderDate")]
  [Index("Freight")]
  [Index("ProductName")]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TimeSpan? ProcessingTime { get; set; }

    [Field]
    public Contact Customer { get; set; }

    [Field]
    public DateTime OrderDate { get; set; }

    [Field(Length = 50)]
    public string ProductName { get; set; }

    [Field]
    public decimal? Freight { get; set; }

    [Field]
    public Address ShippingAddress { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"OrderId: {Id}; OrderDate: {OrderDate}.";
    }
  }
}