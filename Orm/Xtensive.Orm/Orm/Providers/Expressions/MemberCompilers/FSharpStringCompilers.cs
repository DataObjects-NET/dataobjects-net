// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class FSharpStringCompilers
  {
    [Compiler(typeof(string), nameof(string.Concat), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(object))] SqlExpression str0,
      [Type(typeof(object))] SqlExpression str1)
    {
      return SqlDml.Concat(str0, str1);
    }

    [Compiler(typeof(string), nameof(string.Concat), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(object))] SqlExpression str0,
      [Type(typeof(object))] SqlExpression str1,
      [Type(typeof(object))] SqlExpression str2)
    {
      return SqlDml.Concat(SqlDml.Concat(str0, str1), str2);
    }

    [Compiler(typeof(string), nameof(string.Concat), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(object[]))] SqlExpression values)
    {
      if (values.NodeType != SqlNodeType.Container)
        throw new NotSupportedException();
      var container = (SqlContainer) values;
      if (container.Value.GetType() != typeof(SqlExpression[]))
        throw new NotSupportedException();
      var expressions = (SqlExpression[]) container.Value;
      if (expressions.Length == 0)
        return SqlDml.Literal("");
      return expressions.Aggregate(SqlDml.Concat);
    }
  }
}
