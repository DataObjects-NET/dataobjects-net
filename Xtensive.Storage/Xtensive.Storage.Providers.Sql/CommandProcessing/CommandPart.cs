// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class CommandPart
  {
    public string Query;

    public readonly List<DbParameter> Parameters = new List<DbParameter>();
    public readonly List<IDisposable> Disposables = new List<IDisposable>();
  }
}