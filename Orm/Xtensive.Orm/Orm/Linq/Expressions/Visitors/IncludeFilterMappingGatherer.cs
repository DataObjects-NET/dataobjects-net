// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.11.16

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Rse;
using Xtensive.Tuples;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal sealed class IncludeFilterMappingGatherer : ExtendedExpressionVisitor
  {
    public sealed class MappingEntry
    {
      public int ColumnIndex { get; private set; }

      public LambdaExpression CalculatedColumn { get; private set; }

      public MappingEntry(int columnIndex)
      {
        ColumnIndex = columnIndex;
      }

      public MappingEntry(LambdaExpression calculatedColumn)
      {
        ColumnIndex = -1;
        CalculatedColumn = calculatedColumn;
      }
    }

    private static readonly ParameterExpression calculatedColumnParameter = Expression.Parameter(WellKnownOrmTypes.Tuple, "filteredRow");

    private readonly Expression filterDataTuple;
    private readonly ApplyParameter filteredTuple;
    private readonly MappingEntry[] resultMapping;

    public static MappingEntry[] Gather(Expression filterExpression, Expression filterDataTuple, ApplyParameter filteredTuple, int columnCount)
    {
      var mapping = Enumerable.Repeat((MappingEntry) null, columnCount).ToArray();
      var visitor = new IncludeFilterMappingGatherer(filterDataTuple, filteredTuple, mapping);
      _ = visitor.Visit(filterExpression);
      if (mapping.Contains(null)) {
        throw Exceptions.InternalError("Failed to gather mappings for IncludeProvider", OrmLog.Instance);
      }
      return mapping;
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      var result = (BinaryExpression) base.VisitBinary(b);
      var expressions = new[] {result.Left, result.Right};

      var filterDataAccessor = expressions.FirstOrDefault(e => {
        var tupleAccess = e.StripCasts().AsTupleAccess();
        return tupleAccess != null && tupleAccess.Object == filterDataTuple;
      });
      if (filterDataAccessor == null) {
        return result;
      }

      var filteredExpression = expressions.FirstOrDefault(e => e!=filterDataAccessor);
      if (filteredExpression == null) {
        return result;
      }

      var filterDataIndex = filterDataAccessor.StripCasts().GetTupleAccessArgument();
      if (resultMapping.Length <= filterDataIndex) {
        return result;
      }
      resultMapping[filterDataIndex] = CreateMappingEntry(filteredExpression);
      return result;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      var target = m.Expression;
      if (target == null) {
        return base.VisitMemberAccess(m);
      }

      if (target.NodeType == ExpressionType.Constant && ((ConstantExpression) target).Value == filteredTuple) {
        return calculatedColumnParameter;
      }
      return base.VisitMemberAccess(m);
    }

    private MappingEntry CreateMappingEntry(Expression expression)
    {
      var tupleAccess = expression.StripCasts().AsTupleAccess();
      if (tupleAccess != null) {
        return new MappingEntry(tupleAccess.GetTupleAccessArgument());
      }
      expression = ExpressionReplacer.Replace(expression, filterDataTuple, calculatedColumnParameter);
      return new MappingEntry(FastExpression.Lambda(expression, calculatedColumnParameter));
    }

    private IncludeFilterMappingGatherer(Expression filterDataTuple, ApplyParameter filteredTuple, MappingEntry[] resultMapping)
    {
      this.filterDataTuple = filterDataTuple;
      this.filteredTuple = filteredTuple;
      this.resultMapping = resultMapping;
    }
  }
}
