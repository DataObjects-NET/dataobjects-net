// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.29

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public sealed class TypeConversionVerifierTest
  {
    [Test]
    public void ConversionToStringTest()
    {
      var stringTypeInfo = new TypeInfo(typeof (String), 100, null);
      var typeList = new[] {
        typeof (String),
        typeof (Int16),
        typeof (Int32),
        typeof (Int64),
        typeof (UInt16),
        typeof (UInt32), 
        typeof (UInt64), 
        typeof (Char), 
        typeof (Byte), 
        typeof (SByte)
      };
      foreach (var type in typeList)
          Assert.IsTrue(TypeConversionVerifier
            .CanConvert(new TypeInfo(type, null), stringTypeInfo));
      typeList = new[] {
        typeof (Guid),
        typeof (DateTime),
        typeof (TimeSpan),
        typeof (Byte[])
      };
      foreach (var type in typeList)
          Assert.IsFalse(TypeConversionVerifier
            .CanConvert(new TypeInfo(type, null), stringTypeInfo));
    }

    [Test]
    public void CanConvertTest()
    {
      var supportedConversions = CreateSupportedConversions();
      var typeList = CreateTypeList();
      foreach (var type in typeList) {
        Assert.IsTrue(TypeConversionVerifier.CanConvert(new TypeInfo(type, null), new TypeInfo(type, null)));
        if (supportedConversions.ContainsKey(type))
          foreach (var targetType in typeList.Where(t => t != type))
            if (supportedConversions[type].Contains(targetType))
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new TypeInfo(type, null), new TypeInfo(targetType, null)));
            else
              Assert.IsFalse(TypeConversionVerifier.CanConvert(new TypeInfo(type, null), new TypeInfo(targetType, null)));
      }
    }

    [Test]
    public void CanConvertNullableTest()
    {
      var nullableDefinition = typeof (Nullable<>);
      var supportedConversions = CreateSupportedConversions();
      var typeList = CreateTypeList();
      foreach (var type in typeList.Where(t => t.IsValueType)) {
        Assert.IsTrue(TypeConversionVerifier.CanConvert(new TypeInfo(type, null), new TypeInfo(type, null)));
        if (supportedConversions.ContainsKey(type)) {
          foreach (var targetType in typeList.Where(t => t!=type && t.IsValueType)) {
            var nullableSource = nullableDefinition.MakeGenericType(type);
            var nullableTarget = nullableDefinition.MakeGenericType(targetType);
            if (supportedConversions[type].Contains(targetType)) {
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new TypeInfo(nullableSource, null),
                new TypeInfo(nullableTarget, null)));
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new TypeInfo(nullableSource, null),
                new TypeInfo(targetType, null)));
              Assert.IsFalse(TypeConversionVerifier.CanConvertSafely(new TypeInfo(nullableSource, null),
                new TypeInfo(targetType, null)));
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new TypeInfo(type, null),
                new TypeInfo(nullableTarget, null)));
            }
            else
              Assert.IsFalse(TypeConversionVerifier.CanConvert(
                new TypeInfo(nullableDefinition.MakeGenericType(type), null),
                new TypeInfo(nullableDefinition.MakeGenericType(targetType), null)));
          }
        }
      }
    }

    [Test]
    public void CanConvertSafelyTest()
    {
      var sourceType = new TypeInfo(typeof (String), 10, null);
      var targetType = new TypeInfo(typeof (String), 5, null);
      Assert.IsTrue(TypeConversionVerifier.CanConvert(sourceType, targetType));
      Assert.IsFalse(TypeConversionVerifier.CanConvertSafely(sourceType, targetType));
      targetType = new TypeInfo(typeof (String), 10, null);
      Assert.IsTrue(TypeConversionVerifier.CanConvertSafely(sourceType, targetType));
      targetType = new TypeInfo(typeof (String), 11, null);
      Assert.IsTrue(TypeConversionVerifier.CanConvertSafely(sourceType, targetType));
    }

    private static Dictionary<Type, List<Type>> CreateSupportedConversions()
    {
      var supportedConversions = new Dictionary<Type, List<Type>>();
      AddConverter<Boolean>(supportedConversions, typeof(Int16), typeof(UInt16), typeof(Int32), typeof(UInt32),
        typeof(Int64), typeof(UInt64), typeof(Char), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Byte>(supportedConversions, typeof(Int16), typeof(UInt16), typeof(Char), typeof(Int32),
        typeof(UInt32), typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<SByte>(supportedConversions, typeof(Int16), typeof(UInt16), typeof(Char), typeof(Int32),
        typeof(UInt32), typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Int16>(supportedConversions, typeof(Int32), typeof(UInt32), typeof(Int64), typeof(UInt64),
        typeof(Double), typeof(Single), typeof(Decimal));
      AddConverter<UInt16>(supportedConversions, typeof(Char), typeof(Int32), typeof(UInt32), typeof(Int64),
        typeof(UInt64), typeof(Double), typeof(Single), typeof(Decimal));
      AddConverter<Int32>(supportedConversions, typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<UInt32>(supportedConversions, typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Int64>(supportedConversions, typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<UInt64>(supportedConversions, typeof(Int64), typeof(UInt64), typeof(Double), typeof(Single),
        typeof(Decimal));
      AddConverter<Char>(supportedConversions, typeof(UInt16), typeof(Int32), typeof(UInt32), typeof(Int64),
        typeof(UInt64), typeof(Double), typeof(Single), typeof(Decimal));
      AddConverter<Decimal>(supportedConversions, typeof(Double), typeof(Single));
      AddConverter<Single>(supportedConversions, typeof(Double));
      return supportedConversions;
    }

    private static void AddConverter<T>(Dictionary<Type, List<Type>> supportedConversions,
      params Type[] types)
    {
      supportedConversions.Add(typeof(T), new List<Type>(types));
    }

    private List<Type> CreateTypeList()
    {
      var result = new List<Type>();
      result.Add(typeof (Byte));
      result.Add(typeof (SByte));
      result.Add(typeof (Int16));
      result.Add(typeof (UInt16));
      result.Add(typeof (Int32));
      result.Add(typeof (UInt32));
      result.Add(typeof (Int64));
      result.Add(typeof (UInt64));
      result.Add(typeof (Char));
      result.Add(typeof (Single));
      result.Add(typeof (Double));
      result.Add(typeof (Decimal));
      result.Add(typeof (DateTime));
      result.Add(typeof (TimeSpan));
      result.Add(typeof (Byte[]));
      return result;
    }
  }
}