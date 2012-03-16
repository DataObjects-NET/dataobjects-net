// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;


namespace Xtensive.Testing
{
  [Serializable]
  internal class UInt64InstanceGenerator : WrappingInstanceGenerator<ulong, long>
  {
    public override ulong GetInstance(Random random)
    {
      unchecked {
        return (ulong)BaseGenerator.GetInstance(random);
      }
    }


    // Constructors

    public UInt64InstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}