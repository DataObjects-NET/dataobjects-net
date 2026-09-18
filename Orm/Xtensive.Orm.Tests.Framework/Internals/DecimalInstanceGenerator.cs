// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.01.24

using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class DecimalInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<decimal>(provider)
  {
    public override decimal GetInstance(Random random)
    {
      unchecked {
        return new decimal(random.Next(), random.Next(), random.Next(),
           (random.Next() % 2 == 0), (byte)(random.Next() % 29));
      }
    }
  }
}