// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

namespace Xtensive.Tuples.Packed
{
  internal sealed class PackedFieldDescriptor
  {
    public PackedFieldAccessor Accessor;

    public int Index;
    public int FieldIndex;

    public FieldPackingType PackingType;
  }
}