using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Tests
{
  [TestFixture]
  public class CloneTests
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
    public void SqlExpressionCloneTest()
    {
      SqlExpression e = Sql.Literal(1);
      SqlExpression eClone = (SqlExpression)e.Clone();
      Assert.AreNotEqual(e, eClone);
      Assert.AreEqual(e.NodeType, eClone.NodeType);
    }
    
    [Test]
    public void SqlLiteralCloneTest()
    {
      SqlLiteral<int> l = Sql.Literal(1);
      SqlLiteral<int> lClone = (SqlLiteral<int>)l.Clone();
      Assert.AreNotEqual(l, lClone);
      Assert.AreEqual(l.Value, lClone.Value);
      Assert.AreEqual(l.NodeType, lClone.NodeType);
    }

    [Test]
    public void SqlArrayCloneTest()
    {
      SqlArray<int> a = Sql.Array(new int[]{1, 2, 4});
      SqlArray<int> aClone = (SqlArray<int>)a.Clone();
      
      Assert.AreNotEqual(a, aClone);
      Assert.IsTrue(a.Values != aClone.Values);
      Assert.AreEqual(a.Values.Length, aClone.Values.Length);
      for (int i = 0, l = a.Values.Length; i < l; i++)
        Assert.AreEqual(a.Values[i], aClone.Values[i]);
      Assert.AreEqual(a.NodeType, aClone.NodeType);
    }

    [Test]
    public void SqlBinaryCloneTest()
    {
      SqlBinary b = Sql.Literal(2) > 1;
      SqlBinary bClone = (SqlBinary)b.Clone();
      
      Assert.AreNotEqual(b, bClone);
      Assert.AreNotEqual(b.Left, bClone.Left);
      Assert.AreNotEqual(b.Right, bClone.Right);
      Assert.AreEqual(b.NodeType, bClone.NodeType);
      Assert.AreEqual(b.Left.NodeType, bClone.Left.NodeType);
      Assert.AreEqual(b.Right.NodeType, bClone.Right.NodeType);
    }

    [Test]
    public void SqlCaseCloneTest()
    {
      SqlCase c = Sql.Case();
      c.Value = Sql.Literal(1);
      c[Sql.Literal(1)] = Sql.Literal("A");
      c[Sql.Literal(2)] = Sql.Literal("B");
      c.Else = Sql.Literal("C");
      
      SqlCase cClone = (SqlCase) c.Clone();
      
      Assert.AreNotEqual(c, cClone);
      Assert.AreNotEqual(c.Value, cClone.Value);
      Assert.AreNotEqual(c.Else, cClone.Else);
      Assert.AreEqual(c.NodeType, cClone.NodeType);
      Assert.AreEqual(c.Value.NodeType, cClone.Value.NodeType);
      Assert.AreEqual(c.Else.NodeType, cClone.Else.NodeType);
      Assert.AreEqual(c.Count, cClone.Count);
    }

    [Test]
    public void SqlCastCloneTest()
    {
      SqlCast c = Sql.Cast(Sql.Literal(1), SqlDataType.Decimal, 6 ,4);
      SqlCast cClone = (SqlCast)c.Clone();
      
      Assert.AreNotEqual(c, cClone);
      Assert.AreNotEqual(c.Operand, cClone.Operand);
      Assert.AreEqual(c.NodeType, cClone.NodeType);
      Assert.AreEqual(c.Operand.NodeType, cClone.Operand.NodeType);
      Assert.AreEqual(c.Type, cClone.Type);
    }

    [Test]
    public void SqlColumnCloneTest()
    {
      {
        SqlUserColumn c = Sql.Column(1);
        SqlUserColumn cClone = (SqlUserColumn)c.Clone();

        Assert.AreNotEqual(c, cClone);
        Assert.AreNotEqual(c.Expression, cClone.Expression);
        Assert.AreEqual(c.NodeType, cClone.NodeType);
        Assert.AreEqual(c.Expression.NodeType, cClone.Expression.NodeType);
      }
      Console.WriteLine();
      {
        SqlTableRef t = Sql.TableRef(table1);
        SqlTableColumn c = (SqlTableColumn)t[0];
        SqlTableColumn cClone = (SqlTableColumn)c.Clone();

        Assert.AreNotEqual(c, cClone);
        Assert.AreEqual(c.SqlTable, cClone.SqlTable);
        Assert.AreEqual(c.NodeType, cClone.NodeType);
      }
    }

    [Test]
    public void SqlFunctionCallCloneTest()
    {
      {
        SqlFunctionCall fc = Sql.FunctionCall("Function", 1, 2, 4);
        SqlFunctionCall fcClone = (SqlFunctionCall) fc.Clone();

        Assert.AreNotEqual(fc, fcClone);
        Assert.AreNotEqual(fc.Arguments, fcClone.Arguments);
        Assert.AreEqual(fc.NodeType, fcClone.NodeType);
        Assert.AreEqual(fc.Arguments.Count, fcClone.Arguments.Count);
        for (int i = 0, l = fc.Arguments.Count; i < l; i++) {
          Assert.AreNotEqual(fc.Arguments[i], fcClone.Arguments[i]);
          Assert.AreEqual(fc.Arguments[i].NodeType, fcClone.Arguments[i].NodeType);
        }
        Assert.AreEqual(fc.FunctionType, fcClone.FunctionType);
        Assert.AreEqual(((SqlUserFunctionCall) fc).Name, ((SqlUserFunctionCall) fcClone).Name);
      }
      Console.WriteLine();
      {
        SqlFunctionCall fc = Sql.Length("string");
        SqlFunctionCall fcClone = (SqlFunctionCall)fc.Clone();

        Assert.AreNotEqual(fc, fcClone);
        Assert.AreNotEqual(fc.Arguments, fcClone.Arguments);
        for (int i = 0, l = fc.Arguments.Count; i < l; i++) {
          Assert.AreNotEqual(fc.Arguments[i], fcClone.Arguments[i]);
          Assert.AreEqual(fc.Arguments[i].NodeType, fcClone.Arguments[i].NodeType);
        }
        Assert.AreEqual(fc.NodeType, fcClone.NodeType);
        Assert.AreEqual(fc.Arguments.Count, fcClone.Arguments.Count);
        Assert.AreEqual(fc.FunctionType, fcClone.FunctionType);
      }
    }

    [Test]
    public void SqlLikeCloneTest()
    {
      {
        SqlLike l = Sql.Like("epxression", "e%", "\\");
        SqlLike lClone = (SqlLike) l.Clone();

        Assert.AreNotEqual(l, lClone);
        Assert.AreNotEqual(l.Expression, lClone.Expression);
        Assert.AreNotEqual(l.Pattern, lClone.Pattern);
        Assert.AreNotEqual(l.Escape, lClone.Escape);
        Assert.AreEqual(l.NodeType, lClone.NodeType);
        Assert.AreEqual(l.Expression.NodeType, lClone.Expression.NodeType);
        Assert.AreEqual(l.Pattern.NodeType, lClone.Pattern.NodeType);
        Assert.AreEqual(l.Escape.NodeType, lClone.Escape.NodeType);
      }
      Console.WriteLine();
      {
        SqlLike l = Sql.Like("epxression", "e%");
        SqlLike lClone = (SqlLike)l.Clone();

        Assert.AreNotEqual(l, lClone);
        Assert.AreNotEqual(l.Expression, lClone.Expression);
        Assert.AreNotEqual(l.Pattern, lClone.Pattern);
        Assert.AreEqual(l.NodeType, lClone.NodeType);
        Assert.AreEqual(l.Expression.NodeType, lClone.Expression.NodeType);
        Assert.AreEqual(l.Pattern.NodeType, lClone.Pattern.NodeType);
        Assert.AreEqual(l.Escape, null);
      }
    }

    [Test]
    public void SqlParameterCloneTest()
    {
      SqlParameterRef p = Sql.Parameter("p");
      SqlParameterRef pClone = (SqlParameterRef)p.Clone();
      
      Assert.AreNotEqual(p, pClone);
      Assert.AreEqual(p.NodeType, pClone.NodeType);
      Assert.AreEqual(p.Name, pClone.Name);
    }

    [Test]
    public void SqlRowCloneTest()
    {
      SqlRow r = Sql.Row(1, 2, 4, Sql.Literal(6) + 5);
      SqlRow rClone = (SqlRow)r.Clone();

      Assert.AreNotEqual(r, rClone);
      Assert.AreEqual(r.NodeType, rClone.NodeType);
      Assert.AreEqual(r.Count, rClone.Count);

      for (int i = 0, l = r.Count; i < l; i++) {
        Assert.AreNotEqual(r[i], rClone[i]);
        Assert.AreEqual(r[i].NodeType, rClone[i].NodeType);
      }
    }

    [Test]
    public void SqlUnaryCloneTest()
    {
      SqlUnary u = -Sql.Literal(1);
      SqlUnary uClone = (SqlUnary)u.Clone();
      
      Assert.AreNotEqual(u, uClone);
      Assert.AreEqual(u.NodeType, uClone.NodeType);
    }

    [Test]
    public void SqlVariableCloneTest()
    {
      SqlVariable v = Sql.Variable("v", SqlDataType.Int32);
      SqlVariable vClone = (SqlVariable)v.Clone();

      Assert.AreNotEqual(v, vClone);
      Assert.AreEqual(v.NodeType, vClone.NodeType);
      Assert.AreEqual(v.Name, vClone.Name);
    }

    [Test]
    public void SqlAggregateCloneTest()
    {
      {
        SqlAggregate a = Sql.Count();
        SqlAggregate aClone = (SqlAggregate) a.Clone();

        Assert.AreNotEqual(a, aClone);
        Assert.AreEqual(a.NodeType, aClone.NodeType);
        Assert.AreEqual(a.Distinct, aClone.Distinct);
        Assert.AreEqual(aClone.Expression.NodeType, SqlNodeType.Constant);
      }
      Console.WriteLine();
      {
        SqlAggregate a = Sql.Sum(1);
        SqlAggregate aClone = (SqlAggregate)a.Clone();

        Assert.AreNotEqual(a, aClone);
        Assert.AreNotEqual(a.Expression, aClone.Expression);
        Assert.AreEqual(a.NodeType, aClone.NodeType);
        Assert.AreEqual(a.Distinct, aClone.Distinct);
        Assert.AreEqual(a.Expression.NodeType, aClone.Expression.NodeType);
      }
    }

    [Test]
    public void SqlTableRefCloneTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlTableRef tClone = (SqlTableRef)t.Clone();

      Assert.AreNotEqual(t, tClone);
      Assert.AreNotEqual(t.Columns, tClone.Columns);
      Assert.AreEqual(t.NodeType, tClone.NodeType);
      Assert.AreEqual(t.Name, tClone.Name);
      Assert.AreEqual(t.DataTable, tClone.DataTable);
      Assert.AreEqual(t.Columns.Count, tClone.Columns.Count);

      for (int i = 0, l = t.Columns.Count; i < l; i++) {
        Assert.AreNotEqual(t.Columns[i], tClone.Columns[i]);
        Assert.AreEqual(t.Columns[i].NodeType, tClone.Columns[i].NodeType);
        Assert.AreEqual(t.Columns[i].GetType(), tClone.Columns[i].GetType());
      }
    }

    [Test]
    public void SqlQueryRefCloneTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlSelect s = Sql.Select(t);
      s.Columns.Add(t.Columns[0]);

      SqlQueryRef qr = Sql.QueryRef(s);
      SqlQueryRef qrClone = (SqlQueryRef)qr.Clone();

      Assert.AreNotEqual(qr, qrClone);
      Assert.AreNotEqual(qr.Columns, qrClone.Columns);
      Assert.AreEqual(qr.NodeType, qrClone.NodeType);
      Assert.AreEqual(qr.Name, qrClone.Name);
      Assert.AreEqual(qr.Query, qrClone.Query);
      Assert.AreEqual(qr.Columns.Count, qrClone.Columns.Count);

      for (int i = 0, l = qr.Columns.Count; i < l; i++) {
        Assert.AreNotEqual(qr.Columns[i], qrClone.Columns[i]);
        Assert.AreEqual(qr.Columns[i].NodeType, qrClone.Columns[i].NodeType);
        Assert.AreEqual(qr.Columns[i].GetType(), qrClone.Columns[i].GetType());
      }
    }

    [Test]
    public void SqlSubSelectCloneTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlSelect s = Sql.Select(t);
      s.Columns.Add(t.Columns[0]);

      SqlSubQuery ss = Sql.SubQuery(s);
      SqlSubQuery ssClone = (SqlSubQuery)ss.Clone();

      Assert.AreNotEqual(ss, ssClone);
      Assert.AreNotEqual(ss.Query, ssClone.Query);
      Assert.AreEqual(ss.NodeType, ssClone.NodeType);
      Assert.AreEqual(ss.Query.NodeType, ssClone.Query.NodeType);
    }

    [Test]
    public void SqlMatchCloneTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlSelect s = Sql.Select(t);
      s.Columns.Add(t.Columns[0]);

      SqlMatch m = Sql.Match(Sql.Row(4), s, true, SqlMatchType.Full);
      SqlMatch mClone = (SqlMatch)m.Clone();

      Assert.AreNotEqual(m, mClone);
      Assert.AreNotEqual(m.Value, mClone.Value);
      Assert.AreNotEqual(m.SubQuery, mClone.SubQuery);
      Assert.AreEqual(m.NodeType, mClone.NodeType);
      Assert.AreEqual(m.Value.NodeType, mClone.Value.NodeType);
      Assert.AreEqual(m.SubQuery.NodeType, mClone.SubQuery.NodeType);
      Assert.AreEqual(m.Unique, mClone.Unique);
      Assert.AreEqual(m.MatchType, mClone.MatchType);
    }
    
    [Test]
    public void SqlSelectCloneTest()
    {
      SqlTableRef tr1 = Sql.TableRef(table1);
      SqlTableRef tr2 = Sql.TableRef(table2);

      SqlSelect s = Sql.Select();
      s.Distinct = true;
      s.Top = 3;
      s.Columns.Add(tr1["ID"]);
      s.Columns.Add(tr1["ID"], "ID2");
      s.Columns.Add(tr1["ID"] + tr1["ID"], "SUM2");
      s.Columns.Add(tr2.Asterisk);
      s.From = tr1.InnerJoin(tr2, tr1["ID"] == tr2["ID"]);
      s.Where = Sql.Like(tr1["Name"], "Marat");
      s.Hints.Add(Sql.FastFirstRowsHint(10));

      SqlSelect sClone = (SqlSelect)s.Clone();
      
      Assert.AreNotEqual(s, sClone);
      Assert.AreNotEqual(s.Columns, sClone.Columns);
      for (int i = 0, l = s.Columns.Count; i < l; i++) {
        Assert.AreNotEqual(s.Columns[i], sClone.Columns[i]);
      }
      for (int i = 0, l = s.GroupBy.Count; i < l; i++) {
        Assert.AreNotEqual(s.Columns[i], sClone.Columns[i]);
      }
      Assert.AreEqual(s.Distinct, sClone.Distinct);
      if (s.From == null)
        Assert.AreEqual(s.From, sClone.From);
      else {
        Assert.AreNotEqual(s.From, sClone.From);
        Assert.AreEqual(s.From.NodeType, sClone.From.NodeType);
      }
      if (!SqlExpression.IsNull(s.Having)) {
        Assert.AreNotEqual(s.Having, sClone.Having);
        Assert.AreEqual(s.Having.NodeType, sClone.Having.NodeType);
      }
      
      Assert.AreEqual(s.NodeType, sClone.NodeType);
      Assert.IsFalse(s.OrderBy == sClone.OrderBy);
      Assert.AreEqual(s.OrderBy.Count, sClone.OrderBy.Count);
      for (int i = 0, l = s.OrderBy.Count; i < l; i++) {
        Assert.AreNotEqual(s.OrderBy[i], sClone.OrderBy[i]);
        Assert.AreEqual(s.OrderBy[i].Ascending, sClone.OrderBy[i].Ascending);
        Assert.AreNotEqual(s.OrderBy[i].Expression, sClone.OrderBy[i].Expression);
        Assert.AreEqual(s.OrderBy[i].Position, sClone.OrderBy[i].Position);
      }

      Assert.AreEqual(s.Top, sClone.Top);
      if (s.Where != null) {
        Assert.AreNotEqual(s.Where, sClone.Where);
        Assert.AreEqual(s.Where.NodeType, sClone.Where.NodeType);
      }

      s.Where &= tr1[0] > 1200 || tr2[1] != "Marat";
      s.OrderBy.Add(tr1["ID"], false);
      s.OrderBy.Add(2);

      sClone = (SqlSelect)s.Clone();

      Assert.AreNotEqual(s, sClone);
      Assert.AreNotEqual(s.Columns, sClone.Columns);
      for (int i = 0, l = s.Columns.Count; i < l; i++) {
        Assert.AreNotEqual(s.Columns[i], sClone.Columns[i]);
      }
      for (int i = 0, l = s.GroupBy.Count; i < l; i++) {
        Assert.AreNotEqual(s.Columns[i], sClone.Columns[i]);
      }
      Assert.AreEqual(s.Distinct, sClone.Distinct);
      if (s.From == null)
        Assert.AreEqual(s.From, sClone.From);
      else {
        Assert.AreNotEqual(s.From, sClone.From);
        Assert.AreEqual(s.From.NodeType, sClone.From.NodeType);
      }
      if (s.Having != null) {
        Assert.AreNotEqual(s.Having, sClone.Having);
        Assert.AreEqual(s.Having.NodeType, sClone.Having.NodeType);
      }

      Assert.AreEqual(s.NodeType, sClone.NodeType);
      Assert.AreNotEqual(s.OrderBy, sClone.OrderBy);
      Assert.AreEqual(s.OrderBy.Count, sClone.OrderBy.Count);
      for (int i = 0, l = s.OrderBy.Count; i < l; i++) {
        Assert.AreNotEqual(s.OrderBy[i], sClone.OrderBy[i]);
        Assert.AreEqual(s.OrderBy[i].Ascending, sClone.OrderBy[i].Ascending);
        if (SqlExpression.IsNull(s.OrderBy[i].Expression))
          Assert.AreEqual(s.OrderBy[i].Expression, sClone.OrderBy[i].Expression);
        else
          Assert.AreNotEqual(s.OrderBy[i].Expression, sClone.OrderBy[i].Expression);
        Assert.AreEqual(s.OrderBy[i].Position, sClone.OrderBy[i].Position);
      }

      Assert.AreEqual(s.Top, sClone.Top);
      if (s.Where != null) {
        Assert.AreNotEqual(s.Where, sClone.Where);
        Assert.AreEqual(s.Where.NodeType, sClone.Where.NodeType);
      }
      Assert.AreEqual(s.Hints.Count, sClone.Hints.Count);

      SqlSelect s2 = Sql.Select();
      SqlQueryRef t = Sql.QueryRef(s, "SUBSELECT");
      s2.From = t;
      s2.Columns.Add(t.Asterisk);
      SqlSelect s2Clone = (SqlSelect) s2.Clone();
      
      Assert.AreNotEqual(s2, s2Clone);
      Assert.AreEqual(s2.Columns.Count, s2Clone.Columns.Count);
    }

    [Test]
    public void SqlOrderCloneTest()
    {
      {
        SqlOrder o = Sql.Order(2, true);
        SqlOrder oClone = (SqlOrder) o.Clone();

        Assert.AreNotEqual(o, oClone);
        Assert.AreEqual(o.NodeType, oClone.NodeType);
        Assert.AreEqual(o.Ascending, oClone.Ascending);
        Assert.AreEqual(o.Expression, oClone.Expression);
        Assert.AreEqual(o.Position, oClone.Position);
      }

      {
        SqlOrder o = Sql.Order(Sql.Column(Sql.Literal(2)), false);
        SqlOrder oClone = (SqlOrder)o.Clone();

        Assert.AreNotEqual(o, oClone);
        Assert.AreEqual(o.NodeType, oClone.NodeType);
        Assert.AreEqual(o.Ascending, oClone.Ascending);
        Assert.AreNotEqual(o.Expression, oClone.Expression);
        Assert.AreEqual(o.Position, oClone.Position);
      }
    }
    
    [Test]
    public void SqlAssignCloneTest()
    {
      SqlParameterRef p = Sql.Parameter("p");
      SqlAssignment a = Sql.Assign(p, 1);
      SqlAssignment aClone = (SqlAssignment) a.Clone();
      
      Assert.AreNotEqual(a, aClone);
      Assert.AreEqual(a.NodeType, aClone.NodeType);
      Assert.AreNotEqual(a.Left, aClone.Left);
      Assert.AreEqual(a.Left.NodeType, aClone.Left.NodeType);
      Assert.AreNotEqual(a.Right, aClone.Right);
      Assert.AreEqual(a.Right.NodeType, aClone.Right.NodeType);
    }

    [Test]
    public void SqlBatchCloneTest()
    {
      SqlParameterRef p = Sql.Parameter("p");
      SqlAssignment a = Sql.Assign(p, 1);
      SqlBatch b = Sql.Batch();
      b.Add(a);
      b.Add(a);
      SqlBatch bClone = (SqlBatch) b.Clone();

      Assert.AreNotEqual(b, bClone);
      Assert.AreEqual(b.NodeType, bClone.NodeType);
      Assert.AreEqual(b.Count, bClone.Count);
      foreach (SqlStatement s in b)
        Assert.IsFalse(bClone.Contains(s));
    }

    [Test]
    public void SqlDeclareVariableCloneTest()
    {
      {
        SqlVariable dv = Sql.Variable("v", SqlDataType.Char, 5);
        SqlVariable dvClone = (SqlVariable) dv.Clone();

        Assert.AreNotEqual(dv, dvClone);
        Assert.AreEqual(dv.NodeType, dvClone.NodeType);
        Assert.IsTrue(dv.Type.Equals(dvClone.Type));
        Assert.AreEqual(dv.Name, dvClone.Name);
      }

      {
        SqlVariable dv = Sql.Variable("v", SqlDataType.Decimal, 6, 4);
        SqlVariable dvClone = (SqlVariable)dv.Clone();

        Assert.AreNotEqual(dv, dvClone);
        Assert.AreEqual(dv.NodeType, dvClone.NodeType);
        Assert.IsTrue(dv.Type.Equals(dvClone.Type));
        Assert.AreEqual(dv.Name, dvClone.Name);
      }
    }

    [Test]
    public void SqlDeleteCloneTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlDelete d = Sql.Delete(t);
      d.Top = 5;
      d.Where = t[0] < 6;
      d.Hints.Add(Sql.FastFirstRowsHint(10));
      SqlDelete dClone = (SqlDelete) d.Clone();

      Assert.AreNotEqual(d, dClone);
      Assert.AreNotEqual(d.From, dClone.From);
      Assert.AreEqual(d.NodeType, dClone.NodeType);
      Assert.AreEqual(d.Top, dClone.Top);
      Assert.AreEqual(d.Hints.Count, dClone.Hints.Count);
      if (!SqlExpression.IsNull(d.Where)) {
        Assert.AreNotEqual(d.Where, dClone.Where);
        Assert.AreEqual(d.Where.NodeType, dClone.Where.NodeType);
      }
      else
        Assert.AreEqual(dClone.Where, null);
    }
    
    [Test]
    public void SqlIfCloneTest()
    {
      SqlSelect ifTrue = Sql.Select();
      ifTrue.Columns.Add(1, "id");
      SqlSelect ifFalse = Sql.Select();
      ifFalse.Columns.Add(2, "id");
      
      SqlIf i = Sql.If(Sql.SubQuery(ifTrue) > 0, ifTrue);
      SqlIf iClone = (SqlIf) i.Clone();
      
      Assert.AreNotEqual(i, iClone);
      Assert.AreEqual(i.NodeType, iClone.NodeType);
      Assert.AreNotEqual(i.Condition, iClone.Condition);
      Assert.AreEqual(i.Condition.NodeType, iClone.Condition.NodeType);
      Assert.AreNotEqual(i.True, iClone.True);
      Assert.AreNotEqual(i.True.NodeType, iClone.NodeType);
      Assert.AreEqual(iClone.False, null);

      Console.WriteLine();
      
      i.False = ifFalse;
      iClone = (SqlIf)i.Clone();

      Assert.AreNotEqual(i, iClone);
      Assert.AreEqual(i.NodeType, iClone.NodeType);
      Assert.AreNotEqual(i.Condition, iClone.Condition);
      Assert.AreEqual(i.Condition.NodeType, iClone.Condition.NodeType);
      Assert.AreNotEqual(i.True, iClone.True);
      Assert.AreNotEqual(i.True.NodeType, iClone.NodeType);
      Assert.AreNotEqual(i.False, iClone.False);
      Assert.AreEqual(i.False.NodeType, iClone.False.NodeType);
    }

    [Test]
    public void SqlInsertCloneTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlInsert i = Sql.Insert(t);
      i.Top = 5;
      i.Values[t[0]] = 1;
      i.Values[t[1]] = "Anonym";
      i.Hints.Add(Sql.FastFirstRowsHint(10));
      SqlInsert iClone = (SqlInsert)i.Clone();

      Assert.AreNotEqual(i, iClone);
      Assert.AreNotEqual(i.Into, iClone.Into);
      Assert.AreEqual(i.NodeType, iClone.NodeType);
      Assert.AreEqual(i.Top, iClone.Top);
      Assert.AreEqual(i.Values.Count, iClone.Values.Count);
      foreach (KeyValuePair<SqlColumn, SqlExpression> p in i.Values) {
        Assert.IsFalse(iClone.Values.ContainsKey(p.Key));
        Assert.IsFalse(iClone.Values.ContainsValue(p.Value));
      }
      Assert.AreEqual(i.Hints.Count, iClone.Hints.Count);
    }

    [Test]
    public void SqlUpdateCloneTest()
    {
      SqlTableRef t = Sql.TableRef(table1);
      SqlUpdate u = Sql.Update(t);
      u.Top = 5;
      u.Values[t[0]] = 1;
      u.Values[t[1]] = "Anonym";
      u.Where = t.Columns["ID"] == 1;
      u.Hints.Add(Sql.FastFirstRowsHint(10));
      SqlUpdate uClone = (SqlUpdate)u.Clone();

      Assert.AreNotEqual(u, uClone);
      Assert.AreNotEqual(u.Update, uClone.Update);
      Assert.AreEqual(u.NodeType, uClone.NodeType);
      Assert.AreEqual(u.Top, uClone.Top);
      Assert.AreEqual(u.Values.Count, uClone.Values.Count);
      foreach (KeyValuePair<ISqlLValue, SqlExpression> p in u.Values) {
        Assert.IsFalse(uClone.Values.ContainsKey(p.Key));
        Assert.IsFalse(uClone.Values.ContainsValue(p.Value));
      }
      if (u.Where != null) {
        Assert.AreNotEqual(u.Where, uClone.Where);
        Assert.AreEqual(u.Where.NodeType, uClone.Where.NodeType);
      }
      else
        Assert.AreEqual(uClone.Where, null);
      Assert.AreEqual(u.Hints.Count, uClone.Hints.Count);
    }
    
    [Test]
    public void SqlWhileCloneTest()
    {
      SqlVariable i = Sql.Variable("i", SqlDataType.Int32);
      SqlWhile w = Sql.While(i<=1000);
      SqlBatch b = Sql.Batch();
      b.Add(Sql.Assign(i, i+1));
      SqlTableRef t = Sql.TableRef(table1);
      SqlSelect s = Sql.Select(t);
      s.Columns.Add(t["Name"]);
      s.Where = t[0]==i;
      SqlIf f = Sql.If(Sql.SubQuery(s)=="Unkown", Sql.Break, Sql.Continue);
      b.Add(f);
      w.Statement = b;

      SqlWhile wClone = (SqlWhile) w.Clone();
      
      Assert.AreNotEqual(w, wClone);
      Assert.AreEqual(w.NodeType, wClone.NodeType);
      Assert.AreNotEqual(w.Condition, wClone.Condition);
      Assert.AreEqual(w.Condition.NodeType, wClone.Condition.NodeType);
      Assert.AreNotEqual(w.Statement, wClone.Statement);
      Assert.AreEqual(w.Statement.NodeType, wClone.Statement.NodeType);
    }
  }
}


