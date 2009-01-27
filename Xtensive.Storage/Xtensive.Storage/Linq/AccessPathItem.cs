// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.26

namespace Xtensive.Storage.Linq
{
  // TODO: Refactor

  public class AccessPathItem
  {
    public string FieldName { get; private set; }
    public string JoinedFieldName { get; private set; }


    // Constructor

    public AccessPathItem(string fieldName, string joinedFieldName)
    {
      FieldName = fieldName;
      JoinedFieldName = joinedFieldName;
    }
  }
}