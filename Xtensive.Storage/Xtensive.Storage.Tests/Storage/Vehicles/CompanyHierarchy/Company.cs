// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using System;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Company.
  /// </summary>
  [HierarchyRoot(typeof (DefaultGenerator), "Id")]
  public class Company : Entity
  {
    [Field]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets company name.
    /// </summary>
    [Field]
    public string Name
    {
      get { return GetValue<string>("Name"); }
      set { SetValue("Name", value); }
    }

    /// <summary>
    /// Gets or sets compny registration number. Stores <see cref="int"/> value as <see cref="string"/> in storage.
    /// </summary>
    [Field]
    public int RegNumber
    {
      get { return Int32.Parse(RegNumberString); }
      set { RegNumberString = value.ToString(); }
    }

    [Field]
    private string RegNumberString { get; set;}
    

    /// <summary>
    /// Gets collection of <see cref="Division"/> of this company.
    /// </summary>
    [Field]
    public EntitySet<Division> Divisions
    {
      get { return GetValue<EntitySet<Division>>("Divisions"); }
    }

    /// <summary>
    /// Gets collection of <see cref="Carrier"/> assigned to company.
    /// It's many-to-many releation.
    /// </summary>
    [Field]
    public EntitySet<Carrier> Carriers
    {
      get { return GetValue<EntitySet<Carrier>>("Carriers"); }
    }

    /// <summary>
    /// Company address. Structure.
    /// </summary>
    [Field]
    public CompanyAddress Address
    {
      get { return GetValue<CompanyAddress>("Address"); }
      set { SetValue("Address", value); }
    }

    /// <summary>
    /// Company website. Field adds at runtime.
    /// </summary>
    public string WebSiteProperty
    {
      get { return GetValue<string>("WebSite"); }
      set { SetValue("WebSite", value); }
    }

    //[Field]
    public Logo Logo { get; set; }
  }
}