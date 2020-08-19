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
      var stringTypeInfo = new StorageTypeInfo(typeof(string), null, 100);
      var typeList = new[] {
        typeof(string),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(char),
        typeof(byte),
        typeof(sbyte)
      };
      foreach (var type in typeList) {
        Assert.IsTrue(TypeConversionVerifier
          .CanConvert(new StorageTypeInfo(type, null), stringTypeInfo));
      }
      typeList = new[] {
        typeof(Guid),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(byte[])
      };
      foreach (var type in typeList) {
        Assert.IsFalse(TypeConversionVerifier
          .CanConvert(new StorageTypeInfo(type, null), stringTypeInfo));
      }
    }

    [Test]
    public void CanConvertTest()
    {
      var supportedConversions = CreateSupportedConversions();
      var typeList = CreateTypeList();
      foreach (var type in typeList) {
        Assert.IsTrue(TypeConversionVerifier.CanConvert(new StorageTypeInfo(type, null), new StorageTypeInfo(type, null)));
        if (supportedConversions.ContainsKey(type)) {
          foreach (var targetType in typeList.Where(t => t != type)) {
            if (supportedConversions[type].Contains(targetType)) {
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new StorageTypeInfo(type, null), new StorageTypeInfo(targetType, null)));
            }
            else {
              Assert.IsFalse(TypeConversionVerifier.CanConvert(new StorageTypeInfo(type, null), new StorageTypeInfo(targetType, null)));
            }
          }
        }
      }
    }

    [Test]
    public void CanConvertNullableTest()
    {
      var nullableDefinition = typeof(Nullable<>);
      var supportedConversions = CreateSupportedConversions();
      var typeList = CreateTypeList();
      foreach (var type in typeList.Where(t => t.IsValueType)) {
        Assert.IsTrue(TypeConversionVerifier.CanConvert(new StorageTypeInfo(type, null), new StorageTypeInfo(type, null)));
        if (supportedConversions.ContainsKey(type)) {
          foreach (var targetType in typeList.Where(t => t != type && t.IsValueType)) {
            var nullableSource = nullableDefinition.MakeGenericType(type);
            var nullableTarget = nullableDefinition.MakeGenericType(targetType);
            if (supportedConversions[type].Contains(targetType)) {
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new StorageTypeInfo(nullableSource, null),
                new StorageTypeInfo(nullableTarget, null)));
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new StorageTypeInfo(nullableSource, null),
                new StorageTypeInfo(targetType, null)));
              Assert.IsFalse(TypeConversionVerifier.CanConvertSafely(new StorageTypeInfo(nullableSource, null),
                new StorageTypeInfo(targetType, null)));
              Assert.IsTrue(TypeConversionVerifier.CanConvert(new StorageTypeInfo(type, null),
                new StorageTypeInfo(nullableTarget, null)));
            }
            else {
              Assert.IsFalse(TypeConversionVerifier.CanConvert(
                new StorageTypeInfo(nullableDefinition.MakeGenericType(type), null),
                new StorageTypeInfo(nullableDefinition.MakeGenericType(targetType), null)));
            }
          }
        }
      }
    }

    [Test]
    public void CanConvertSafelyTest()
    {
      var sourceType = new StorageTypeInfo(typeof(string), null, 10);
      var targetType = new StorageTypeInfo(typeof(string), null, 5);
      Assert.IsTrue(TypeConversionVerifier.CanConvert(sourceType, targetType));
      Assert.IsFalse(TypeConversionVerifier.CanConvertSafely(sourceType, targetType));
      targetType = new StorageTypeInfo(typeof(string), null, 10);
      Assert.IsTrue(TypeConversionVerifier.CanConvertSafely(sourceType, targetType));
      targetType = new StorageTypeInfo(typeof(string), null, 11);
      Assert.IsTrue(TypeConversionVerifier.CanConvertSafely(sourceType, targetType));
    }

    private static Dictionary<Type, List<Type>> CreateSupportedConversions()
    {
      var supportedConversions = new Dictionary<Type, List<Type>>();
      AddConverter<bool>(supportedConversions, typeof(short), typeof(ushort), typeof(int), typeof(uint),
        typeof(long), typeof(ulong), typeof(char), typeof(double), typeof(float),
        typeof(decimal));
      AddConverter<byte>(supportedConversions, typeof(short), typeof(ushort), typeof(char), typeof(int),
        typeof(uint), typeof(long), typeof(ulong), typeof(double), typeof(float),
        typeof(decimal));
      AddConverter<sbyte>(supportedConversions, typeof(short), typeof(ushort), typeof(char), typeof(int),
        typeof(uint), typeof(long), typeof(ulong), typeof(double), typeof(float),
        typeof(decimal));
      AddConverter<short>(supportedConversions, typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(double), typeof(float), typeof(decimal));
      AddConverter<ushort>(supportedConversions, typeof(char), typeof(int), typeof(uint), typeof(long),
        typeof(ulong), typeof(double), typeof(float), typeof(decimal));
      AddConverter<int>(supportedConversions, typeof(long), typeof(ulong), typeof(double), typeof(float),
        typeof(decimal));
      AddConverter<uint>(supportedConversions, typeof(long), typeof(ulong), typeof(double), typeof(float),
        typeof(decimal));
      AddConverter<long>(supportedConversions, typeof(long), typeof(ulong), typeof(double), typeof(float),
        typeof(decimal));
      AddConverter<ulong>(supportedConversions, typeof(long), typeof(ulong), typeof(double), typeof(float),
        typeof(decimal));
      AddConverter<char>(supportedConversions, typeof(ushort), typeof(int), typeof(uint), typeof(long),
        typeof(ulong), typeof(double), typeof(float), typeof(decimal));
      AddConverter<decimal>(supportedConversions, typeof(double), typeof(float));
      AddConverter<float>(supportedConversions, typeof(double));
      return supportedConversions;
    }

    private static void AddConverter<T>(Dictionary<Type, List<Type>> supportedConversions, params Type[] types) =>
      supportedConversions.Add(typeof(T), new List<Type>(types));

    private List<Type> CreateTypeList()
    {
      var result = new List<Type>();
      result.Add(typeof(byte));
      result.Add(typeof(sbyte));
      result.Add(typeof(short));
      result.Add(typeof(ushort));
      result.Add(typeof(int));
      result.Add(typeof(uint));
      result.Add(typeof(long));
      result.Add(typeof(ulong));
      result.Add(typeof(char));
      result.Add(typeof(float));
      result.Add(typeof(double));
      result.Add(typeof(decimal));
      result.Add(typeof(DateTime));
      result.Add(typeof(TimeSpan));
      result.Add(typeof(byte[]));
      return result;
    }
  }
}