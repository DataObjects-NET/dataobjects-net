using System;
using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.Sqlite
{
  [TestFixture]
  [Explicit]
  public class ExtractorTest : SqlTest
  {
    [Test]
    public void BaseTest()
    {
      var schema = ExtractDefaultSchema();
      foreach (var table in schema.Tables) {
        Console.WriteLine("Table: " + table.Name);
        Console.WriteLine("Columns");
        foreach (var column in table.TableColumns)
          Console.WriteLine(string.Format("  {0}, {1}, {2}, {3}", column.Name, column.DataType, column.IsNullable, column.DefaultValue));
        Console.WriteLine("Indexes");
        foreach (var index in table.Indexes) {
          Console.WriteLine(string.Format("  Name: {0}, Unique : {1}", index.Name, index.IsUnique));
          foreach (var column in index.Columns)
            Console.WriteLine("    " + column.Name);
        }
        Console.WriteLine("Constraints");
        foreach (var constraint in table.TableConstraints) {
          var uniqueConstraint = constraint as UniqueConstraint;
          if (uniqueConstraint!=null) {
            Console.WriteLine(string.Format("  {0}, Primary Key", constraint.Name));
            foreach (var column in uniqueConstraint.Columns)
              Console.WriteLine("    " + column.Name);
            continue;
          }
          var foreignKey = constraint as ForeignKey;
          if (foreignKey!=null) {
            Console.WriteLine(string.Format("  {0}, Foreign Key, {1}, On Update = {2}, On Delete = {3}", constraint.Name, foreignKey.ReferencedTable.Name, foreignKey.OnUpdate, foreignKey.OnDelete));
            foreach (var column in foreignKey.Columns)
              Console.WriteLine("    " + column.Name);
            continue;
          }
        }
      }
    }

    protected override string Url
    {
      get { return TestUrl.Sqlite3; }
    }
  }
}