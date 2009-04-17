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
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Mappings;
using Xtensive.Storage.Linq.Rewriters;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Expressions;
using Xtensive.Storage.Rse.Providers.Compilable;
using FieldInfo=System.Reflection.FieldInfo;

namespace Xtensive.Storage.Linq
{
  internal partial class Translator
  {
    private const string SurrogateKeyNameFormatString = "#_Key_{0}";

    private readonly Parameter<List<CalculatedColumnDescriptor>> calculatedColumns = new Parameter<List<CalculatedColumnDescriptor>>("calculatedColumns");
    private readonly Parameter<ParameterExpression[]> parameters = new Parameter<ParameterExpression[]>("parameters");
    private readonly Parameter<MappingReference> mappingRef = new Parameter<MappingReference>("mapping");
    private readonly Parameter<ParameterExpression> tuple = new Parameter<ParameterExpression>("tuple");
    private readonly Parameter<ParameterExpression> record = new Parameter<ParameterExpression>("record");
    private readonly Parameter<bool> entityAsKey = new Parameter<bool>("entityAsKey");
    private readonly Parameter<bool> calculateExpressions = new Parameter<bool>("calculateExpressions");
    private readonly Parameter<bool> recordIsUsedParameter;

    private bool recordIsUsed {
      get { return recordIsUsedParameter.Value; }
      set {
        if (value) {
          if (!entityAsKey.HasValue || !entityAsKey.Value)
            recordIsUsedParameter.Value = true;
        }
        else
          recordIsUsedParameter.Value = false;
      }
    }
    
    protected override Expression VisitTypeIs(TypeBinaryExpression tb)
    {
      var expressionType = tb.Expression.Type;
      var operandType = tb.TypeOperand;
      if (operandType.IsAssignableFrom(expressionType))
        return Expression.Constant(true);

      // Structure
      if (tb.Expression.GetMemberType()==MemberType.Structure
        && typeof (Structure).IsAssignableFrom(operandType))
        return Expression.Constant(false);

      // Entity
      if (tb.Expression.GetMemberType() == MemberType.Entity 
        && typeof (IEntity).IsAssignableFrom(operandType))
      {
        var typeInfo = context.Model.Types[operandType];
        var typeIds = typeInfo.GetDescendants().AddOne(typeInfo).Select(ti=>ti.TypeId);
        var memberExpression = Expression.Property(tb.Expression, "TypeId");
        Expression boolExpression = null;
        foreach (int typeId in typeIds)
          boolExpression = MakeBinaryExpression(boolExpression, memberExpression, Expression.Constant(typeId), ExpressionType.Equal, ExpressionType.OrElse);
        
        return Visit(boolExpression);
      }

      throw new NotSupportedException();
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (((ExtendedExpressionType)e.NodeType) == ExtendedExpressionType.Result)
        return e;
      if (context.Evaluator.CanBeEvaluated(e)) {
        if (context.ParameterExtractor.IsParameter(e))
          return e;
        return context.Evaluator.Evaluate(e);
      }
      return base.Visit(e);
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      return base.VisitUnary(u);
      switch (u.NodeType)
      {
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
          if (u.GetMemberType() == MemberType.Entity)
          {
            if (u.Type==u.Operand.Type || u.Type.IsAssignableFrom(u.Operand.Type))
              return base.VisitUnary(u);
            if (u.Operand.NodeType==ExpressionType.Parameter) {
              using (new ParameterScope())
              {
                var parameter = (ParameterExpression) u.Operand;
                var source = context.Bindings[parameter];
                var recordSet = source.RecordSet;

                // JOIN recordSet = recordSet.Calculate(calculatedColumns.Value.ToArray());
                var re = new ResultExpression(source.Type, recordSet, source.Mapping, source.ItemProjector);
                context.Bindings.ReplaceBound(parameter, re);                
              }
//              var visitedOperand = Visit(u.Operand);
              throw new NotImplementedException();
            }
            throw new NotImplementedException();
          }
          break;
      }
      return base.VisitUnary(u);
    }

