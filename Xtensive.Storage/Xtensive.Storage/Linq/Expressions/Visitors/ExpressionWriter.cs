// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  /// <summary>
  /// Writes out an expression tree in a C#-ish syntax.
  /// </summary>
  public class ExpressionWriter : ExpressionVisitor
  {
    private static readonly char[] special = new[] {'\n', '\n', '\\'};
    private static readonly char[] splitters = new[] {'\n', '\r'};
    private readonly TextWriter writer;
    private int depth;

    #region Nested type: Indentation

    protected enum Indentation
    {
      Same,
      Inner,
      Outer
    }

    #endregion

    protected int IndentationWidth { get; set; }

    public static void Write(TextWriter writer, Expression expression)
    {
      new ExpressionWriter(writer).Visit(expression);
    }

    public static string WriteToString(Expression expression)
    {
      var sw = new StringWriter();
      Write(sw, expression);
      return sw.ToString();
    }

    protected void WriteLine(Indentation style)
    {
      writer.WriteLine();
      Indent(style);
      for (int i = 0, n = depth * IndentationWidth; i < n; i++) {
        writer.Write(" ");
      }
    }

    protected void Write(string text)
    {
      if (text.IndexOf('\n') >= 0) {
        string[] lines = text.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0, n = lines.Length; i < n; i++) {
          Write(lines[i]);
          if (i < n - 1) {
            WriteLine(Indentation.Same);
          }
        }
      }
      else {
        writer.Write(text);
      }
    }

    protected void Indent(Indentation style)
    {
      if (style==Indentation.Inner) {
        depth++;
      }
      else if (style==Indentation.Outer) {
        depth--;
        Debug.Assert(depth >= 0);
      }
    }

    protected virtual string GetOperator(ExpressionType type)
    {
      switch (type) {
      case ExpressionType.Not:
        return "!";
      case ExpressionType.Add:
      case ExpressionType.AddChecked:
        return "+";
      case ExpressionType.Negate:
      case ExpressionType.NegateChecked:
      case ExpressionType.Subtract:
      case ExpressionType.SubtractChecked:
        return "-";
      case ExpressionType.Multiply:
      case ExpressionType.MultiplyChecked:
        return "*";
      case ExpressionType.Divide:
        return "/";
      case ExpressionType.Modulo:
        return "%";
      case ExpressionType.And:
        return "&";
      case ExpressionType.AndAlso:
        return "&&";
      case ExpressionType.Or:
        return "|";
      case ExpressionType.OrElse:
        return "||";
      case ExpressionType.LessThan:
        return "<";
      case ExpressionType.LessThanOrEqual:
        return "<=";
      case ExpressionType.GreaterThan:
        return ">";
      case ExpressionType.GreaterThanOrEqual:
        return ">=";
      case ExpressionType.Equal:
        return "==";
      case ExpressionType.NotEqual:
        return "!=";
      case ExpressionType.Coalesce:
        return "??";
      case ExpressionType.RightShift:
        return ">>";
      case ExpressionType.LeftShift:
        return "<<";
      case ExpressionType.ExclusiveOr:
        return "^";
      default:
        return null;
      }
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      switch (b.NodeType) {
      case ExpressionType.ArrayIndex:
        Visit(b.Left);
        Write("[");
        Visit(b.Right);
        Write("]");
        break;
      case ExpressionType.Power:
        Write("POW(");
        Visit(b.Left);
        Write(", ");
        Visit(b.Right);
        Write(")");
        break;
      default:
        Visit(b.Left);
        Write(" ");
        Write(GetOperator(b.NodeType));
        Write(" ");
        Visit(b.Right);
        break;
      }
      return b;
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      switch (u.NodeType) {
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
        Write("((");
        Write(GetTypeName(u.Type));
        Write(")");
        Visit(u.Operand);
        Write(")");
        break;
      case ExpressionType.ArrayLength:
        Visit(u.Operand);
        Write(".Length");
        break;
      case ExpressionType.Quote:
        Visit(u.Operand);
        break;
      case ExpressionType.TypeAs:
        Visit(u.Operand);
        Write(" as ");
        Write(GetTypeName(u.Type));
        break;
      case ExpressionType.UnaryPlus:
        Visit(u.Operand);
        break;
      default:
        Write(GetOperator(u.NodeType));
        Visit(u.Operand);
        break;
      }
      return u;
    }

    protected virtual string GetTypeName(Type type)
    {
      string name = type.Name;
      name = name.Replace('+', '.');
      int iGeneneric = name.IndexOf('`');
      if (iGeneneric > 0) {
        name = name.Substring(0, iGeneneric);
      }
      if (type.IsGenericType || type.IsGenericTypeDefinition) {
        var sb = new StringBuilder();
        sb.Append(name);
        sb.Append("<");
        Type[] args = type.GetGenericArguments();
        for (int i = 0, n = args.Length; i < n; i++) {
          if (i > 0) {
            sb.Append(",");
          }
          if (type.IsGenericType) {
            sb.Append(GetTypeName(args[i]));
          }
        }
        sb.Append(">");
        name = sb.ToString();
      }
      return name;
    }

    protected override Expression VisitConditional(ConditionalExpression c)
    {
      Visit(c.Test);
      WriteLine(Indentation.Inner);
      Write("? ");
      Visit(c.IfTrue);
      WriteLine(Indentation.Same);
      Write(": ");
      Visit(c.IfFalse);
      Indent(Indentation.Outer);
      return c;
    }

    protected override ReadOnlyCollection<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
    {
      for (int i = 0, n = original.Count; i < n; i++) {
        VisitBinding(original[i]);
        if (i < n - 1) {
          Write(",");
          WriteLine(Indentation.Same);
        }
      }
      return original;
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value==null) {
        Write("null");
      }
      else if (c.Type==typeof (string)) {
        string value = c.Value.ToString();
        if (value.IndexOfAny(special) >= 0)
          Write("@");
        Write("\"");
        Write(c.Value.ToString());
        Write("\"");
      }
      else if (c.Type==typeof (DateTime)) {
        Write("new DataTime(\"");
        Write(c.Value.ToString());
        Write("\")");
      }
      else if (c.Type.IsArray) {
        Type elementType = c.Type.GetElementType();
        VisitNewArray(
          Expression.NewArrayInit(
            elementType,
            ((IEnumerable) c.Value).OfType<object>().Select(v => (Expression) Expression.Constant(v, elementType))
            ));
      }
      else {
        Write(c.Value.ToString());
      }
      return c;
    }

    protected override ElementInit VisitElementInitializer(ElementInit initializer)
    {
      if (initializer.Arguments.Count > 1) {
        Write("{");
        for (int i = 0, n = initializer.Arguments.Count; i < n; i++) {
          Visit(initializer.Arguments[i]);
          if (i < n - 1) {
            Write(", ");
          }
        }
        Write("}");
      }
      else {
        Visit(initializer.Arguments[0]);
      }
      return initializer;
    }

    protected override ReadOnlyCollection<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
    {
      for (int i = 0, n = original.Count; i < n; i++) {
        VisitElementInitializer(original[i]);
        if (i < n - 1) {
          Write(",");
          WriteLine(Indentation.Same);
        }
      }
      return original;
    }

    protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> expressions)
    {
      for (int i = 0, n = expressions.Count; i < n; i++) {
        Visit(expressions[i]);
        if (i < n - 1) {
          Write(",");
          WriteLine(Indentation.Same);
        }
      }
      return expressions;
    }

    protected override Expression VisitInvocation(InvocationExpression i)
    {
      Write("Invoke(");
      WriteLine(Indentation.Inner);
      VisitExpressionList(i.Arguments);
      Write(", ");
      WriteLine(Indentation.Same);
      Visit(i.Expression);
      WriteLine(Indentation.Same);
      Write(")");
      Indent(Indentation.Outer);
      return i;
    }

    protected override Expression VisitLambda(LambdaExpression l)
    {
      if (l.Parameters.Count > 1) {
        Write("(");
        for (int i = 0, n = l.Parameters.Count; i < n; i++) {
          Write(l.Parameters[i].Name);
          if (i < n - 1) {
            Write(", ");
          }
        }
        Write(")");
      }
      else {
        Write(l.Parameters[0].Name);
      }
      Write(" => ");
      Visit(l.Body);
      return l;
    }

    protected override Expression VisitListInit(ListInitExpression li)
    {
      Visit(li.NewExpression);
      Write(" {");
      WriteLine(Indentation.Inner);
      VisitElementInitializerList(li.Initializers);
      WriteLine(Indentation.Outer);
      Write("}");
      return li;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      Visit(m.Expression);
      Write(".");
      Write(m.Member.Name);
      return m;
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
      Write(assignment.Member.Name);
      Write(" = ");
      Visit(assignment.Expression);
      return assignment;
    }

    protected override Expression VisitMemberInit(MemberInitExpression mi)
    {
      Visit(mi.NewExpression);
      Write(" {");
      WriteLine(Indentation.Inner);
      VisitBindingList(mi.Bindings);
      WriteLine(Indentation.Outer);
      Write("}");
      return mi;
    }

    protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
      Write(binding.Member.Name);
      Write(" = {");
      WriteLine(Indentation.Inner);
      VisitElementInitializerList(binding.Initializers);
      WriteLine(Indentation.Outer);
      Write("}");
      return binding;
    }

    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
      Write(binding.Member.Name);
      Write(" = {");
      WriteLine(Indentation.Inner);
      VisitBindingList(binding.Bindings);
      WriteLine(Indentation.Outer);
      Write("}");
      return binding;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Object!=null) {
        Visit(mc.Object);
      }
      else {
        Write(GetTypeName(mc.Method.DeclaringType));
      }
      Write(".");
      Write(mc.Method.Name);
      Write("(");
      if (mc.Arguments.Count > 1)
        WriteLine(Indentation.Inner);
      VisitExpressionList(mc.Arguments);
      if (mc.Arguments.Count > 1)
        WriteLine(Indentation.Outer);
      Write(")");
      return mc;
    }

    protected override Expression VisitNew(NewExpression n)
    {
      Write("new ");
      Write(GetTypeName(n.Constructor.DeclaringType));
      Write("(");
      if (n.Arguments.Count > 1)
        WriteLine(Indentation.Inner);
      VisitExpressionList(n.Arguments);
      if (n.Arguments.Count > 1)
        WriteLine(Indentation.Outer);
      Write(")");
      return n;
    }

    protected override Expression VisitNewArray(NewArrayExpression na)
    {
      Write("new ");
      Write(GetTypeName(TypeHelper.GetElementType(na.Type)));
      Write("[] {");
      if (na.Expressions.Count > 1)
        WriteLine(Indentation.Inner);
      VisitExpressionList(na.Expressions);
      if (na.Expressions.Count > 1)
        WriteLine(Indentation.Outer);
      Write("}");
      return na;
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      Write(p.Name);
      return p;
    }

    protected override Expression VisitTypeIs(TypeBinaryExpression tb)
    {
      Visit(tb.Expression);
      Write(" is ");
      Write(GetTypeName(tb.TypeOperand));
      return tb;
    }

    protected override Expression VisitUnknown(Expression e)
    {
      Write(e.ToString());
      return e;
    }


    // Constructors

    protected ExpressionWriter(TextWriter writer)
    {
      this.writer = writer;
      IndentationWidth = 2;
    }
  }
}