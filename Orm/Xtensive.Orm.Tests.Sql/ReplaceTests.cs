// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public class ReplaceTests
  {
    private Table table1;
    private Table table2;

    [OneTimeSetUp]
    public void SetUp()
    {
      var catalog = new Catalog("test");
      Schema schema1 = catalog.CreateSchema("dbo");

      table1 = schema1.CreateTable("table1");
      _ = table1.CreateColumn("ID", new SqlValueType(SqlType.Int32));
      _ = table1.CreateColumn("Name", new SqlValueType(SqlType.VarChar));

      table2 = schema1.CreateTable("table2");
      _ = table2.CreateColumn("ID", new SqlValueType(SqlType.Int32));
      _ = table2.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
    }
    
    [Test]
    public void SqlAggregateReplacingTest()
    {
      SqlAggregate a = SqlDml.Count();
      SqlAggregate aReplacing = SqlDml.Avg(1, true);
      a.ReplaceWith(aReplacing);

      bool passed = false;
      try {
        a.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(aReplacing, Is.Not.EqualTo(a));
      Assert.That(aReplacing.NodeType, Is.EqualTo(a.NodeType));
      Assert.That(aReplacing.Distinct, Is.EqualTo(a.Distinct));
      Assert.That(aReplacing.Expression, Is.EqualTo(a.Expression));
    }

    [Test]
    public void SqlArrayReplacingTest()
    {
      SqlArray<int> a = SqlDml.Array(new int[]{1, 2, 4});
      SqlArray<int> aReplacing = SqlDml.Array(new int[]{1, 2, 4, 8});
      a.ReplaceWith(aReplacing);

      bool passed = false;
      try {
        a.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(aReplacing, Is.Not.EqualTo(a));
      Assert.That(aReplacing.NodeType, Is.EqualTo(a.NodeType));
      Assert.That(aReplacing.Values, Is.EqualTo(a.Values));
    }

    [Test]
    public void SqlBinaryReplacingTest()
    {
      SqlBinary b = SqlDml.Literal(1) + 2;
      SqlBinary bReplacing = SqlDml.Divide(1, 2);
      b.ReplaceWith(bReplacing);

      bool passed = false;
      try {
        b.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(bReplacing, Is.Not.EqualTo(b));
      Assert.That(bReplacing.NodeType, Is.EqualTo(b.NodeType));
      Assert.That(bReplacing.Left, Is.EqualTo(b.Left));
      Assert.That(bReplacing.Right, Is.EqualTo(b.Right));
    }

    [Test]
    public void SqlCaseReplacingTest()
    {
      SqlCase c1 = SqlDml.Case();
      SqlCase c2 = SqlDml.Case(SqlDml.Literal(1));
      c2.Else = SqlDml.Null;
      c2[1] = 2;
      c2[2] = 3;
      c2[4] = 5;
      c2[6] = 7;
      c1.ReplaceWith(c2);

      bool passed = false;
      try {
        c1.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(c1, Is.Not.SameAs(c2));
      //Possibly NUnit compares items of IEnumirable
      //and if they the same it sugests two objects are equals 
      //Assert.AreNotEqual(c1, c2);
      Assert.That(c2.NodeType, Is.EqualTo(c1.NodeType));
      Assert.That(c2.Value, Is.EqualTo(c1.Value));
      Assert.That(c2.Else, Is.EqualTo(c1.Else));
      Assert.That(c2.Count, Is.EqualTo(c1.Count));
      foreach (KeyValuePair<SqlExpression, SqlExpression> p in c1)
        Assert.That(c2[p.Key], Is.EqualTo(p.Value));
    }

    [Test]
    public void SqlCastReplacingTest()
    {
      SqlCast c = SqlDml.Cast(1, SqlType.Float);
      SqlCast cReplacing = SqlDml.Cast(2, SqlType.Char);
      c.ReplaceWith(cReplacing);

      bool passed = false;
      try {
        c.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(cReplacing, Is.Not.EqualTo(c));
      Assert.That(cReplacing.NodeType, Is.EqualTo(c.NodeType));
      Assert.That(cReplacing.Operand, Is.EqualTo(c.Operand));
      Assert.That(cReplacing.Type, Is.EqualTo(c.Type));
    }

    [Test]
    public void SqlColumnReplacingTest()
    {
//      SqlTableColumn c = Sql.Column(1);
//      SqlTable t = Sql.DataTable(table1);
//      SqlTableColumn cReplacing = t[0];
//      c.ReplaceWith(cReplacing);
//
//      bool passed = false;
//      try {
//        c.ReplaceWith(1);
//      }
//      catch {
//        passed = true;
//      }
//
//      Assert.IsTrue(passed);
//      Assert.AreNotEqual(c, cReplacing);
//      Assert.AreEqual(c.NodeType, cReplacing.NodeType);
//      Assert.AreEqual(c.Name, cReplacing.Name);
//      Assert.AreEqual(c.Column, cReplacing.Column);
//      Assert.AreEqual(c.Expression, cReplacing.Expression);
//      Assert.AreEqual(c.SqlTable, cReplacing.SqlTable);
//
//      c.Dump(Console.Write);
//      Console.WriteLine();
//      cReplacing.Dump(Console.Write);
    }

    [Test]
    public void SqlDefaultValueReplacingTest()
    {
      SqlDefaultValue d = SqlDml.DefaultValue;
      SqlDefaultValue dReplacing = SqlDml.DefaultValue;
      d.ReplaceWith(dReplacing);

      bool passed = false;
      try {
        d.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(dReplacing, Is.EqualTo(d));
    }

    [Test]
    public void SqlFunctionCallReplacingTest()
    {
      SqlFunctionCall fc = SqlDml.CharLength(" text ");
      SqlFunctionCall fcReplacing = SqlDml.Substring("text", 0, 2);
      fc.ReplaceWith(fcReplacing);

      bool passed = false;
      try {
        fc.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(fcReplacing, Is.Not.EqualTo(fc));
      Assert.That(fcReplacing.NodeType, Is.EqualTo(fc.NodeType));
      Assert.That(fcReplacing.FunctionType, Is.EqualTo(fc.FunctionType));
      Assert.That(fcReplacing.Arguments.Count, Is.EqualTo(fc.Arguments.Count));
      for (int i = 0, l = fc.Arguments.Count; i<l; i++)
        Assert.That(fcReplacing.Arguments[i], Is.EqualTo(fc.Arguments[i]));
    }

    [Test]
    public void SqlLikeReplacingTest()
    {
      SqlLike l = SqlDml.Like("text", "%");
      SqlLike lReplacing = SqlDml.Like("newText", "new%", '\\');
      l.ReplaceWith(lReplacing);

      bool passed = false;
      try {
        l.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(lReplacing, Is.Not.EqualTo(l));
      Assert.That(lReplacing.NodeType, Is.EqualTo(l.NodeType));
      Assert.That(lReplacing.Expression, Is.EqualTo(l.Expression));
      Assert.That(lReplacing.Pattern, Is.EqualTo(l.Pattern));
      Assert.That(lReplacing.Escape, Is.EqualTo(l.Escape));
    }

    [Test]
    public void SqlLiteralReplacingTest()
    {
      SqlLiteral<int> l = SqlDml.Literal(1);
      SqlLiteral<int> lReplacing = SqlDml.Literal(2);
      l.ReplaceWith(lReplacing);

      bool passed = false;
      try {
        l.ReplaceWith("string");
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(lReplacing, Is.Not.EqualTo(l));
      Assert.That(lReplacing.NodeType, Is.EqualTo(l.NodeType));
      Assert.That(lReplacing.Value, Is.EqualTo(l.Value));
    }

    [Test]
    public void SqlMatchReplacingTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlSelect s1 = SqlDml.Select(t);
      s1.Columns.Add(t[0]);
      s1.Columns.Add(SqlDml.Null, "ID");
      SqlSelect s2 = SqlDml.Select(t);
      s2.Columns.Add(t[0]);
      s2.Columns.Add(t[1]);
      SqlMatch m = SqlDml.Match(SqlDml.Row(1, "name"), s1, false, SqlMatchType.Partial);
      SqlMatch mReplacing = SqlDml.Match(SqlDml.Row(1, "name"), s2, true, SqlMatchType.None);
      m.ReplaceWith(mReplacing);

      bool passed = false;
      try {
        m.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(mReplacing, Is.Not.EqualTo(m));
      Assert.That(mReplacing.NodeType, Is.EqualTo(m.NodeType));
      Assert.That(mReplacing.MatchType, Is.EqualTo(m.MatchType));
      Assert.That(mReplacing.Unique, Is.EqualTo(m.Unique));
      Assert.That(mReplacing.Value, Is.EqualTo(m.Value));
      Assert.That(mReplacing.SubQuery, Is.EqualTo(m.SubQuery));
    }

    [Test]
    public void SqlNullReplacingTest()
    {
      SqlNull n = SqlDml.Null;
      SqlNull nReplacing = SqlDml.Null;
      n.ReplaceWith(nReplacing);

      bool passed = false;
      try {
        n.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(nReplacing, Is.EqualTo(n));
    }

    [Test]
    public void SqlRowReplacingTest()
    {
      SqlRow r = SqlDml.Row(1, 2, 4);
      SqlRow rReplacing = SqlDml.Row(1, 2, 4, "text", 'c');
      r.ReplaceWith(rReplacing);

      bool passed = false;
      try {
        r.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(r, Is.Not.SameAs(rReplacing));
      Assert.That(rReplacing.NodeType, Is.EqualTo(r.NodeType));
      Assert.That(rReplacing.Count, Is.EqualTo(r.Count));
      for (int i = 0, l = r.Count; i<l; i++)
        Assert.That(rReplacing[i], Is.EqualTo(r[i]));
    }

    [Test, Ignore("FixGetEnumerator")]
    public void SqlSubSelectReplacingTest()
    {
      SqlSelect s = SqlDml.Select();
      s.Columns.Add(1, "value");
      SqlSelect sReplacing = SqlDml.Select();
      sReplacing.Columns.Add(2, "value");
      SqlSubQuery ss = SqlDml.SubQuery(s);
      SqlSubQuery ssReplacing = SqlDml.SubQuery(sReplacing);
      ss.ReplaceWith(ssReplacing);

      bool passed = false;
      try {
        ss.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(ssReplacing, Is.Not.EqualTo(ss));
      Assert.That(ssReplacing.NodeType, Is.EqualTo(ss.NodeType));
      Assert.That(ssReplacing.Query, Is.EqualTo(ss.Query));
    }

    [Test]
    public void SqlUnaryReplacingTest()
    {
      SqlUnary u = -SqlDml.Literal(1);
      SqlUnary uReplacing = ~SqlDml.Literal(2);
      u.ReplaceWith(uReplacing);

      bool passed = false;
      try {
        u.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(uReplacing, Is.Not.EqualTo(u));
      Assert.That(uReplacing.NodeType, Is.EqualTo(u.NodeType));
      Assert.That(uReplacing.Operand, Is.EqualTo(u.Operand));
    }

    [Test]
    public void SqlUserFunctionCallReplacingTest()
    {
      SqlUserFunctionCall ufc = SqlDml.FunctionCall("Func1", 1, 2, 3);
      SqlUserFunctionCall ufcReplacing = SqlDml.FunctionCall("Func2", "string");
      ufc.ReplaceWith(ufcReplacing);

      bool passed = false;
      try {
        ufc.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(ufcReplacing, Is.Not.EqualTo(ufc));
      Assert.That(ufcReplacing.NodeType, Is.EqualTo(ufc.NodeType));
      Assert.That(ufcReplacing.Name, Is.EqualTo(ufc.Name));
      Assert.That(ufcReplacing.FunctionType, Is.EqualTo(ufc.FunctionType));
      Assert.That(ufcReplacing.Arguments.Count, Is.EqualTo(ufc.Arguments.Count));
      for (int i = 0, l = ufc.Arguments.Count; i < l; i++)
        Assert.That(ufcReplacing.Arguments[i], Is.EqualTo(ufc.Arguments[i]));
    }

    [Test]
    public void SqlVariableReplacingTest()
    {
      SqlVariable v = SqlDml.Variable("vOld", SqlType.Int32);
      SqlVariable vReplacing = SqlDml.Variable("vNew", SqlType.Int32);
      v.ReplaceWith(vReplacing);

      bool passed = false;
      try {
        v.ReplaceWith(1);
      }
      catch
      {
        passed = true;
      }

      Assert.That(passed, Is.True);
      Assert.That(vReplacing, Is.Not.EqualTo(v));
      Assert.That(vReplacing.NodeType, Is.EqualTo(v.NodeType));
      Assert.That(vReplacing.Name, Is.EqualTo(v.Name));
    }
  }
}
