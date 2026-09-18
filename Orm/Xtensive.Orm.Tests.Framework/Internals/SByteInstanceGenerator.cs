// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class SByteInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<sbyte>(provider)
  {
    private readonly IInstanceGenerator<byte> byteItemGenerator = provider.GetInstanceGenerator<byte>();

    public override sbyte GetInstance(Random random)
    {
      unchecked {
        return (sbyte)byteItemGenerator.GetInstance(random);
      }
    }
  }
}