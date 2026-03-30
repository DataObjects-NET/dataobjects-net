// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25


using System;
using NUnit.Framework;
using Xtensive.Conversion;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class ConverterTestBase
  {
    protected bool UseLog;

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
                  TestLog.Info($"Reconversion of {convertedValue} of {typeof(TTo).GetShortName()} type to type {typeof(TFrom).GetShortName()} failed.");
              }
              catch (ArgumentOutOfRangeException e) {
                if (innerLogFlag)
                  TestLog.Info($"Reconversion of {convertedValue} of {typeof(TTo).GetShortName()} type to type {typeof(TFrom).GetShortName()} failed.");
              }
              catch (FormatException e) {
                if (innerLogFlag)
                  TestLog.Info($"Reconversion of {convertedValue} of {typeof(TTo).GetShortName()} type to type {typeof(TFrom).GetShortName()} failed.");
              }
            }
          }
          catch (OverflowException e) {
            if (innerLogFlag)
              TestLog.Info($"Conversion of {value} of {typeof(TFrom).GetShortName()} type to type {typeof(TTo).GetShortName()} failed.");
          }
          catch (ArgumentOutOfRangeException e) {
            if (innerLogFlag)
              TestLog.Info($"Conversion of {value} of {typeof(TFrom).GetShortName()} type to type {typeof(TTo).GetShortName()} failed.");
          }
          catch (FormatException e) {
            if (innerLogFlag)
              TestLog.Info($"Conversion of {value} of {typeof(TFrom).GetShortName()} type to type {typeof(TTo).GetShortName()} failed.");
          }
        }
        else if (innerLogFlag)
          TestLog.Info($"Conversion from {typeof(TFrom).GetShortName()} to {typeof(TTo).GetShortName()} is not supported.");
      }
    }
  }
}
