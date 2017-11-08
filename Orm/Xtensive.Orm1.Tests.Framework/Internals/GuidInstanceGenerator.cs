// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.22


using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class GuidInstanceGenerator : InstanceGeneratorBase<Guid>
  {
    public override Guid GetInstance(Random random)
    {
      byte[] byteBuffer = new byte[16];
      random.NextBytes(byteBuffer);
      return new Guid(byteBuffer);
    }


    // Constructors

    public GuidInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}