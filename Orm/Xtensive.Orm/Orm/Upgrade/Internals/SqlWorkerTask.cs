// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.15

using System;

namespace Xtensive.Orm.Upgrade
{
  [Flags]
  internal enum SqlWorkerTask
  {
    None = 0,
    DropSchema = 0x1,
    ExtractSchema = 0x2,
    ExtractMetadata = 0x4,
  }
}