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
using Xtensive.Storage.Model;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal class MemberAccessBasedJoiner : MemberPathVisitor
  {
    private readonly TranslatorContext context;
    private bool joinFinalEntity;

    public void Process(Expression e)
    {
      Process(e, false);
    }

    public void Process(Expression e, bool joinFinalEntity)
    {
      this.joinFinalEntity = joinFinalEntity;
      Visit(e);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (context.Evaluator.CanBeEvaluated(m))
        return m;
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitMemberPath(MemberPathExpression mpe)
    {
      var path = mpe.Path;
      var pe = path.Parameter;
      var source = context.GetBound(pe);
      var mapping = source.Mapping;
      int number = 0;
      foreach (var item in path) {
        number++;
        if (item.Type == MemberType.Entity && (joinFinalEntity || number != path.Count)) {
          ResultMapping innerMapping;
          var name = item.Name;
          var typeInfo = context.Model.Types[item.Expression.Type];
          if (!mapping.JoinedRelations.TryGetValue(name, out innerMapping)) {
            var joinedIndex = typeInfo.Indexes.PrimaryIndex;
            var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
            var keySegment = mapping.Fields[name];
            var keyPairs =
              Enumerable.Range(keySegment.Offset, keySegment.Length).Select(
                (leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex)).ToArray();
            var rs = source.RecordSet.Join(joinedRs, JoinType.Default, keyPairs);
            var fieldMapping = Translator.BuildFieldMapping(typeInfo, source.RecordSet.Header.Columns.Count);
            var joinedMapping = new ResultMapping(fieldMapping, new Dictionary<string, ResultMapping>());
            mapping.JoinedRelations.Add(name, joinedMapping);

            source = new ResultExpression(source.Type, rs, source.Mapping, source.Projector, source.ItemProjector);
            context.ReplaceBound(pe, source);
          }
          mapping = innerMapping;
        }
      }
      return mpe.Expression;
    }


    // Constructor

    public MemberAccessBasedJoiner(TranslatorContext context)
      : base (context.Model)
    {
      this.context = context;
    }
  }
}