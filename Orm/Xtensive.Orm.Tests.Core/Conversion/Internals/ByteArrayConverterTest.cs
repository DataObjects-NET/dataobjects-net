// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using System.Collections.Generic;
namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class ByteArrayConverterTest : ConverterTestBase<byte[]>
  {
    private readonly byte[][] constants = {
      Array.Empty<byte>(),
      new byte[] { 0, 1, 15, 26, 52, 75, 127},
    };
    private readonly HashSet<Type> allowedTargetTypes = new() {
      // strict conversions
      typeof(string),
    };

    /// <inheritdoc/>
    protected override byte[][] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;
  }
}
