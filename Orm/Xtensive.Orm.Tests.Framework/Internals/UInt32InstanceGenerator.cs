// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class UInt32InstanceGenerator : WrappingInstanceGenerator<uint, int>
  {
    public override uint GetInstance(Random random)
    {
      unchecked {
        return (uint)BaseGenerator.GetInstance(random);
      }
    }


    // Constructors

    public UInt32InstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}