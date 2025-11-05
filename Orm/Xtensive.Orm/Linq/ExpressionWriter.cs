// Copyright (C) 2008-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Linq
{
  /// <summary>
  /// Writes out an expression tree in a C#-ish syntax.
  /// </summary>
  public class ExpressionWriter : ExpressionVisitor
  {
    private static readonly char[] special = {'\n', '\n', '\\'};
    private static readonly char[] splitters = {'\n', '\r'};
    private readonly TextWriter writer;
    private readonly int indentSize;
    private int currentDepth;

    #region Nested type: IndentType

    /// <summary>
    /// Enumerates possible indent types.
    /// </summary>
    protected enum IndentType
    {
      /// <summary>
      /// The same indent must be used.
      /// </summary>
      Same,
      /// <summary>
      /// Indent must be 1-unit smaller then before.
      /// </summary>
      Inner,
      /// <summary>
      /// Indent must be 1-unit larger then before.
      /// </summary>
      Outer
    }

    #endregion

    /// <summary>
    /// Gets the writer used by the instance.
    /// </summary>
    public TextWriter Writer => writer;

    /// <summary>
    /// Gets the size of the indent.
    /// </summary>
    public int IndentSize => indentSize;

    /// <summary>
    /// Writes the expression to the specified writer.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    /// <param name="expression">The expression to write.</param>
    public static void Write(TextWriter writer, Expression expression) =>
      new ExpressionWriter(writer).Visit(expression);

    /// <summary>
    /// Writes the expression to string.
    /// </summary>
    /// <param name="expression">The expression to write.</param>
    /// <returns>The string containing written expression.</returns>
    public static string Write(Expression expression)
    {
      var sw = new StringWriter();
      Write(sw, expression);
      return sw.ToString();
    }

    /// <summary>
    /// Writes the line break.
    /// </summary>
    /// <param name="indentType">Type of the indent to use for the further lines.</param>
    protected void WriteLine(IndentType indentType)
    {
      writer.WriteLine();
      ChangeIndent(indentType);
      for (int i = 0, n = currentDepth * IndentSize; i < n; i++) {
        writer.Write(" ");
      }
    }

    /// <summary>
    /// Writes the specified text.
    /// </summary>
    /// <param name="text">The text to write.</param>
    protected void Write(string text)
    {
      if (text.IsNullOrEmpty()) {
        return;
      }

      if (text.IndexOf('\n') >= 0) {
        var lines = text.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0, n = lines.Length; i < n; i++) {
          Write(lines[i]);
          if (i < n - 1) {
            WriteLine(IndentType.Same);
          }
        }
      }
      else {
        writer.Write(text);
      }
    }

    /// <summary>
    /// Writes the list of arguments.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="suffix">The suffix.</param>
    protected void WriteArguments(string prefix, System.Collections.ObjectModel.ReadOnlyCollection<Expression> arguments, string suffix)
    {
      Write(prefix);
      if (arguments.Count > 1) {
        WriteLine(IndentType.Inner);
      }

      VisitExpressionList(arguments);
      if (arguments.Count > 1) {
        WriteLine(IndentType.Outer);
      }

      Write(suffix);
    }

    /// <summary>
    /// Changes the indent.
    /// </summary>
    /// <param name="indentType">New type of the indent.</param>
    protected void ChangeIndent(IndentType indentType)
    {
      if (indentType == IndentType.Inner) {
        currentDepth++;
      }
      else if (indentType == IndentType.Outer) {
        currentDepth--;
        Debug.Assert(currentDepth >= 0);
      }
    }

    /// <summary>
    /// Gets the C# operator for the specified expression type.
    /// </summary>
    /// <param name="type">The type of expression to get the operator for.</param>
    /// <returns>The C# operator.</returns>
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

    /// <inheritdoc/>
    protected override BinaryExpression VisitBinary(BinaryExpression b)
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
          Write("(");
          Visit(b.Left);
          Write(" ");
          Write(GetOperator(b.NodeType));
          Write(" ");
          Visit(b.Right);
          Write(")");
          break;
      }

      return b;
    }

    /// <inheritdoc/>
    protected override UnaryExpression VisitUnary(UnaryExpression u)
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
          Write("(");
          Visit(u.Operand);
          Write(" as ");
          Write(GetTypeName(u.Type));
          Write(")");
          break;
        case ExpressionType.UnaryPlus:
          Visit(u.Operand);
          break;
        default:
          Write(GetOperator(u.NodeType));
          Write("(");
          Visit(u.Operand);
          Write(")");
          break;
      }

      return u;
    }

    /// <summary>
    /// Gets the name of the type.
    /// </summary>
    /// <param name="type">The type to get the name for.</param>
    /// <returns>The name of the type.</returns>
    protected virtual string GetTypeName(Type type)
    {
      var name = type.GetShortName();
      name = name.Replace('+', '.');

      if (name.IndexOf("__DisplayClass", StringComparison.Ordinal) > 0 &&
        type.GetAttributes<CompilerGeneratedAttribute>(AttributeSearchOptions.InheritNone).Count > 0) {
        return "@";
      }

      if (name.IndexOf("__AnonymousType", StringComparison.Ordinal) > 0 &&
        type.GetAttributes<CompilerGeneratedAttribute>(AttributeSearchOptions.InheritNone).Count > 0) {
        return $"@<{(from pi in type.GetProperties() select pi.Name).ToCommaDelimitedString()}>";
      }

      return name;
    }

    /// <inheritdoc/>
    protected override ConditionalExpression VisitConditional(ConditionalExpression c)
    {
      Visit(c.Test);
      WriteLine(IndentType.Inner);
      Write("? ");
      Visit(c.IfTrue);
      WriteLine(IndentType.Same);
      Write(": ");
      Visit(c.IfFalse);
      ChangeIndent(IndentType.Outer);
      return c;
    }

    /// <inheritdoc/>
    protected override System.Collections.ObjectModel.ReadOnlyCollection<MemberBinding> VisitBindingList(System.Collections.ObjectModel.ReadOnlyCollection<MemberBinding> original)
    {
      for (int i = 0, n = original.Count; i < n; i++) {
        VisitBinding(original[i]);
        if (i < n - 1) {
          Write(",");
          WriteLine(IndentType.Same);
        }
      }

      return original;
    }

    /// <inheritdoc/>
    protected override ConstantExpression VisitConstant(ConstantExpression c)
    {
      var type = c.Type;
      if (type.Name.IndexOf("__DisplayClass", StringComparison.Ordinal) > 0 &&
        type.GetAttributes<CompilerGeneratedAttribute>(AttributeSearchOptions.InheritNone).Count > 0) {
        // A constant of display class type
        Write("@");
      }
      else if (c.Value == null) {
        Write("null");
      }
      else if (type == WellKnownTypes.String) {
        var value = c.Value.ToString();
        if (value.IndexOfAny(special) >= 0) {
          Write("@");
        }

        Write("\"");
        Write(c.Value.ToString());
        Write("\"");
      }
      else if (type == WellKnownTypes.DateTime) {
        Write("new DateTime(\"");
        Write(c.Value.ToString());
        Write("\")");
      }
      else if (type == WellKnownTypes.DateOnly) {
        Write("DateOnly.Parse(\"");
        Write(c.Value.ToString());
        Write("\")");
      }
      else if (type == WellKnownTypes.TimeOnly) {
        Write("TimeOnly.Parse(\"");
        Write(c.Value.ToString());
        Write("\")");
      }
      else if (c.Value is Type typeValue) {
        Write("typeof(");
        Write(GetTypeName(typeValue));
        Write(")");
      }
      else if (type.IsArray) {
        var elementType = type.GetElementType();
        VisitNewArray(
          Expression.NewArrayInit(
            elementType,
            ((IEnumerable) c.Value).OfType<object>().Select(v => (Expression) Expression.Constant(v, elementType))
          ));
      }
      else if (type.IsPrimitive) {
        Write(c.Value.ToString());
      }
      else {
        Write("$<");
        Write(GetTypeName(type));
        Write(">(");
        Write(c.Value.ToString());
        Write(")");
      }

      return c;
    }

    protected override DefaultExpression VisitDefault(DefaultExpression d)
    {
      Write("default(");
      Write(GetTypeName(d.Type));
      Write(")");
      return d;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override System.Collections.ObjectModel.ReadOnlyCollection<ElementInit> VisitElementInitializerList(
      System.Collections.ObjectModel.ReadOnlyCollection<ElementInit> original)
    {
      for (int i = 0, n = original.Count; i < n; i++) {
        VisitElementInitializer(original[i]);
        if (i < n - 1) {
          Write(",");
          WriteLine(IndentType.Same);
        }
      }

      return original;
    }

    /// <inheritdoc/>
    protected override System.Collections.ObjectModel.ReadOnlyCollection<Expression> VisitExpressionList(
      System.Collections.ObjectModel.ReadOnlyCollection<Expression> expressions)
    {
      for (int i = 0, n = expressions.Count; i < n; i++) {
        Visit(expressions[i]);
        if (i < n - 1) {
          Write(",");
          WriteLine(IndentType.Same);
        }
      }

      return expressions;
    }

    /// <inheritdoc/>
    protected override InvocationExpression VisitInvocation(InvocationExpression i)
    {
      Write("Invoke(");
      WriteLine(IndentType.Inner);
      VisitExpressionList(i.Arguments);
      Write(", ");
      WriteLine(IndentType.Same);
      Visit(i.Expression);
      WriteLine(IndentType.Same);
      Write(")");
      ChangeIndent(IndentType.Outer);
      return i;
    }

    /// <inheritdoc/>
    protected override LambdaExpression VisitLambda(LambdaExpression l)
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
      else if (l.Parameters.Count == 1) {
        Write(l.Parameters[0].Name);
      }
      else {
        Write("()");
      }

      Write(" => ");
      Visit(l.Body);
      return l;
    }

    /// <inheritdoc/>
    protected override ListInitExpression VisitListInit(ListInitExpression li)
    {
      Visit(li.NewExpression);
      Write(" {");
      WriteLine(IndentType.Inner);
      VisitElementInitializerList(li.Initializers);
      WriteLine(IndentType.Outer);
      Write("}");
      return li;
    }

    /// <inheritdoc/>
    protected override MemberExpression VisitMemberAccess(MemberExpression m)
    {
      Visit(m.Expression);
      Write(".");
      Write(m.Member.Name);
      return m;
    }

    /// <inheritdoc/>
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment ma)
    {
      Write(ma.Member.Name);
      Write(" = ");
      Visit(ma.Expression);
      return ma;
    }

    /// <inheritdoc/>
    protected override MemberInitExpression VisitMemberInit(MemberInitExpression mi)
    {
      Visit(mi.NewExpression);
      Write(" {");
      WriteLine(IndentType.Inner);
      VisitBindingList(mi.Bindings);
      WriteLine(IndentType.Outer);
      Write("}");
      return mi;
    }

    /// <inheritdoc/>
    protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
      Write(binding.Member.Name);
      Write(" = {");
      WriteLine(IndentType.Inner);
      VisitElementInitializerList(binding.Initializers);
      WriteLine(IndentType.Outer);
      Write("}");
      return binding;
    }

    /// <inheritdoc/>
    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
      Write(binding.Member.Name);
      Write(" = {");
      WriteLine(IndentType.Inner);
      VisitBindingList(binding.Bindings);
      WriteLine(IndentType.Outer);
      Write("}");
      return binding;
    }

    /// <inheritdoc/>
    protected override MethodCallExpression VisitMethodCall(MethodCallExpression mc)
    {
      var arguments = mc.Arguments;
      if (mc.Object != null) {
        Visit(mc.Object);
      }
      else {
        // Static method
        if (mc.Method.GetAttributes<ExtensionAttribute>(AttributeSearchOptions.InheritNone).Count > 0) {
          // A special case: extension method
          Visit(mc.Arguments[0]);
          arguments = new System.Collections.ObjectModel.ReadOnlyCollection<Expression>(mc.Arguments.Skip(1).ToList());
        }
        else {
          Write(GetTypeName(mc.Method.DeclaringType));
        }
      }

      Write(".");
      Write(mc.Method.Name);
      WriteArguments("(", arguments, ")");
      return mc;
    }

    /// <inheritdoc/>
    protected override NewExpression VisitNew(NewExpression n)
    {
      Write("new ");
      Write(GetTypeName(n.Type));
      WriteArguments("(", n.Arguments, ")");
      return n;
    }

    /// <inheritdoc/>
    protected override NewArrayExpression VisitNewArray(NewArrayExpression na)
    {
      Write("new ");
      Write(GetTypeName(SequenceHelper.GetElementType(na.Type)));
      WriteArguments("[] {", na.Expressions, "}");
      return na;
    }

    /// <inheritdoc/>
    protected override ParameterExpression VisitParameter(ParameterExpression p)
    {
      Write(p.Name);
      return p;
    }

    /// <inheritdoc/>
    protected override TypeBinaryExpression VisitTypeIs(TypeBinaryExpression tb)
    {
      Visit(tb.Expression);
      Write(" is ");
      Write(GetTypeName(tb.TypeOperand));
      return tb;
    }

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e)
    {
      Write(e.ToString());
      return e;
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    public ExpressionWriter(TextWriter writer)
      : this(writer, 2)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="indentSize">Size of the indent to use.</param>
    public ExpressionWriter(TextWriter writer, int indentSize)
    {
      this.writer = writer;
      this.indentSize = indentSize;
    }
  }
}