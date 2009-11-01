// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.26

using Xtensive.Storage.Attributes;
using Xtensive.Storage.KeyProviders;

namespace Xtensive.Storage.Tests.Storage.States
{
  /// <summary>
  /// States(regions) of country.
  /// </summary>
  [HierarchyRoot(typeof (Int32Provider), "Id")]
  public class State : Entity
  {
    /// <summary>
    /// Key field.
    /// </summary>
    [Field]
    public int Id { get; set; }

    /// <summary>
    /// State/region official name.
    /// </summary>
    [Field]
    public string Name { get; set; }

    /// <summary>
    /// Country.
    /// </summary>
    [Field(IsNullable = true)]
    public Country Country { get; set; }

    /// <summary>
    /// State/region description.
    /// </summary>
    [Field(LazyLoad = true)]
    public string Description { get; set; }
  }
}