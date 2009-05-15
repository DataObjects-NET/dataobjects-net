// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections;
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
  internal sealed partial class Translator
  {
    private const string SurrogateKeyNameFormatString = "#_Key_{0}";

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
      if (tb.Expression.GetMemberType()==MemberType.Entity
        && typeof (IEntity).IsAssignableFrom(operandType)) {
        var typeInfo = context.Model.Types[operandType];
        var typeIds = typeInfo.GetDescendants().AddOne(typeInfo).Select(ti => ti.TypeId);
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
      if (e==null)
        return null;
      if (e.IsResult())
        return e;
      if (context.Evaluator.CanBeEvaluated(e))
        return context.ParameterExtractor.IsParameter(e)
          ? e
          : context.Evaluator.Evaluate(e);
      return base.Visit(e);
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      switch (u.NodeType) {
      case ExpressionType.TypeAs:
        if (u.GetMemberType()==MemberType.Entity)
          return VisitTypeAs(u.Operand, u.Type);
        break;
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
        if (u.GetMemberType()==MemberType.Entity) {
          if (u.Type==u.Operand.Type || u.Type.IsAssignableFrom(u.Operand.Type))
            return base.VisitUnary(u);
          throw new NotSupportedException(String.Format(Strings.ExDowncastFromXToXNotSupportedUseOfTypeOrAsOperatorInstead, u.Operand.Type, u.Type));
        }
        break;
      }
      return base.VisitUnary(u);
    }

    private Expression VisitTypeAs(Expression source, Type targetType)
    {
      if (source.GetMemberType()!=MemberType.Entity)
        throw new NotSupportedException(Strings.ExAsOperatorSupportsEntityOnly);

      // Expression is already of requested type.
      if (source.Type==targetType)
        return Visit(source);

      // Call convert to parent type.
      if (!targetType.IsSubclassOf(source.Type))
        return Visit(Expression.Convert(source, targetType));

      // Cast to subclass.
      using (state.CreateScope()) {
        var targetTypeInfo = context.Model.Types[targetType];
        var parameter = state.Parameters[0];

        var visitedSource = Visit(source);

        var resultExpression = context.Bindings[parameter];
        var recordSet = resultExpression.RecordSet;
        int offset = recordSet.Header.Columns.Count;

        var mapping = new ComplexMapping();
        mapping.Fill(state.MappingRef.Mapping);

        // Join primary index of target type
        var joinedIndex = targetTypeInfo.Indexes.PrimaryIndex;
        var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
        var keySegment = mapping.GetFieldMapping(StorageWellKnown.Key);
        var keyPairs = keySegment.GetItems()
          .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
          .ToArray();

        var joinedRecordSet = recordSet.JoinLeft(joinedRs, JoinAlgorithm.Default, keyPairs);
        var groupMappings = new int[joinedRecordSet.Header.ColumnGroups.Count];
        for (int i = 0; i < groupMappings.Length; i++)
          groupMappings[i] = -1;

        // Remap fields to new recordset.
        foreach (var targetField in targetTypeInfo.Fields) {
          int originalGroup = -1;
          Segment<int> originalSegment;
          if (mapping.Fields.TryGetValue(targetField.Name, out originalSegment))
            originalGroup = recordSet.Header.ColumnGroups.GetGroupIndexBySegment(originalSegment);

          var segment = new Segment<int>(targetField.MappingInfo.Offset + offset, targetField.MappingInfo.Length);
          mapping.RegisterField(targetField.Name, segment);
          if (targetField.IsEntity)
            mapping.RegisterField(targetField.Name + ".Key", segment);
          if (targetField.IsPrimaryKey)
            mapping.RegisterField("Key", segment);

          if (originalGroup >= 0) {
            int newGroup = joinedRecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
            if (newGroup!=originalGroup)
              groupMappings[newGroup] = originalGroup;
          }
        }

        var columnList = mapping.GetColumns(false);
        var rewriter = new ItemProjectorRewriter(columnList, groupMappings, joinedRecordSet.Header);
        var rewrittenSource = rewriter.Rewrite(visitedSource);
        var re = new ResultExpression(resultExpression.Type, joinedRecordSet, resultExpression.Mapping, resultExpression.ItemProjector);
        context.Bindings.ReplaceBound(parameter, re);
        state.MappingRef.Replace(mapping);
        return Expression.Convert(rewrittenSource, targetType);
      }
    }

    protected override Expression VisitLambda(LambdaExpression le)
    {
      LambdaExpression result;
      using (state.CreateLambdaScope(le)) {
        var body = Visit(le.Body);
        if (body.IsResult())
          body = BuildSubqueryResult((ResultExpression) body, le.Body.Type);
        else if (state.CalculateExpressions && body.GetMemberType()==MemberType.Unknown) {
          if (!body.IsSubqueryConstructor()
            && !body.IsGroupingConstructor()
              && body.AsTupleAccess()==null
                && body.NodeType!=ExpressionType.ArrayIndex
            ) {
            var originalBodyType = body.Type;
            bool isEnum = ConvertEnumToInteger(ref body);
            var calculator = FastExpression.Lambda(Expression.Convert(body, typeof (object)), state.Tuple);
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), body.Type, (Expression<Func<Tuple, object>>) calculator);
            state.CalculatedColumns.Add(ccd);
            var parameter = state.Parameters[0];
            int position = context.Bindings[parameter].RecordSet.Header.Length + state.CalculatedColumns.Count - 1;
            body = MakeTupleAccess(parameter, body.Type, position);
            if (isEnum)
              body = Expression.Convert(body, originalBodyType);
            state.MappingRef.Replace(new PrimitiveMapping(new Segment<int>(position, 1)));
          }
        }
        if (state.CalculatedColumns.Count > 0) {
          var source = context.Bindings[le.Parameters[0]];
          var recordSet = source.RecordSet;
          recordSet = recordSet.Calculate(state.CalculatedColumns.ToArray());
          var re = new ResultExpression(source.Type, recordSet, source.Mapping, source.ItemProjector);
          context.Bindings.ReplaceBound(le.Parameters[0], re);
        }
        result = state.RecordIsUsed
          ? Expression.Lambda(
            typeof (Func<,,>).MakeGenericType(typeof (Tuple), typeof (Record), body.Type),
            body,
            state.Tuple,
            state.Record)
          : FastExpression.Lambda(body, state.Tuple);
      }
      return result;
    }

    protected override Expression VisitMemberPath(MemberPath path, Expression e)
    {
      var pe = path.Parameter;
      if (!state.Parameters.Contains(pe) && !state.OuterParameters.Contains(pe)) {
        var referencedSource = context.Bindings[pe];
        return path.TranslateParameter(referencedSource.ItemProjector.Body);
      }
      var source = context.Bindings[pe];
      var mapping = source.Mapping as ComplexMapping;
      int number = 0;
      if (mapping!=null) {
        foreach (var item in path) {
          number++;
          var name = item.Name;
          if (item.Type==MemberType.Entity) {
            ComplexMapping innerMapping;
            var typeInfo = context.Model.Types[item.Expression.Type];
            if (!mapping.TryGetJoinedEntity(name, out innerMapping)) {
              if (state.EntityAsKey && number==path.Count)
                break;
              var joinedIndex = typeInfo.Indexes.PrimaryIndex;
              var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
              var keySegment = mapping.GetFieldMapping(name);
              var keyPairs = keySegment.GetItems()
                .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
                .ToArray();
              var rs = source.RecordSet.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
              var joinedMapping = new ComplexMapping(typeInfo, source.RecordSet.Header.Columns.Count);
              mapping.RegisterJoinedEntity(name, joinedMapping);
              source = new ResultExpression(source.Type, rs, source.Mapping, source.ItemProjector);
              context.Bindings.ReplaceBound(pe, source);
            }
            else {
              if (typeof (IEnumerable).IsAssignableFrom(path.Parameter.Type) && number >= path.Count - 1) {
                var columns = innerMapping.GetColumns(false);
                columns.Sort();
                var joinedIndex = typeInfo.Indexes.PrimaryIndex;
                if (columns.Count < joinedIndex.Columns.Count) {
                  var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
                  var keyPairs = columns
                    .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
                    .ToArray();
                  var rs = source.RecordSet.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
                  var joinedMapping = new ComplexMapping(typeInfo, source.RecordSet.Header.Columns.Count);
                  mapping.OverwriteJoinedEntity(name, joinedMapping);
                  source = new ResultExpression(source.Type, rs, source.Mapping, source.ItemProjector);
                  context.Bindings.ReplaceBound(pe, source);
                  break;
                }
              }
            }
            mapping = innerMapping;
          }
          else if (item.Type==MemberType.Anonymous)
            mapping = mapping.GetAnonymousMapping(name).First;
          else if (item.Type==MemberType.Grouping)
            mapping = mapping.GetGroupingMapping(name);
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
        if (!state.EntityAsKey)
          return VisitMemberPathEntity(path, source, resultType);
        path = MemberPath.Parse(Expression.MakeMemberAccess(e, WellKnownMembers.IEntityKey), context.Model);
        var keyExpression = VisitMemberPathKey(path, source);
        var result = Expression.Call(WellKnownMembers.KeyTryResolveOfT.MakeGenericMethod(resultType), keyExpression);
        return result;
      case MemberType.EntitySet:
        return VisitMemberPathSubquery(e);
      case MemberType.Anonymous:
        return VisitMemberPathAnonymous(path, source);
      case MemberType.Subquery:
        return VisitMemberPathSubquery(e);
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
      case MemberType.Array:
        return VisitBinaryArray(binaryExpression);
      case MemberType.EntitySet:
      case MemberType.Grouping:
        throw new NotSupportedException();
      default:
        throw new ArgumentOutOfRangeException();
      }
      return base.VisitBinary(binaryExpression);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      bool isInnerParameter = state.Parameters.Contains(p);
      bool isOuterParameter = state.OuterParameters.Contains(p);
      if (!isInnerParameter && !isOuterParameter)
        throw new InvalidOperationException("Lambda parameter is out of scope!");
      if (isOuterParameter)
        return context.Bindings[p].ItemProjector.Body; // TODO: replace outer parameters?
      var source = context.Bindings[p];
      state.MappingRef.Replace(source.Mapping);
      var result = new ParameterRewriter(state.Tuple, state.Record).Rewrite(source.ItemProjector.Body);
      state.RecordIsUsed |= result.Second;
      return result.First;
    }

    protected override Expression VisitMemberAccess(MemberExpression ma)
    {
      if (context.Evaluator.CanBeEvaluated(ma) && context.ParameterExtractor.IsParameter(ma))
        return ma;
      if (ma.Expression==null) {
        if (typeof (IQueryable).IsAssignableFrom(ma.Type)) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).CompileCached();
          var rootPoint = lambda();
          if (rootPoint!=null)
            return ConstructQueryable(rootPoint);
        }
      }
      else if (ma.Expression.NodeType==ExpressionType.Constant) {
        var rfi = ma.Member as FieldInfo;
        if (rfi!=null && (rfi.FieldType.IsGenericType && typeof (IQueryable).IsAssignableFrom(rfi.FieldType))) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).CompileCached();
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
      else if (ma.Expression.GetMemberType()==MemberType.Entity && ma.Member.Name!="Key") {
        if (!context.Model.Types[ma.Expression.Type].Fields.Contains(ma.Member.Name))
          throw new NotSupportedException("Nonpersistent fields are not supported.");
      }
      return base.VisitMemberAccess(ma);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      var arguments = new List<Expression>();
      if (n.Members==null) {
        if (n.IsGroupingConstructor()
          || n.IsSubqueryConstructor()
            || n.Type==typeof (TimeSpan)
              || n.Type==typeof (DateTime))
          return base.VisitNew(n);
        throw new NotSupportedException();
      }
      for (int i = 0; i < n.Arguments.Count; i++) {
        var arg = n.Arguments[i];
        Expression newArg;
        var member = n.Members[i];
        var memberName = member.Name.TryCutPrefix(WellKnown.GetterPrefix);
        var path = MemberPath.Parse(arg, context.Model);
        if (path.IsValid || arg.NodeType==ExpressionType.New) {
          var argFMRef = new MappingReference(state.MappingRef.FillMapping);
          using (state.CreateScope()) {
            state.MappingRef = argFMRef;
            newArg = Visit(arg);
          }
          if (state.MappingRef.FillMapping && argFMRef.FillMapping) {
            var fieldMapping = argFMRef.Mapping;
            var memberType = arg.NodeType==ExpressionType.New
              ? MemberType.Anonymous
              : path.PathType;
            Func<string, string, string> rename = (oldName, newName) => oldName.IsNullOrEmpty()
              ? newName
              : newName + "." + oldName;

            switch (memberType) {
            case MemberType.Default:
            case MemberType.Primitive:
            case MemberType.Key: {
              var primitiveFieldMapping = (PrimitiveMapping) fieldMapping;
              state.MappingRef.RegisterField(memberName, primitiveFieldMapping.Segment);
              break;
            }
            case MemberType.Structure: {
              var complexMapping = (ComplexMapping) fieldMapping;
              foreach (var p in complexMapping.Fields)
                state.MappingRef.RegisterField(rename(p.Key, memberName), p.Value);
              break;
            }
            case MemberType.Entity:
              if (fieldMapping is PrimitiveMapping) {
                var primitiveFieldMapping = (PrimitiveMapping) fieldMapping;
                var fields = new Dictionary<string, Segment<int>> {{StorageWellKnown.Key, primitiveFieldMapping.Segment}};
                var entityMapping = new ComplexMapping(fields);
                state.MappingRef.RegisterEntity(memberName, entityMapping);
              }
              else
                state.MappingRef.RegisterEntity(memberName, (ComplexMapping) fieldMapping);
              break;
            case MemberType.Anonymous:
              state.MappingRef.RegisterAnonymous(memberName, (ComplexMapping) fieldMapping, newArg);
              break;
            case MemberType.Grouping:
              state.MappingRef.RegisterGrouping(memberName, (ComplexMapping) fieldMapping);
              break;
            case MemberType.Subquery:
              state.MappingRef.RegisterSubquery(memberName, (ComplexMapping) fieldMapping);
              break;
            }
          }
        }
        else {
          Expression body;
          using (state.CreateScope()) {
            state.CalculateExpressions = false;
            state.MappingRef = new MappingReference(false);
            body = Visit(arg);
          }
          ConvertEnumToInteger(ref body);
          if (body.StripCasts().AsTupleAccess()!=null || body.GetMemberType()==MemberType.Array)
            newArg = body;
          else if (body.IsResult())
            newArg = BuildSubqueryResult((ResultExpression) body, arg.Type);
          else {
            var calculator = FastExpression.Lambda(
              body.Type==typeof (object)
                ? body
                : Expression.Convert(body, typeof (object)),
              state.Tuple);
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), body.Type, (Expression<Func<Tuple, object>>) calculator);
            state.CalculatedColumns.Add(ccd);
            var parameter = state.Parameters[0];
            int position = context.Bindings[parameter].RecordSet.Header.Length + state.CalculatedColumns.Count - 1;
            newArg = MakeTupleAccess(parameter, body.Type, position);
            state.MappingRef.RegisterField(memberName, new Segment<int>(position, 1));
          }
        }
        newArg = newArg ?? Visit(arg);
        arguments.Add(newArg);
      }
      var constructorParameters = n.Constructor.GetParameters();
      for (int i = 0; i < arguments.Count; i++) {
        if (arguments[i].Type!=constructorParameters[i].ParameterType)
          arguments[i] = Expression.Convert(arguments[i], constructorParameters[i].ParameterType);
      }
      var result = Expression.New(n.Constructor, arguments, n.Members);
      return result;
    }

    #region Private helper methods

    private bool ConvertEnumToInteger(ref Expression expression)
    {
      if (expression.Type.IsEnum) {
        expression = Expression.Convert(expression, Enum.GetUnderlyingType(expression.Type));
        return true;
      }
      return false;
    }

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
        FastExpression.Lambda(
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

    private Expression BuildSubqueryResult(ResultExpression subQuery, Type resultType)
    {
      if (state.Parameters.Length!=1)
        throw new NotImplementedException();

      if (!resultType.IsOfGenericInterface(typeof (IEnumerable<>)))
        throw new NotImplementedException();

      Type type = resultType
        .GetInterfaces(true)
        .AddOne(resultType)
        .Where(interfaceType =>
          interfaceType.IsGenericType
            && interfaceType.GetGenericTypeDefinition()==typeof (IEnumerable<>))
        .Select(interfaceType => interfaceType.GetGenericArguments()[0])
        .First();

      var parameterResultExpression = context.Bindings[state.Parameters[0]];
      var applyParameter = context.GetApplyParameter(parameterResultExpression);
      var tupleParameter = new Parameter<Tuple>("tupleParameter");

      var rewrittenRecordset = ApplyParameterToTupleParameterRewriter
        .Rewrite(subQuery.RecordSet.Provider, tupleParameter, applyParameter)
        .Result;

      //  state.MappingRef = new MappingReference(state.MappingRef.FillMapping);

      var newResultExpression = new ResultExpression(subQuery.Type, rewrittenRecordset, subQuery.Mapping, subQuery.ItemProjector, subQuery.ResultType);

      var constructor = (typeof (SubQuery<>)
        .MakeGenericType(type)
        .GetConstructor(new[] {typeof (ResultExpression), typeof (Tuple), typeof (Parameter<Tuple>)}));

      var subqueryResult = Expression.New(constructor, new Expression[] {
        Expression.Constant(newResultExpression),
        state.Tuple,
        Expression.Constant(tupleParameter)
      });

      return Expression.Convert(subqueryResult, resultType);
    }

    private static Expression MakeBinaryExpression(Expression previous, Expression left, Expression right, ExpressionType operationType, ExpressionType concatenationExpression)
    {
      var newExpression = operationType==ExpressionType.Equal
        ? Expression.Equal(left, right)
        : Expression.NotEqual(left, right);

      if (previous==null)
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
      var target = state.Parameters.Contains(parameter)
        ? (Expression) state.Tuple
        : Expression.Property(
          Expression.Constant(context.GetApplyParameter(context.Bindings[parameter])),
          WellKnownMembers.ApplyParameterValue
          );

      return ExpressionHelper.TupleAccess(target, accessorType, index);
    }

    #endregion

    #region VisitBinary implementations

    private Expression VisitBinaryKey(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionsWithNodeTypeXAreNotSupported, binaryExpression.NodeType));

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
      if (!state.Parameters.Contains(path.Parameter))
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
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionsWithNodeTypeXAreNotSupported, binaryExpression.NodeType));

      bool leftIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Left);
      bool rightIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Right);

      if (!leftIsParameter && !rightIsParameter) {
        var bLeft = binaryExpression.Left.NodeType==ExpressionType.Constant && ((ConstantExpression) binaryExpression.Left).Value==null
          ? binaryExpression.Left
          : Expression.MakeMemberAccess(binaryExpression.Left, WellKnownMembers.IEntityKey);
        var bRight = binaryExpression.Right.NodeType==ExpressionType.Constant && ((ConstantExpression) binaryExpression.Right).Value==null
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
        if (!state.Parameters.Contains(path.Parameter))
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

    private Expression VisitBinaryArray(BinaryExpression expression)
    {
      switch (expression.NodeType) {
      case ExpressionType.ArrayIndex:
        var arrayExpression = Visit(expression.Left);
        var arrayIndex = Visit(expression.Right);
        return Expression.ArrayIndex(arrayExpression, arrayIndex);
      case ExpressionType.Equal:
      case ExpressionType.NotEqual:
        var rightExpression = expression.Right.StripCasts();
        if (rightExpression.NodeType==ExpressionType.Constant && ((ConstantExpression) rightExpression).Value==null)
          return base.VisitBinary(expression);
        break;
      }
      throw new NotSupportedException();
    }

    private Expression VisitBinaryAnonymous(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionsWithNodeTypeXAreNotSupported, binaryExpression.NodeType));

      Expression leftExpression = binaryExpression.Left;
      Expression rightExpression = binaryExpression.Right;

      var properties = leftExpression.Type.GetProperties();
      Expression result = null;
      foreach (PropertyInfo propertyInfo in properties) {
        Expression left;
        string propertyName = propertyInfo.GetGetMethod().Name;
        if (leftExpression.NodeType==ExpressionType.New) {
          var newExpression = ((NewExpression) leftExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name==propertyName);
          int index = newExpression.Members.IndexOf(member);
          left = newExpression.Arguments[index];
        }
        else
          left = Expression.Property(leftExpression, propertyInfo);
        Expression right;
        if (rightExpression.NodeType==ExpressionType.New) {
          var newExpression = ((NewExpression) rightExpression);
          var member = newExpression.Members.First(memberInfo => memberInfo.Name==propertyName);
          int index = newExpression.Members.IndexOf(member);
          right = newExpression.Arguments[index];
        }
        else
          right = Expression.Property(leftExpression, propertyInfo);
        var expression = VisitBinary((BinaryExpression) MakeBinaryExpression(null, left, right, binaryExpression.NodeType, ExpressionType.AndAlso));
        result = result==null
          ? expression
          : Expression.AndAlso(result, expression);
      }
      return result;
    }

    private Expression VisitBinaryStructure(BinaryExpression binaryExpression)
    {
      if (binaryExpression.NodeType!=ExpressionType.Equal && binaryExpression.NodeType!=ExpressionType.NotEqual)
        throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionsWithNodeTypeXAreNotSupported, binaryExpression.NodeType));

