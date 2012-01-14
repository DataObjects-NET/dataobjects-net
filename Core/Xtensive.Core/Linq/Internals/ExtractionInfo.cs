// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;

namespace Xtensive.Linq
{
  [Serializable]
  internal class ExtractionInfo
  {
    public Expression Key { get; set; }

    public Expression Value { get; set; }

    public ComparisonMethodInfo MethodInfo { get; set; }

    public Expression ComparisonMethod { get; set; }

    public bool ReversingRequired { get; set; }

    public bool InversingRequired { get; set; }

    public ExpressionType? ComparisonOperation { get; set; }
  }
}