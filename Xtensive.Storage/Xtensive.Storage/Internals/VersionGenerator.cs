// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.06

using System;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Default generator providing next values for fields marked by <see cref="VersionAttribute"/>.
  /// </summary>
  public static class VersionGenerator
  {
    /// <summary>
    /// Gets the next version value.
    /// </summary>
    /// <param name="currentVersion">The current version.</param>
    /// <returns>Next version value.</returns>
    /// <exception cref="NotSupportedException">Unsupported <paramref name="currentVersion"/> type.</exception>
    public static object GenerateNextVersion(object currentVersion)
    {
      ArgumentValidator.EnsureArgumentNotNull(currentVersion, "currentValue");

      TypeCode code = Type.GetTypeCode(currentVersion.GetType());
      switch (code) {
      case TypeCode.SByte:
        return (sbyte) (((sbyte) currentVersion) + 1);
      case TypeCode.Byte:
        return (byte) (((byte) currentVersion) + 1);
      case TypeCode.Int16:
        return (short) (((short) currentVersion) + 1);
      case TypeCode.UInt16:
        return (ushort) (((ushort) currentVersion) + 1);
      case TypeCode.Int32:
        return ((int) currentVersion) + 1;
      case TypeCode.UInt32:
        return ((uint) currentVersion) + 1;
      case TypeCode.Int64:
        return ((long) currentVersion) + 1;
      case TypeCode.UInt64:
        return ((ulong) currentVersion) + 1;
      case TypeCode.Decimal:
        return ((decimal) currentVersion) + 1;
      case TypeCode.Double:
        return ((double) currentVersion) + 1;
      case TypeCode.Single:
        return ((float) currentVersion) + 1;
      case TypeCode.DateTime:
        var next = DateTime.UtcNow;
        next = new DateTime(next.Year, next.Month, next.Day, next.Hour, next.Minute, next.Second, next.Millisecond - next.Millisecond % 10); 
        var current = ((DateTime) currentVersion);
        current = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, current.Second, current.Millisecond - current.Millisecond % 10); 
        var increment = current.AddMilliseconds(10);
        return next<increment ? increment : next;
      case TypeCode.String:
        return Guid.NewGuid().ToString();
      case TypeCode.Object:
        if (currentVersion is Guid)
          return Guid.NewGuid();
        throw new NotSupportedException(string.Format(
          Strings.ExCannotGenerateNextVersionValueOfTypeX, currentVersion.GetType().GetShortName()));
      default:
        throw new NotSupportedException(string.Format(
          Strings.ExCannotGenerateNextVersionValueOfTypeX, currentVersion.GetType().GetShortName()));
      }
    }
  }
}