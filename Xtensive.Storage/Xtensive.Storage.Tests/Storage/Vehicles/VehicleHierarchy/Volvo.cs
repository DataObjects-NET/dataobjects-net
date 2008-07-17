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
  /// Volvo truck. Yes, it's truck :D.
  /// </summary>
  public sealed class Volvo : Truck
  {
    /// <summary>
    /// Gets or sets volvo's weight.
    /// Attention! Overrides base abstract field.
    /// Attention! Autoproperty.
    /// </summary>
    [Field(IsNullable = true)]
    public override double Weight { get; set; }

    /// <summary>
    /// Gets or sets volvo's power.
    /// Attention! Overrides base abstract field.
    /// Attention! Field column type don't correspond to base field column type.
    /// </summary>
    public override int Power
    {
      get { return GetValue<int>("Power"); }
      set { SetValue("Power", value); }
    }
  }
}