// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.19

using System;

namespace Xtensive.Indexing.Differential
{
  [Flags]
  internal enum DifferentialReaderEndMark
  {
    EndOfOriginReached = 1,
    EndOfInsertionsReached = 2,
    EndOfRemovalsReached = 4,
  }
}