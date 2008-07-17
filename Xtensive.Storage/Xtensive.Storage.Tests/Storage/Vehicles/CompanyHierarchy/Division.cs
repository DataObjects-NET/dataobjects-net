// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using Xtensive.Storage.Attributes;
using Xtensive.Storage.KeyProviders;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Company division. A part of company.
  /// </summary>
  [HierarchyRoot(typeof (Int64Provider), "Id")]
  public class Division : Entity
  {
    [Field]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets division's company.
    /// </summary>
    [Field]
    public Company Company { get; set; }

    /// <summary>
    /// Gets or sets division name.
    /// </summary>
    [Field]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets division number. Stores <see cref="int"/> value as <see cref="string"/> in storage.
    /// </summary>
    
    public int Number
    {
      get { return int.Parse(NumberString); }
      set { NumberString = value.ToString(); }
    }

    [Field]
    private string NumberString { get; set;}

    /// <summary>
    /// Gets collection of fleets. Every division can contains several truck fleets.
    /// </summary>
    [Field]
    public EntitySet<Fleet> Fleets
    {
      get { return GetValue<EntitySet<Fleet>>("Fleets"); }
    }
  }
}