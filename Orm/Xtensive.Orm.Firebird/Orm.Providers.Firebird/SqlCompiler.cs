// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.27

using System;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Providers.Firebird
{
  internal class SqlCompiler : Providers.SqlCompiler
  {
    // Constructors
    
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}