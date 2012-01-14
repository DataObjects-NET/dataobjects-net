// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25


using System;
using NUnit.Framework;
using Xtensive.Conversion;
using Xtensive.Reflection;

namespace Xtensive.Tests.Conversion
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
        Log.Info("{0}", e.Message);
      }

      for (int i = 0; i < count; i++) {
        bool innerLogFlag = UseLog && (i==0);
        if (advancedConverterTo!=null) {
          try {
            TTo convertedValue = advancedConverterTo.Convert(value);
            if (innerLogFlag)
              Log.Info("Converting {0} of {1} type. Result: {2} of {3} type", value,
                typeof(TFrom).GetShortName(),
                convertedValue, typeof(TTo).GetShortName());
            if (advancedConverterFrom != null) {
              try {
                TFrom reconvertedValue = advancedConverterFrom.Convert(convertedValue);
                if (innerLogFlag)
                  Log.Info("Reconverting {0} of {1} type. Result: {2} of {3} type.", convertedValue,
                    typeof(TTo).GetShortName(),
                    reconvertedValue, typeof(TFrom).GetShortName());
                if ((advancedConverterTo.IsRough)) {
                  if (innerLogFlag)
                    Log.Info("Conversion from {0} to {1} is rough.",
                      typeof(TFrom).GetShortName(), typeof(TTo).GetShortName());
                }
                else
                  Assert.AreEqual(value, reconvertedValue, "reconvertedValue");
              }
              catch (OverflowException e) {
                if (innerLogFlag)
                  Log.Info(e, "Reconversion of {0} of {1} type to type {2} failed.", convertedValue,
                    typeof(TTo).GetShortName(), typeof(TFrom).GetShortName());
              }
              catch (ArgumentOutOfRangeException e) {
                if (innerLogFlag)
                  Log.Info(e, "Reconversion of {0} of {1} type to type {2} failed.", convertedValue,
                    typeof(TTo).GetShortName(), typeof(TFrom).GetShortName());
              }
              catch (FormatException e) {
                if (innerLogFlag)
                  Log.Info(e, "Reconversion of {0} of {1} type to type {2} failed.", convertedValue,
                    typeof(TTo).GetShortName(), typeof(TFrom).GetShortName());
              }
            }
          }
          catch (OverflowException e) {
            if (innerLogFlag)
              Log.Info(e, "Conversion of {0} of {1} type to type {2} failed.", value,
                typeof(TFrom).GetShortName(), typeof(TTo).GetShortName());
          }
          catch (ArgumentOutOfRangeException e) {
            if (innerLogFlag)
              Log.Info(e, "Conversion of {0} of {1} type to type {2} failed.", value,
                typeof (TFrom).GetShortName(), typeof (TTo).GetShortName());
          }
          catch (FormatException e) {
            if (innerLogFlag)
              Log.Info(e, "Conversion of {0} of {1} type to type {2} failed.", value,
                typeof(TFrom).GetShortName(), typeof(TTo).GetShortName());
          }
        }
        else if (innerLogFlag)
          Log.Info("Conversion from {0} to {1} is not supported.", typeof (TFrom).GetShortName(),
            typeof (TTo).GetShortName());
      }
    }
  }
}