//      bool leftIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Left);
//      bool rightIsParameter = context.ParameterExtractor.IsParameter(binaryExpression.Right);
//
//      if (!leftIsParameter && !rightIsParameter)
      return MakeComplexBinaryExpression(binaryExpression.Left, binaryExpression.Right, binaryExpression.NodeType);

//      throw new NotSupportedException();
    }

    #endregion

    #region VisitMemberPathImplementation

    private Expression VisitMemberPathSubquery(Expression e)
    {
      state.RecordIsUsed = true;
      var m = (MemberExpression) e;
      var expression = Visit(m.Expression);
      var result = Expression.MakeMemberAccess(expression, m.Member);
      return result;
    }

    private Expression VisitMemberPathEntity(MemberPath path, ResultExpression source, Type resultType)
    {
      state.RecordIsUsed = true;
      var segment = source.Mapping.GetMemberSegment(path);
      int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
      var result = Expression.Call(WellKnownMembers.KeyTryResolveOfT.MakeGenericMethod(resultType),
        Expression.Call(state.Record, WellKnownMembers.RecordKey, Expression.Constant(groupIndex)));
      state.MappingRef.Replace(source.Mapping.GetMemberMapping(path));
      return result;
    }

    private Expression VisitMemberPathStructure(MemberPath path, ResultExpression source)
    {
      state.RecordIsUsed = true;
      var segment = source.Mapping.GetMemberSegment(path);
      var structureColumn = (MappedColumn) source.RecordSet.Header.Columns[segment.Offset];
      var field = structureColumn.ColumnInfoRef.Resolve(context.Model).Field;
      while (field.Parent!=null)
        field = field.Parent;
      int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
      var result =
        Expression.MakeMemberAccess(
          Expression.Call(WellKnownMembers.KeyTryResolveOfT.MakeGenericMethod(field.ReflectedType.UnderlyingType),
            Expression.Call(state.Record, WellKnownMembers.RecordKey, Expression.Constant(groupIndex))),
          field.UnderlyingProperty);
      var columnGroup = source.RecordSet.Header.ColumnGroups[groupIndex];
      var keyOffset = columnGroup.Keys.Min();
      var keyLength = columnGroup.Keys.Max() - keyOffset + 1;
      var cfm = (ComplexMapping) source.Mapping.GetMemberMapping(path);
      var mappedFields = cfm.Fields.Where(p => (p.Value.Offset >= segment.Offset && p.Value.EndOffset <= segment.EndOffset)).ToList();
      var name = mappedFields.Select(pair => pair.Key).OrderBy(s => s.Length).First();
      foreach (var pair in mappedFields) {
        var key = pair.Key.TryCutPrefix(name).TrimStart('.');
        state.MappingRef.RegisterField(key, pair.Value);
      }
      state.MappingRef.RegisterField(string.Format(SurrogateKeyNameFormatString, groupIndex), new Segment<int>(keyOffset, keyLength));
      return result;
    }

    private Expression VisitMemberPathAnonymous(MemberPath path, ResultExpression source)
    {
      if (path.Count==0)
        return VisitParameter(path.Parameter);
      var anonymousMapping = new Pair<ComplexMapping, Expression>((ComplexMapping) source.Mapping, source.ItemProjector);
      foreach (var pathItem in path)
        anonymousMapping = anonymousMapping.First.GetAnonymousMapping(pathItem.Name);
      state.MappingRef.Replace(anonymousMapping.First);
      var result = new ParameterRewriter(state.Tuple, state.Record).Rewrite(anonymousMapping.Second);
      state.RecordIsUsed |= result.Second;
      return result.First;
    }

    private Expression VisitMemberPathPrimitive(MemberPath path, ResultExpression source, Type resultType)
    {
      var segment = source.Mapping.GetMemberSegment(path);
      state.MappingRef.RegisterPrimitive(segment);
      return MakeTupleAccess(path.Parameter, resultType, segment.Offset);
    }

    private Expression VisitMemberPathKey(MemberPath path, ResultExpression source)
    {
      Segment<int> segment = source.Mapping.GetMemberSegment(path);
      var keyColumn = (MappedColumn) source.RecordSet.Header.Columns[segment.Offset];
      var field = keyColumn.ColumnInfoRef.Resolve(context.Model).Field;
      var type = field.Parent==null
        ? field.ReflectedType
        : context.Model.Types[field.Parent.ValueType];
      var transform = new SegmentTransform(true, field.ReflectedType.TupleDescriptor, segment);
      var keyExtractor = Expression.Call(WellKnownMembers.KeyCreate, Expression.Constant(type),
        Expression.Call(Expression.Constant(transform), WellKnownMembers.SegmentTransformApply,
          Expression.Constant(TupleTransformType.Auto), state.Tuple),
        Expression.Constant(false));
      state.MappingRef.RegisterPrimitive(segment);
      return keyExtractor;
    }

    #endregion
  }
}