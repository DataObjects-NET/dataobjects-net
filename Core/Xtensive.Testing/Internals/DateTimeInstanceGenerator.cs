// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.22


using System;

namespace Xtensive.Testing
{
  [Serializable]
  internal class DateTimeInstanceGenerator : InstanceGeneratorBase<DateTime>
  {
    private readonly IInstanceGenerator<long> longInstanceGeneratorProvider;

    public override DateTime GetInstance(Random random)
    {
      long randomLong = long.MinValue;
      // MinValue must be excluded
      while (randomLong == long.MinValue)
        randomLong = longInstanceGeneratorProvider.GetInstance(random);
      long correctDateTime = Math.Abs(Math.Abs(randomLong) %(DateTime.MaxValue - DateTime.MinValue).Ticks + DateTime.MinValue.Ticks);
      return new DateTime(correctDateTime);
      
    }


    // Constructors

    public DateTimeInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
      longInstanceGeneratorProvider = provider.GetInstanceGenerator<long>();
    }
  }
}