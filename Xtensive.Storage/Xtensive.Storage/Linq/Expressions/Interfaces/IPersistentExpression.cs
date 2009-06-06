// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System.Collections.Generic;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Expressions
{
  internal interface IPersistentExpression : IMappedExpression
  {
    TypeInfo PersistentType { get; }
    List<PersistentFieldExpression> Fields { get; }
  }
}