// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Linq.Linq2Rse.Internal;
using Xtensive.Storage.Model;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class FieldAccessFlattener : ExpressionVisitor
  {
    private readonly RseQueryTranslator translator;
    private ProjectionExpression projection;


    public ProjectionExpression FlattenFieldAccess(ProjectionExpression source, LambdaExpression le)
    {
      projection = source;
      Visit(le);
      return projection;
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
        var parameterType = translator.Model.Types[expression.Type];
        if (typesStack.Count > 0)
          typesPath.Push(new Pair<TypeInfo, string>(typesStack.Peek(), fieldName));
        List<Pair<TypeInfo, string>> list = typesPath.ToList();
        var mapping = projection.Mappings[parameterType];
        foreach (var pair in list) {
          TypeMapping innerMapping;
          if(!mapping.JoinedRelations.TryGetValue(pair.Second, out innerMapping)) {
            var joinedIndex = pair.First.Indexes.PrimaryIndex;
            var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(translator.GetNextAlias());
            var keyPairs = pair.First.Hierarchy.KeyFields.Select((kf,i) => new Pair<int>(mapping.FieldMapping[pair.Second + "." + kf.Key], i)).ToArray();
            var rs = projection.RecordSet.Join(joinedRs, JoinType.Default, keyPairs);
            var fieldMapping = new Dictionary<string, int>();
            var joinedMapping = new TypeMapping(pair.First, fieldMapping, new Dictionary<string, TypeMapping>());
            mapping.JoinedRelations.Add(pair.Second, joinedMapping);
            foreach (var column in joinedRs.Header.Columns) {
              var mapped = column as MappedColumn;
              if (mapped != null)
                fieldMapping.Add(mapped.ColumnInfoRef.FieldName, mapped.Index + projection.RecordSet.Header.Columns.Count);
            }
            projection = new ProjectionExpression(projection.Type, rs, projection.Mappings, projection.Projector);

          }
          mapping = innerMapping;
        }
      }
      return m;
    }

   
    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FieldAccessFlattener(RseQueryTranslator translator)
    {
      this.translator = translator;
    }
  }
}