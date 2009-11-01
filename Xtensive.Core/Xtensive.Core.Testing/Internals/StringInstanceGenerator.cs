﻿// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.22


using System;

namespace Xtensive.Core.Testing
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