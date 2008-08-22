using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Sql.Dom.Exceptions;
using Xtensive.Sql.Dom.Tests.MsSql;

namespace Xtensive.Sql.Dom.Tests.MsSql
{
  [TestFixture]
  public class MiscTests: AdventureWorks
  {
    private SqlConnection sqlConnection;
    private SqlCommand sqlCommand;
    private SqlDriver sqlDriver;


    [TestFixtureSetUp]
    public override void SetUp()
    {
      base.SetUp();

      SqlConnectionProvider provider = new SqlConnectionProvider();
      sqlConnection = (SqlConnection)provider.CreateConnection(@"mssql://localhost/AdventureWorks");
      sqlDriver = sqlConnection.Driver as SqlDriver;
      sqlCommand = new SqlCommand(sqlConnection);
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      try {
        if ((sqlConnection != null) && (sqlConnection.State != ConnectionState.Closed))
          sqlConnection.Close();
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }

    [Test]
    public void BinaryExpressionTest()
    {
      SqlLiteral<int> l = Sql.Literal(1);
      bool passed = false;
      if (!SqlExpression.IsNull(l))
        passed = true;
      Assert.IsTrue(passed);
      if (SqlExpression.IsNull(l))
        passed = false;
      Assert.IsTrue(passed);
    }

    [Test]
    public void ImplicitConversionTest()
    {
      SqlExpression e = new byte[3];
      Assert.AreEqual(e.GetType(), typeof(SqlLiteral<byte[]>));
    }

    [Test]
    public void ArrayTest()
    {
      SqlArray<int> i = Sql.Array(new int[] {1, 2});
      i.Values[0] = 10;
      SqlSelect select = Sql.Select();
      select.Where = Sql.In(1, i);

      MemoryStream ms = new MemoryStream();
      BinaryFormatter bf = new BinaryFormatter();
      bf.Serialize(ms, select);

      ms.Seek(0, SeekOrigin.Begin);
      select = (SqlSelect)bf.Deserialize(ms);

      Console.WriteLine(sqlDriver.Compile(select).CommandText);
    }

    [Test]
    public void TypeTest()
    {
      SqlValueType t1 = new SqlValueType(SqlDataType.Decimal, 6, 4);
      SqlValueType t2 = new SqlValueType(SqlDataType.Decimal, 6, 4);
      Assert.IsFalse(t1 != t2);
      Assert.IsTrue(t1 == t2);
      Assert.IsTrue(t1.Equals(t2));
    }

    [Test]
    public void AddTest()
    {
      SqlLiteral<int> l1 = Sql.Literal(1);
      SqlLiteral<int> l2 = Sql.Literal(2);
      SqlBinary b = l1 + l2;
      Assert.AreEqual(b.NodeType, SqlNodeType.Add);

      b = b - ~l1;
      Assert.AreEqual(b.NodeType, SqlNodeType.Subtract);
      Assert.AreEqual(b.Right.NodeType, SqlNodeType.BitNot);

      SqlSelect s = Sql.Select();
      s.Columns.Add(1, "id");
      b = b/s;
      Assert.AreEqual(b.NodeType, SqlNodeType.Divide);
      Assert.AreEqual(b.Right.NodeType, SqlNodeType.SubSelect);

      SqlCast c = Sql.Cast(l1, SqlDataType.Decimal);
      Assert.AreEqual(c.NodeType, SqlNodeType.Cast);

      SqlFunctionCall l = Sql.Length(Sql.Literal("name"));
      b = c%l;
      Assert.AreEqual(b.NodeType, SqlNodeType.Modulo);
      Assert.AreEqual(b.Right.NodeType, SqlNodeType.FunctionCall);

      b = l1*(-l2);
      Assert.AreEqual(b.NodeType, SqlNodeType.Multiply);
      Assert.AreEqual(b.Right.NodeType, SqlNodeType.Negate);

      SqlBatch batch = Sql.Batch();
      SqlVariable v1 = Sql.Variable("v1", SqlDataType.Double);
      batch.Add(v1.Declare());
      batch.Add(Sql.Assign(v1, 1.0));
      s = Sql.Select();
      s.Columns.Add(b, "value");
      batch.Add(s);
    }

    [Test]
    [ExpectedException(typeof (SqlCompilerException))]
    public void CircularReferencesTest()
    {
      SqlSelect select = Sql.Select();
      SqlBinary b = Sql.Literal(1) + 2;
      SqlBinary rb = b + 3;
      rb.Left.ReplaceWith(rb);
      select.Where = rb > 1;
      Console.WriteLine(sqlDriver.Compile(select).CommandText);
    }

    [Test]
    public void PositionTest()
    {
      SqlSelect select = Sql.Select();
      select.Columns.Add(Sql.Multiply(Sql.Position("b", "abc"), 4));
      Console.WriteLine(sqlDriver.Compile(select).CommandText);
    }

    [Test]
    public void SubstringTest()
    {
      SqlSelect select = Sql.Select();
      select.Columns.Add(Sql.Substring("abc", 1, 1));
      select.Columns.Add(Sql.Substring("Xtensive", 2));
//      select.Columns.Add(Sql.Power(2, 2));
      Console.WriteLine(sqlDriver.Compile(select).CommandText);
    }

    [Test]
    public void TrimTest()
    {
      SqlSelect select = Sql.Select();
      select.Columns.Add(Sql.Trim(" abc "));
      select.Columns.Add(Sql.Trim(" abc ", SqlTrimType.Leading));
      select.Columns.Add(Sql.Trim(" abc ", SqlTrimType.Trailing));
      select.Columns.Add(Sql.Trim(" abc ", SqlTrimType.Both));
      select.Columns.Add(Sql.Trim(" abc ", SqlTrimType.Both, ' '));
      Console.WriteLine(sqlDriver.Compile(select).CommandText);
    }

    [Test]
    public void ExtractTest()
    {
      SqlSelect select = Sql.Select();
      select.Columns.Add(Sql.Extract(SqlDateTimePart.Day, "2006-01-23"));
      Console.WriteLine(sqlDriver.Compile(select).CommandText);
    }

    //    [Test]
    //    public void SequenceTest()
    //    {
    //      SqlSelect select = Sql.Select();
    //      select.Columns.Add(Sql.NextValue("myGenerator"));
    //      SqlDriver driver = new MSSQLDriver(new MSSQLVersionInfo(new Version("9.0")));
    //      Console.WriteLine(driver.Compile(select).CommandText);
    //    }

    [Test]
    public void ConcatTest()
    {
      SqlSelect select = Sql.Select();
      select.Columns.Add(Sql.Concat("a", "b"));
      select.Columns.Add("User: " + Sql.SessionUser());
      Console.WriteLine(sqlDriver.Compile(select).CommandText);
    }

    [Test]
    public void IsBooleanExpressionTest()
    {
      SqlExpression ex = !Sql.Literal(true) || true;
    }

    [Test]
    public void JoinTest()
    {
      SqlTableRef tr1 = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);
      SqlTableRef tr2 = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);
      SqlTableRef tr3 = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);

