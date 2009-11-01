// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.04.03

namespace Xtensive.Core.Serialization
{
  public struct RecordRef
  {
    public long Id;
    public RecordRefType Type;


    public RecordRef(RecordRefType type)
    {
      Id = 0L;
      Type = type;
    }

    public RecordRef(long id)
    {
      Id = id;
      Type = RecordRefType.Extern;
    }
  }
}