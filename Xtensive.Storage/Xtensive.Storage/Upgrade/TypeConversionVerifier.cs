// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Verifies whether the conversion between types is allowed or not.
  /// </summary>
  public static class TypeConversionVerifier
  {
    private static readonly Dictionary<Type, List<Type>> supportedConversions;

    /// <summary>
    /// Verifies whether the source type can be converted to the target type. 
    /// Loss of data is allowed.
    /// </summary>
    /// <param name="from">The source type.</param>
    /// <param name="to">The target type.</param>
    /// <returns>
    /// <see langword="true"/> if the source type can be converted to the 
    /// target type; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool CanConvert(TypeInfo from, TypeInfo to)
    {
      ArgumentValidator.EnsureArgumentNotNull(from, "from");
      ArgumentValidator.EnsureArgumentNotNull(to, "to");
      if(from == to || (from.Type != typeof(Byte[]) && to.Type == typeof(String)))
        return true;
      if(from.IsNullable && !to.IsNullable)
        return false;
      var fromType = from.Type;
      var toType = to.Type;
      if(fromType.IsNullable())
        fromType = from.Type.GetGenericArguments()[0];
      if(toType.IsNullable())
        toType = to.Type.GetGenericArguments()[0];
      if(!supportedConversions.ContainsKey(fromType))
        return false;
      return supportedConversions[fromType].Contains(toType);
    }

    /// <summary>
    /// Verifies whether the source type can be converted to the target 
    /// type without loss of data.
    /// </summary>
    /// <param name="from">The source type.</param>
    /// <param name="to">The target type.</param>
    /// <returns>
    /// <see langword="true"/> if the source type can be converted to the 
    /// target type without loss of data; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool CanConvertSafely(TypeInfo from, TypeInfo to)
    {
      if(!CanConvert(from, to))
        return false;
      return to.Length >= from.Length;
    }


    // Constructors

    static TypeConversionVerifier()
    {
      supportedConversions = new Dictionary<Type, List<Type>>();
      AddConverter<Boolean>(typeof(Int16), typeof(UInt16), typeof(Int32), typeof(UInt32),
        typeof(Int64), typeof(UInt64), typeof(Char), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Byte>(typeof(Int16), typeof(UInt16), typeof(Char), typeof(Int32),
        typeof(UInt32), typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<SByte>(typeof(Int16), typeof(UInt16), typeof(Char), typeof(Int32),
        typeof(UInt32), typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Int16>(typeof(Int32), typeof(UInt32), typeof(Int64), typeof(UInt64),
        typeof(Double), typeof(Single), typeof(Decimal));
      AddConverter<UInt16>(typeof(Char), typeof(Int32), typeof(UInt32), typeof(Int64),
        typeof(UInt64), typeof(Double), typeof(Single), typeof(Decimal));
      AddConverter<Int32>(typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<UInt32>(typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Int64>(typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<UInt64>(typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Char>(typeof(UInt16), typeof(Int32), typeof(UInt32), typeof(Int64),
        typeof(UInt64), typeof(Double), typeof(Single), typeof(Decimal));
      AddConverter<Decimal>(typeof(Double), typeof(Single));
      AddConverter<Single>(typeof(Double));
    }

    private static void AddConverter<T>(params Type[] types)
    {
      ArgumentValidator.EnsureArgumentIsInRange(types.Length, 0, int.MaxValue, "types.Length");
      supportedConversions.Add(typeof(T), new List<Type>(types));
    }
  }
}