      SqlSelect select = Sql.Select(tr1.InnerJoin(tr2, tr1[0] == tr2[0]).InnerJoin(tr3, tr2[0] == tr3[0]));
      sqlCommand.Statement = select;
      sqlCommand.Prepare();

      int i = 0;
      SqlTableRef[] refs = new SqlTableRef[] {tr1, tr2, tr3};
      foreach (SqlTable source in select.From)
        Assert.AreEqual(refs[i++], source);
    }

    [Test]
    public void UniqueTest()
    {
      SqlSelect s1 = Sql.Select();
      SqlSelect s2 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"]));
      s2.Columns.Add(Sql.Asterisk);
      s1.Columns.Add(Sql.Unique(s2) == true);
      Console.WriteLine(sqlDriver.Compile(s1).CommandText);
    }

    [Test]
    public void TrueTest()
    {
      SqlSelect s1 = Sql.Select();
      s1.Where = true;
      Console.WriteLine(sqlDriver.Compile(s1).CommandText);
    }

    [Test]
    public void UnionTest()
    {
      SqlSelect s1 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s1.Columns.Add(s1.From["AddressID"]);
      SqlSelect s2 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s2.Columns.Add(s2.From["AddressID"]);
      SqlSelect s3 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s3.Columns.Add(s3.From["AddressID"]);

      Console.WriteLine(sqlDriver.Compile(s1.Union(s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.Union(s2).Union(s3)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.Union(s2.Union(s3))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Union(s1, s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Union(s1, s1.Union(s2))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Union(s1.Union(s2), s1)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Union(s1.Union(s2), s1.Union(s2))).CommandText);
      s3.Where = Sql.In(50.00, s1.Union(s2));
      Console.WriteLine(sqlDriver.Compile(s3).CommandText);
      SqlQueryRef qr = Sql.QueryRef(s1.Union(s2), "qr");
      Assert.Greater(qr.Columns.Count, 0);
    }

    [Test]
    public void UnionAllTest()
    {
      SqlSelect s1 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s1.Columns.Add(Sql.Asterisk);
      SqlSelect s2 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s2.Columns.Add(Sql.Asterisk);
      SqlSelect s3 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s3.Columns.Add(Sql.Asterisk);

      Console.WriteLine(sqlDriver.Compile(s1.UnionAll(s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.UnionAll(s2).UnionAll(s3)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.UnionAll(s2.UnionAll(s3))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.UnionAll(s1, s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.UnionAll(s1, s1.UnionAll(s2))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.UnionAll(s1.UnionAll(s2), s1)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.UnionAll(s1.UnionAll(s2), s1.UnionAll(s2))).CommandText);
    }

    [Test]
    public void IntersectTest()
    {
      SqlSelect s1 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s1.Columns.Add(Sql.Asterisk);
      SqlSelect s2 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s2.Columns.Add(Sql.Asterisk);
      SqlSelect s3 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s3.Columns.Add(Sql.Asterisk);

      Console.WriteLine(sqlDriver.Compile(s1.Intersect(s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.Intersect(s2).Intersect(s3)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.Intersect(s2.Intersect(s3))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Intersect(s1, s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Intersect(s1, s1.Intersect(s2))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Intersect(s1.Intersect(s2), s1)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Intersect(s1.Intersect(s2), s1.Intersect(s2))).CommandText);
    }

    [Test]
    public void IntersectAllTest()
    {
      SqlSelect s1 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s1.Columns.Add(Sql.Asterisk);
      SqlSelect s2 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s2.Columns.Add(Sql.Asterisk);
      SqlSelect s3 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s3.Columns.Add(Sql.Asterisk);

      Console.WriteLine(sqlDriver.Compile(s1.IntersectAll(s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.IntersectAll(s2).IntersectAll(s3)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.IntersectAll(s2.IntersectAll(s3))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.IntersectAll(s1, s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.IntersectAll(s1, s1.IntersectAll(s2))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.IntersectAll(s1.IntersectAll(s2), s1)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.IntersectAll(s1.IntersectAll(s2), s1.IntersectAll(s2))).CommandText);
    }

    [Test]
    public void ExceptTest()
    {
      SqlSelect s1 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s1.Columns.Add(Sql.Asterisk);
      SqlSelect s2 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s2.Columns.Add(Sql.Asterisk);
      SqlSelect s3 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s3.Columns.Add(Sql.Asterisk);

      Console.WriteLine(sqlDriver.Compile(s1.Except(s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.Except(s2).Except(s3)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.Except(s2.Except(s3))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Except(s1, s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Except(s1, s1.Except(s2))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Except(s1.Except(s2), s1)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.Except(s1.Except(s2), s1.Except(s2))).CommandText);
    }

    [Test]
    public void ExceptAllTest()
    {
      SqlSelect s1 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s1.Columns.Add(Sql.Asterisk);
      SqlSelect s2 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s2.Columns.Add(Sql.Asterisk);
      SqlSelect s3 = Sql.Select(Sql.TableRef(Catalog.Schemas["Person"].Tables["Address"]));
      s3.Columns.Add(Sql.Asterisk);

      Console.WriteLine(sqlDriver.Compile(s1.ExceptAll(s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.ExceptAll(s2).ExceptAll(s3)).CommandText);
      Console.WriteLine(sqlDriver.Compile(s1.ExceptAll(s2.ExceptAll(s3))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.ExceptAll(s1, s2)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.ExceptAll(s1, s1.ExceptAll(s2))).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.ExceptAll(s1.ExceptAll(s2), s1)).CommandText);
      Console.WriteLine(sqlDriver.Compile(Sql.ExceptAll(s1.ExceptAll(s2), s1.ExceptAll(s2))).CommandText);
    }

  }
}