// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  internal class ByteInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<byte>(provider)
  {
    public override byte GetInstance(Random random) => (byte) random.Next(byte.MinValue, byte.MaxValue + 1);
  }
}