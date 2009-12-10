// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Core.ObjectMapping
{
  internal interface IExistanceInfoProvider
  {
    IDisposable Open(ReadOnlyDictionary<object,object> modified,
      ReadOnlyDictionary<object,object> original);

    IEnumerable<object> GetCreatedObjects();

    IEnumerable<object> GetRemovedObjects();
  }
}