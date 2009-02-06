// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal sealed class ColumnProjector : ExpressionVisitor
  {
    private readonly QueryTranslator translator;
    private List<int> projectedColumns;
    private ResultExpression source;
    private List<CalculatedColumnDescriptor> calculatedColumns;

    public ResultExpression GetColumns(ResultExpression source, Expression projector, out IEnumerable<int> columnIndexes)
    {
      this.source = source;
      projectedColumns = new List<int>();
      calculatedColumns = new List<CalculatedColumnDescriptor>();
      Visit(projector);
      columnIndexes = projectedColumns;
      var recordSet = calculatedColumns.Count > 0 ? 
        source.RecordSet.Calculate(calculatedColumns.ToArray()) : 
        source.RecordSet;
      var result = new ResultExpression(source.Type, recordSet, source.Mapping, source.Projector, source.ItemProjector);
      return result;
    }

    protected override Expression Visit(Expression e)
    {
      var path = MemberPath.Parse(e, translator.Model);
      if (path.IsValid) {
        var segment = source.GetMemberSegment(path);
        projectedColumns.AddRange(Enumerable.Range(segment.Offset, segment.Length));
      }
      else {
        if (e.NodeType == ExpressionType.New)
          return VisitNew((NewExpression) e);
        // Calculated column processing
        LambdaExpression le = translator.MemberAccessReplacer.ProcessCalculated(source, e);
        var ccd = new CalculatedColumnDescriptor(translator.GetNextAlias(), e.Type, (Expression<Func<Tuple, object>>) le);
        projectedColumns.Add(source.Mapping.Segment.Length + calculatedColumns.Count);
        calculatedColumns.Add(ccd);
      }
      return e;
    }

    protected override Expression VisitNew(NewExpression n)
    {
      if (n.Members == null)
        throw new NotSupportedException();
      for (int i = 0; i < n.Arguments.Count; i++) {
        var arg = n.Arguments[i];
        Visit(arg);
      }
      return n;
    }


    // Constructors
      
    public ColumnProjector(QueryTranslator translator)
    {
      this.translator = translator;
    }
  }
}