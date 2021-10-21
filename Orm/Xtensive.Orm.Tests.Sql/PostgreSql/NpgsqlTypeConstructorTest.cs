﻿// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.14

using System;
using NUnit.Framework;
using Xtensive.Orm.Providers.PostgreSql;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public class NpgsqlTypeConstructorTest : SqlTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    [Test]
    public void NpgsqlPointConstructorTest()
    {
      SqlExpression point = SqlDml.Native("point'(0,1)'");
      CheckInequality(PostgresqlSqlGeometryDml.NpgsqlPointConstructor(2, 3), point);
    }

    [Test]
    public void NpgsqlBoxConstructorTest()
    {
      SqlExpression box = SqlDml.Native("box'(2,3),(0,1)'");
      CheckEquality(
        PostgresqlSqlGeometryDml.NpgsqlBoxConstructor(
          PostgresqlSqlGeometryDml.NpgsqlPointConstructor(2, 3),
          PostgresqlSqlGeometryDml.NpgsqlPointConstructor(0, 1)),
        box);

      CheckEquality(
        PostgresqlSqlGeometryDml.NpgsqlBoxConstructor(
          SqlDml.Native("point'(2,3)'"),
          SqlDml.Native("point'(0,1)'")),
        box);
    }

    [Test]
    public void NpgsqlLSegConstructorTest()
    {
      SqlExpression lSeg = SqlDml.Native("lseg'[(0,1),(2,3)]'");
      CheckEquality(
        PostgresqlSqlGeometryDml.NpgsqlLSegConstructor(
          PostgresqlSqlGeometryDml.NpgsqlPointConstructor(0, 1),
          PostgresqlSqlGeometryDml.NpgsqlPointConstructor(2, 3)),
        lSeg);

      CheckEquality(
        PostgresqlSqlGeometryDml.NpgsqlLSegConstructor(
          SqlDml.Native("point'(0,1)'"),
          SqlDml.Native("point'(2,3)'")),
        lSeg);
    }

    [Test]
    public void NpgsqlCircleConstructorTest()
    {
      SqlExpression circle = SqlDml.Native("circle'<(0,1),10>'");

      CheckEquality(
        PostgresqlSqlGeometryDml.NpgsqlCircleConstructor(
          PostgresqlSqlGeometryDml.NpgsqlPointConstructor(0, 1),
          10),
        circle);

      CheckEquality(
        PostgresqlSqlGeometryDml.NpgsqlCircleConstructor(
          SqlDml.Native("point'(0,1)'"),
          10),
        circle);
    }

    private void CheckInequality(SqlExpression left, SqlExpression right)
    {
      var select = SqlDml.Select("ok");
      select.Where = left!=right;
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = command.ExecuteReader()) {
          Assert.IsTrue(reader.Read());
        }
      }
    }

    private void CheckEquality(SqlExpression left, SqlExpression right)
    {
      var select = SqlDml.Select("ok");
      select.Where = left==right;
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = command.ExecuteReader()) {
          Assert.IsTrue(reader.Read());
        }
      }
    }
  }
}
