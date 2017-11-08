// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class Int64InstanceGenerator : InstanceGeneratorBase<long>
  {
    public override long GetInstance(Random random)
    {
      byte[] byteBuffer = new byte[8];
      random.NextBytes(byteBuffer);
      long result = 0;
      for (int i = 0; i < 8; i++) {
        result = (result << 8) | byteBuffer[i];
      }
      return result;
    }


    // Constructors

    public Int64InstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}