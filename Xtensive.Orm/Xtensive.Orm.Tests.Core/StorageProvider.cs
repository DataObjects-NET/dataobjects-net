// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.09

using System;

namespace Xtensive.Orm.Tests
{
  [Flags]
  public enum StorageProvider
  {
    Memory = 0x1,
    SqlServer = 0x2,
    SqlServerCe = 0x4,
    PostgreSql = 0x8,
    Oracle = 0x10,

    Sql = SqlServer | PostgreSql | Oracle | SqlServerCe,
    Index = Memory,
  }
}