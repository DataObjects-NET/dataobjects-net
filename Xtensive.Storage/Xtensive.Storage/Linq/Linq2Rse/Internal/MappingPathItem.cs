// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.26

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Linq2Rse.Internal
{
  internal class MappingPathItem
  {
    public TypeInfo Type { get; private set; }
    public string FieldName { get; private set; }
    public string JoinedFieldName { get; private set; }


    // Constructor

    public MappingPathItem(TypeInfo type, string fieldName, string joinedFieldName)
    {
      Type = type;
      FieldName = fieldName;
      JoinedFieldName = joinedFieldName;
    }
  }
}