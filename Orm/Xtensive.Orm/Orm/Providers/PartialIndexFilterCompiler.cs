// Copyright (C) 2013-2024 Xtensive LLC
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.07.18

using System.Linq;
using Xtensive.Orm.Model;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal sealed class PartialIndexFilterCompiler
  {
    public string Compile(HandlerAccessor handlers, IndexInfo index)
    {
      var filter = index.Filter;
      var fieldsCount = filter.Fields.Count;

      var table = SqlDml.TableRef(CreateStubTable(index.ReflectedType.MappingName, fieldsCount));
      // Translation of ColumnRefs without alias seems broken, use original name as alias.
      var columns = filter.Fields
        .Select(field => field.Column.Name)
        .Select((name, i) => SqlDml.ColumnRef(table.Columns[i], name))
        .Cast<SqlExpression>()
        .ToList(fieldsCount);

      var processor = new ExpressionProcessor(filter.Expression, handlers, null, true, columns);
      var fragment = SqlDml.Fragment(processor.Translate());
      var result = handlers.StorageDriver.Compile(fragment).GetCommandText();
      return result;
    }

    private Table CreateStubTable(string name, int columnsCount)
    {
      var catalog = new Catalog(string.Empty);
      var schema = catalog.CreateSchema(string.Empty);
      var table = schema.CreateTable(name);
      for (int i = 0; i < columnsCount; i++)
        table.CreateColumn("c" + i);
      return table;
    }
  }
}