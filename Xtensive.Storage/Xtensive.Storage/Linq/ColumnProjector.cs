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
    private readonly TranslatorContext context;
    private List<int> projectedColumns;
    private List<CalculatedColumnDescriptor> calculatedColumns;

    public IEnumerable<int> GetColumns(LambdaExpression le)
    {
      projectedColumns = new List<int>();
      calculatedColumns = new List<CalculatedColumnDescriptor>();
      Visit(le.Body);
      var source = context.GetBound(le.Parameters[0]);
      var recordSet = calculatedColumns.Count > 0 ? 
        source.RecordSet.Calculate(calculatedColumns.ToArray()) : 
        source.RecordSet;
      var result = new ResultExpression(source.Type, recordSet, source.Mapping, source.Projector, source.ItemProjector);
      context.ReplaceBound(le.Parameters[0], result);
      var ccIndex = source.RecordSet.Header.Columns.Count;
      return projectedColumns.Select(pc => pc == int.MinValue ? ccIndex++ : pc).ToList();
    }

    protected override Expression Visit(Expression e)
    {
      var path = MemberPath.Parse(e, context.Model);
      if (path.IsValid) {
        var source = context.GetBound(path.Parameter);
        var segment = source.GetMemberSegment(path);
        projectedColumns.AddRange(Enumerable.Range(segment.Offset, segment.Length));
      }
      else {
        if (e.NodeType == ExpressionType.New)
          return VisitNew((NewExpression) e);
        // Calculated column processing
        LambdaExpression le = context.MemberAccessReplacer.ProcessCalculated(e);
        var ccd = new CalculatedColumnDescriptor(context.GetNextAlias(), e.Type, (Expression<Func<Tuple, object>>) le);
        projectedColumns.Add(int.MinValue); // calculated column placeholder
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
      
    public ColumnProjector(TranslatorContext context)
    {
      this.context = context;
    }
  }
}