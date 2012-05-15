// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using System;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  public sealed class GuidGenerator : GloballyUniqueValueGenerator<Guid>
  {
    protected override Guid GenerateValue()
    {
      return Guid.NewGuid();
    }
  }
}