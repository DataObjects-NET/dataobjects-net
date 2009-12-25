// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.15

using System;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.All
{
  /// <summary>
  /// Does nothing, but references types from all Storage assemblies.
  /// </summary>
  public sealed class Referencer
  {
    private Type[] types = new [] {
      typeof (Sql.All.Referencer), // Referencing all SQL DOM providers
      typeof (Entity),
      typeof (Command),
      typeof (StorageInfo),
      typeof (TypeInfo),
      typeof (Providers.Index.Memory.DomainHandler),
      typeof (Providers.Sql.DomainHandler),
      typeof (RecordSet),
    };


    // Constructors

    // This is the only one. So you can't instantiate this type.
    private Referencer()
    {
    }
  }
}