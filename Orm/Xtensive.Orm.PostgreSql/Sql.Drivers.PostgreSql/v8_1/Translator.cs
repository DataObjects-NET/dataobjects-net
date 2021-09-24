// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_1
{
  internal class Translator : v8_0.Translator
  {
    public override void Translate(SqlCompilerContext context, SqlContinue node)
    {
      context.Output.Append("CONTINUE");
    }

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
          context.Output.Append("from justify_hours(");
          break;
        case ExtractSection.Exit:
          context.Output.Append("))");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    public override string Translate(SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.SkipLocked)) {
        return base.Translate(lockType);
      }
      return string.Format("FOR {0}{1}",
        lockType.Supports(SqlLockType.Shared) ? "SHARE" : "UPDATE",
        lockType.Supports(SqlLockType.ThrowIfLocked) ? " NOWAIT" : "");
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}