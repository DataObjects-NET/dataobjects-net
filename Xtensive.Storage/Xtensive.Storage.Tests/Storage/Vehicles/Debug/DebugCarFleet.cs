// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.25

using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles.Debug
{
  public class DebugCarFleet : DebugFleet
  {
    [Field]
    public string BodyType { get; set; }
  }
}