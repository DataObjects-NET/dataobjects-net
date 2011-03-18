// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.MySQL.v5_0
{
    [Serializable]
    public class IndexTest50 : IndexTest
    {
        private SqlDriver sqlDriver;
        private SqlConnection sqlConnection;
        private DbCommand dbCommand;
        private DbCommand sqlCommand;

        private Schema schema = null;

        protected override string Url
        {
            get { return TestUrl.MySQL50; }
        }

        #region Helpers

        private void Dump(ISqlCompileUnit statement)
        {
            sqlCommand.CommandText = sqlDriver.Compile(statement).GetCommandText();
            sqlCommand.Prepare();
            Console.WriteLine(sqlCommand.CommandText);
        }

        private SqlCompilationResult Compile(ISqlCompileUnit statement)
        {
            return sqlDriver.Compile(statement);
        }

        #endregion

        #region Setup and TearDown

        [TestFixtureSetUp]
        public void RealTestFixtureSetUp()
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
                catalog = sqlDriver.ExtractCatalog(sqlConnection);
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
        public void RealTestFixtureTearDown()
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


        protected override void CreateTable()
        {
            Table t;
            t = schema.CreateTable(TableName);
            t.CreateColumn("first", new SqlValueType(SqlType.VarChar, 50));
            t.CreateColumn("second", new SqlValueType(SqlType.VarChar, 50));
            var c1 = t.CreateColumn("third", new SqlValueType(SqlType.VarChar));
            var c2 = t.CreateColumn("forth", new SqlValueType(SqlType.VarChar));

            var tr = SqlDml.TableRef(t);
            c1.Expression = SqlDml.Concat(tr["first"], " ", tr["second"]);
            c1.IsPersisted = false;
            c1.IsNullable = true;

            c2.Expression = SqlDml.Concat(tr["second"], " ", tr["first"]);
            c2.IsPersisted = false;
            c2.IsNullable = true;

            Dump(SqlDdl.Create(t));
            //ExecuteNonQuery(SqlDdl.Create(t));
        }

        [Test]
        public override void CreateExpressionIndexTest()
        {
            // Creating index
            var t = schema.Tables[TableName];
            var i = t.CreateIndex(ExpressionIndexName);
            i.CreateIndexColumn(t.TableColumns["third"]);
            i.CreateIndexColumn(t.TableColumns["forth"]);
            //ExecuteNonQuery(SqlDdl.Create(i));
            Dump(SqlDdl.Create(i));

            // Extracting index and checking its properties
            var c2 = ExtractCatalog();
            var s2 = c2.DefaultSchema;
            var t2 = s2.Tables[TableName];
            var i2 = t2.Indexes[ExpressionIndexName];
            Assert.IsNotNull(i2);
            Assert.AreEqual(2, i2.Columns.Count);

            Assert.IsTrue(!t2.TableColumns["third"].Expression.IsNullReference());
            Assert.IsTrue(!t2.TableColumns["forth"].Expression.IsNullReference());
        }

        public override void CreateFilteredIndexTest()
        {
            if (GetType() == typeof(IndexTest50))
                Assert.Ignore("Filtered indexes are not supported.");
        }
    }

}
