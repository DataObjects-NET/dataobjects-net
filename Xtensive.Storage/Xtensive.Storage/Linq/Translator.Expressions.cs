// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using FieldInfo=System.Reflection.FieldInfo;

namespace Xtensive.Storage.Linq
{
  internal partial class Translator
  {
    private const string SurrogateKeyNameFormatString = "#_Key_{0}";

    private readonly Parameter<List<CalculatedColumnDescriptor>> calculatedColumns = new Parameter<List<CalculatedColumnDescriptor>>("calculatedColumns");
    private readonly Parameter<ParameterExpression[]> parameters = new Parameter<ParameterExpression[]>("parameters");
    private readonly Parameter<ResultMapping> resultMapping = new Parameter<ResultMapping>("resultMapping");
    private readonly Parameter<ParameterExpression> tuple = new Parameter<ParameterExpression>("tuple");
    private readonly Parameter<ParameterExpression> record = new Parameter<ParameterExpression>("record");
    private readonly Parameter<bool> joinFinalEntity = new Parameter<bool>("joinFinalEntity");
    private readonly Parameter<bool> calculateExpressions = new Parameter<bool>("calculateExpressions");
    private readonly Parameter<bool> recordIsUsed;
    private readonly Parameter<bool> ignoreRecordUsage = new Parameter<bool>("ignoreRecordUsage");

    protected override Expression Visit(Expression e)
    {
      if (e==null)
        return null;
      if (((ExtendedExpressionType) e.NodeType)==ExtendedExpressionType.Result)
        return e;
      if (context.Evaluator.CanBeEvaluated(e)) {
        if (context.ParameterExtractor.IsParameter(e))
          return e;
        return context.Evaluator.Evaluate(e);
      }
      return base.Visit(e);
    }

