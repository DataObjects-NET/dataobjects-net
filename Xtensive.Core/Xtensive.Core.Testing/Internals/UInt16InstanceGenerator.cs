// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Testing
{
  [Serializable]
  internal class UInt16InstanceGenerator : WrappingInstanceGenerator<ushort, short>
  {
    public override ushort GetInstance(Random random)
    {
      unchecked {
        return (ushort)BaseGenerator.GetInstance(random);
      }
    }


    // Constructors

    public UInt16InstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
 }
}