    protected override Expression VisitLambda(LambdaExpression le)
    {
      LambdaExpression result;
      using (new ParameterScope()) {
        recordIsUsed = false;
        tuple.Value = Expression.Parameter(typeof (Tuple), "t");
        record.Value = Expression.Parameter(typeof (Record), "r");
        parameters.Value = le.Parameters.ToArray();
        calculatedColumns.Value = new List<CalculatedColumnDescriptor>();
        Expression body;
        using (context.SubqueryParameterBindings.Bind(parameters.Value)) {
          body = Visit(le.Body);
        }
        if (calculateExpressions.Value && body.GetMemberType() == MemberType.Unknown) {
          if (
            ((ExtendedExpressionType) body.NodeType)!=ExtendedExpressionType.Result &&
            !body.IsGrouping() &&
              (body.NodeType != ExpressionType.Call ||
              ((MethodCallExpression)body).Object == null ||
              ((MethodCallExpression) body).Object.Type!=typeof (Tuple))) {
            var calculator = Expression.Lambda(Expression.Convert(body, typeof (object)), tuple.Value);
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), body.Type, (Expression<Func<Tuple, object>>)calculator);
            calculatedColumns.Value.Add(ccd);
            var parameter = parameters.Value[0];
            int position = context.Bindings[parameter].RecordSet.Header.Length + calculatedColumns.Value.Count - 1;
            body = MakeTupleAccess(parameter, body.Type, position);
            mappingRef.Value.Replace(new PrimitiveMapping(new Segment<int>(position, 1)));
          }
        }
        if (calculatedColumns.Value.Count > 0) {
          var source = context.Bindings[le.Parameters[0]];
          var recordSet = source.RecordSet;
          recordSet = recordSet.Calculate(calculatedColumns.Value.ToArray());
          var re = new ResultExpression(source.Type, recordSet, source.Mapping, source.ItemProjector);
          context.Bindings.ReplaceBound(le.Parameters[0], re);
        }
        result = recordIsUsed
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
      if (!parameters.Value.Contains(pe) && !context.SubqueryParameterBindings.IsBound(pe)) {
        var referencedSource = context.Bindings[pe];
        return path.TranslateParameter(referencedSource.ItemProjector.Body);
      }
      var source = context.Bindings[pe];
      var mapping = source.Mapping as ComplexMapping;
      int number = 0;
      if (mapping != null) {
        foreach (var item in path) {
          number++;
          var name = item.Name;
          if (item.Type == MemberType.Entity) {
            ComplexMapping innerMapping;
            var typeInfo = context.Model.Types[item.Expression.Type];
            if (!mapping.TryGetJoinedEntity(name, out innerMapping)) {
              if (entityAsKey.Value && number == path.Count)
                break;
              var joinedIndex = typeInfo.Indexes.PrimaryIndex;
              var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
              var keySegment = mapping.GetFieldMapping(name);
              var keyPairs = keySegment.GetItems()
                .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
                .ToArray();
              var rs = source.RecordSet.Join(joinedRs, JoinType.Default, keyPairs);
              var joinedMapping = new ComplexMapping(typeInfo, source.RecordSet.Header.Columns.Count);
              mapping.RegisterJoinedEntity(name, joinedMapping);
              source = new ResultExpression(source.Type, rs, source.Mapping, source.ItemProjector);
              context.Bindings.ReplaceBound(pe, source);
            }
            mapping = innerMapping;
          }
          else if (item.Type == MemberType.Anonymous)
            mapping = mapping.GetAnonymousMapping(name).First;
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
          if (!entityAsKey.Value)
            return VisitMemberPathEntity(path, source, resultType);
          path = MemberPath.Parse(Expression.MakeMemberAccess(e, WellKnownMembers.IEntityKey), context.Model);
          var keyExpression = VisitMemberPathKey(path, source);
          var result = Expression.Call(WellKnownMembers.KeyTryResolveOfT.MakeGenericMethod(resultType), keyExpression);
          return result;
        case MemberType.EntitySet:
          return VisitMemberPathEntitySet(e);
        case MemberType.Anonymous:
          return VisitMemberPathAnonymous(path, source);
        default:
          throw new ArgumentOutOfRangeException();
      }
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
      bool isInnerParameter = parameters.Value.Contains(p);
      bool isOuterParemeter = !isInnerParameter && context.SubqueryParameterBindings.IsBound(p);
      if (!isInnerParameter && !isOuterParemeter)
        throw new InvalidOperationException("Lambda parameter is out of scope!");
      if (isOuterParemeter)
        return context.Bindings[p].ItemProjector.Body; // TODO: replace outer parameters?
      var source = context.Bindings[p];
      mappingRef.Value.Replace(source.Mapping);
      var result = new ParameterRewriter(tuple.Value, record.Value).Rewrite(source.ItemProjector.Body);
      recordIsUsed |= result.Second;
      return result.First;
    }

