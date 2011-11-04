// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.SqlServer.v10
{
  internal class Translator : v09.Translator
  {
    public override string DateTimeFormatString { get { return @"'cast ('\'yyyy\-MM\-ddTHH\:mm\:ss\.fff\'' as datetime2)'"; } }

    public override string Translate(SqlCompilerContext context, Ddl.SqlCreateIndex node, CreateIndexSection section)
    {
      switch (section) {
        case CreateIndexSection.ColumnsExit:
          if (!node.Index.IsSpatial)
            return base.Translate(context, node, section);

          var table = node.Index.DataTable as Table;
          var column = table.TableColumns[node.Index.Columns[0].Name];
          return column.DataType.Type == SqlType.Geometry ? ") USING GEOMETRY_GRID WITH ( BOUNDING_BOX = ( 0, 0, 500, 200))" : ") USING GEOGRAPHY_GRID";
      }
      return base.Translate(context, node, section);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}