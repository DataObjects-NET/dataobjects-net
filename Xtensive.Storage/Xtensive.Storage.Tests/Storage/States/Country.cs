// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.26

using Xtensive.Storage.Attributes;
using Xtensive.Storage.KeyProviders;

namespace Xtensive.Storage.Tests.Storage.States
{
  [HierarchyRoot(typeof (Int32Generator), "Id")]
  public class Country : Entity
  {
    /// <summary>
    /// Key field.
    /// </summary>
    [Field]
    public int Id { get; set; }

    /// <summary>
    /// Country name.
    /// </summary>
    [Field]
    public string Name { get; set; }
  }
}