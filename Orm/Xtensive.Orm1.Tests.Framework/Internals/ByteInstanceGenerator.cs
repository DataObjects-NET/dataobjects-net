// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class ByteInstanceGenerator : InstanceGeneratorBase<byte>
  {
    public override byte GetInstance(Random random)
    {
      return (byte)random.Next(byte.MinValue, byte.MaxValue + 1);
    }


    // Constructors

    public ByteInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}