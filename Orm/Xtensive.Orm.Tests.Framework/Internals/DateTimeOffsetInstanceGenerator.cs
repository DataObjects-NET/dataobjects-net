// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class DateTimeOffsetInstanceGenerator : InstanceGeneratorBase<DateTimeOffset>
  {
    private readonly IInstanceGenerator<DateTime> dateTimeInstanceGenerator;

    public override DateTimeOffset GetInstance(Random random)
    {
      var randomDateTime = dateTimeInstanceGenerator.GetInstance(random);
      var randomTimeSpan = new TimeSpan(random.Next(0, 10), random.Next(0, 60), 0);

      var signBase = random.Next(-100, 100);
      if (signBase != 0 && (signBase / Math.Abs(signBase)) < 0)
        randomTimeSpan = randomTimeSpan.Negate();

      return new DateTimeOffset(randomDateTime, randomTimeSpan);
    }

    public DateTimeOffsetInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
      dateTimeInstanceGenerator = provider.GetInstanceGenerator<DateTime>();
    }
  }
}