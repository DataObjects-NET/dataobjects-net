// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.07

using System;
using System.Data.Common;

namespace Xtensive.Sql
{
  public abstract class CustomTypeMapper
  {
    public abstract Type Type { get; }

    public virtual bool Enabled { get { return true; } }

    public virtual bool ParameterCastRequired { get { return false; } }

    public abstract object ReadValue(DbDataReader reader, int index);

    public abstract void BindValue(DbParameter parameter, object value);

    public abstract SqlValueType MapType(int? length, int? precision, int? scale);
  }
}