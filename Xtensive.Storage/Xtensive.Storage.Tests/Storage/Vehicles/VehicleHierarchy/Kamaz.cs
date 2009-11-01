// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Kamaz.
  /// </summary>
  public sealed class Kamaz : Truck
  {
    /// <summary>
    /// Gets or sets kamaz power. 
    /// Attention! Overrides base abstract field.
    /// Attention! Autoproperty.
    /// </summary>
    public override int Power { get; set; }

    /// <summary>
    /// Gets or sets kamaz weight. 
    /// Attention! Overrides base abstract field.
    /// Attention! Autoproperty.
    /// </summary>
    [Field]
    public override double Weight { get; set; }
  }
}