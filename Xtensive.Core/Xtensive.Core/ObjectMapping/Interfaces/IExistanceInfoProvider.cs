// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Core.ObjectMapping
{
  internal interface IExistanceInfoProvider
  {
    void Get(ReadOnlyDictionary<object,object> modified, ReadOnlyDictionary<object,object> original,
      out IEnumerable<object> created, out IEnumerable<object> removed);
  }
}