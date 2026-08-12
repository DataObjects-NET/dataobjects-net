// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.23


using System;

namespace Xtensive.Orm.Tests
{
  internal class EnumInstanceGenerator<TEnum, TSystem>(IInstanceGeneratorProvider provider)
    : WrappingInstanceGenerator<TEnum, TSystem>(provider)
    where TEnum: struct
    where TSystem: struct
  {
    private static readonly Array values = Enum.GetValues(typeof(TEnum));

    public override TEnum GetInstance(Random random) => (TEnum) values.GetValue(random.Next(values.Length));
  }
}