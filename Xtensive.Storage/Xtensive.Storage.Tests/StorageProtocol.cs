// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.09

using System;

namespace Xtensive.Storage.Tests
{
  [Flags]
  public enum StorageProtocol
  {
    Memory = 0x1,
    SqlServer = 0x2,
    PostgreSql = 0x4,
    Oracle = 0x8,

    Sql = SqlServer | PostgreSql | Oracle,
    Index = Memory,
  }
}