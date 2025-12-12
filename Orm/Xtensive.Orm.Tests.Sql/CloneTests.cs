// Copyright (C) 2003-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public class CloneTests
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
    public void SqlExpressionCloneTest()
    {
      SqlExpression e = SqlDml.Literal(1);
      SqlExpression eClone = e.Clone();
      Assert.That(eClone, Is.Not.EqualTo(e));
      Assert.That(eClone.NodeType, Is.EqualTo(e.NodeType));
    }
    
    [Test]
    public void SqlLiteralCloneTest()
    {
      SqlLiteral<int> l = SqlDml.Literal(1);
      SqlLiteral<int> lClone = (SqlLiteral<int>) l.Clone();
      Assert.That(lClone, Is.Not.EqualTo(l));
      Assert.That(lClone.Value, Is.EqualTo(l.Value));
      Assert.That(lClone.NodeType, Is.EqualTo(l.NodeType));
    }

    [Test]
    public void SqlArrayCloneTest()
    {
      SqlArray<int> a = SqlDml.Array(new int[]{1, 2, 4});
      SqlArray<int> aClone = (SqlArray<int>) a.Clone();

      Assert.That(aClone, Is.Not.EqualTo(a));
      Assert.That(a.Values!=aClone.Values, Is.True);
      Assert.That(aClone.Values.Length, Is.EqualTo(a.Values.Length));
      for (int i = 0, l = a.Values.Length; i < l; i++)
        Assert.That(aClone.Values[i], Is.EqualTo(a.Values[i]));
      Assert.That(aClone.NodeType, Is.EqualTo(a.NodeType));
    }

    [Test]
    public void SqlBinaryCloneTest()
    {
      SqlBinary b = SqlDml.Literal(2) > 1;
      SqlBinary bClone = (SqlBinary)b.Clone();

      Assert.That(bClone, Is.Not.EqualTo(b));
      Assert.That(bClone.Left, Is.Not.EqualTo(b.Left));
      Assert.That(bClone.Right, Is.Not.EqualTo(b.Right));
      Assert.That(bClone.NodeType, Is.EqualTo(b.NodeType));
      Assert.That(bClone.Left.NodeType, Is.EqualTo(b.Left.NodeType));
      Assert.That(bClone.Right.NodeType, Is.EqualTo(b.Right.NodeType));
    }

    [Test]
    public void SqlCaseCloneTest()
    {
      SqlCase c = SqlDml.Case();
      c.Value = SqlDml.Literal(1);
      c[SqlDml.Literal(1)] = SqlDml.Literal("A");
      c[SqlDml.Literal(2)] = SqlDml.Literal("B");
      c.Else = SqlDml.Literal("C");
      
      SqlCase cClone = (SqlCase) c.Clone();

      Assert.That(cClone, Is.Not.EqualTo(c));
      Assert.That(cClone.Value, Is.Not.EqualTo(c.Value));
      Assert.That(cClone.Else, Is.Not.EqualTo(c.Else));
      Assert.That(cClone.NodeType, Is.EqualTo(c.NodeType));
      Assert.That(cClone.Value.NodeType, Is.EqualTo(c.Value.NodeType));
      Assert.That(cClone.Else.NodeType, Is.EqualTo(c.Else.NodeType));
      Assert.That(cClone.Count, Is.EqualTo(c.Count));
    }

    [Test]
    public void SqlCastCloneTest()
    {
      SqlCast c = SqlDml.Cast(SqlDml.Literal(1), SqlType.Decimal, 6 ,4);
      SqlCast cClone = (SqlCast)c.Clone();

      Assert.That(cClone, Is.Not.EqualTo(c));
      Assert.That(cClone.Operand, Is.Not.EqualTo(c.Operand));
      Assert.That(cClone.NodeType, Is.EqualTo(c.NodeType));
      Assert.That(cClone.Operand.NodeType, Is.EqualTo(c.Operand.NodeType));
      Assert.That(cClone.Type, Is.EqualTo(c.Type));
    }

    [Test]
    public void SqlColumnCloneTest()
    {
      {
        SqlUserColumn c = SqlDml.Column(1);
        SqlUserColumn cClone = (SqlUserColumn)c.Clone();

        Assert.That(cClone, Is.Not.EqualTo(c));
        Assert.That(cClone.Expression, Is.Not.EqualTo(c.Expression));
        Assert.That(cClone.NodeType, Is.EqualTo(c.NodeType));
        Assert.That(cClone.Expression.NodeType, Is.EqualTo(c.Expression.NodeType));
      }
      Console.WriteLine();
      {
        SqlTableRef t = SqlDml.TableRef(table1);
        SqlTableColumn c = (SqlTableColumn)t[0];
        SqlTableColumn cClone = (SqlTableColumn)c.Clone();

        Assert.That(cClone, Is.Not.EqualTo(c));
        //        Assert.AreNotEqual(c.SqlTable, cClone.SqlTable);
        Assert.That(cClone.NodeType, Is.EqualTo(c.NodeType));
      }
    }

    [Test]
    public void SqlFunctionCallCloneTest()
    {
      {
        SqlFunctionCall fc = SqlDml.FunctionCall("Function", 1, 2, 4);
        SqlFunctionCall fcClone = (SqlFunctionCall) fc.Clone();

        Assert.That(fcClone, Is.Not.EqualTo(fc));
        Assert.That(fcClone.Arguments, Is.Not.EqualTo(fc.Arguments));
        Assert.That(fcClone.NodeType, Is.EqualTo(fc.NodeType));
        Assert.That(fcClone.Arguments.Count, Is.EqualTo(fc.Arguments.Count));
        for (int i = 0, l = fc.Arguments.Count; i < l; i++) {
          Assert.That(fcClone.Arguments[i], Is.Not.EqualTo(fc.Arguments[i]));
          Assert.That(fcClone.Arguments[i].NodeType, Is.EqualTo(fc.Arguments[i].NodeType));
        }
        Assert.That(fcClone.FunctionType, Is.EqualTo(fc.FunctionType));
        Assert.That(((SqlUserFunctionCall) fcClone).Name, Is.EqualTo(((SqlUserFunctionCall) fc).Name));
      }
      Console.WriteLine();
      {
        SqlFunctionCall fc = SqlDml.CharLength("string");
        SqlFunctionCall fcClone = (SqlFunctionCall) fc.Clone();

        Assert.That(fcClone, Is.Not.EqualTo(fc));
        Assert.That(fcClone.Arguments, Is.Not.EqualTo(fc.Arguments));
        for (int i = 0, l = fc.Arguments.Count; i < l; i++) {
          Assert.That(fcClone.Arguments[i], Is.Not.EqualTo(fc.Arguments[i]));
          Assert.That(fcClone.Arguments[i].NodeType, Is.EqualTo(fc.Arguments[i].NodeType));
        }
        Assert.That(fcClone.NodeType, Is.EqualTo(fc.NodeType));
        Assert.That(fcClone.Arguments.Count, Is.EqualTo(fc.Arguments.Count));
        Assert.That(fcClone.FunctionType, Is.EqualTo(fc.FunctionType));
      }
    }

    [Test]
    public void SqlLikeCloneTest()
    {
      {
        SqlLike l = SqlDml.Like("epxression", "e%", "\\");
        SqlLike lClone = (SqlLike) l.Clone();

        Assert.That(lClone, Is.Not.EqualTo(l));
        Assert.That(lClone.Expression, Is.Not.EqualTo(l.Expression));
        Assert.That(lClone.Pattern, Is.Not.EqualTo(l.Pattern));
        Assert.That(lClone.Escape, Is.Not.EqualTo(l.Escape));
        Assert.That(lClone.NodeType, Is.EqualTo(l.NodeType));
        Assert.That(lClone.Expression.NodeType, Is.EqualTo(l.Expression.NodeType));
        Assert.That(lClone.Pattern.NodeType, Is.EqualTo(l.Pattern.NodeType));
        Assert.That(lClone.Escape.NodeType, Is.EqualTo(l.Escape.NodeType));
      }
      Console.WriteLine();
      {
        SqlLike l = SqlDml.Like("epxression", "e%");
        SqlLike lClone = (SqlLike)l.Clone();

        Assert.That(lClone, Is.Not.EqualTo(l));
        Assert.That(lClone.Expression, Is.Not.EqualTo(l.Expression));
        Assert.That(lClone.Pattern, Is.Not.EqualTo(l.Pattern));
        Assert.That(lClone.NodeType, Is.EqualTo(l.NodeType));
        Assert.That(lClone.Expression.NodeType, Is.EqualTo(l.Expression.NodeType));
        Assert.That(lClone.Pattern.NodeType, Is.EqualTo(l.Pattern.NodeType));
        Assert.That(l.Escape, Is.Null);
      }
    }

    [Test]
    public void SqlRowCloneTest()
    {
      SqlRow r = SqlDml.Row(1, 2, 4, SqlDml.Literal(6) + 5);
      SqlRow rClone = (SqlRow)r.Clone();

      Assert.That(rClone, Is.Not.EqualTo(r));
      Assert.That(rClone.NodeType, Is.EqualTo(r.NodeType));
      Assert.That(rClone.Count, Is.EqualTo(r.Count));

      for (int i = 0, l = r.Count; i < l; i++) {
        Assert.That(rClone[i], Is.Not.EqualTo(r[i]));
        Assert.That(rClone[i].NodeType, Is.EqualTo(r[i].NodeType));
      }
    }

    [Test]
    public void SqlUnaryCloneTest()
    {
      SqlUnary u = -SqlDml.Literal(1);
      SqlUnary uClone = (SqlUnary)u.Clone();

      Assert.That(uClone, Is.Not.EqualTo(u));
      Assert.That(uClone.NodeType, Is.EqualTo(u.NodeType));
    }

    [Test]
    public void SqlVariableCloneTest()
    {
      SqlVariable v = SqlDml.Variable("v", SqlType.Int32);
      SqlVariable vClone = (SqlVariable)v.Clone();

      Assert.That(vClone, Is.Not.EqualTo(v));
      Assert.That(vClone.NodeType, Is.EqualTo(v.NodeType));
      Assert.That(vClone.Name, Is.EqualTo(v.Name));
    }

    [Test]
    public void SqlAggregateCloneTest()
    {
      {
        SqlAggregate a = SqlDml.Count();
        SqlAggregate aClone = (SqlAggregate) a.Clone();

        Assert.That(aClone, Is.Not.EqualTo(a));
        Assert.That(aClone.NodeType, Is.EqualTo(a.NodeType));
        Assert.That(aClone.Distinct, Is.EqualTo(a.Distinct));
        Assert.That(SqlNodeType.Native, Is.EqualTo(aClone.Expression.NodeType));
      }
      Console.WriteLine();
      {
        SqlAggregate a = SqlDml.Sum(1);
        SqlAggregate aClone = (SqlAggregate)a.Clone();

        Assert.That(aClone, Is.Not.EqualTo(a));
        Assert.That(aClone.Expression, Is.Not.EqualTo(a.Expression));
        Assert.That(aClone.NodeType, Is.EqualTo(a.NodeType));
        Assert.That(aClone.Distinct, Is.EqualTo(a.Distinct));
        Assert.That(aClone.Expression.NodeType, Is.EqualTo(a.Expression.NodeType));
      }
    }

    [Test, Ignore("FixGetEnumerator")]
    public void SqlTableRefCloneTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlTableRef tClone = (SqlTableRef)t.Clone();

      Assert.That(tClone, Is.Not.EqualTo(t));
      Assert.That(tClone.Columns, Is.Not.EqualTo(t.Columns));
      Assert.That(tClone.NodeType, Is.EqualTo(t.NodeType));
      Assert.That(tClone.Name, Is.EqualTo(t.Name));
      Assert.That(tClone.DataTable, Is.EqualTo(t.DataTable));
      Assert.That(tClone.Columns.Count, Is.EqualTo(t.Columns.Count));

      for (int i = 0, l = t.Columns.Count; i < l; i++) {
        Assert.That(tClone.Columns[i], Is.Not.EqualTo(t.Columns[i]));
        Assert.That(tClone.Columns[i].NodeType, Is.EqualTo(t.Columns[i].NodeType));
        Assert.That(tClone.Columns[i].GetType(), Is.EqualTo(t.Columns[i].GetType()));
      }
    }

    [Test, Ignore("FixGetEnumerator")]
    public void SqlQueryRefCloneTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlSelect s = SqlDml.Select(t);
      s.Columns.Add(t.Columns[0]);

      SqlQueryRef qr = SqlDml.QueryRef(s);
      SqlQueryRef qrClone = (SqlQueryRef)qr.Clone();

      Assert.That(qrClone, Is.Not.EqualTo(qr));
      Assert.That(qrClone.Columns, Is.Not.EqualTo(qr.Columns));
      Assert.That(qrClone.NodeType, Is.EqualTo(qr.NodeType));
      Assert.That(qrClone.Name, Is.EqualTo(qr.Name));
      Assert.That(qrClone.Query, Is.Not.EqualTo(qr.Query));
      Assert.That(qrClone.Columns.Count, Is.EqualTo(qr.Columns.Count));

      for (int i = 0, l = qr.Columns.Count; i < l; i++) {
        Assert.That(qrClone.Columns[i], Is.Not.EqualTo(qr.Columns[i]));
        Assert.That(qrClone.Columns[i].NodeType, Is.EqualTo(qr.Columns[i].NodeType));
        Assert.That(qrClone.Columns[i].GetType(), Is.EqualTo(qr.Columns[i].GetType()));
      }
    }

    [Test, Ignore("FixGetEnumerator")]
    public void SqlSubSelectCloneTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlSelect s = SqlDml.Select(t);
      s.Columns.Add(t.Columns[0]);

      SqlSubQuery ss = SqlDml.SubQuery(s);
      SqlSubQuery ssClone = (SqlSubQuery)ss.Clone();

      Assert.That(ssClone, Is.Not.EqualTo(ss));
      Assert.That(ssClone.Query, Is.Not.EqualTo(ss.Query));
      Assert.That(ssClone.NodeType, Is.EqualTo(ss.NodeType));
      Assert.That(ssClone.Query.NodeType, Is.EqualTo(ss.Query.NodeType));
    }

    [Test]
    public void SqlMatchCloneTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlSelect s = SqlDml.Select(t);
      s.Columns.Add(t.Columns[0]);

      SqlMatch m = SqlDml.Match(SqlDml.Row(4), s, true, SqlMatchType.Full);
      SqlMatch mClone = (SqlMatch)m.Clone();

      Assert.That(mClone, Is.Not.EqualTo(m));
      Assert.That(mClone.Value, Is.Not.EqualTo(m.Value));
      Assert.That(mClone.SubQuery, Is.Not.EqualTo(m.SubQuery));
      Assert.That(mClone.NodeType, Is.EqualTo(m.NodeType));
      Assert.That(mClone.Value.NodeType, Is.EqualTo(m.Value.NodeType));
      Assert.That(mClone.SubQuery.NodeType, Is.EqualTo(m.SubQuery.NodeType));
      Assert.That(mClone.Unique, Is.EqualTo(m.Unique));
      Assert.That(mClone.MatchType, Is.EqualTo(m.MatchType));
    }
    
    [Test, Ignore("FixGetEnumerator")]
    public void SqlSelectCloneTest()
    {
      SqlTableRef tr1 = SqlDml.TableRef(table1);
      SqlTableRef tr2 = SqlDml.TableRef(table2);

      SqlSelect s = SqlDml.Select();
      s.Distinct = true;
      s.Limit = 3;
      s.Columns.Add(tr1["ID"]);
      s.Columns.Add(tr1["ID"], "ID2");
      s.Columns.Add(tr1["ID"] + tr1["ID"], "SUM2");
      s.Columns.Add(tr2.Asterisk);
      s.From = tr1.InnerJoin(tr2, tr1["ID"]==tr2["ID"]);
      s.Where = SqlDml.Like(tr1["Name"], "Marat");
      s.Hints.Add(SqlDml.FastFirstRowsHint(10));

      SqlSelect sClone = (SqlSelect)s.Clone();

      Assert.That(sClone, Is.Not.EqualTo(s));
      Assert.That(sClone.Columns, Is.Not.EqualTo(s.Columns));
      for (int i = 0, l = s.Columns.Count; i < l; i++) {
        Assert.That(sClone.Columns[i], Is.Not.EqualTo(s.Columns[i]));
      }
      for (int i = 0, l = s.GroupBy.Count; i < l; i++) {
        Assert.That(sClone.Columns[i], Is.Not.EqualTo(s.Columns[i]));
      }
      Assert.That(sClone.Distinct, Is.EqualTo(s.Distinct));
      if (s.From==null)
        Assert.That(sClone.From, Is.EqualTo(s.From));
      else {
        Assert.That(sClone.From, Is.Not.EqualTo(s.From));
        Assert.That(sClone.From.NodeType, Is.EqualTo(s.From.NodeType));
      }
      if (s.Having is not null) {
        Assert.That(sClone.Having, Is.Not.EqualTo(s.Having));
        Assert.That(sClone.Having.NodeType, Is.EqualTo(s.Having.NodeType));
      }

      Assert.That(sClone.NodeType, Is.EqualTo(s.NodeType));
      Assert.That(s.OrderBy==sClone.OrderBy, Is.False);
      Assert.That(sClone.OrderBy.Count, Is.EqualTo(s.OrderBy.Count));
      for (int i = 0, l = s.OrderBy.Count; i < l; i++) {
        Assert.That(sClone.OrderBy[i], Is.Not.EqualTo(s.OrderBy[i]));
        Assert.That(sClone.OrderBy[i].Ascending, Is.EqualTo(s.OrderBy[i].Ascending));
        Assert.That(sClone.OrderBy[i].Expression, Is.Not.EqualTo(s.OrderBy[i].Expression));
        Assert.That(sClone.OrderBy[i].Position, Is.EqualTo(s.OrderBy[i].Position));
      }

      Assert.That(sClone.Limit, Is.EqualTo(s.Limit));
      if (s.Where!=null) {
        Assert.That(sClone.Where, Is.Not.EqualTo(s.Where));
        Assert.That(sClone.Where.NodeType, Is.EqualTo(s.Where.NodeType));
      }

      s.Where &= tr1[0] > 1200 || tr2[1]!="Marat";
      s.OrderBy.Add(tr1["ID"], false);
      s.OrderBy.Add(2);

      sClone = (SqlSelect)s.Clone();

      Assert.That(sClone, Is.Not.EqualTo(s));
      Assert.That(sClone.Columns, Is.Not.EqualTo(s.Columns));
      for (int i = 0, l = s.Columns.Count; i < l; i++) {
        Assert.That(sClone.Columns[i], Is.Not.EqualTo(s.Columns[i]));
      }
      for (int i = 0, l = s.GroupBy.Count; i < l; i++) {
        Assert.That(sClone.Columns[i], Is.Not.EqualTo(s.Columns[i]));
      }
      Assert.That(sClone.Distinct, Is.EqualTo(s.Distinct));
      if (s.From==null)
        Assert.That(sClone.From, Is.EqualTo(s.From));
      else {
        Assert.That(sClone.From, Is.Not.EqualTo(s.From));
        Assert.That(sClone.From.NodeType, Is.EqualTo(s.From.NodeType));
      }
      if (s.Having!=null) {
        Assert.That(sClone.Having, Is.Not.EqualTo(s.Having));
        Assert.That(sClone.Having.NodeType, Is.EqualTo(s.Having.NodeType));
      }

      Assert.That(sClone.NodeType, Is.EqualTo(s.NodeType));
      Assert.That(sClone.OrderBy, Is.Not.EqualTo(s.OrderBy));
      Assert.That(sClone.OrderBy.Count, Is.EqualTo(s.OrderBy.Count));
      for (int i = 0, l = s.OrderBy.Count; i < l; i++) {
        Assert.That(sClone.OrderBy[i], Is.Not.EqualTo(s.OrderBy[i]));
        Assert.That(sClone.OrderBy[i].Ascending, Is.EqualTo(s.OrderBy[i].Ascending));
        if (s.OrderBy[i].Expression is null)
          Assert.That(sClone.OrderBy[i].Expression, Is.EqualTo(s.OrderBy[i].Expression));
        else
          Assert.That(sClone.OrderBy[i].Expression, Is.Not.EqualTo(s.OrderBy[i].Expression));
        Assert.That(sClone.OrderBy[i].Position, Is.EqualTo(s.OrderBy[i].Position));
      }

      Assert.That(sClone.Limit, Is.EqualTo(s.Limit));
      if (s.Where!=null) {
        Assert.That(sClone.Where, Is.Not.EqualTo(s.Where));
        Assert.That(sClone.Where.NodeType, Is.EqualTo(s.Where.NodeType));
      }
      Assert.That(sClone.Hints.Count, Is.EqualTo(s.Hints.Count));

      SqlSelect s2 = SqlDml.Select();
      SqlQueryRef t = SqlDml.QueryRef(s, "SUBSELECT");
      s2.From = t;
      s2.Columns.Add(t.Asterisk);
      SqlSelect s2Clone = (SqlSelect) s2.Clone();

      Assert.That(s2Clone, Is.Not.EqualTo(s2));
      Assert.That(s2Clone.Columns.Count, Is.EqualTo(s2.Columns.Count));
    }

    [Test]
    public void SqlOrderCloneTest()
    {
      {
        SqlOrder o = SqlDml.Order(2, true);
        SqlOrder oClone = (SqlOrder) o.Clone();

        Assert.That(oClone, Is.Not.EqualTo(o));
        Assert.That(oClone.NodeType, Is.EqualTo(o.NodeType));
        Assert.That(oClone.Ascending, Is.EqualTo(o.Ascending));
        Assert.That(oClone.Expression, Is.EqualTo(o.Expression));
        Assert.That(oClone.Position, Is.EqualTo(o.Position));
      }

      {
        SqlOrder o = SqlDml.Order(SqlDml.Column(SqlDml.Literal(2)), false);
        SqlOrder oClone = (SqlOrder)o.Clone();

        Assert.That(oClone, Is.Not.EqualTo(o));
        Assert.That(oClone.NodeType, Is.EqualTo(o.NodeType));
        Assert.That(oClone.Ascending, Is.EqualTo(o.Ascending));
        Assert.That(oClone.Expression, Is.Not.EqualTo(o.Expression));
        Assert.That(oClone.Position, Is.EqualTo(o.Position));
      }
    }
    
    [Test]
    public void SqlAssignCloneTest()
    {
      SqlParameterRef p = SqlDml.ParameterRef("p");
      SqlAssignment a = SqlDml.Assign(p, 1);
      SqlAssignment aClone = (SqlAssignment) a.Clone();

      Assert.That(aClone, Is.Not.EqualTo(a));
      Assert.That(aClone.NodeType, Is.EqualTo(a.NodeType));
      Assert.That(aClone.Left, Is.Not.EqualTo(a.Left));
      Assert.That(aClone.Left.NodeType, Is.EqualTo(a.Left.NodeType));
      Assert.That(aClone.Right, Is.Not.EqualTo(a.Right));
      Assert.That(aClone.Right.NodeType, Is.EqualTo(a.Right.NodeType));
    }

    [Test]
    public void SqlBatchCloneTest()
    {
      SqlParameterRef p = SqlDml.ParameterRef("p");
      SqlAssignment a = SqlDml.Assign(p, 1);
      SqlBatch b = SqlDml.Batch();
      b.Add(a);
      b.Add(a);
      SqlBatch bClone = (SqlBatch) b.Clone();

      Assert.That(bClone, Is.Not.EqualTo(b));
      Assert.That(bClone.NodeType, Is.EqualTo(b.NodeType));
      Assert.That(bClone.Count, Is.EqualTo(b.Count));
      foreach (SqlStatement s in b)
        Assert.That(bClone.Contains(s), Is.False);
    }

    [Test]
    public void SqlDeclareVariableCloneTest()
    {
      {
        SqlVariable dv = SqlDml.Variable("v", SqlType.Char, 5);
        SqlVariable dvClone = (SqlVariable) dv.Clone();

        Assert.That(dvClone, Is.Not.EqualTo(dv));
        Assert.That(dvClone.NodeType, Is.EqualTo(dv.NodeType));
        Assert.That(dv.Type.Equals(dvClone.Type), Is.True);
        Assert.That(dvClone.Name, Is.EqualTo(dv.Name));
      }

      {
        SqlVariable dv = SqlDml.Variable("v", SqlType.Decimal, 6, 4);
        SqlVariable dvClone = (SqlVariable)dv.Clone();

        Assert.That(dvClone, Is.Not.EqualTo(dv));
        Assert.That(dvClone.NodeType, Is.EqualTo(dv.NodeType));
        Assert.That(dv.Type.Equals(dvClone.Type), Is.True);
        Assert.That(dvClone.Name, Is.EqualTo(dv.Name));
      }
    }

    [Test, Ignore("FixGetEnumerator")]
    public void SqlDeleteCloneTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlDelete d = SqlDml.Delete(t);
      d.Where = t[0] < 6;
      d.Hints.Add(SqlDml.FastFirstRowsHint(10));
      SqlDelete dClone = (SqlDelete) d.Clone();

      Assert.That(dClone, Is.Not.EqualTo(d));
      Assert.That(dClone.Delete, Is.Not.EqualTo(d.Delete));
      Assert.That(dClone.NodeType, Is.EqualTo(d.NodeType));
      Assert.That(dClone.Hints.Count, Is.EqualTo(d.Hints.Count));
      if (d.Where is not null) {
        Assert.That(dClone.Where, Is.Not.EqualTo(d.Where));
        Assert.That(dClone.Where.NodeType, Is.EqualTo(d.Where.NodeType));
      }
      else
        Assert.That(dClone.Where, Is.Null);
    }
    
    [Test, Ignore("FixGetEnumerator")]
    public void SqlIfCloneTest()
    {
      SqlSelect ifTrue = SqlDml.Select();
      ifTrue.Columns.Add(1, "id");
      SqlSelect ifFalse = SqlDml.Select();
      ifFalse.Columns.Add(2, "id");
      
      SqlIf i = SqlDml.If(SqlDml.SubQuery(ifTrue) > 0, ifTrue);
      SqlIf iClone = (SqlIf) i.Clone();

      Assert.That(iClone, Is.Not.EqualTo(i));
      Assert.That(iClone.NodeType, Is.EqualTo(i.NodeType));
      Assert.That(iClone.Condition, Is.Not.EqualTo(i.Condition));
      Assert.That(iClone.Condition.NodeType, Is.EqualTo(i.Condition.NodeType));
      Assert.That(iClone.True, Is.Not.EqualTo(i.True));
      Assert.That(iClone.NodeType, Is.Not.EqualTo(i.True.NodeType));
      Assert.That(iClone.False, Is.Null);

      Console.WriteLine();
      
      i.False = ifFalse;
      iClone = (SqlIf)i.Clone();

      Assert.That(iClone, Is.Not.EqualTo(i));
      Assert.That(iClone.NodeType, Is.EqualTo(i.NodeType));
      Assert.That(iClone.Condition, Is.Not.EqualTo(i.Condition));
      Assert.That(iClone.Condition.NodeType, Is.EqualTo(i.Condition.NodeType));
      Assert.That(iClone.True, Is.Not.EqualTo(i.True));
      Assert.That(iClone.NodeType, Is.Not.EqualTo(i.True.NodeType));
      Assert.That(iClone.False, Is.Not.EqualTo(i.False));
      Assert.That(iClone.False.NodeType, Is.EqualTo(i.False.NodeType));
    }

    [Test, Ignore("FixGetEnumerator")]
    public void SqlInsertCloneTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlInsert i = SqlDml.Insert(t);
      i.AddValueRow(( t[0], 1 ), ( t[1], "Anonym" ));
      i.Hints.Add(SqlDml.FastFirstRowsHint(10));
      SqlInsert iClone = (SqlInsert)i.Clone();

      Assert.That(iClone, Is.Not.EqualTo(i));
      Assert.That(iClone.Into, Is.Not.EqualTo(i.Into));
      Assert.That(iClone.NodeType, Is.EqualTo(i.NodeType));
      Assert.That(iClone.ValueRows.Count, Is.EqualTo(i.ValueRows.Count));
      Assert.That(iClone.ValueRows.Columns.Count, Is.EqualTo(i.ValueRows.Columns.Count));
      for(var indx = 0; indx < i.ValueRows.Count; indx++) {
        var iRow = i.ValueRows[indx];
        var iClonedRow = iClone.ValueRows[indx];
        Assert.That(iRow.SequenceEqual(iClonedRow), Is.False);
      }
      Assert.That(iClone.Hints.Count, Is.EqualTo(i.Hints.Count));
    }

    [Test, Ignore("FixGetEnumerator")]
    public void SqlUpdateCloneTest()
    {
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlUpdate u = SqlDml.Update(t);
      u.Values[t[0]] = 1;
      u.Values[t[1]] = "Anonym";
      u.Where = t.Columns["ID"]==1;
      u.Hints.Add(SqlDml.FastFirstRowsHint(10));
      SqlUpdate uClone = (SqlUpdate)u.Clone();

      Assert.That(uClone, Is.Not.EqualTo(u));
      Assert.That(uClone.Update, Is.Not.EqualTo(u.Update));
      Assert.That(uClone.NodeType, Is.EqualTo(u.NodeType));
      Assert.That(uClone.Values.Count, Is.EqualTo(u.Values.Count));
      foreach (KeyValuePair<ISqlLValue, SqlExpression> p in u.Values) {
        Assert.That(uClone.Values.ContainsKey(p.Key), Is.False);
        Assert.That(uClone.Values.ContainsValue(p.Value), Is.False);
      }
      if (u.Where!=null) {
        Assert.That(uClone.Where, Is.Not.EqualTo(u.Where));
        Assert.That(uClone.Where.NodeType, Is.EqualTo(u.Where.NodeType));
      }
      else
        Assert.That(uClone.Where, Is.Null);
      Assert.That(uClone.Hints.Count, Is.EqualTo(u.Hints.Count));
    }
    
    [Test]
    public void SqlWhileCloneTest()
    {
      SqlVariable i = SqlDml.Variable("i", SqlType.Int32);
      SqlWhile w = SqlDml.While(i<=1000);
      SqlBatch b = SqlDml.Batch();
      b.Add(SqlDml.Assign(i, i+1));
      SqlTableRef t = SqlDml.TableRef(table1);
      SqlSelect s = SqlDml.Select(t);
      s.Columns.Add(t["Name"]);
      s.Where = t[0]==i;
      SqlIf f = SqlDml.If(SqlDml.SubQuery(s)=="Unkown", SqlDml.Break, SqlDml.Continue);
      b.Add(f);
      w.Statement = b;

      SqlWhile wClone = (SqlWhile) w.Clone();

      Assert.That(wClone, Is.Not.EqualTo(w));
      Assert.That(wClone.NodeType, Is.EqualTo(w.NodeType));
      Assert.That(wClone.Condition, Is.Not.EqualTo(w.Condition));
      Assert.That(wClone.Condition.NodeType, Is.EqualTo(w.Condition.NodeType));
      Assert.That(wClone.Statement, Is.Not.EqualTo(w.Statement));
      Assert.That(wClone.Statement.NodeType, Is.EqualTo(w.Statement.NodeType));
    }
  }
}


