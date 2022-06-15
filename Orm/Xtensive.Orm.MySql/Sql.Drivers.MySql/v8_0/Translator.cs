// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2022.02.03

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.MySql.v8_0
{
  internal class Translator : v5_7.Translator
  {
    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlLockType lockType)
    {
      var forShare = lockType.Supports(SqlLockType.Shared);
      var forUpdate = lockType.SupportsAny(SqlLockType.Update | SqlLockType.Exclusive);

      if (!forShare && !forUpdate) {
        throw new NotSupportedException($"Lock '{lockType.ToString(true)}' is not supported.");
      }

      _ = output
        .Append(forShare ? "FOR SHARE" : "FOR UPDATE")
        .Append(lockType.Supports(SqlLockType.SkipLocked)
          ? " SKIP LOCKED"
          : lockType.Supports(SqlLockType.ThrowIfLocked)
            ? " NOWAIT"
            : string.Empty);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}