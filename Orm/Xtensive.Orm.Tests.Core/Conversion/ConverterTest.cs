// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.03

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Conversion;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;
using MethodInfo=System.Reflection.MethodInfo;
using PropertyInfo=System.Reflection.PropertyInfo;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  [TestFixture]
  public class ConverterTest : ConverterTestBase
  {
    private readonly IAdvancedConverterProvider provider = AdvancedConverterProvider.Default;
    private static readonly Random random = RandomManager.CreateRandom(SeedVariatorType.CallingType);
    private const int enumTestCount = 1000;

    private readonly Type[] types = new Type[] {
      typeof (bool),
      typeof (byte),
      typeof (sbyte),
      typeof (short),
      typeof (ushort),
      typeof (int),
      typeof (uint),
      typeof (long),
      typeof (ulong),
      typeof (float),
      typeof (double),
      typeof (decimal),
      typeof (DateTime),
      typeof (Guid),
      typeof (string),
      typeof (char)
    };

    public enum ByteEnum : byte
    {
      Value1_1,
      Value1_2,
      Value1_3,
    }

    public enum SByteEnum : sbyte
    {
      Value2_1,
      Value2_2,
      Value2_3,
    }

    public enum IntEnum : int
    {
      Value2_1,
      Value2_2,
      Value2_3,
    }

    public enum UIntEnum : uint
    {
      Value2_1,
      Value2_2,
      Value2_3,
    }

    public enum ShortEnum : short
    {
      Value2_1,
      Value2_2,
      Value2_3,
    }

    public enum UShortEnum : ushort
    {
      Value2_1,
      Value2_2,
      Value2_3,
    }

    public enum LongEnum : long
    {
      Value2_1,
      Value2_2,
      Value2_3,
    }

    public enum ULongEnum : ulong
    {
      Value2_1,
      Value2_2,
      Value2_3,
    }

    [Test]
    [Explicit]
    [Category("Debug")]
    public void DelegateTest()
    {
      Converter<ByteEnum, byte> converter = DelegateHelper.CreatePrimitiveCastDelegate<ByteEnum, byte>();
      converter(ByteEnum.Value1_1);
    }

    [Test]
    public void EnumTest()
    {
      EnumTestInternal<ByteEnum>();
      EnumTestInternal<SByteEnum>();
      EnumTestInternal<ShortEnum>();
      EnumTestInternal<UShortEnum>();
      EnumTestInternal<IntEnum>();
      EnumTestInternal<UIntEnum>();
      EnumTestInternal<LongEnum>();
      EnumTestInternal<ULongEnum>();
    }

    private void EnumTestInternal<T>()
    {
      EnumTestInternal<T, byte>();
      EnumTestInternal<T, sbyte>();
      EnumTestInternal<T, short>();
      EnumTestInternal<T, ushort>();
      EnumTestInternal<T, int>();
      EnumTestInternal<T, uint>();
      EnumTestInternal<T, long>();
      EnumTestInternal<T, ulong>();
      EnumTestInternal<T, ByteEnum>();
      EnumTestInternal<T, SByteEnum>();
      EnumTestInternal<T, ShortEnum>();
      EnumTestInternal<T, UShortEnum>();
      EnumTestInternal<T, IntEnum>();
      EnumTestInternal<T, UIntEnum>();
      EnumTestInternal<T, LongEnum>();
      EnumTestInternal<T, ULongEnum>();
    }

    private void EnumTestInternal<TFrom, TTo>()
    {
      AdvancedConverter<TFrom, TTo> advancedConverter = AdvancedConverter<TFrom, TTo>.Default;
      AdvancedConverter<TTo, TFrom> backwardAdvancedConverter = AdvancedConverter<TTo, TFrom>.Default;
      IInstanceGenerator<TFrom> instanceGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<TFrom>();
      Assert.IsNotNull(advancedConverter);
      Assert.IsNotNull(backwardAdvancedConverter);
      Assert.IsNotNull(instanceGenerator);
      foreach (TFrom from in instanceGenerator.GetInstances(random, enumTestCount)) {
        TTo result = advancedConverter.Convert(from);
        TFrom backwardResult = backwardAdvancedConverter.Convert(result);
        Assert.AreEqual(from, backwardResult);
      }
    }

    [Test]
    public void ConstructorTest()
    {
      Biconverter<int, bool> bi = new Biconverter<int, bool>(
        delegate(int value) { return value > 0; },
        delegate(bool value) { return value ? 1 : -1; });
      Assert.AreEqual(true, bi.ConvertForward(1));
      Assert.AreEqual(false, bi.ConvertForward(0));
      Assert.AreEqual(1, bi.ConvertBackward(true));
      Assert.AreEqual(-1, bi.ConvertBackward(false));
    }

    [Test]
    public void Test()
    {
      foreach (Type typeFrom in types) {
        foreach (Type typeTo in types) {
          //AdvancedConverter<typeFrom, typeTo>();
          Type providerType = provider.GetType();
          MethodInfo getConverterMethod =
            providerType.GetMethod("GetConverter", Array.Empty<Type>()).GetGenericMethodDefinition().MakeGenericMethod(new Type[] {typeFrom, typeTo});
          object converter = null;
          try {
            converter = getConverterMethod.Invoke(provider, null);
          }
          catch {
            TestLog.Info("Conversion from {0} to {1} is not supported", typeFrom.GetShortName(), typeTo.GetShortName());
          }
          if (converter!=null) {
            Type maskInterface = typeof (IAdvancedConverter<,>).MakeGenericType(new Type[] {typeFrom, typeTo});
            Type[] foundInterfaces =
              converter.GetType().FindInterfaces(
                delegate(Type typeObj, Object criteriaObj) { return typeObj==maskInterface; }, null);
            foreach (Type type in foundInterfaces) {
              PropertyInfo propertyInfo = type.GetProperty("IsRough");
              bool isRough = (bool) propertyInfo.GetValue(converter, null);
              TestLog.Info("Conversion from {0} to {1} is {2}", typeFrom.GetShortName(),
                typeTo.GetShortName(), isRough ? "Rough" : "Strict");
            }
          }
        }
      }
    }

    [Test]
    public void NullableTest()
    {
      int count = 100;

      NullableTestInternal<int, int?>(count, true);
      NullableTestInternal<int?, int?>(count, false);
      NullableTestInternal<int?, int>(count, true);

      NullableTestInternal<int?, long?>(count, false);
      NullableTestInternal<int?, long>(count, true);
      NullableTestInternal<int, long?>(count, true);

      NullableTestInternal<int?, string>(count, false);
    }

    private void NullableTestInternal<TFrom, TTo>(int count, bool assertNullConversion)
    {
      AdvancedConverter<TFrom, TTo> advancedConverter = AdvancedConverter<TFrom, TTo>.Default;
      Assert.IsNotNull(advancedConverter);
      Type fromType = typeof (TFrom);
      Type toType = typeof (TTo);
      bool fromIsNullable = fromType.IsNullable();
      Type fromNonNullableType = fromIsNullable ? fromType.GetGenericArguments()[0] : typeof (TFrom);

      // Random test
      Action<object, int> nullableConverterTest =
        DelegateHelper.CreateDelegate<Action<object, int>>(this, GetType(),
          "NullableConverterTest", fromNonNullableType, fromType, toType);
      nullableConverterTest.Invoke(advancedConverter, count);

      // Null test
      if (fromIsNullable) {
        if (assertNullConversion) {
          AssertEx.Throws<ArgumentNullException>(delegate { advancedConverter.Convert((TFrom) (object) null); });
        }
        else {
          advancedConverter.Convert((TFrom) (object) null);
        }
      }
    }

    private void NullableConverterTest<T, TFrom, TTo>(object converterObject, int count)
    {
      AdvancedConverter<TFrom, TTo> advancedConverter = (AdvancedConverter<TFrom, TTo>) converterObject;
      AdvancedConverter<TTo, TFrom> reverseAdvancedConverter = AdvancedConverter<TTo, TFrom>.Default;
      IInstanceGenerator<T> instanceGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>();
      foreach (T instance in instanceGenerator.GetInstances(random, count)) {
        TTo result = advancedConverter.Convert((TFrom) (object) instance);
        if (reverseAdvancedConverter!=null) {
          TFrom backwardResult = reverseAdvancedConverter.Convert(result);
          if (!advancedConverter.IsRough && !reverseAdvancedConverter.IsRough)
            Assert.AreEqual(instance, backwardResult);
        }
      }
    }

    private byte Enum1ToByte(ByteEnum value)
    {
      return (byte) value;
    }

    private sbyte Enum1ToSByte(ByteEnum value)
    {
      return (sbyte) value;
    }

    private ulong Enum1ToULong(ByteEnum value)
    {
      return (ulong) value;
    }

    private long Enum1ToLong(ByteEnum value)
    {
      return (long) value;
    }

    private ByteEnum ByteToEnum1(byte value)
    {
      return (ByteEnum) value;
    }

    private ByteEnum SByteToEnum1(sbyte value)
    {
      return (ByteEnum) value;
    }

    private ByteEnum ULongToEnum1(ulong value)
    {
      return (ByteEnum) value;
    }

    private ByteEnum LongToEnum1(long value)
    {
      return (ByteEnum) value;
    }

    private SByteEnum ULongToEnum2(ulong value)
    {
      return (SByteEnum) value;
    }

    private SByteEnum LongToEnum2(long value)
    {
      return (SByteEnum) value;
    }
  }
}