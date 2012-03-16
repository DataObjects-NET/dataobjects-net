// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System.Collections.Generic;
using Xtensive.Collections;

namespace Xtensive.ObjectMapping
{
  internal interface IExistanceInfoProvider
  {
    void Get(ReadOnlyDictionary<object,object> modified, ReadOnlyDictionary<object,object> original,
      out IEnumerable<object> created, out IEnumerable<object> removed);
  }
}