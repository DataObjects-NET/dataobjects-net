// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class Int16InstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<short>(provider)
  {
    public override short GetInstance(Random random)
      => (short) random.Next(short.MinValue, short.MaxValue + 1);
  }
}