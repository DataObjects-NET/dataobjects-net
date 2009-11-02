// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql
{
  public interface ISqlQueryExpression : ISqlCompileUnit, IEnumerable<ISqlQueryExpression>
  {
    SqlQueryExpression Except(ISqlQueryExpression operand);
    SqlQueryExpression ExceptAll(ISqlQueryExpression operand);
    SqlQueryExpression Intersect(ISqlQueryExpression operand);
    SqlQueryExpression IntersectAll(ISqlQueryExpression operand);
    SqlQueryExpression Union(ISqlQueryExpression operand);
    SqlQueryExpression UnionAll(ISqlQueryExpression operand);
  }
}