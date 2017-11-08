// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.22


using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class StringInstanceGenerator : InstanceGeneratorBase<string>
  {
    public override string GetInstance(Random random)
    {
      return "Random String " + random.Next();
    }


    // Constructors

    public StringInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}