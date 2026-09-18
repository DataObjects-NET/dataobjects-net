// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.22


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class GuidInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<Guid>(provider)
  {
    public override Guid GetInstance(Random random)
    {
      byte[] byteBuffer = new byte[16];
      random.NextBytes(byteBuffer);
      return new Guid(byteBuffer);
    }
  }
}