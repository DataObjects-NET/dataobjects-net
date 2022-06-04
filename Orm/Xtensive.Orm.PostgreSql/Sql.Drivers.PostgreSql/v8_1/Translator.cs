// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_1
{
  internal class Translator : v8_0.Translator
  {
    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlContinue node) =>
      context.Output.Append("CONTINUE");

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      switch (node.IntervalPart) {
        case SqlIntervalPart.Day:
        case SqlIntervalPart.Hour:
          break;
        default:
          base.Translate(context, node, section);
          return;
      }
      switch (section) {
        case ExtractSection.From:
          _ = context.Output.AppendOpeningPunctuation("from justify_hours(");
          break;
        case ExtractSection.Exit:
          _ = context.Output.Append("))");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.SkipLocked)) {
        base.Translate(output, lockType);
      }
      _ = lockType.Supports(SqlLockType.Shared)
        ? output.Append("FOR SHARE")
        : output.Append("FOR UPDATE");
      if (lockType.Supports(SqlLockType.ThrowIfLocked)) {
        _ = output.Append(" NOWAIT");
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}