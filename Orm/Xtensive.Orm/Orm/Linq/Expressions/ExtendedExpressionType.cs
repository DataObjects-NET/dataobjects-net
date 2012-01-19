// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.05

using System;

namespace Xtensive.Orm.Linq.Expressions
{
  [Serializable]
  internal enum ExtendedExpressionType
  {
    Projection = 1000,
    Key,
    Column,
    Field,
    StructureField,
    Entity,
    EntityField,
    EntitySet,
    ItemProjector,
    Grouping,
    SubQuery,
    Marker,
    LocalCollection,
    Structure,
    Constructor,
    FullText
  }
}