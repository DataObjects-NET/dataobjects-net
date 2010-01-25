// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.PostgreSql.v8_3
{
  internal class Compiler : v8_2.Compiler
  {
    
  public override void Visit(SqlCreateIndex node, IndexColumn item)
  {
    if (!node.Index.IsFullText) 
      base.Visit(node, item);
    // FullText builds expression instead of list of columns in Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
  }

   // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }    
  }
}