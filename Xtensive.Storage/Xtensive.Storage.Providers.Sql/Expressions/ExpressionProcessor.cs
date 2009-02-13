// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using System.Linq;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class ExpressionProcessor : ExpressionVisitor<SqlExpression>
  {
    private static readonly MemberCompilerProvider<SqlExpression> mappingsProvider;
    private readonly DomainModel model;
    private readonly SqlFetchRequest request;
    private readonly SqlSelect query;
    private ExpressionEvaluator evaluator;
    private ParameterExtractor parameterExtractor;

    public SqlFetchRequest Request
    {
      get { return request; }
    }

    public void AppendFilterToRequest(Expression<Func<Tuple,bool>> exp)
    {
      var result = Transform(exp);
      query.Where &= result;
    }

    public void AppendCalculatedColumnToRequest(Expression<Func<Tuple, object>> exp, string name)
    {
      var result = Transform(exp);
      query.Columns.Add(result, name);
    }

    private SqlExpression Transform(Expression e)
    {
      evaluator = new ExpressionEvaluator(e);
      parameterExtractor = new ParameterExtractor(evaluator);
      var result = Visit(e);
      return result;
    }

    protected override SqlExpression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (evaluator.CanBeEvaluated(e)) {
        if (parameterExtractor.IsParameter(e))
          return VisitParameterAccess(parameterExtractor.ExtractParameter<object>(e));
        return VisitConstant(evaluator.Evaluate(e));
      }
      return base.Visit(e);
    }

    private SqlExpression VisitParameterAccess(Expression<Func<object>> expression)
    {
      var binding = new SqlFetchParameterBinding(expression.Compile());
      request.ParameterBindings.Add(binding);
      return binding.SqlParameter;

    }

    protected override SqlExpression VisitUnary(UnaryExpression expression)
    {
      var operand = Visit(expression.Operand);
      switch (expression.NodeType) {
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          return SqlFactory.Negate(operand);
        case ExpressionType.UnaryPlus:
          return operand;

        case ExpressionType.Not:
          if ((expression.Operand.Type!=typeof (bool)) && (expression.Operand.Type!=typeof (bool?)))
            return SqlFactory.BitNot(operand);
          return SqlFactory.Not(operand);

//        case ExpressionType.TypeAs:
//          return operand;
      }
      return operand;
    }

    protected override SqlExpression VisitBinary(BinaryExpression expression)
    {
      var left = Visit(expression.Left);
      var right = Visit(expression.Right);
      switch(expression.NodeType) {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          return SqlFactory.Add(left,right);
        case ExpressionType.And:
          if ((expression.Left.Type!=typeof (bool)) && (expression.Left.Type!=typeof (bool?)))
            return SqlFactory.BitAnd(left, right);
          return SqlFactory.And(left, right);
        case ExpressionType.AndAlso:
          return SqlFactory.And(left, right);
        case ExpressionType.Coalesce:
          return SqlFactory.Coalesce(left,right);
        case ExpressionType.Divide:
          return SqlFactory.Divide(left, right);
        case ExpressionType.Equal: {
          if (left.NodeType == SqlNodeType.Null || right.NodeType == SqlNodeType.Null) {
            if (left.NodeType == SqlNodeType.Null)
              return SqlFactory.IsNull(right);
            return SqlFactory.IsNull(left);
          }
          return SqlFactory.Equals(left, right);
        }
        case ExpressionType.ExclusiveOr:
          return SqlFactory.BitXor(left, right);
        case ExpressionType.GreaterThan:
          return SqlFactory.GreaterThan(left, right);
        case ExpressionType.GreaterThanOrEqual:
          return SqlFactory.GreaterThanOrEquals(left, right);
        case ExpressionType.LessThan:
          return SqlFactory.LessThan(left, right);
        case ExpressionType.LessThanOrEqual:
          return SqlFactory.LessThanOrEquals(left, right);
        case ExpressionType.Modulo:
          return SqlFactory.Modulo(left, right);
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          return SqlFactory.Multiply(left, right);
        case ExpressionType.NotEqual:
          if (left.NodeType == SqlNodeType.Null || right.NodeType == SqlNodeType.Null) {
            if (left.NodeType == SqlNodeType.Null)
              return SqlFactory.IsNotNull(right);
            return SqlFactory.IsNotNull(left);
          }
          return SqlFactory.NotEquals(left, right);
        case ExpressionType.Or:
          if ((expression.Left.Type != typeof(bool)) && (expression.Left.Type != typeof(bool?)))
            return SqlFactory.BitOr(left, right);
          return SqlFactory.Or(left, right);
        case ExpressionType.OrElse:
          return SqlFactory.Or(left, right);
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return SqlFactory.Subtract(left, right);
        default:
          throw new ArgumentOutOfRangeException("expression");
      }
    }

    protected override SqlExpression VisitTypeIs(TypeBinaryExpression tb)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitConditional(ConditionalExpression expression)
    {
      var check = Visit(expression.Test);
      var ifTrue = Visit(expression.IfTrue);
      var ifFalse = Visit(expression.IfFalse);
      var c = SqlFactory.Case();
      c[check] = ifTrue;
      c.Else = ifFalse;
      return c;
    }

    protected override SqlExpression VisitConstant(ConstantExpression expression)
    {
      var constant = expression.Value != null ? 
        SqlFactory.Literal(expression.Value, expression.Type) : 
        SqlFactory.Null;
      return constant;
    }

    protected override SqlExpression VisitParameter(ParameterExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMemberAccess(MemberExpression m)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Object != null && mc.Object.Type == typeof(Tuple)) {
        if (mc.Method.Name == "GetValue" || mc.Method.Name == "GetValueOrDefault") {
          var type = mc.Method.ReturnType;
          var columnArgument = mc.Arguments[0];
          int columnIndex;
          if (columnArgument.NodeType == ExpressionType.Constant)
            columnIndex = (int)((ConstantExpression)columnArgument).Value;
          else {
            var columnFunc = Expression.Lambda<Func<int>>(columnArgument).Compile();
            columnIndex = columnFunc();
          }
          var sqlSelect = (SqlSelect)request.Statement;
          return sqlSelect[columnIndex];
        }
      }

      var arguments = mc.Arguments.Select(a => Visit(a)).ToArray();

      SqlExpression[] argArray;
      var mi = mc.Method;

      if (mi.IsStatic) {
        argArray = arguments;
      }
      else {
        if (mi.ReflectedType != mc.Object.Type)
          mi = mc.Object.Type.GetMethod(mi.Name, mi.GetParameterTypes());

        argArray = new SqlExpression[arguments.Length + 1];
        argArray[0] = Visit(mc.Object);
        arguments.CopyTo(argArray, 1);
      }

      var map = mappingsProvider.GetCompiler(mi);
      if (map == null)
        throw new NotSupportedException();
      return map(argArray);
    }

    protected override SqlExpression VisitLambda(LambdaExpression l)
    {
      return Visit(l.Body);
    }

    protected override SqlExpression VisitNew(NewExpression n)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitNewArray(NewArrayExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitInvocation(InvocationExpression i)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMemberInit(MemberInitExpression mi)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitListInit(ListInitExpression li)
    {
      throw new NotSupportedException();
    }
    
    // Constructor

    public ExpressionProcessor(SqlFetchRequest request, DomainModel model)
    {
      this.request = request;
      this.model = model;
      query = (SqlSelect)request.Statement;
    }

    static ExpressionProcessor()
    {
      mappingsProvider = new MemberCompilerProvider<SqlExpression>();
      mappingsProvider.RegisterCompilers(typeof(StringMappings));
    }
  }
}