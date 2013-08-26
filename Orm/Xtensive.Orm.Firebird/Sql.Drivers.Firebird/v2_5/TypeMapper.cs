﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.10

using System;
using System.Data.Common;
using System.Data;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
  internal class TypeMapper : Sql.TypeMapper
  {
    public override bool IsParameterCastRequired(Type type)
    {
      return true;
    }

    public override SqlValueType MapBoolean(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    public override SqlValueType MapUShort(int? length, int? precision, int? scale)
    {
      return base.MapInt(length, precision, scale);
    }

    public override SqlValueType MapUInt(int? length, int? precision, int? scale)
    {
      return base.MapLong(length, precision, scale);
    }

    public override SqlValueType MapULong(int? length, int? precision, int? scale)
    {
      return base.MapString(30, null, null);
      ;
    }

    public override SqlValueType MapChar(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Char, 1);
    }

    public override SqlValueType MapGuid(int? length, int? precision, int? scale)
    {
      return base.MapString(36, null, null);
      ;
    }

    public override SqlValueType MapByte(int? length, int? precision, int? scale)
    {
      return base.MapShort(length, precision, scale);
    }

    public override SqlValueType MapSByte(int? length, int? precision, int? scale)
    {
      return base.MapShort(length, precision, scale);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      string s = reader.GetString(index);
      if (string.IsNullOrEmpty(s))
        return null;
      return SqlHelper.GuidFromString(s);
    }

    public override object ReadChar(DbDataReader reader, int index)
    {
      char c = (char) base.ReadChar(reader, index);
      if (char.IsControl(c) || char.IsPunctuation(c))
        return c;
      if (char.IsWhiteSpace(c))
        return null;
      return c;
    }

    public override void BindChar(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      if (value==null || (default(char).Equals(value))) {
        parameter.Value = DBNull.Value;
        return;
      }
      var _char = (char) value;
      parameter.Value = _char==default(char) ? string.Empty : _char.ToString();
    }

    public override object ReadString(DbDataReader reader, int index)
    {
      string s = (string) base.ReadString(reader, index);
      if (s!=null)
        s = s.Trim();
      return s;
    }

    public override void BindULong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void BindBoolean(DbParameter parameter, object value)
    {
      object val = DBNull.Value;
      if (value!=null)
        val = ((bool) value) ? (short) 1 : (short) 0;
      BindShort(parameter, val);
    }

    public override void BindGuid(DbParameter parameter, object value)
    {
      BindString(parameter, value==null ? DBNull.Value : (object) SqlHelper.GuidToString((Guid) value));
    }

    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}