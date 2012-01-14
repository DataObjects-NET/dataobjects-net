// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;

namespace Xtensive.Orm.Linq
{
  [Flags]
  internal enum ColumnExtractionModes
  {
    Default = 0,
    Distinct = 0x2,
    Ordered = 0x4,
    TreatEntityAsKey = 0x8,
    KeepTypeId = 0x10,
    OmitLazyLoad = 0x20,
    KeepSegment = Distinct | Ordered
  }
}