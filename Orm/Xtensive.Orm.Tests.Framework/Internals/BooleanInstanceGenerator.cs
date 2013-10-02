// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class BooleanInstanceGenerator : InstanceGeneratorBase<bool>
  {
    public override bool GetInstance(Random random)
    {
      unchecked {
        byte[] randomByte = {(byte)random.Next(0, 2)};

        return BitConverter.ToBoolean(randomByte, 0);
      }
    }


    // Constructors

    public BooleanInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}