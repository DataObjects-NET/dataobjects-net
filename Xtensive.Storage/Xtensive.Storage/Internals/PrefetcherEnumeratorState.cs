// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.14

namespace Xtensive.Storage.Internals
{
  internal enum PrefetcherEnumeratorState
  {
    EnumerationOfSource = 0,
    FlushingOfElementsProcessedDuringEnumeration,
    FlushingOfElementsFromTail,
    WaitingElementsProcessed,
    Finished
  }
}