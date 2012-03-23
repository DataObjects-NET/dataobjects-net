// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.15

using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class SqlWorkerResult
  {
    public SqlExtractionResult Schema { get; set; }

    public MetadataSet Metadata { get; set; }
  }
}