    protected override Expression VisitLambda(LambdaExpression le)
    {
      LambdaExpression result;
      using (new ParameterScope()) {
        recordIsUsed.Value = false;
        tuple.Value = Expression.Parameter(typeof (Tuple), "t");
        record.Value = Expression.Parameter(typeof (Record), "r");
        parameters.Value = le.Parameters.ToArray();
        calculatedColumns.Value = new List<CalculatedColumnDescriptor>();
        Expression body;
        using (context.SubqueryParameterBindings.Bind(parameters.Value)) {
          body = Visit(le.Body);
        }
        if (calculateExpressions.Value && body.GetMemberType()==MemberType.Unknown) {
          if (
            ((ExtendedExpressionType) body.NodeType)!=ExtendedExpressionType.Result &&
              (body.NodeType!=ExpressionType.Call ||
                ((MethodCallExpression) body).Object==null ||
                  ((MethodCallExpression) body).Object.Type!=typeof (Tuple))) {
            var calculator = Expression.Lambda(Expression.Convert(body, typeof (object)), tuple.Value);
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), body.Type, (Expression<Func<Tuple, object>>) calculator);
            calculatedColumns.Value.Add(ccd);
            var parameter = parameters.Value[0];
            int position = context.Bindings[parameter].RecordSet.Header.Length + calculatedColumns.Value.Count - 1;
            body = MakeTupleAccess(parameter, body.Type, Expression.Constant(position));
            resultMapping.Value.RegisterPrimitive(new Segment<int>(position, 1));
          }
        }
        if (calculatedColumns.Value.Count > 0) {
          var source = context.Bindings[le.Parameters[0]];
          var recordSet = source.RecordSet;
          recordSet = recordSet.Calculate(calculatedColumns.Value.ToArray());
          var re = new ResultExpression(source.Type, recordSet, source.Mapping, source.Projector, source.ItemProjector);
          context.Bindings.ReplaceBound(le.Parameters[0], re);
        }
        result = recordIsUsed.Value && !ignoreRecordUsage.Value
          ? Expression.Lambda(
            typeof (Func<,,>).MakeGenericType(typeof (Tuple), typeof (Record), body.Type),
            body,
            tuple.Value,
            record.Value)
          : Expression.Lambda(body, tuple.Value);
      }
      return result;
    }

    protected override Expression VisitMemberPath(MemberPath path, Expression e)
    {
      var pe = path.Parameter;
      if (!parameters.Value.Contains(pe)) {
        var referencedSource = context.Bindings[pe];
        return path.TranslateParameter(referencedSource.ItemProjector.Body);
      }
      var source = context.Bindings[pe];
      var mapping = source.Mapping;
      int number = 0;
      foreach (var item in path) {
        number++;
        if (item.Type==MemberType.Entity && (joinFinalEntity.Value || number!=path.Count)) {
          ResultMapping innerMapping;
          var name = item.Name;
          var typeInfo = context.Model.Types[item.Expression.Type];
          if (!mapping.JoinedRelations.TryGetValue(name, out innerMapping)) {
            var joinedIndex = typeInfo.Indexes.PrimaryIndex;
            var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
            var keySegment = mapping.Fields[name];
            var keyPairs = keySegment.GetItems()
              .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
              .ToArray();
            var rs = source.RecordSet.Join(joinedRs, JoinType.Default, keyPairs);
            var fieldMapping = BuildFieldMapping(typeInfo, source.RecordSet.Header.Columns.Count);
            var joinedMapping = new ResultMapping(fieldMapping, new Dictionary<string, ResultMapping>());
            mapping.JoinedRelations.Add(name, joinedMapping);

            source = new ResultExpression(source.Type, rs, source.Mapping, source.Projector, source.ItemProjector);
            context.Bindings.ReplaceBound(pe, source);
          }
          mapping = innerMapping;
        }
      }

      var resultType = e.Type;
      source = context.Bindings[path.Parameter];
      switch (path.PathType) {
        case MemberType.Primitive:
          return VisitMemberPathPrimitive(path, source, resultType);
        case MemberType.Key:
          return VisitMemberPathKey(path, source);
        case MemberType.Structure:
          return VisitMemberPathStructure(path, source);
        case MemberType.Entity:
          if (joinFinalEntity.Value)
            return VisitMemberPathEntity(path, source, resultType);
          path = MemberPath.Parse(Expression.MakeMemberAccess(e, WellKnownMethods.IEntityKey), context.Model);
          var keyExpression = VisitMemberPathKey(path, source);
          var result = Expression.Convert(
            Expression.Call(
              keyExpression,
              WellKnownMethods.KeyResolve), resultType);
          return result;
        case MemberType.EntitySet:
          return VisitMemberPathEntitySet(e);
        case MemberType.Anonymous:
          return VisitMemberPathAnonymous(path, source);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      return base.VisitMethodCall(mc);
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      return base.VisitUnary(u);
    }

    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
      switch (binaryExpression.Left.GetMemberType()) {
        case MemberType.Unknown:
        case MemberType.Primitive:
          break;
        case MemberType.Key:
          return VisitBinaryKey(binaryExpression);
        case MemberType.Entity:
          return VisitBinaryEntity(binaryExpression);
        case MemberType.Anonymous:
          return VisitBinaryAnonymous(binaryExpression);
        case MemberType.Structure:
          return VisitBinaryStructure(binaryExpression);
        case MemberType.EntitySet:
          throw new NotSupportedException();
        default:
          throw new ArgumentOutOfRangeException();
      }
      return base.VisitBinary(binaryExpression);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      if (!parameters.Value.Contains(p))
        throw new InvalidOperationException("Lambda parameter is out of scope!");
      var source = context.Bindings[p];
      resultMapping.Value.Replace(source.Mapping);
      var parameterRewriter = new ParameterRewriter(tuple.Value, record.Value);
      var result = parameterRewriter.Rewrite(source.ItemProjector.Body);
      recordIsUsed.Value |= result.Second;
      return result.First;
    }

    protected override Expression VisitMemberAccess(MemberExpression ma)
    {
      if (context.Evaluator.CanBeEvaluated(ma) && context.ParameterExtractor.IsParameter(ma))
        return ma;
      if (ma.Expression==null) {
        if (typeof (IQueryable).IsAssignableFrom(ma.Type)) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).Compile();
          var rootPoint = lambda();
          if (rootPoint!=null)
            return ConstructQueryable(rootPoint);
        }
      }
      else if (ma.Expression.NodeType==ExpressionType.Constant) {
        var rfi = ma.Member as FieldInfo;
        if (rfi!=null && (rfi.FieldType.IsGenericType && typeof (IQueryable).IsAssignableFrom(rfi.FieldType))) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).Compile();
          var rootPoint = lambda();
          if (rootPoint!=null)
            return ConstructQueryable(rootPoint);
        }
      }
      else if (ma.Expression.NodeType==ExpressionType.New && ma.Expression.GetMemberType()==MemberType.Anonymous) {
        var name = ma.Member.Name;
        var newExpression = (NewExpression) ma.Expression;
        var propertyInfo = newExpression.Type.GetProperty(name);
        var memberName = propertyInfo.GetGetMethod().Name;
        var member = newExpression.Members.First(m => m.Name==memberName);
        var argument = newExpression.Arguments[newExpression.Members.IndexOf(member)];
        return Visit(argument);
      }
      return base.VisitMemberAccess(ma);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      var arguments = new List<Expression>();
      if (n.Members==null)
        return base.VisitNew(n);
      for (int i = 0; i < n.Arguments.Count; i++) {
        var arg = n.Arguments[i];
        Expression newArg;
//        Expression argumentResolver = null;
        var member = n.Members[i];
        var memberName = member.Name.TryCutPrefix(WellKnown.GetterPrefix);
        Func<string, string> rename = key => key.IsNullOrEmpty()
          ? memberName
          : memberName + "." + key;
        var path = MemberPath.Parse(arg, context.Model);
        if (path.IsValid || arg.NodeType==ExpressionType.New) {
          ResultMapping rm;
          using (new ParameterScope()) {
            resultMapping.Value = new ResultMapping();
            newArg = Visit(arg);
            rm = resultMapping.Value;
//            if (newArg.Type==typeof(Key) && arg.Type.IsSubclassOf(typeof(Entity))) {
//              var argumentResolveMethod = WellKnownMethods.KeyResolveOfT.MakeGenericMethod(arg.Type);
//              argumentResolver = Expression.Call(newArg, argumentResolveMethod);
//            }
          }
          if (rm.MapsToPrimitive)
            resultMapping.Value.RegisterFieldMapping(memberName, rm.Segment);
          else {
            foreach (var p in rm.Fields)
              resultMapping.Value.RegisterFieldMapping(rename(p.Key), p.Value);
            foreach (var p in rm.JoinedRelations)
              resultMapping.Value.RegisterJoined(rename(p.Key), p.Value);
            foreach (var p in rm.AnonymousProjections)
              resultMapping.Value.RegisterAnonymous(rename(p.Key), p.Value);
            var memberType = arg.GetMemberType();
            if (memberType==MemberType.Anonymous || memberType==MemberType.Entity) {
              resultMapping.Value.RegisterJoined(memberName, rm);
              if (memberType==MemberType.Anonymous)
                resultMapping.Value.RegisterAnonymous(memberName, newArg);
            }
          }
        }
        else {
          // TODO: Add check of queries
          Expression body;
          using (new ParameterScope()) {
            calculateExpressions.Value = false;
            resultMapping.Value = new ResultMapping();
            body = Visit(arg);
          }
          if (body.AsTupleAccess() != null)
            newArg = body;
          else if (((ExtendedExpressionType) body.NodeType)==ExtendedExpressionType.Result) {
            var outerParameters = context.Bindings.GetKeys()
              .OfType<ParameterExpression>()
              .ToList();
            if (outerParameters.Count==0)
              newArg = arg;
            else {
              var searchFor = outerParameters.ToArray();
              var replaceWithList = new List<Expression>();
              foreach (var projection in outerParameters.Select(pe => context.Bindings[pe].ItemProjector)) {
                recordIsUsed.Value |= projection.Parameters.Count(pe => pe.Type==typeof (Record)) > 0;
                var replacedParameters = projection.Parameters.ToArray();
                var replacingParameters = projection.Parameters.Select(pe => pe.Type==typeof (Tuple) ? tuple.Value : record.Value).ToArray();
                replaceWithList.Add(ExpressionReplacer.ReplaceAll(projection.Body, replacedParameters, replacingParameters));
              }
              newArg = ExpressionReplacer.ReplaceAll(arg, searchFor, replaceWithList.ToArray());
            }
          }
          else {
            var calculator = Expression.Lambda(
              body.Type==typeof (object)
                ? body
                : Expression.Convert(body, typeof (object)),
              tuple.Value);
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), arg.Type, (Expression<Func<Tuple, object>>) calculator);
            calculatedColumns.Value.Add(ccd);
            var parameter = parameters.Value[0];
            int position = context.Bindings[parameter].RecordSet.Header.Length + calculatedColumns.Value.Count - 1;
            newArg = MakeTupleAccess(parameter, arg.Type, Expression.Constant(position));
            resultMapping.Value.RegisterFieldMapping(memberName, new Segment<int>(position, 1));
          }
        }
        newArg = newArg ?? Visit(arg);
        arguments.Add( /*argumentResolver ?? */newArg);
      }
      return Expression.New(n.Constructor, arguments, n.Members);
    }

    #region Private helper methods

    private Expression ConstructQueryable(IQueryable rootPoint)
    {
      var elementType = rootPoint.ElementType;
      var type = context.Model.Types[elementType];
      var index = type.Indexes.PrimaryIndex;

      var fieldMapping = BuildFieldMapping(type, 0);
      var mapping = new ResultMapping(fieldMapping, new Dictionary<string, ResultMapping>());
      var recordSet = IndexProvider.Get(index).Result;
      Expression<Func<RecordSet, object>> projector = rs => rs.Parse().Select(r => r[0].Resolve());
      Expression<Func<Record, Entity>> ipt = r => r[0].Resolve();
      LambdaExpression itemProjector = Expression.Lambda(Expression.Convert(ipt.Body, elementType), ipt.Parameters[0]);

      return new ResultExpression(
        typeof (IQueryable<>).MakeGenericType(elementType),
        recordSet,
        mapping,
        projector,
        itemProjector);
    }

    private static IEnumerable<TResult> MakeProjection<TResult>(RecordSet rs, Expression<Func<Tuple, Record, TResult>> le)
    {
      var func = le.Compile();
      foreach (var r in rs.Parse())
        yield return func(r.Data, r);
    }

    private static Expression MakeBinaryExpression(Expression previous, Expression left, Expression right, ExpressionType operationType)
    {
      if (previous==null) {
        previous = operationType==ExpressionType.Equal
          ? Expression.Equal(left, right)
          : Expression.NotEqual(left, right);
      }
      else {
        previous = operationType==ExpressionType.Equal
          ? Expression.AndAlso(previous, Expression.Equal(left, right))
          : Expression.AndAlso(previous, Expression.NotEqual(left, right));
      }
      return previous;
    }

    private Expression MakeComplexBinaryExpression(Expression bLeft, Expression bRight, ExpressionType operationType)
    {
      Expression result = null;
      if (bLeft.NodeType==ExpressionType.Constant || bRight.NodeType==ExpressionType.Constant) {
        var constant = bLeft.NodeType==ExpressionType.Constant
          ? (ConstantExpression) bLeft
          : (ConstantExpression) bRight;
        var member = bLeft.NodeType!=ExpressionType.Constant
          ? bLeft
          : bRight;
        if (constant.Value==null) {
          var path = MemberPath.Parse(member, context.Model);
          var source = context.Bindings[path.Parameter];
          var segment = source.GetMemberSegment(path);
          foreach (var i in segment.GetItems()) {
            Expression left = MakeTupleAccess(path.Parameter, null, Expression.Constant(i));
            Expression right = Expression.Constant(null);
            result = MakeBinaryExpression(result, left, right, operationType);
          }
          return result;
        }
      }
      var leftPath = MemberPath.Parse(bLeft, context.Model);
      var leftSource = context.Bindings[leftPath.Parameter];
      var leftSegment = leftSource.GetMemberSegment(leftPath);
      var rightPath = MemberPath.Parse(bRight, context.Model);
      var rightSource = context.Bindings[rightPath.Parameter];
      var rightSegment = rightSource.GetMemberSegment(rightPath);
      foreach (var pair in leftSegment.GetItems().ZipWith(rightSegment.GetItems(), (l, r) => new {l, r})) {
        var type = leftSource.RecordSet.Header.TupleDescriptor[pair.l];
        Expression left = MakeTupleAccess(leftPath.Parameter, type, Expression.Constant(pair.l));
        Expression right = MakeTupleAccess(rightPath.Parameter, type, Expression.Constant(pair.r));
        result = MakeBinaryExpression(result, left, right, operationType);
      }
      return result;
    }

    private MethodCallExpression MakeTupleAccess(ParameterExpression parameter, Type accessorType, Expression index)
    {
      Parameter<Tuple> outerParameter;
      Expression target;

      if (parameters.Value.Contains(parameter))
        target = tuple.Value;
      else if (context.SubqueryParameterBindings.TryGetBound(parameter, out outerParameter))
        target = Expression.Property(Expression.Constant(outerParameter), WellKnownMethods.ParameterOfTupleValue);
      else
        throw new InvalidOperationException();

      var method = accessorType==null || accessorType==typeof (object)
        ? WellKnownMethods.TupleNonGenericAccessor
        : WellKnownMethods.TupleGenericAccessor.MakeGenericMethod(accessorType);

      return Expression.Call(target, method, index);
    }

    #endregion

    #region VisitBinary implementations

    private Expression VisitBinaryKey(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException();

      bool leftIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Left);
      bool rightIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Right);

      if (!leftIsParameter && !rightIsParameter)
        return MakeComplexBinaryExpression(binaryExpression.Left, binaryExpression.Right, binaryExpression.NodeType);

      var bLeft = binaryExpression.Left;
      var bRight = binaryExpression.Right;
      if (leftIsParameter) {
        bLeft = binaryExpression.Right;
        bRight = binaryExpression.Left;
      }

      var path = MemberPath.Parse(bLeft, context.Model);
      if (!parameters.Value.Contains(path.Parameter))
        throw new NotSupportedException();

      var source = context.Bindings[path.Parameter];
      var segment = source.GetMemberSegment(path);
      Expression result = null;
      foreach (var pair in segment.GetItems().Select((ci, pi) => new {ColumnIndex = ci, ParameterIndex = pi})) {
        Expression left = MakeTupleAccess(path.Parameter, null, Expression.Constant(pair.ColumnIndex));
        Expression right = Expression.Condition(
          Expression.Equal(bRight, Expression.Constant(null, bRight.Type)),
          Expression.Constant(null, typeof (object)),
          Expression.Call(Expression.MakeMemberAccess(bRight, WellKnownMethods.KeyValue), WellKnownMethods.TupleNonGenericAccessor, Expression.Constant(pair.ParameterIndex)));
        result = MakeBinaryExpression(result, left, right, binaryExpression.NodeType);
      }
      return result;
    }

    private Expression VisitBinaryEntity(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException();

      bool leftIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Left);
      bool rightIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Right);

      if (!leftIsParameter && !rightIsParameter) {
        var bLeft = binaryExpression.Left.NodeType==ExpressionType.Constant && ((ConstantExpression) binaryExpression.Left).Value==null
          ? binaryExpression.Left
          : Expression.MakeMemberAccess(binaryExpression.Left, WellKnownMethods.IEntityKey);
        var bRight = binaryExpression.Right.NodeType==ExpressionType.Constant && ((ConstantExpression) binaryExpression.Right).Value==null
          ? binaryExpression.Right
          : Expression.MakeMemberAccess(binaryExpression.Right, WellKnownMethods.IEntityKey);
        return MakeComplexBinaryExpression(bLeft, bRight, binaryExpression.NodeType);
      }
      else {
        var bLeft = Expression.MakeMemberAccess(binaryExpression.Left, WellKnownMethods.IEntityKey);
        var bRight = binaryExpression.Right;
        if (leftIsParameter) {
          bLeft = Expression.MakeMemberAccess(binaryExpression.Right, WellKnownMethods.IEntityKey);
          bRight = binaryExpression.Left;
        }

        var path = MemberPath.Parse(bLeft, context.Model);
        if (!parameters.Value.Contains(path.Parameter))
          throw new NotSupportedException();

        var source = context.Bindings[path.Parameter];
        var segment = source.GetMemberSegment(path);

        Expression result = null;
        foreach (var pair in segment.GetItems().Select((ci, pi) => new {ColumnIndex = ci, ParameterIndex = pi})) {
          Expression left = MakeTupleAccess(path.Parameter, null, Expression.Constant(pair.ColumnIndex));
          Expression right = Expression.Condition(
            Expression.Equal(bRight, Expression.Constant(null, bRight.Type)),
            Expression.Constant(null, typeof (object)),
            Expression.Call(
              Expression.MakeMemberAccess(Expression.MakeMemberAccess(bRight, WellKnownMethods.IEntityKey), WellKnownMethods.KeyValue),
              WellKnownMethods.TupleNonGenericAccessor, Expression.Constant(pair.ParameterIndex)));
          result = MakeBinaryExpression(result, left, right, binaryExpression.NodeType);
        }
        return result;
      }
    }

    private Expression VisitBinaryAnonymous(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException();

      Expression leftExpression = binaryExpression.Left;
      Expression rightExpression = binaryExpression.Right;

      var properties = leftExpression.Type.GetProperties();
      Expression result = null;
      foreach (PropertyInfo propertyInfo in properties) {
        Expression left;
        string propertyName = propertyInfo.GetGetMethod().Name;
        if (leftExpression.NodeType == ExpressionType.New) {
          var newExpression = ((NewExpression) leftExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name==propertyName);
          int index = newExpression.Members.IndexOf(member);
          left = newExpression.Arguments[index];
        }
        else
          left = Expression.Property(leftExpression, propertyInfo);
        Expression right;
        if (rightExpression.NodeType == ExpressionType.New)
        {
          var newExpression = ((NewExpression)rightExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name == propertyName);
          int index = newExpression.Members.IndexOf(member);
          right = newExpression.Arguments[index];
        }
        else
          right = Expression.Property(leftExpression, propertyInfo);
        var expression = VisitBinary((BinaryExpression) MakeBinaryExpression(null, left, right, binaryExpression.NodeType));
        result = result==null ? expression : Expression.AndAlso(result, expression);
      }
      return result;
    }

    private Expression VisitBinaryStructure(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException();

      bool leftIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Left);
      bool rightIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Right);

      if (!leftIsParameter && !rightIsParameter)
        return MakeComplexBinaryExpression(binaryExpression.Left, binaryExpression.Right, binaryExpression.NodeType);


      Expression leftExpression = binaryExpression.Left;
      Expression rightExpression = binaryExpression.Right;

      var properties = leftExpression.Type.GetProperties();
      Expression result = null;
      foreach (PropertyInfo propertyInfo in properties)
      {
        if (!propertyInfo.DeclaringType.IsSubclassOf(typeof (Structure)))
          continue;
        Expression left;
        string propertyName = propertyInfo.GetGetMethod().Name;
        if (leftExpression.NodeType == ExpressionType.New)
        {
          var newExpression = ((NewExpression)leftExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name == propertyName);
          int index = newExpression.Members.IndexOf(member);
          left = newExpression.Arguments[index];
        }
        else
          left = Expression.Property(leftExpression, propertyInfo);
        Expression right;
        if (rightExpression.NodeType == ExpressionType.New)
        {
          var newExpression = ((NewExpression)rightExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name == propertyName);
          int index = newExpression.Members.IndexOf(member);
          right = newExpression.Arguments[index];
        }
        else
          right = Expression.Property(leftExpression, propertyInfo);
        var expression = VisitBinary((BinaryExpression)MakeBinaryExpression(null, left, right, binaryExpression.NodeType));
        result = result == null ? expression : Expression.AndAlso(result, expression);
      }
      return result;
    }

    #endregion

    #region VisitMemberPathImplementation

    private Expression VisitMemberPathEntitySet(Expression e)
    {
      recordIsUsed.Value = true;
      var m = (MemberExpression) e;
      var expression = Visit(m.Expression);
      var result = Expression.MakeMemberAccess(expression, m.Member);
      return result;
    }

    private Expression VisitMemberPathEntity(MemberPath path, ResultExpression source, Type resultType)
    {
      recordIsUsed.Value = true;
      var segment = source.GetMemberSegment(path);
      int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
      var result = Expression.Convert(
        Expression.Call(
          Expression.Call(record.Value, WellKnownMethods.RecordKey, Expression.Constant(groupIndex)),
          WellKnownMethods.KeyResolve), resultType);
      var rm = source.GetMemberMapping(path);
      var name = rm.Fields.Select(pair => pair.Key).OrderBy(s => s.Length).First();
      foreach (var pair in rm.Fields) {
        var key = pair.Key.TryCutPrefix(name).TrimStart('.');
        resultMapping.Value.RegisterFieldMapping(key, pair.Value);
      }
      foreach (var pair in rm.JoinedRelations) {
        var key = pair.Key.TryCutPrefix(name).TrimStart('.');
        resultMapping.Value.RegisterJoined(key, pair.Value);
      }
      resultMapping.Value.RegisterJoined(string.Empty, rm);
      return result;
    }

    private Expression VisitMemberPathStructure(MemberPath path, ResultExpression source)
    {
      recordIsUsed.Value = true;
      var segment = source.GetMemberSegment(path);
      var structureColumn = (MappedColumn) source.RecordSet.Header.Columns[segment.Offset];
      var field = structureColumn.ColumnInfoRef.Resolve(context.Model).Field;
      while (field.Parent!=null)
        field = field.Parent;
      int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
      var result =
        Expression.MakeMemberAccess(
          Expression.Convert(
            Expression.Call(
              Expression.Call(record.Value, WellKnownMethods.RecordKey, Expression.Constant(groupIndex)),
              WellKnownMethods.KeyResolve),
            field.ReflectedType.UnderlyingType),
          field.UnderlyingProperty);
      var columnGroup = source.RecordSet.Header.ColumnGroups[groupIndex];
      var keyOffset = columnGroup.Keys.Min();
      var keyLength = columnGroup.Keys.Max() - keyOffset + 1;
      var rm = source.GetMemberMapping(path);
      var mappedFields = rm.Fields.Where(p => (p.Value.Offset >= segment.Offset && p.Value.EndOffset <= segment.EndOffset)).ToList();
      var name = mappedFields.Select(pair => pair.Key).OrderBy(s => s.Length).First();
      foreach (var pair in mappedFields) {
        var key = pair.Key.TryCutPrefix(name).TrimStart('.');
        resultMapping.Value.RegisterFieldMapping(key, pair.Value);
      }
      resultMapping.Value.RegisterFieldMapping(string.Format(SurrogateKeyNameFormatString, groupIndex), new Segment<int>(keyOffset, keyLength));
      return result;
    }

    private Expression VisitMemberPathAnonymous(MemberPath path, ResultExpression source)
    {
      var rm = source.GetMemberMapping(path);
      resultMapping.Value.RegisterJoined(string.Empty, rm);
      if (path.Count==0)
        return VisitParameter(path.Parameter);
      var projector = source.Mapping.AnonymousProjections[path.First().Name];
      var parameterRewriter = new ParameterRewriter(tuple.Value, record.Value);
      var result = parameterRewriter.Rewrite(projector);
      recordIsUsed.Value |= result.Second;
      return result.First;
    }

    private Expression VisitMemberPathPrimitive(MemberPath path, ResultExpression source, Type resultType)
    {
      var segment = source.GetMemberSegment(path);
      resultMapping.Value.RegisterPrimitive(segment);
      return MakeTupleAccess(path.Parameter, resultType, Expression.Constant(segment.Offset));
    }

    private Expression VisitMemberPathKey(MemberPath path, ResultExpression source)
    {
      Segment<int> segment = source.GetMemberSegment(path);
      var keyColumn = (MappedColumn) source.RecordSet.Header.Columns[segment.Offset];
      var field = keyColumn.ColumnInfoRef.Resolve(context.Model).Field;
      var type = field.Parent==null ? field.ReflectedType : context.Model.Types[field.Parent.ValueType];
      var transform = new SegmentTransform(true, field.ReflectedType.TupleDescriptor, segment);
      var keyExtractor = Expression.Call(WellKnownMethods.KeyCreate, Expression.Constant(type),
        Expression.Call(Expression.Constant(transform), WellKnownMethods.SegmentTransformApply,
          Expression.Constant(TupleTransformType.Auto), tuple.Value),
        Expression.Constant(false));
      resultMapping.Value.RegisterPrimitive(segment);
      return keyExtractor;
    }

    #endregion
  }
}