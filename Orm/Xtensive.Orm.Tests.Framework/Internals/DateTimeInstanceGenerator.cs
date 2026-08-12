// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.22


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class DateTimeInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<DateTime>(provider)
  {
    private readonly IInstanceGenerator<long> longInstanceGeneratorProvider = provider.GetInstanceGenerator<long>();

    public override DateTime GetInstance(Random random)
    {
      var randomLong = longInstanceGeneratorProvider.GetInstance(random);
      // MinValue must be excluded
      while (randomLong == long.MinValue)
        randomLong = longInstanceGeneratorProvider.GetInstance(random);
      var correctDateTime = Math.Abs(Math.Abs(randomLong) %(DateTime.MaxValue - DateTime.MinValue).Ticks + DateTime.MinValue.Ticks);
      return new DateTime(correctDateTime);
      
    }
  }
}