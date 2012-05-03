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
    SqlServer = 0x1,
    SqlServerCe = 0x2,
    PostgreSql = 0x4,
    Oracle = 0x8,
    MySql = 0x10,
    Firebird = 0x20,
		Sqlite = 0x40
  }
}