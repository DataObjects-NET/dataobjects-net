// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.06.03

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal class RemapContext
  {
    public ParameterExpression SubqueryParameterExpression { get; set; }
  }
}