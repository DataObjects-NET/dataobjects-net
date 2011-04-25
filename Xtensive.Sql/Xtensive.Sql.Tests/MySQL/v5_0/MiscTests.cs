﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.MySQL.v5_0
{
    public class MiscTests : Sakila
    {
        private SqlDriver sqlDriver;
        private SqlConnection sqlConnection;
        private DbCommand dbCommand;
        private DbCommand sqlCommand;

        private Schema schema = null;

        #region Internals

        private bool CompareExecuteDataReader(string commandText, ISqlCompileUnit statement)
        {
            sqlCommand.CommandText = sqlDriver.Compile(statement).GetCommandText();
            sqlCommand.Prepare();
            Console.WriteLine(sqlCommand.CommandText);

            Console.WriteLine(commandText);
            dbCommand.CommandText = commandText;

            DbCommandExecutionResult r1, r2;
            r1 = GetExecuteDataReaderResult(dbCommand);
            r2 = GetExecuteDataReaderResult(sqlCommand);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(r1);
            Console.WriteLine(r2);

            if (r1.RowCount != r2.RowCount)
                return false;
            if (r1.FieldCount != r2.FieldCount)
                return false;
            for (int i = 0; i < r1.FieldCount; i++)
            {
                if (r1.FieldNames[i] != r2.FieldNames[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool CompareExecuteNonQuery(string commandText, ISqlCompileUnit statement)
        {
            sqlCommand.CommandText = sqlDriver.Compile(statement).GetCommandText();
            sqlCommand.Prepare();
            Console.WriteLine(sqlCommand.CommandText);

            Console.WriteLine(commandText);
            dbCommand.CommandText = commandText;

            DbCommandExecutionResult r1, r2;
            r1 = GetExecuteNonQueryResult(dbCommand);
            r2 = GetExecuteNonQueryResult(sqlCommand);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(r1);
            Console.WriteLine(r2);

            if (r1.RowCount != r2.RowCount)
                return false;
            return true;
        }

        private SqlCompilationResult Compile(ISqlCompileUnit statement)
        {
            return sqlDriver.Compile(statement);
        }

        #endregion

        #region Setup and TearDown

        [TestFixtureSetUp]
        public override void SetUp()
        {
            sqlDriver = SqlDriver.Create(TestUrl.MySQL50);
            sqlConnection = sqlDriver.CreateConnection();

            dbCommand = sqlConnection.CreateCommand();
            sqlCommand = sqlConnection.CreateCommand();
            try
            {
                sqlConnection.Open();
            }
            catch (SystemException e)
            {
                Console.WriteLine(e);
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                sqlConnection.BeginTransaction();
                Catalog = sqlDriver.ExtractCatalog(sqlConnection);
                schema = sqlDriver.ExtractDefaultSchema(sqlConnection);
                sqlConnection.Commit();
            }
            catch
            {
                sqlConnection.Rollback();
                throw;
            }
            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            try
            {
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                    sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        [Test]
        public void BinaryExpressionTest()
        {
            SqlLiteral<int> l = SqlDml.Literal(1);
            bool passed = false;
            if (!l.IsNullReference())
                passed = true;
            Assert.IsTrue(passed);
            if (l.IsNullReference())
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
        public void ArrayTest()//TODO: Find reason why this pattern is structured like this.(Malisa)
        {
            SqlArray<int> i = SqlDml.Array(new int[] { 1, 2 });
            i.Values[0] = 10;
            SqlSelect select = SqlDml.Select();
            select.Where = SqlDml.In(1, i);

            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, select);

            ms.Seek(0, SeekOrigin.Begin);
            select = (SqlSelect)bf.Deserialize(ms);

            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        public void TypeTest()
        {
            SqlValueType t1 = new SqlValueType(SqlType.Decimal, 6, 4);
            SqlValueType t2 = new SqlValueType(SqlType.Decimal, 6, 4);
            Assert.IsFalse(t1 != t2);
            Assert.IsTrue(t1 == t2);
            Assert.IsTrue(t1.Equals(t2));
        }

        [Test]
        public void AddTest()
        {
            SqlLiteral<int> l1 = SqlDml.Literal(1);
            SqlLiteral<int> l2 = SqlDml.Literal(2);
            SqlBinary b = l1 + l2;
            Assert.AreEqual(b.NodeType, SqlNodeType.Add);

            b = b - ~l1;
            Assert.AreEqual(b.NodeType, SqlNodeType.Subtract);
            Assert.AreEqual(b.Right.NodeType, SqlNodeType.BitNot);

            SqlSelect s = SqlDml.Select();
            s.Columns.Add(1, "id");
            b = b / s;
            Assert.AreEqual(b.NodeType, SqlNodeType.Divide);
            Assert.AreEqual(b.Right.NodeType, SqlNodeType.SubSelect);

            SqlCast c = SqlDml.Cast(l1, SqlType.Decimal);
            Assert.AreEqual(c.NodeType, SqlNodeType.Cast);

            SqlFunctionCall l = SqlDml.CharLength(SqlDml.Literal("name"));
            b = c % l;
            Assert.AreEqual(b.NodeType, SqlNodeType.Modulo);
            Assert.AreEqual(b.Right.NodeType, SqlNodeType.FunctionCall);

            b = l1 * (-l2);
            Assert.AreEqual(b.NodeType, SqlNodeType.Multiply);
            Assert.AreEqual(b.Right.NodeType, SqlNodeType.Negate);

            SqlBatch batch = SqlDml.Batch();
            SqlVariable v1 = SqlDml.Variable("v1", SqlType.Double);
            batch.Add(v1.Declare());
            batch.Add(SqlDml.Assign(v1, 1.0));
            s = SqlDml.Select();
            s.Columns.Add(b, "value");
            batch.Add(s);
        }

        [Test]
        [ExpectedException(typeof(SqlCompilerException))]
        public void CircularReferencesTest()
        {
            SqlSelect select = SqlDml.Select();
            SqlBinary b = SqlDml.Literal(1) + 2;
            SqlBinary rb = b + 3;
            rb.Left.ReplaceWith(rb);
            select.Where = rb > 1;
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        public void PositionTest()
        {
            SqlSelect select = SqlDml.Select();
            select.Columns.Add(SqlDml.Multiply(SqlDml.Position("b", "abc"), 4));
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        public void SubstringTest()
        {
            SqlSelect select = SqlDml.Select();
            select.Columns.Add(SqlDml.Substring("abc", 1, 1));
            select.Columns.Add(SqlDml.Substring("Xtensive", 2));
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        public void TrimTest()
        {
            SqlSelect select = SqlDml.Select();
            select.Columns.Add(SqlDml.Trim(" abc "));
            select.Columns.Add(SqlDml.Trim(" abc ", SqlTrimType.Leading));
            select.Columns.Add(SqlDml.Trim(" abc ", SqlTrimType.Trailing));
            select.Columns.Add(SqlDml.Trim(" abc ", SqlTrimType.Both));
            select.Columns.Add(SqlDml.Trim(" abc ", SqlTrimType.Both, " "));
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        public void ExtractTest()
        {
            SqlSelect select = SqlDml.Select();
            select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Day, "2006-01-23"));
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        public void ConcatTest()
        {
            SqlSelect select = SqlDml.Select();
            select.Columns.Add(SqlDml.Concat("a", "b"));
            //select.Columns.Add("User: " + SqlDml.SessionUser()); //NOTE: Not supported by MySQL.
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        public void IsBooleanExpressionTest()
        {
            SqlExpression ex = !SqlDml.Literal(true) || true;
        }

        [Test]
        public void JoinTest()
        {
            SqlTableRef tr1 = SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]);
            SqlTableRef tr2 = SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]);
            SqlTableRef tr3 = SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]);

            SqlSelect select = SqlDml.Select(tr1.InnerJoin(tr2, tr1[0] == tr2[0]).InnerJoin(tr3, tr2[0] == tr3[0]));
            select.Columns.Add(SqlDml.Asterisk);
            sqlCommand.CommandText = sqlDriver.Compile(select).GetCommandText();
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
            sqlCommand.Prepare();

            //int i = 0;
            //SqlTableRef[] refs = new[] { tr1, tr2, tr3 };
            //foreach (SqlTable source in select.From)
            //    Assert.AreEqual(refs[i++], source);
        }

        [Test]
        [Ignore]
        public void UniqueTest()
        {
            SqlSelect s1 = SqlDml.Select();
            SqlSelect s2 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["customer"]));
            s2.Columns.Add(SqlDml.Asterisk);
            s1.Columns.Add(SqlDml.Unique(s2) == true);
            Console.WriteLine(sqlDriver.Compile(s1).GetCommandText());
        }

        [Test]
        public void TrueTest()
        {
            SqlSelect s1 = SqlDml.Select();
            s1.Where = true;
            Console.WriteLine(sqlDriver.Compile(s1).GetCommandText());
        }

        [Test]
        public void UnionTest()
        {
            SqlSelect s1 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s1.Columns.Add(s1.From["address_id"]);
            SqlSelect s2 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s2.Columns.Add(s2.From["address_id"]);
            SqlSelect s3 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s3.Columns.Add(s3.From["address_id"]);

            Console.WriteLine(sqlDriver.Compile(s1.Union(s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.Union(s2).Union(s3)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.Union(s2.Union(s3))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Union(s1, s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Union(s1, s1.Union(s2))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Union(s1.Union(s2), s1)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Union(s1.Union(s2), s1.Union(s2))).GetCommandText());
            s3.Where = SqlDml.In(50.00, s1.Union(s2));
            Console.WriteLine(sqlDriver.Compile(s3).GetCommandText());
            SqlQueryRef qr = SqlDml.QueryRef(s1.Union(s2), "qr");
            Assert.Greater(qr.Columns.Count, 0);
        }

        [Test]
        public void UnionAllTest()
        {
            SqlSelect s1 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s1.Columns.Add(SqlDml.Asterisk);
            SqlSelect s2 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s2.Columns.Add(SqlDml.Asterisk);
            SqlSelect s3 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s3.Columns.Add(SqlDml.Asterisk);

            Console.WriteLine(sqlDriver.Compile(s1.UnionAll(s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.UnionAll(s2).UnionAll(s3)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.UnionAll(s2.UnionAll(s3))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.UnionAll(s1, s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.UnionAll(s1, s1.UnionAll(s2))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.UnionAll(s1.UnionAll(s2), s1)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.UnionAll(s1.UnionAll(s2), s1.UnionAll(s2))).GetCommandText());
        }

        [Test]
        [Ignore]
        public void IntersectTest() //TODO: Relook into the keyword for INTERSECT
        {
            SqlSelect s1 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s1.Columns.Add(SqlDml.Asterisk);
            SqlSelect s2 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s2.Columns.Add(SqlDml.Asterisk);
            SqlSelect s3 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s3.Columns.Add(SqlDml.Asterisk);

            Console.WriteLine(sqlDriver.Compile(s1.Intersect(s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.Intersect(s2).Intersect(s3)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.Intersect(s2.Intersect(s3))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Intersect(s1, s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Intersect(s1, s1.Intersect(s2))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Intersect(s1.Intersect(s2), s1)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.Intersect(s1.Intersect(s2), s1.Intersect(s2))).GetCommandText());
        }

        [Test]
        [Ignore]
        public void IntersectAllTest()//TODO: Relook into the keyword for INTERSECT ALL
        {
            SqlSelect s1 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s1.Columns.Add(SqlDml.Asterisk);
            SqlSelect s2 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s2.Columns.Add(SqlDml.Asterisk);
            SqlSelect s3 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["address"]));
            s3.Columns.Add(SqlDml.Asterisk);

            Console.WriteLine(sqlDriver.Compile(s1.IntersectAll(s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.IntersectAll(s2).IntersectAll(s3)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(s1.IntersectAll(s2.IntersectAll(s3))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.IntersectAll(s1, s2)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.IntersectAll(s1, s1.IntersectAll(s2))).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.IntersectAll(s1.IntersectAll(s2), s1)).GetCommandText());
            Console.WriteLine(sqlDriver.Compile(SqlDml.IntersectAll(s1.IntersectAll(s2), s1.IntersectAll(s2))).GetCommandText());
        }

        [Test]
        [Ignore]
        public void ExceptTest()
        {
            //SqlSelect s1 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["Address"]));
            //s1.Columns.Add(SqlDml.Asterisk);
            //SqlSelect s2 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["Address"]));
            //s2.Columns.Add(SqlDml.Asterisk);
            //SqlSelect s3 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["Address"]));
            //s3.Columns.Add(SqlDml.Asterisk);

            //Console.WriteLine(sqlDriver.Compile(s1.Except(s2)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(s1.Except(s2).Except(s3)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(s1.Except(s2.Except(s3))).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.Except(s1, s2)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.Except(s1, s1.Except(s2))).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.Except(s1.Except(s2), s1)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.Except(s1.Except(s2), s1.Except(s2))).GetCommandText());
        }

        [Test]
        [Ignore]
        public void ExceptAllTest()
        {
            //SqlSelect s1 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["Address"]));
            //s1.Columns.Add(SqlDml.Asterisk);
            //SqlSelect s2 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["Address"]));
            //s2.Columns.Add(SqlDml.Asterisk);
            //SqlSelect s3 = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["sakila"].Tables["Address"]));
            //s3.Columns.Add(SqlDml.Asterisk);

            //Console.WriteLine(sqlDriver.Compile(s1.ExceptAll(s2)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(s1.ExceptAll(s2).ExceptAll(s3)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(s1.ExceptAll(s2.ExceptAll(s3))).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.ExceptAll(s1, s2)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.ExceptAll(s1, s1.ExceptAll(s2))).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.ExceptAll(s1.ExceptAll(s2), s1)).GetCommandText());
            //Console.WriteLine(sqlDriver.Compile(SqlDml.ExceptAll(s1.ExceptAll(s2), s1.ExceptAll(s2))).GetCommandText());
        }

        [Test]
        [Ignore]
        public void FreeTextTest()
        {
            SqlSelect select = SqlDml.Select();
            var table = Catalog.Schemas["sakila"].Tables["Address"];
            select.From = SqlDml.QueryRef(SqlDml.FreeTextTable(table, "How can I make my own beers and ales?", EnumerableUtils.One(table.Columns[0].Name).ToList(), EnumerableUtils.One(table.Columns[0].Name).ToList()));
            select.Columns.Add(select.From.Asterisk);
            Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
        }

        [Test]
        [Ignore]
        public void FreeTextCreateTest()
        {
            //var table = Catalog.Schemas["Person"].Tables["Address"];
            //var ftindex = table.CreateFullTextIndex(string.Empty);
            //var ftColumn = ftindex.CreateIndexColumn(table.Columns[1]);
            //ftColumn.Languages.Add(new Language("English"));
            //ftindex.UnderlyingUniqueIndex = "PK_Address_AddressID";
            //var createIndex = SqlDdl.Create(ftindex);
            //Console.WriteLine(sqlDriver.Compile(createIndex).GetCommandText());

            //var dropIndex = SqlDdl.Drop(ftindex);
            //Console.WriteLine(sqlDriver.Compile(dropIndex).GetCommandText());
        }

    }
}