    protected override Expression VisitMemberAccess(MemberExpression ma)
    {
      if (context.Evaluator.CanBeEvaluated(ma) && context.ParameterExtractor.IsParameter(ma))
        return ma;
      if (ma.Expression == null) {
        if (typeof (IQueryable).IsAssignableFrom(ma.Type)) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).Compile();
          var rootPoint = lambda();
          if (rootPoint != null)
            return ConstructQueryable(rootPoint);
        }
      }
      else if (ma.Expression.NodeType == ExpressionType.Constant) {
        var rfi = ma.Member as FieldInfo;
        if (rfi != null && (rfi.FieldType.IsGenericType && typeof (IQueryable).IsAssignableFrom(rfi.FieldType))) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).Compile();
          var rootPoint = lambda();
          if (rootPoint != null)
            return ConstructQueryable(rootPoint);
        }
      }
      else if (ma.Expression.NodeType == ExpressionType.New && ma.Expression.GetMemberType() == MemberType.Anonymous) {
        var name = ma.Member.Name;
        var newExpression = (NewExpression)ma.Expression;
        var propertyInfo = newExpression.Type.GetProperty(name);
        var memberName = propertyInfo.GetGetMethod().Name;
        var member = newExpression.Members.First(m => m.Name == memberName);
        var argument = newExpression.Arguments[newExpression.Members.IndexOf(member)];
        return Visit(argument);
      }
      return base.VisitMemberAccess(ma);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      var arguments = new List<Expression>();
      if (n.Members == null)
        return base.VisitNew(n);
      for (int i = 0; i < n.Arguments.Count; i++) {
        var arg = n.Arguments[i];
        Expression newArg;
        var member = n.Members[i];
        var memberName = member.Name.TryCutPrefix(WellKnown.GetterPrefix);
        var path = MemberPath.Parse(arg, context.Model);
        if (path.IsValid || arg.NodeType == ExpressionType.New) {
          var argFMRef = new MappingReference(mappingRef.Value.FillMapping);
          using (new ParameterScope()) {
            mappingRef.Value = argFMRef;
            newArg = Visit(arg);
          }
          if (mappingRef.Value.FillMapping && argFMRef.FillMapping) {
            var fieldMapping = argFMRef.Mapping;
            var memberType = arg.NodeType == ExpressionType.New
              ? MemberType.Anonymous
              : path.PathType;
            Func<string, string, string> rename = (oldName, newName) => oldName.IsNullOrEmpty()
              ? newName
              : newName + "." + oldName;

            switch (memberType) {
              case MemberType.Default:
              case MemberType.Primitive:
              case MemberType.Key: {
                var primitiveFieldMapping = (PrimitiveMapping)fieldMapping;
                mappingRef.Value.RegisterField(memberName, primitiveFieldMapping.Segment);
                break;
              }
              case MemberType.Structure: {
                var complexMapping = (ComplexMapping)fieldMapping;
                foreach (var p in complexMapping.Fields)
                  mappingRef.Value.RegisterField(rename(p.Key, memberName), p.Value);
                foreach (var p in complexMapping.Entities)
                  mappingRef.Value.RegisterEntity(rename(p.Key, memberName), p.Value);
                break;
              }
              case MemberType.Entity:
                if (fieldMapping is PrimitiveMapping) {
                  var primitiveFieldMapping = (PrimitiveMapping)fieldMapping;
                  var fields = new Dictionary<string, Segment<int>> {{"Key", primitiveFieldMapping.Segment}};
                  var entityMapping = new ComplexMapping(fields);
                  mappingRef.Value.RegisterEntity(memberName, entityMapping);
                }
                else
                  mappingRef.Value.RegisterEntity(memberName, (ComplexMapping)fieldMapping);
                break;
              case MemberType.Anonymous:
                mappingRef.Value.RegisterAnonymous(memberName, (ComplexMapping)fieldMapping, newArg);
                break;
            }
          }
        }
        else {
          // TODO: Add check of queries
          Expression body;
          using (new ParameterScope()) {
            calculateExpressions.Value = false;
            mappingRef.Value = new MappingReference(false);
            body = Visit(arg);
          }
          if (body.AsTupleAccess() != null)
            newArg = body;
          else if (((ExtendedExpressionType)body.NodeType) == ExtendedExpressionType.Result) {
            var outerParameters = context.Bindings.GetKeys()
              .OfType<ParameterExpression>()
              .ToList();
            if (outerParameters.Count == 0)
              newArg = arg;
            else {
              var searchFor = outerParameters.ToArray();
              var replaceWithList = new List<Expression>();
              foreach (var projection in outerParameters.Select(pe => context.Bindings[pe].ItemProjector)) {
                recordIsUsed |= projection.Parameters.Count(pe => pe.Type == typeof (Record)) > 0;
                var replacedParameters = projection.Parameters.ToArray();
                var replacingParameters = projection.Parameters.Select(pe => pe.Type == typeof (Tuple)
                  ? tuple.Value
                  : record.Value).ToArray();
                replaceWithList.Add(ExpressionReplacer.ReplaceAll(projection.Body, replacedParameters, replacingParameters));
              }
              newArg = ExpressionReplacer.ReplaceAll(arg, searchFor, replaceWithList.ToArray());
            }
          }
          else {
            var calculator = Expression.Lambda(
              body.Type == typeof (object)
                ? body
                : Expression.Convert(body, typeof (object)),
              tuple.Value);
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), arg.Type, (Expression<Func<Tuple, object>>)calculator);
            calculatedColumns.Value.Add(ccd);
            var parameter = parameters.Value[0];
            int position = context.Bindings[parameter].RecordSet.Header.Length + calculatedColumns.Value.Count - 1;
            newArg = MakeTupleAccess(parameter, arg.Type, position);
            mappingRef.Value.RegisterField(memberName, new Segment<int>(position, 1));
          }
        }
        newArg = newArg ?? Visit(arg);
        arguments.Add(newArg);
      }
      var result = Expression.New(n.Constructor, arguments, n.Members);
        //      if (mappingRef.Value.FillMapping) {
        //        var cfm = (ComplexMapping)newFMRef.Mapping;
        //        mappingRef.Value.Mapping.Fill(cfm);
        //        mappingRef.Value.RegisterAnonymous(string.Empty, cfm, result);
        //      }
      return result;
    }

    #region Private helper methods

    private Expression ConstructQueryable(IQueryable rootPoint)
    {
      var elementType = rootPoint.ElementType;
      TypeInfo type;

      if (!context.Model.Types.TryGetValue(elementType, out type))
        throw new NotSupportedException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));

      var index = type.Indexes.PrimaryIndex;

      var mapping = new ComplexMapping(type, 0);
      var recordSet = IndexProvider.Get(index).Result;
      var pRecord = Expression.Parameter(typeof (Record), "r");
      var itemProjector = 
        Expression.Lambda(
          Expression.Call(
            WellKnownMembers.KeyTryResolveOfT.MakeGenericMethod(elementType),
            Expression.Call(pRecord, WellKnownMembers.RecordKey, Expression.Constant(0))), 
          pRecord);
      return new ResultExpression(
        typeof (IQueryable<>).MakeGenericType(elementType),
        recordSet,
        mapping,
        itemProjector);
    }

    private static IEnumerable<TResult> MakeProjection<TResult>(RecordSet rs, Expression<Func<Tuple, Record, TResult>> le)
    {
      var func = le.Compile();
      foreach (var r in rs.Parse())
        yield return func(r.Data, r);
    }

    private static Expression MakeBinaryExpression(Expression previous, Expression left, Expression right, ExpressionType operationType, ExpressionType concatenationExpression)
    {
      var newExpression = operationType==ExpressionType.Equal
        ? Expression.Equal(left, right)
        : Expression.NotEqual(left, right);
      
      if (previous == null)
        return newExpression;

      switch (concatenationExpression) {
        case ExpressionType.AndAlso:
          return Expression.AndAlso(previous, newExpression);
        case ExpressionType.OrElse:
          return Expression.OrElse(previous, newExpression);
        default:
          throw new ArgumentOutOfRangeException("concatenationExpression");
      }
    }

    private Expression MakeComplexBinaryExpression(Expression bLeft, Expression bRight, ExpressionType operationType)
    {
      Expression result = null;
      if (bLeft.NodeType == ExpressionType.Constant || bRight.NodeType == ExpressionType.Constant) {
        var constant = bLeft.NodeType == ExpressionType.Constant
          ? (ConstantExpression)bLeft
          : (ConstantExpression)bRight;
        var member = bLeft.NodeType != ExpressionType.Constant
          ? bLeft
          : bRight;
        if (constant.Value == null) {
          var path = MemberPath.Parse(member, context.Model);
          var source = context.Bindings[path.Parameter];
          var segment = source.Mapping.GetMemberSegment(path);
          foreach (var i in segment.GetItems()) {
            var columnType = source.RecordSet.Header.Columns[i].Type.ToNullable();
            Expression left = MakeTupleAccess(path.Parameter, columnType, i);
            Expression right = Expression.Constant(null, columnType);
            result = MakeBinaryExpression(result, left, right, operationType, ExpressionType.AndAlso);
          }
          return result;
        }
      }
      var leftPath = MemberPath.Parse(bLeft, context.Model);
      var leftSource = context.Bindings[leftPath.Parameter];
      var leftSegment = leftSource.Mapping.GetMemberSegment(leftPath);
      var rightPath = MemberPath.Parse(bRight, context.Model);
      var rightSource = context.Bindings[rightPath.Parameter];
      var rightSegment = rightSource.Mapping.GetMemberSegment(rightPath);
      foreach (var pair in leftSegment.GetItems().Zip(rightSegment.GetItems(), (l, r) => new {l, r})) {
        var type = leftSource.RecordSet.Header.TupleDescriptor[pair.l];
        Expression left = MakeTupleAccess(leftPath.Parameter, type, pair.l);
        Expression right = MakeTupleAccess(rightPath.Parameter, type, pair.r);
        result = MakeBinaryExpression(result, left, right, operationType, ExpressionType.AndAlso);
      }
      return result;
    }

    private Expression MakeTupleAccess(ParameterExpression parameter, Type accessorType, int index)
    {
      ApplyParameter outerParameter;
      Expression target;

      if (parameters.Value.Contains(parameter))
        target = tuple.Value;
      else if (context.SubqueryParameterBindings.TryGetBound(parameter, out outerParameter))
        target = Expression.Property(Expression.Constant(outerParameter), WellKnownMembers.ApplyParameterValue);
      else
        throw new InvalidOperationException();

      return ExpressionHelper.TupleAccess(target, accessorType, index);
    }

    private void RecordIsUsedOnOutOfScope(bool oldValue)
    {
      if (recordIsUsedParameter.HasValue)
        recordIsUsed |= oldValue;
    }

    #endregion

    #region VisitBinary implementations

    private Expression VisitBinaryKey(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType != ExpressionType.Equal && binaryExpression.NodeType != ExpressionType.NotEqual)
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
      var segment = source.Mapping.GetMemberSegment(path);
      Expression result = null;
      foreach (var pair in segment.GetItems().Select((ci, pi) => new {ColumnIndex = ci, ParameterIndex = pi})) {
        Type columnType = source.RecordSet.Header.Columns[pair.ColumnIndex].Type.ToNullable();
        Expression left = MakeTupleAccess(path.Parameter, columnType, pair.ColumnIndex);
        Expression right = ExpressionHelper.IsNullCondition(bRight,
          Expression.Constant(null, columnType),
          ExpressionHelper.TupleAccess(
            Expression.MakeMemberAccess(bRight, WellKnownMembers.KeyValue),
            columnType,
            pair.ParameterIndex
            )
          );
        result = MakeBinaryExpression(result, left, right, binaryExpression.NodeType, ExpressionType.AndAlso);
      }
      return result;
    }

    private Expression VisitBinaryEntity(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType != ExpressionType.Equal && binaryExpression.NodeType != ExpressionType.NotEqual)
        throw new NotSupportedException();

      bool leftIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Left);
      bool rightIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Right);

      if (!leftIsParameter && !rightIsParameter) {
        var bLeft = binaryExpression.Left.NodeType == ExpressionType.Constant && ((ConstantExpression)binaryExpression.Left).Value == null
          ? binaryExpression.Left
          : Expression.MakeMemberAccess(binaryExpression.Left, WellKnownMembers.IEntityKey);
        var bRight = binaryExpression.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)binaryExpression.Right).Value == null
          ? binaryExpression.Right
          : Expression.MakeMemberAccess(binaryExpression.Right, WellKnownMembers.IEntityKey);
        return MakeComplexBinaryExpression(bLeft, bRight, binaryExpression.NodeType);
      }
      else {
        var bLeft = Expression.MakeMemberAccess(binaryExpression.Left, WellKnownMembers.IEntityKey);
        var bRight = binaryExpression.Right;
        if (leftIsParameter) {
          bLeft = Expression.MakeMemberAccess(binaryExpression.Right, WellKnownMembers.IEntityKey);
          bRight = binaryExpression.Left;
        }

        var path = MemberPath.Parse(bLeft, context.Model);
        if (!parameters.Value.Contains(path.Parameter))
          throw new NotSupportedException();

        var source = context.Bindings[path.Parameter];
        var segment = source.Mapping.GetMemberSegment(path);

        Expression result = null;
        foreach (var pair in segment.GetItems().Select((ci, pi) => new {ColumnIndex = ci, ParameterIndex = pi})) {
          Type columnType = source.RecordSet.Header.Columns[pair.ColumnIndex].Type.ToNullable();
          Expression left = MakeTupleAccess(path.Parameter, columnType, pair.ColumnIndex);
          Expression right = ExpressionHelper.IsNullCondition(bRight,
            Expression.Constant(null, columnType),
            ExpressionHelper.TupleAccess(
              Expression.MakeMemberAccess(Expression.MakeMemberAccess(bRight, WellKnownMembers.IEntityKey), WellKnownMembers.KeyValue),
              columnType,
              pair.ParameterIndex
              )
            );
          result = MakeBinaryExpression(result, left, right, binaryExpression.NodeType, ExpressionType.AndAlso);
        }
        return result;
      }
    }

    private Expression VisitBinaryAnonymous(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType != ExpressionType.Equal && binaryExpression.NodeType != ExpressionType.NotEqual)
        throw new NotSupportedException();

      Expression leftExpression = binaryExpression.Left;
      Expression rightExpression = binaryExpression.Right;

      var properties = leftExpression.Type.GetProperties();
      Expression result = null;
      foreach (PropertyInfo propertyInfo in properties) {
        Expression left;
        string propertyName = propertyInfo.GetGetMethod().Name;
        if (leftExpression.NodeType == ExpressionType.New) {
          var newExpression = ((NewExpression)leftExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name == propertyName);
          int index = newExpression.Members.IndexOf(member);
          left = newExpression.Arguments[index];
        }
        else
          left = Expression.Property(leftExpression, propertyInfo);
        Expression right;
        if (rightExpression.NodeType == ExpressionType.New) {
          var newExpression = ((NewExpression)rightExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name == propertyName);
          int index = newExpression.Members.IndexOf(member);
          right = newExpression.Arguments[index];
        }
        else
          right = Expression.Property(leftExpression, propertyInfo);
        var expression = VisitBinary((BinaryExpression)MakeBinaryExpression(null, left, right, binaryExpression.NodeType, ExpressionType.AndAlso));
        result = result == null
          ? expression
          : Expression.AndAlso(result, expression);
      }
      return result;
    }

    private Expression VisitBinaryStructure(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType != ExpressionType.Equal && binaryExpression.NodeType != ExpressionType.NotEqual)
        throw new NotSupportedException();

      bool leftIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Left);
      bool rightIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Right);

      if (!leftIsParameter && !rightIsParameter)
        return MakeComplexBinaryExpression(binaryExpression.Left, binaryExpression.Right, binaryExpression.NodeType);

      throw new NotSupportedException();
    }

    #endregion

    #region VisitMemberPathImplementation

    private Expression VisitMemberPathEntitySet(Expression e)
    {
      recordIsUsed = true;
      var m = (MemberExpression)e;
      var expression = Visit(m.Expression);
      var result = Expression.MakeMemberAccess(expression, m.Member);
      return result;
    }

    private Expression VisitMemberPathEntity(MemberPath path, ResultExpression source, Type resultType)
    {
      recordIsUsed = true;
      var segment = source.Mapping.GetMemberSegment(path);
      int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
      var result = Expression.Call(WellKnownMembers.KeyTryResolveOfT.MakeGenericMethod(resultType),
        Expression.Call(record.Value, WellKnownMembers.RecordKey, Expression.Constant(groupIndex)));
//      var cfm = (ComplexFieldMapping)source.Mapping.GetMemberMapping(path);
      mappingRef.Value.Replace(source.Mapping.GetMemberMapping(path));
//      var name = cfm.Fields.Select(pair => pair.Key).OrderBy(s => s.Length).First();
//      Func<string, string> rename = oldName => oldName.TryCutPrefix(name).TrimStart('.');
//      if (mappingRef.Value.FillMapping) {
//        foreach (var p in cfm.Fields)
//          mappingRef.Value.RegisterField(rename(p.Key), p.Value);
//        foreach (var p in cfm.Entities)
//          mappingRef.Value.RegisterEntity(rename(p.Key), p.Value);
//        mappingRef.Value.RegisterEntity(string.Empty, cfm);
//      }
      return result;
    }

    private Expression VisitMemberPathStructure(MemberPath path, ResultExpression source)
    {
      recordIsUsed = true;
      var segment = source.Mapping.GetMemberSegment(path);
      var structureColumn = (MappedColumn)source.RecordSet.Header.Columns[segment.Offset];
      var field = structureColumn.ColumnInfoRef.Resolve(context.Model).Field;
      while (field.Parent != null)
        field = field.Parent;
      int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
      var result =
        Expression.MakeMemberAccess(
          Expression.Call(WellKnownMembers.KeyTryResolveOfT.MakeGenericMethod(field.ReflectedType.UnderlyingType),
            Expression.Call(record.Value, WellKnownMembers.RecordKey, Expression.Constant(groupIndex))),
          field.UnderlyingProperty);
      var columnGroup = source.RecordSet.Header.ColumnGroups[groupIndex];
      var keyOffset = columnGroup.Keys.Min();
      var keyLength = columnGroup.Keys.Max() - keyOffset + 1;
      var cfm = (ComplexMapping)source.Mapping.GetMemberMapping(path);
      var mappedFields = cfm.Fields.Where(p => (p.Value.Offset >= segment.Offset && p.Value.EndOffset <= segment.EndOffset)).ToList();
      var name = mappedFields.Select(pair => pair.Key).OrderBy(s => s.Length).First();
      foreach (var pair in mappedFields) {
        var key = pair.Key.TryCutPrefix(name).TrimStart('.');
        mappingRef.Value.RegisterField(key, pair.Value);
      }
      mappingRef.Value.RegisterField(string.Format(SurrogateKeyNameFormatString, groupIndex), new Segment<int>(keyOffset, keyLength));
      return result;
    }

    private Expression VisitMemberPathAnonymous(MemberPath path, ResultExpression source)
    {
      if (path.Count == 0)
        return VisitParameter(path.Parameter);
      var sourceMapping = (ComplexMapping)source.Mapping;
      var anonymousMapping = sourceMapping.GetAnonymousMapping(path.First().Name);
      mappingRef.Value.Replace(anonymousMapping.First);
      var result = new ParameterRewriter(tuple.Value, record.Value).Rewrite(anonymousMapping.Second);
      recordIsUsed |= result.Second;
      return result.First;
    }

    private Expression VisitMemberPathPrimitive(MemberPath path, ResultExpression source, Type resultType)
    {
      var segment = source.Mapping.GetMemberSegment(path);
      mappingRef.Value.RegisterPrimitive(segment);
      return MakeTupleAccess(path.Parameter, resultType, segment.Offset);
    }

    private Expression VisitMemberPathKey(MemberPath path, ResultExpression source)
    {
      Segment<int> segment = source.Mapping.GetMemberSegment(path);
      var keyColumn = (MappedColumn)source.RecordSet.Header.Columns[segment.Offset];
      var field = keyColumn.ColumnInfoRef.Resolve(context.Model).Field;
      var type = field.Parent == null
        ? field.ReflectedType
        : context.Model.Types[field.Parent.ValueType];
      var transform = new SegmentTransform(true, field.ReflectedType.TupleDescriptor, segment);
      var keyExtractor = Expression.Call(WellKnownMembers.KeyCreate, Expression.Constant(type),
        Expression.Call(Expression.Constant(transform), WellKnownMembers.SegmentTransformApply,
          Expression.Constant(TupleTransformType.Auto), tuple.Value),
        Expression.Constant(false));
      mappingRef.Value.RegisterPrimitive(segment);
      return keyExtractor;
    }

    #endregion
  }
}
