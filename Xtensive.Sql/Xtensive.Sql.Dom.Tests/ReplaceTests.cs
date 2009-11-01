using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Tests
{
  [TestFixture]
  public class ReplaceTests
  {
    private Table table1;
    private Table table2;

    [TestFixtureSetUp]
    public void SetUp()
    {
      Model m = new Model("default");
      m.CreateServer("localhost");
      m.DefaultServer.CreateCatalog("test");
      Schema schema1 = m.DefaultServer.DefaultCatalog.CreateSchema("dbo");

      table1 = schema1.CreateTable("table1");
      table1.CreateColumn("ID", new SqlValueType(SqlDataType.Int32));
      table1.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));

      table2 = schema1.CreateTable("table2");
      table2.CreateColumn("ID", new SqlValueType(SqlDataType.Int32));
      table2.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
    }
    
    [Test]
    public void SqlAggregateReplacingTest()
    {
      SqlAggregate a = Sql.Count();
      SqlAggregate aReplacing = Sql.Avg(1, true);
      a.ReplaceWith(aReplacing);

      bool passed = false;
      try {
        a.ReplaceWith(1);
      }
      catch {
        passed = true;
      }
      
      Assert.IsTrue(passed);
      Assert.AreNotEqual(a, aReplacing);
      Assert.AreEqual(a.NodeType, aReplacing.NodeType);
      Assert.AreEqual(a.Distinct, aReplacing.Distinct);
      Assert.AreEqual(a.Expression, aReplacing.Expression);
    }

    [Test]
    public void SqlArrayReplacingTest()
    {
      SqlArray<int> a = Sql.Array(new int[]{1, 2, 4});
      SqlArray<int> aReplacing = Sql.Array(new int[]{1, 2, 4, 8});
      a.ReplaceWith(aReplacing);

      bool passed = false;
      try {
        a.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(a, aReplacing);
      Assert.AreEqual(a.NodeType, aReplacing.NodeType);
      Assert.AreEqual(a.Values, aReplacing.Values);
    }

    [Test]
    public void SqlBinaryReplacingTest()
    {
      SqlBinary b = Sql.Literal(1) + 2;
      SqlBinary bReplacing = Sql.Divide(1, 2);
      b.ReplaceWith(bReplacing);

      bool passed = false;
      try {
        b.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(b, bReplacing);
      Assert.AreEqual(b.NodeType, bReplacing.NodeType);
      Assert.AreEqual(b.Left, bReplacing.Left);
      Assert.AreEqual(b.Right, bReplacing.Right);
    }

    [Test]
    public void SqlCaseReplacingTest()
    {
      SqlCase c1 = Sql.Case();
      SqlCase c2 = Sql.Case(Sql.Literal(1));
      c2.Else = Sql.Null;
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

      Assert.IsTrue(passed);
      Assert.AreNotEqual(c1, c2);
      Assert.AreEqual(c1.NodeType, c2.NodeType);
      Assert.AreEqual(c1.Value, c2.Value);
      Assert.AreEqual(c1.Else, c2.Else);
      Assert.AreEqual(c1.Count, c2.Count);
      foreach (KeyValuePair<SqlExpression, SqlExpression> p in c1)
        Assert.AreEqual(p.Value, c2[p.Key]);
    }

    [Test]
    public void SqlCastReplacingTest()
    {
      SqlCast c = Sql.Cast(1, SqlDataType.Float);
      SqlCast cReplacing = Sql.Cast(2, SqlDataType.Char);
      c.ReplaceWith(cReplacing);

      bool passed = false;
      try {
        c.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(c, cReplacing);
      Assert.AreEqual(c.NodeType, cReplacing.NodeType);
      Assert.AreEqual(c.Operand, cReplacing.Operand);
      Assert.AreEqual(c.Type, cReplacing.Type);
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
      SqlDefaultValue d = Sql.DefaultValue;
      SqlDefaultValue dReplacing = Sql.DefaultValue;
      d.ReplaceWith(dReplacing);

      bool passed = false;
      try {
        d.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreEqual(d, dReplacing);
    }

    [Test]
    public void SqlFunctionCallReplacingTest()
    {
      SqlFunctionCall fc = Sql.Length(" text ");
      SqlFunctionCall fcReplacing = Sql.Substring("text", 0, 2);
      fc.ReplaceWith(fcReplacing);

      bool passed = false;
      try {
        fc.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(fc, fcReplacing);
      Assert.AreEqual(fc.NodeType, fcReplacing.NodeType);
      Assert.AreEqual(fc.FunctionType, fcReplacing.FunctionType);
      Assert.AreEqual(fc.Arguments.Count, fcReplacing.Arguments.Count);
      for (int i = 0, l = fc.Arguments.Count; i<l; i++)
        Assert.AreEqual(fc.Arguments[i], fcReplacing.Arguments[i]);
    }

    [Test]
    public void SqlLikeReplacingTest()
    {
      SqlLike l = Sql.Like("text", "%");
      SqlLike lReplacing = Sql.Like("newText", "new%", '\\');
      l.ReplaceWith(lReplacing);

      bool passed = false;
      try {
        l.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(l, lReplacing);
      Assert.AreEqual(l.NodeType, lReplacing.NodeType);
      Assert.AreEqual(l.Expression, lReplacing.Expression);
      Assert.AreEqual(l.Pattern, lReplacing.Pattern);
      Assert.AreEqual(l.Escape, lReplacing.Escape);
    }

    [Test]
    public void SqlLiteralReplacingTest()
    {
      SqlLiteral<int> l = Sql.Literal(1);
      SqlLiteral<int> lReplacing = Sql.Literal(2);
      l.ReplaceWith(lReplacing);

      bool passed = false;
      try {
        l.ReplaceWith("string");
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(l, lReplacing);
      Assert.AreEqual(l.NodeType, lReplacing.NodeType);
      Assert.AreEqual(l.Value, lReplacing.Value);
    }

    [Test]
    public void SqlMatchReplacingTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlSelect s1 = Sql.Select(t);
      s1.Columns.Add(t[0]);
      s1.Columns.Add(Sql.Null, "ID");
      SqlSelect s2 = Sql.Select(t);
      s2.Columns.Add(t[0]);
      s2.Columns.Add(t[1]);
      SqlMatch m = Sql.Match(Sql.Row(1, "name"), s1, false, SqlMatchType.Partial);
      SqlMatch mReplacing = Sql.Match(Sql.Row(1, "name"), s2, true, SqlMatchType.None);
      m.ReplaceWith(mReplacing);

      bool passed = false;
      try {
        m.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(m, mReplacing);
      Assert.AreEqual(m.NodeType, mReplacing.NodeType);
      Assert.AreEqual(m.MatchType, mReplacing.MatchType);
      Assert.AreEqual(m.Unique, mReplacing.Unique);
      Assert.AreEqual(m.Value, mReplacing.Value);
      Assert.AreEqual(m.SubQuery, mReplacing.SubQuery);
    }

    [Test]
    public void SqlNullReplacingTest()
    {
      SqlNull n = Sql.Null;
      SqlNull nReplacing = Sql.Null;
      n.ReplaceWith(nReplacing);

      bool passed = false;
      try {
        n.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreEqual(n, nReplacing);
    }

    [Test]
    public void SqlParameterReplacingTest()
    {
      SqlParameterRef p = Sql.Parameter("pOld");
      SqlParameterRef pReplacing = Sql.Parameter("pNew");
      p.ReplaceWith(pReplacing);

      bool passed = false;
      try {
        p.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(p, pReplacing);
      Assert.AreEqual(p.NodeType, pReplacing.NodeType);
      Assert.AreEqual(p.Name, pReplacing.Name);
    }

    [Test]
    public void SqlRowReplacingTest()
    {
      SqlRow r = Sql.Row(1, 2, 4);
      SqlRow rReplacing = Sql.Row(1, 2, 4, "text", 'c');
      r.ReplaceWith(rReplacing);

      bool passed = false;
      try {
        r.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(r, rReplacing);
      Assert.AreEqual(r.NodeType, rReplacing.NodeType);
      Assert.AreEqual(r.Count, rReplacing.Count);
      for (int i = 0, l = r.Count; i<l; i++)
        Assert.AreEqual(r[i], rReplacing[i]);
    }

    [Test]
    public void SqlSubSelectReplacingTest()
    {
      SqlSelect s = Sql.Select();
      s.Columns.Add(1, "value");
      SqlSelect sReplacing = Sql.Select();
      sReplacing.Columns.Add(2, "value");
      SqlSubQuery ss = Sql.SubQuery(s);
      SqlSubQuery ssReplacing = Sql.SubQuery(sReplacing);
      ss.ReplaceWith(ssReplacing);

      bool passed = false;
      try {
        ss.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(ss, ssReplacing);
      Assert.AreEqual(ss.NodeType, ssReplacing.NodeType);
      Assert.AreEqual(ss.Query, ssReplacing.Query);
    }

    [Test]
    public void SqlUnaryReplacingTest()
    {
      SqlUnary u = -Sql.Literal(1);
      SqlUnary uReplacing = ~Sql.Literal(2);
      u.ReplaceWith(uReplacing);

      bool passed = false;
      try {
        u.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(u, uReplacing);
      Assert.AreEqual(u.NodeType, uReplacing.NodeType);
      Assert.AreEqual(u.Operand, uReplacing.Operand);
    }

    [Test]
    public void SqlUserFunctionCallReplacingTest()
    {
      SqlUserFunctionCall ufc = Sql.FunctionCall("Func1", 1, 2, 3);
      SqlUserFunctionCall ufcReplacing = Sql.FunctionCall("Func2", "string");
      ufc.ReplaceWith(ufcReplacing);

      bool passed = false;
      try {
        ufc.ReplaceWith(1);
      }
      catch {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(ufc, ufcReplacing);
      Assert.AreEqual(ufc.NodeType, ufcReplacing.NodeType);
      Assert.AreEqual(ufc.Name, ufcReplacing.Name);
      Assert.AreEqual(ufc.FunctionType, ufcReplacing.FunctionType);
      Assert.AreEqual(ufc.Arguments.Count, ufcReplacing.Arguments.Count);
      for (int i = 0, l = ufc.Arguments.Count; i < l; i++)
        Assert.AreEqual(ufc.Arguments[i], ufcReplacing.Arguments[i]);
    }

    [Test]
    public void SqlVariableReplacingTest()
    {
      SqlVariable v = Sql.Variable("vOld", SqlDataType.Int32);
      SqlVariable vReplacing = Sql.Variable("vNew", SqlDataType.Int32);
      v.ReplaceWith(vReplacing);

      bool passed = false;
      try {
        v.ReplaceWith(1);
      }
      catch
      {
        passed = true;
      }

      Assert.IsTrue(passed);
      Assert.AreNotEqual(v, vReplacing);
      Assert.AreEqual(v.NodeType, vReplacing.NodeType);
      Assert.AreEqual(v.Name, vReplacing.Name);
    }
  }
}
