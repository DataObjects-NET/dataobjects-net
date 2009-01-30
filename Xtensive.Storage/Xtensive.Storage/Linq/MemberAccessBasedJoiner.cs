// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.19

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Model;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal class MemberAccessBasedJoiner : ExpressionVisitor
  {
    private readonly QueryTranslator translator;
    private ResultExpression currentResult;
    private bool joinFinalEntity;

    public ResultExpression Process(ResultExpression source, Expression e)
    {
      return Process(source, e, false);
    }

    public ResultExpression Process(ResultExpression source, Expression e, bool joinFinalEntity)
    {
      this.joinFinalEntity = joinFinalEntity;
      currentResult = source;
      Visit(e);
      return currentResult;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (!translator.Evaluator.CanBeEvaluated(m)) {
        var path = MemberPath.Parse(m, translator.Model);
        var mapping = currentResult.Mapping;
        int number = 0;
        foreach (var item in path) {
          number++;
          if (item.Type == MemberType.Entity && (joinFinalEntity || number != path.Count)) {
            ResultMapping innerMapping;
            var name = item.Name;
            var typeInfo = translator.Model.Types[item.Expression.Type];
            if (!mapping.JoinedRelations.TryGetValue(name, out innerMapping)) {
              var joinedIndex = typeInfo.Indexes.PrimaryIndex;
              var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(translator.GetNextAlias());
              var keySegment = mapping.Fields[name];
              var keyPairs =
                Enumerable.Range(keySegment.Offset, keySegment.Length).Select(
                  (leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex)).ToArray();
              var rs = currentResult.RecordSet.Join(joinedRs, JoinType.Default, keyPairs);
              var fieldMapping = translator.BuildFieldMapping(typeInfo, currentResult.RecordSet.Header.Columns.Count);
              var joinedMapping = new ResultMapping(fieldMapping, new Dictionary<string, ResultMapping>());
              mapping.JoinedRelations.Add(name, joinedMapping);

              currentResult = new ResultExpression(currentResult.Type, rs, currentResult.Mapping,
                                                   currentResult.Projector);
            }
            mapping = innerMapping;
          }
        }
      }
      return m;
    }

   
    // Constructor

    public MemberAccessBasedJoiner(QueryTranslator translator)
    {
      this.translator = translator;
    }
  }
}