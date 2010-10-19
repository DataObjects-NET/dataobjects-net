// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Testing
{
  [Serializable]
  internal class Int16InstanceGenerator : InstanceGeneratorBase<short>
  {
    public override short GetInstance(Random random)
    {
      return (short)random.Next(short.MinValue, short.MaxValue + 1);
    }


    // Constructors

    public Int16InstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}