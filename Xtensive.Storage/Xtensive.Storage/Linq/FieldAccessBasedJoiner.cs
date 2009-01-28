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
  internal class FieldAccessBasedJoiner : ExpressionVisitor
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
        var typesStack = new Stack<TypeInfo>();
        var typesPath = new Stack<Pair<TypeInfo, string>>();
        string fieldName = null;
        string lastFieldName = null;
        Expression expression = m;
        if (typeof(Key).IsAssignableFrom(m.Type))
          expression = ((MemberExpression) expression).Expression;
        while (expression.NodeType==ExpressionType.MemberAccess) {
          var memberAccess = (MemberExpression) expression;
          var member = (PropertyInfo) memberAccess.Member;
          expression = memberAccess.Expression;
          if (fieldName == null)
            fieldName = member.Name;
          else
            fieldName = member.Name + "." + fieldName;
          if (expression.NodeType==ExpressionType.MemberAccess) {
            if (typeof (IEntity).IsAssignableFrom(expression.Type)) {
              var type = translator.Model.Types[expression.Type];
              var field = type.Fields[fieldName];
              if(!field.IsPrimaryKey)
                typesStack.Push(type);
              if (lastFieldName == null)
                lastFieldName = fieldName;
              else {
                typesPath.Push(new Pair<TypeInfo, string>(typesStack.Peek(),fieldName));
              }
              fieldName = null;
            }
          }
        }
        if (typesStack.Count > 0)
          typesPath.Push(new Pair<TypeInfo, string>(typesStack.Peek(), fieldName));
        List<Pair<TypeInfo, string>> list = typesPath.ToList();
        var mapping = currentResult.Mapping;
        foreach (var pair in list) {
          ResultMapping innerMapping;
          if(!mapping.JoinedRelations.TryGetValue(pair.Second, out innerMapping)) {
            var joinedIndex = pair.First.Indexes.PrimaryIndex;
            var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(translator.GetNextAlias());
            var keySegment = mapping.Fields[pair.Second];
            var keyPairs = Enumerable.Range(keySegment.Offset, keySegment.Length).Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex)).ToArray();
            var rs = currentResult.RecordSet.Join(joinedRs, JoinType.Default, keyPairs);
            var fieldMapping = translator.BuildFieldMapping(pair.First, currentResult.RecordSet.Header.Columns.Count);
            var joinedMapping = new ResultMapping(fieldMapping, new Dictionary<string, ResultMapping>());
            mapping.JoinedRelations.Add(pair.Second, joinedMapping);
            
            currentResult = new ResultExpression(currentResult.Type, rs, currentResult.Mapping, currentResult.Projector);
          }
          mapping = innerMapping;
        }
      }
      return m;
    }

   
    // Constructor

    public FieldAccessBasedJoiner(QueryTranslator translator)
    {
      this.translator = translator;
    }
  }
}