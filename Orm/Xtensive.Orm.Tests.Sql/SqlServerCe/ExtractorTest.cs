// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.29

using System;
using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.SqlServerCe
{
  [TestFixture, Explicit]
  public class ExtractorTest : SqlTest
  {
    [Test]
    public void BaseTest()
    {
      var schema = ExtractDefaultSchema();
      foreach (var table in schema.Tables) {
        Console.WriteLine("Table: " + table.Name);
        Console.WriteLine("Columns");
        foreach (var column in table.TableColumns) {
          Console.WriteLine($"  {column.Name}, {column.DataType}, {column.IsNullable}, {column.DefaultValue}");
        }
        Console.WriteLine("Indexes");
        foreach (var index in table.Indexes) {
          Console.WriteLine($"  {index.Name}, {index.IsUnique}");
          foreach (var column in index.Columns) {
            Console.WriteLine("    "+column.Name);
          }
        }
        Console.WriteLine("Constraints");
        foreach (var constraint in table.TableConstraints) {
          var uniqueConstraint = constraint as UniqueConstraint;
          if (uniqueConstraint!=null) {
            Console.WriteLine($"  {constraint.Name}, Primary Key");
            foreach (var column in uniqueConstraint.Columns)
              Console.WriteLine("    " + column.Name);
            continue;
          }
          var foreignKey = constraint as ForeignKey;
          if (foreignKey!=null) {
            Console.WriteLine($"  {constraint.Name}, Foreign Key, {foreignKey.ReferencedTable.Name}, On Update = {foreignKey.OnUpdate}, On Delete = {foreignKey.OnDelete}");
            foreach (var column in foreignKey.Columns)
              Console.WriteLine("    " + column.Name);
            continue;
          }
        }
        Console.WriteLine();
      }
    }
  }
}