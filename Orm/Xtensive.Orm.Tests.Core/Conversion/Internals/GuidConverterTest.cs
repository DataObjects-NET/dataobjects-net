// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class GuidConverterTest : ConverterTestBase<Guid>
  {
    private readonly Guid[] constants = new Guid[] {
      Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), Guid.NewGuid(),
    };
    private readonly HashSet<Type> allowedTargetTypes = new() { typeof(string) };

    /// <inheritdoc/>
    protected override Guid[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;
  }
}
