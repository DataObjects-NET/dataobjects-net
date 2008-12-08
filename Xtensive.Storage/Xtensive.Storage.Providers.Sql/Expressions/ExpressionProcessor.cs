// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using System.Linq;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class ExpressionProcessor : Visitor<SqlExpression>
  {
    private readonly DomainModel model;
    private readonly SqlFetchRequest request;
    private readonly SqlSelect query;

    public SqlFetchRequest Request
    {
      get { return request; }
    }

    public void AppendFilterToRequest(Expression<Func<Tuple,bool>> exp)
    {
      var expression = QueryPreprocessor.Translate(exp, model);
      var result = Visit(expression);
      query.Where &= result;
    }

    public void AppendCalculatedColumnToRequest(Expression<Func<Tuple, object>> exp, string name)
    {
      var expression = QueryPreprocessor.Translate(exp, model);
      var result = Visit(expression);
      query.Columns.Add(result, name);
    }

    protected override SqlExpression VisitUnknown(Expression expression)
    {
      var type = (ExtendedExpressionType) expression.NodeType;
      switch (type) {
        case ExtendedExpressionType.FieldAccess:
          return VisitFieldAccess((ColumnAccessExpression)expression);
        case ExtendedExpressionType.ParameterAccess:
          return VisitParameterAccess((ParameterAccessExpression)expression);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private SqlExpression VisitParameterAccess(ParameterAccessExpression expression)
    {
      var binding = new SqlFetchParameterBinding(expression.Binding.Compile());
      request.ParameterBindings.Add(binding);
      return binding.SqlParameter;

    }

    private SqlExpression VisitFieldAccess(ColumnAccessExpression expression)
    {
      var sqlSelect = (SqlSelect)request.Statement;
      return sqlSelect[expression.ColumnIndex];
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
//        case ExpressionType.Coalesce:
//          break;
        case ExpressionType.Divide:
          return SqlFactory.Divide(left, right);
        case ExpressionType.Equal:
          return SqlFactory.Equals(left, right);
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

    protected override SqlExpression VisitTypeIs(TypeBinaryExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitConditional(ConditionalExpression expression)
    {
      throw new NotSupportedException();
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

    protected override SqlExpression VisitMethodCall(MethodCallExpression expression)
    {
      var map = MethodMapping.GetMapping(expression.Method);
      var target = Visit(expression.Object);
      var arguments = expression.Arguments.Select(a => Visit(a)).ToArray();
      return map(target, arguments);
    }

    protected override SqlExpression VisitLambda(LambdaExpression expression)
    {
      return Visit(expression.Body);
    }

    protected override SqlExpression VisitNew(NewExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitNewArray(NewArrayExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitInvocation(InvocationExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMemberInit(MemberInitExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitListInit(ListInitExpression expression)
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
  }
}