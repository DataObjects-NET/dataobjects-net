// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Testing
{
  [Serializable]
  internal class SByteInstanceGenerator : InstanceGeneratorBase<sbyte>
  {
    private readonly IInstanceGenerator<byte> byteItemGenerator;

    public override sbyte GetInstance(Random random)
    {
      unchecked {
        return (sbyte)byteItemGenerator.GetInstance(random);
      }
    }


    // Constructors

    public SByteInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
      byteItemGenerator = provider.GetInstanceGenerator<byte>();
    }
  }
}