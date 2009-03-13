// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings;
using Xtensive.Storage.Rse.Compilation;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class ExpressionProcessor : ExpressionVisitor<SqlExpression>
  {
    private static readonly MemberCompilerProvider<SqlExpression> mappingsProvider;
    private static readonly Type[] builtinMappings = new[] {
      typeof(NullableMappings),
      typeof(StringMappings),
      typeof(DateTimeMappings),
      typeof(TimeSpanMappings),
      typeof(MathMappings),
      typeof(NumericMappings),
      typeof(DecimalMappings)
    };

    private readonly DomainModel model;
    private readonly SqlSelect[] selects;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly LambdaExpression lambda;
    private readonly HashSet<SqlFetchParameterBinding> bindings;
    private readonly Dictionary<ParameterExpression, SqlSelect> parameterMapping;
    private bool executed;

    public HashSet<SqlFetchParameterBinding> Bindings
    {
      get { return bindings; }
    }

    public ICompiler Compiler { get; private set; }

    public DomainModel Model
    {
      get { return model; }
    }

    public SqlExpression Translate()
    {
      if (executed)
        throw new InvalidOperationException();
      executed = true;
      return Visit(lambda);
    }

    protected override SqlExpression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (evaluator.CanBeEvaluated(e)) {
        if (parameterExtractor.IsParameter(e))
          return VisitParameterAccess(e);
        return VisitConstant(evaluator.Evaluate(e));
      }
      return base.Visit(e);
    }

    private SqlExpression VisitParameterAccess(Expression e)
    {
      if (e.NodeType == ExpressionType.Call) {
        var mc = (MethodCallExpression)e;
        if (mc.Method.Name == "GetValue" || mc.Method.Name == "GetValueOrDefault") {
          if (mc.Object.NodeType == ExpressionType.MemberAccess) {
            var ma = (MemberExpression)mc.Object;
            if (ma.Member.Name == "Value" && ma.Expression.Type == typeof(Parameter<Tuple>)) {
              var parameter = Expression.Lambda<Func<Parameter<Tuple>>>(ma.Expression).Compile().Invoke();
              int columnIndex = mc.Arguments[0].NodeType == ExpressionType.Constant
                ? (int)((int)((ConstantExpression)mc.Arguments[0]).Value)
                : Expression.Lambda<Func<int>>(mc.Arguments[0]).Compile().Invoke();
              var provider = CompilationContext.Current.BindingContext.GetBound(parameter);
              if (!Compiler.IsCompatible(provider)) {
                provider = Compiler.ToCompatible(provider);
                CompilationContext.Current.BindingContext.ReplaceBound(parameter, provider);
              }
              var sqlProvider = (SqlProvider)provider;
              var sqlSelect = (SqlSelect)sqlProvider.Request.Statement;
              return sqlSelect[columnIndex];
            }
          }
        }
      }
      var expression = parameterExtractor.ExtractParameter<object>(e);
      var binding = new SqlFetchParameterBinding(expression.Compile());
      bindings.Add(binding);
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
      SqlExpression result;
      if (TryTranslateCompareExpression(expression, out result))
        return result;

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
      var map = mappingsProvider.GetCompiler(m.Member);
      if (map == null)
        throw new NotSupportedException();

      return map(Visit(m.Expression), null);
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
          var sqlSelect = parameterMapping[(ParameterExpression)mc.Object];
          return sqlSelect[columnIndex];
        }
      }

      var arguments = mc.Arguments.Select(a => Visit(a)).ToArray();
      var mi = mc.Method;

      if (mc.Object != null && mc.Object.Type != mi.ReflectedType)
        mi = mc.Object.Type.GetMethod(mi.Name, mi.GetParameterTypes());

      var map = mappingsProvider.GetCompiler(mi);
      if (map == null)
        throw new NotSupportedException();

      return map(Visit(mc.Object), arguments);
    }

    protected override SqlExpression VisitLambda(LambdaExpression l)
    {
      for (int i = 0; i < l.Parameters.Count; i++) {
        var p = l.Parameters[i];
        var select = selects[i];
        parameterMapping[p] = select;
      }
      return Visit(l.Body);
    }

    protected override SqlExpression VisitNew(NewExpression n)
    {
      var mapping = mappingsProvider.GetCompiler(n.Constructor);
      if (mapping == null)
        throw new NotSupportedException();
      return mapping(null, n.Arguments.Select(a => Visit(a)).ToArray());
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
    
    private bool TryTranslateCompareExpression(BinaryExpression expression, out SqlExpression result)
    {
      result = null;

      bool isGoodExpression =
        expression.Left.NodeType==ExpressionType.Call
          && expression.Right.NodeType==ExpressionType.Constant ||
        expression.Right.NodeType==ExpressionType.Call
          && expression.Left.NodeType==ExpressionType.Constant;

      if (!isGoodExpression)
        return false;

      MethodCallExpression callExpression;
      ConstantExpression constantExpression;
      bool swapped;

      if (expression.Left.NodeType==ExpressionType.Call) {
        callExpression = (MethodCallExpression) expression.Left;
        constantExpression = (ConstantExpression) expression.Right;
        swapped = false;
      }
      else {
        callExpression = (MethodCallExpression)expression.Right;
        constantExpression = (ConstantExpression)expression.Left;
        swapped = true;
      }
      
      var method = (MethodInfo)callExpression.Method.GetInterfaceMember() ?? callExpression.Method;
      var methodType = method.DeclaringType;
      
      // There no methods in IComparable except CompareTo so checking only DeclatingType.
      bool isCompareTo = methodType == typeof(IComparable)
        || methodType.IsGenericType && methodType.GetGenericTypeDefinition() == typeof(IComparable<>);

      bool isCompare = method.Name=="Compare" && method.GetParameters().Length==2 && method.IsStatic;

      if (!isCompareTo && !isCompare)
        return false;

      if (constantExpression.Value == null)
        return false;

      if (!(constantExpression.Value is int))
        return false;

      int constant = (int) constantExpression.Value;

      SqlExpression leftComparand = null;
      SqlExpression rightComparand = null;

      if (isCompareTo) {
        leftComparand = Visit(callExpression.Object);
        rightComparand = Visit(callExpression.Arguments[0]);
      }

      if (isCompare) {
        leftComparand = Visit(callExpression.Arguments[0]);
        rightComparand = Visit(callExpression.Arguments[1]);
      }

      if (swapped) {
        var tmp = leftComparand;
        leftComparand = rightComparand;
        rightComparand = tmp;
      }

      if (constant==0)
        switch (expression.NodeType) {
          case ExpressionType.GreaterThan:
            result = SqlFactory.GreaterThan(leftComparand, rightComparand);
            return true;
          case ExpressionType.GreaterThanOrEqual:
            result = SqlFactory.GreaterThanOrEquals(leftComparand, rightComparand);
            return true;
          case ExpressionType.Equal:
            result = SqlFactory.Equals(leftComparand, rightComparand);
            return true;
          case ExpressionType.NotEqual:
            result = SqlFactory.NotEquals(leftComparand, rightComparand);
            return true;
          case ExpressionType.LessThanOrEqual:
            result = SqlFactory.LessThanOrEquals(leftComparand, rightComparand);
            return true;
          case ExpressionType.LessThan:
            result = SqlFactory.LessThan(leftComparand, rightComparand);
            return true;
          default:
            return false;
        }

    if (constant > 0)
      switch (expression.NodeType) {
        case ExpressionType.Equal:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
          result = SqlFactory.GreaterThan(leftComparand, rightComparand);
          return true;
        case ExpressionType.NotEqual:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.LessThan:
          result = SqlFactory.LessThanOrEquals(leftComparand, rightComparand);
          return true;
        default:
          return false;
      }

    if (constant < 0)
      switch (expression.NodeType)
      {
        case ExpressionType.NotEqual:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
          result = SqlFactory.GreaterThanOrEquals(leftComparand, rightComparand);
          return true;
        case ExpressionType.Equal:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.LessThan:
          result = SqlFactory.LessThan(leftComparand, rightComparand);
          return true;
        default:
          return false;
      }

      return false; // make compiler happy
    }

    // Constructor

    public ExpressionProcessor(ICompiler compiler, DomainModel model, LambdaExpression le, params SqlSelect[] selects)
    {
      Compiler = compiler;
      if (selects == null)
        throw new ArgumentNullException("selects");

      if (le.Parameters.Count != selects.Length)
        throw new InvalidOperationException();
      this.model = model;
      this.selects = selects;
      lambda = le;
      bindings = new HashSet<SqlFetchParameterBinding>();
      parameterMapping = new Dictionary<ParameterExpression, SqlSelect>();
      evaluator = new ExpressionEvaluator(le);
      parameterExtractor = new ParameterExtractor(evaluator);
    }

    static ExpressionProcessor()
    {
      mappingsProvider = new MemberCompilerProvider<SqlExpression>();
      foreach (var t in builtinMappings)
        mappingsProvider.RegisterCompilers(t);
    }
  }
}