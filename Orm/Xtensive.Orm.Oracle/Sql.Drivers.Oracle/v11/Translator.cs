// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.Oracle.v11
{
  internal class Translator : v10.Translator
  {
    //so called INITRANS value
    protected const int TableTransactionalParallelismLevel = 7;
    protected const int IndexTransactionalParallelismLevel = 7;

    public override string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      if (section == CreateTableSection.TableElementsExit)
        return $") INITRANS {TableTransactionalParallelismLevel} ";
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      if (section == CreateIndexSection.ColumnsExit)
        return $") INITRANS {IndexTransactionalParallelismLevel}";
      //CreateIndexSection.ColumnsExit:
      //return ")"
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      if (section == NodeSection.Exit) {
        return node.Ascending ? "ASC NULLS FIRST" : "DESC NULLS LAST";
      }
      return string.Empty;
    }

    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      if (type.Type == SqlType.Interval)
        return "INTERVAL DAY(6) TO SECOND(6)";
      return base.Translate(type);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}