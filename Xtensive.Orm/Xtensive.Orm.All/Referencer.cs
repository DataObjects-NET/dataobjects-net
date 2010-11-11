// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.15

using System;
using Xtensive.Orm;
using Xtensive.Storage.Commands;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.All
{
  /// <summary>
  /// Does nothing, but references types from all Storage assemblies.
  /// </summary>
  public sealed class Referencer
  {
    private Type[] types = new [] {
      typeof (Sql.SqlDriver), 
      typeof (Entity),
      typeof (Command),
      typeof (StorageInfo),
      typeof (TypeInfo),
      typeof (Providers.Indexing.Memory.DomainHandler),
      typeof (Providers.Sql.DomainHandler),
      typeof (RecordQuery),
    };


    // Constructors

    // This is the only one. So you can't instantiate this type.
    private Referencer()
    {
    }
  }
}