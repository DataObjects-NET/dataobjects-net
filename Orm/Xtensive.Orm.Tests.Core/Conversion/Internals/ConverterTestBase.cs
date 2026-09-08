// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.25


using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Conversion;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public abstract class ConverterTestBase
  {
    /// <summary>
    /// Defines whether tests will write output messagest to log. Default is false
    /// </summary>
    protected virtual bool UseLog => false;

    public void OneValueTest<TFrom, TTo>(TFrom value, int count)
    {
      AdvancedConverter<TFrom, TTo> advancedConverterTo   = null;
      AdvancedConverter<TTo, TFrom> advancedConverterFrom = null;
      try {
        advancedConverterTo   = AdvancedConverter<TFrom, TTo>.Default;
        advancedConverterFrom = AdvancedConverter<TTo, TFrom>.Default;
      }
      catch (InvalidOperationException e) {
        if (UseLog)
          TestLog.Info($"{e.Message}");
      }

      for (int i = 0; i < count; i++) {
        bool innerLogFlag = UseLog && (i==0);
        if (advancedConverterTo!=null) {
          try {
            TTo convertedValue = advancedConverterTo.Convert(value);
            if (innerLogFlag)
              TestLog.Info($"Converting {value} of {typeof(TFrom).GetShortName()} type. Result: {convertedValue} of {typeof(TTo).GetShortName()} type");
            if (advancedConverterFrom != null) {
              try {
                TFrom reconvertedValue = advancedConverterFrom.Convert(convertedValue);
                if (innerLogFlag)
                  TestLog.Info($"Reconverting {convertedValue} of {typeof(TTo).GetShortName()} type. Result: {reconvertedValue} of {typeof(TFrom).GetShortName()} type.");
                if ((advancedConverterTo.IsRough)) {
                  if (innerLogFlag)
                    TestLog.Info($"Conversion from {typeof(TFrom).GetShortName()} to {typeof(TTo).GetShortName()} is rough.");
                }
                else
                  Assert.That(reconvertedValue, Is.EqualTo(value), "reconvertedValue");
              }
              catch (OverflowException e) {
                if (innerLogFlag)
                  TestLog.Error($"Reconversion of {convertedValue} of {typeof(TTo).GetShortName()} type to type {typeof(TFrom).GetShortName()} failed.");
              }
              catch (ArgumentOutOfRangeException e) {
                if (innerLogFlag)
                  TestLog.Error($"Reconversion of {convertedValue} of {typeof(TTo).GetShortName()} type to type {typeof(TFrom).GetShortName()} failed.");
              }
              catch (FormatException e) {
                if (innerLogFlag)
                  TestLog.Error($"Reconversion of {convertedValue} of {typeof(TTo).GetShortName()} type to type {typeof(TFrom).GetShortName()} failed.");
              }
            }
          }
          catch (OverflowException e) {
            if (innerLogFlag)
              TestLog.Error($"Conversion of {value} of {typeof(TFrom).GetShortName()} type to type {typeof(TTo).GetShortName()} failed.");
          }
          catch (ArgumentOutOfRangeException e) {
            if (innerLogFlag)
              TestLog.Error($"Conversion of {value} of {typeof(TFrom).GetShortName()} type to type {typeof(TTo).GetShortName()} failed.");
          }
          catch (FormatException e) {
            if (innerLogFlag)
              TestLog.Error($"Conversion of {value} of {typeof(TFrom).GetShortName()} type to type {typeof(TTo).GetShortName()} failed.");
          }
        }
        else if (innerLogFlag)
          TestLog.Warning($"Conversion from {typeof(TFrom).GetShortName()} to {typeof(TTo).GetShortName()} is not supported.");
      }
    }
  }


  [TestFixture(Category = "AdvancedTypeConverters")]
  public abstract class ConverterTestBase<TFrom> : ConverterTestBase
  {
    /// <summary>
    /// Itterations for <see cref="OneValueTest{TFrom, TTo}(TFrom, TTo)"/>
    /// </summary>
    protected virtual int IterationCount => 50;

    /// <summary>
    /// Constants to test on.
    /// </summary>
    protected abstract TFrom[] Constants { get; }

    /// <summary>
    /// Allowed target types for converston. If type is not in this collection, then test for the type will be ignored
    /// </summary>
    protected abstract HashSet<Type> AllowedTargetTypes { get; }

    [Test]
    public void BooleanTest()
    {
      RequireConversionTo<bool>();
      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, bool>(constant, IterationCount);
      }
    }

    [Test]
    public void ByteTest()
    {
      RequireConversionTo<byte>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, byte>(constant, IterationCount);
      }
    }

    [Test]
    public void SByteTest()
    {
      RequireConversionTo<sbyte>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, sbyte>(constant, IterationCount);
      }
    }

    [Test]
    public void ShortTest()
    {
      RequireConversionTo<short>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, short>(constant, IterationCount);
      }
    }

    [Test]
    public void UShortTest()
    {
      RequireConversionTo<ushort>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, ushort>(constant, IterationCount);
      }
    }

    [Test]
    public void IntTest()
    {
      RequireConversionTo<int>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, int>(constant, IterationCount);
      }
    }

    [Test]
    public void UIntTest()
    {
      RequireConversionTo<uint>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, uint>(constant, IterationCount);
      }
    }

    [Test]
    public void LongTest()
    {
      RequireConversionTo<long>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, long>(constant, IterationCount);
      }
    }

    [Test]
    public void ULongTest()
    {
      RequireConversionTo<ulong>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, ulong>(constant, IterationCount);
      }
    }

    [Test]
    public void FloatTest()
    {
      RequireConversionTo<float>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, float>(constant, IterationCount);
      }
    }

    [Test]
    public void DoubleTest()
    {
      RequireConversionTo<double>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, double>(constant, IterationCount);
      }
    }

    [Test]
    public void DecimalTest()
    {
      RequireConversionTo<decimal>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, decimal>(constant, IterationCount);
      }
    }

    [Test]
    public void DateTimeTest()
    {
      RequireConversionTo<DateTime>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, DateTime>(constant, IterationCount);
      }
    }

    [Test]
    public void DateOnlyTest()
    {
      RequireConversionTo<DateOnly>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, DateOnly>(constant, IterationCount);
      }
    }

    [Test]
    public void TimeOnlyTest()
    {
      RequireConversionTo<TimeOnly>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, TimeOnly>(constant, IterationCount);
      }
    }

    [Test]
    public void TimeSpanTest()
    {
      RequireConversionTo<TimeSpan>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, TimeSpan>(constant, IterationCount);
      }
    }

    [Test]
    public void GuidTest()
    {
      RequireConversionTo<Guid>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, Guid>(constant, IterationCount);
      }
    }

    [Test]
    public void StringTest()
    {
      RequireConversionTo<string>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, string>(constant, IterationCount);
      }
    }

    [Test]
    public void CharTest()
    {
      RequireConversionTo<char>();

      foreach (TFrom constant in Constants) {
        OneValueTest<TFrom, char>(constant, IterationCount);
      }
    }

    private void RequireConversionTo<TTo>()
    {
      var targetType = typeof(TTo);
      if (!AllowedTargetTypes.Contains(targetType))
        throw new IgnoreException($"Conversion to {targetType.Name} is not allowed to check");
    }
  }
}
