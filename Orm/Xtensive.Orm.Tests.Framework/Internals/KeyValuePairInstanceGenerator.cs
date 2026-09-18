// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.25


using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Tests
{
  internal sealed class KeyValuePairInstanceGenerator<T1, T2>(IInstanceGeneratorProvider provider)
    : WrappingInstanceGenerator<KeyValuePair<T1, T2>, T1, T2>(provider)
  {
    public override KeyValuePair<T1, T2> GetInstance(Random random)
      => new(BaseGenerator1.GetInstance(random), BaseGenerator2.GetInstance(random));
  }
}