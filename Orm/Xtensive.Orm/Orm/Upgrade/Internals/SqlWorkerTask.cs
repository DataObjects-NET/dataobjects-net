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
    DropSchema = 1 << 0,
    ExtractSchema = 1 << 1,
    ExtractMetadataTypes = 1 << 2,
    ExtractMetadataAssemblies = 1 << 3,
    ExtractMetadataExtension = 1 << 4,
    ExtractMetadata = ExtractMetadataAssemblies | ExtractMetadataExtension | ExtractMetadataTypes
  }
}