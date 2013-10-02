// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.24

using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class DecimalInstanceGenerator : InstanceGeneratorBase<decimal>
  {
    public override decimal GetInstance(Random random)
    {
      unchecked {
        return new decimal(random.Next(), random.Next(), random.Next(),
           (random.Next() % 2 == 0), (byte)(random.Next() % 29));
      }
    }


    // Constructors

    public DecimalInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}