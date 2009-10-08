// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.06

using System;
using Xtensive.Core;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Internals
{
  internal static class IncrementalVersionGenerator
  {
    public static object GetNext(object currentValue)
    {
      ArgumentValidator.EnsureArgumentNotNull(currentValue, "currentValue");

      TypeCode code = Type.GetTypeCode(currentValue.GetType());
      switch (code) {
      case TypeCode.SByte:
        return (sbyte) (((sbyte) currentValue) + 1);
      case TypeCode.Byte:
        return (byte) (((byte) currentValue) + 1);
      case TypeCode.Int16:
        return (short) (((short) currentValue) + 1);
      case TypeCode.UInt16:
        return (ushort) (((ushort) currentValue) + 1);
      case TypeCode.Int32:
        return ((int) currentValue) + 1;
      case TypeCode.UInt32:
        return ((uint) currentValue) + 1;
      case TypeCode.Int64:
        return ((long) currentValue) + 1;
      case TypeCode.UInt64:
        return ((ulong) currentValue) + 1;
      default:
        throw new NotSupportedException(string.Format(
          "Can't generate next value of type '{0}'.", currentValue.GetType().GetShortName()));
      }
    }
  }
}