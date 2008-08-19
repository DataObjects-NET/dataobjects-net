// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.19

using System;

namespace Xtensive.Indexing.Differential
{
  [Flags]
  internal enum DifferentialReaderState
  {
    ReadingOrigin = 1,
    ReadingInsertions = 2,
    AtTheBeginning = 4, 
    EndOfOriginReached = 8,
    EndOfInsertionsReached = 16,
    EndOfRemovalsReached = 32,
  }
}