// Copyright (C) 2008-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Sql.Dml;


namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Test helper class.
  /// </summary>
  public static class TestHelper
  {
    /// <summary>
    /// Ensures full garbage collection.
    /// </summary>
    public static void CollectGarbage()
    {
      CollectGarbage(TestInfo.IsPerformanceTestRunning);
    }

    /// <summary>
    /// Ensures full garbage collection.
    /// </summary>
    /// <param name="preferFullRatherThanFast">Full rather then fast collection should be performed.</param>
    public static void CollectGarbage(bool preferFullRatherThanFast)
    {
      var baseSleepTime = 1;
      if (preferFullRatherThanFast) {
        baseSleepTime = 100;
      }

      for (var i = 0; i<5; i++) {
        Thread.Sleep(baseSleepTime);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
      }
    }

    public static System.Configuration.Configuration GetConfigurationForAssembly(this Type typeFromAssembly)
    {
      return typeFromAssembly.Assembly.GetAssemblyConfiguration();
    }

    public static System.Configuration.Configuration GetConfigurationForAssembly(this object instanceOfTypeFromAssembly)
    {
      return instanceOfTypeFromAssembly.GetType().Assembly.GetAssemblyConfiguration();
    }

    /// <summary>
    /// Cuts down resolution of <see cref="DateTime"/> value if needed, according to current <see cref="StorageProviderInfo.Instance"/>.
    /// </summary>
    /// <param name="origin">The value to adjust.</param>
    /// <returns>New value with less resolution if the provider requires it, otherwise, untouched <paramref name="origin"/>.</returns>
    public static DateTime AdjustDateTimeForCurrentProvider(this DateTime origin)
    {
      var provider = StorageProviderInfo.Instance;
      return AdjustDateTimeForProvider(origin, provider);
    }

    /// <summary>
    /// Cuts down resolution of <see cref="DateTime"/> value if needed.
    /// </summary>
    /// <param name="origin">The value to adjust.</param>
    /// <param name="providerInfo">Type of provider.</param>
    /// <returns>New value with less resolution if the provider requires it, otherwise, untouched <paramref name="origin"/>.</returns>
    public static DateTime AdjustDateTimeForProvider(this DateTime origin, StorageProviderInfo providerInfo)
    {
      var provider = providerInfo.Provider;
      switch (provider) {
        case StorageProvider.MySql:
          return providerInfo.Info.StorageVersion < StorageProviderVersion.MySql56
            ? AdjustDateTime(origin, 0)
            : AdjustDateTime(origin, 6);
        case StorageProvider.Firebird:
          return AdjustDateTime(origin, 4);
        case StorageProvider.PostgreSql:
          return AdjustDateTime(origin, 6);
        case StorageProvider.Oracle:
          return AdjustDateTime(origin, 7);
        default:
          return origin;
      }
    }

    /// <summary>
    /// Cuts down fractions of <see cref="DateTime"/> value (nanoseconds, milliseconds, etc) to desired value.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="desiredFractions">Number of fractional points to keep (from 0 to 7).</param>
    /// <param name="requireRound">Indicates whether value should be rounded after cutting off.</param>
    /// <returns>Result value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Valid fractions should be between 0 and 7 (7 included).</exception>
    public static DateTime AdjustDateTime(this DateTime origin, byte desiredFractions, bool requireRound = false)
    {
      if (desiredFractions == 7) {
        return origin;
      }

      const int baseDivider = 10_000_000; // no fractions
      var currentDivider = baseDivider / (desiredFractions switch {
        0 => 1,
        1 => 10,
        2 => 100,
        3 => 1000,
        4 => 10000,
        5 => 100000,
        6 => 1000000,
        _ => throw new ArgumentOutOfRangeException(nameof(desiredFractions))
      });

      var ticks = origin.Ticks;

      var newTicks = requireRound
        ? ((ticks % currentDivider) / (currentDivider / 10)) >= 5
          ? ticks - (ticks % currentDivider) + currentDivider
          : ticks - (ticks % currentDivider)
        : ticks - (ticks % currentDivider);
      return new DateTime(newTicks);
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Cuts down resolution of <see cref="TimeOnly"/> value if needed, according to current <see cref="StorageProviderInfo.Instance"/>.
    /// </summary>
    /// <param name="origin">The value to adjust.</param>
    /// <returns>New value with less resolution if the provider requires it, otherwise, untouched <paramref name="origin"/>.</returns>
    public static TimeOnly AdjustTimeOnlyForCurrentProvider(this TimeOnly origin)
    {
      var provider = StorageProviderInfo.Instance;
      return AdjustTimeOnlyForProvider(origin, provider);
    }

    /// <summary>
    /// Cuts down resolution of <see cref="TimeOnly"/> value if needed.
    /// </summary>
    /// <param name="origin">The value to adjust.</param>
    /// <param name="providerInfo">Type of provider.</param>
    /// <returns>New value with less resolution if the provider requires it, otherwise, untouched <paramref name="origin"/>.</returns>
    public static TimeOnly AdjustTimeOnlyForProvider(this TimeOnly origin, StorageProviderInfo providerInfo)
    {
      long? divider;
      var provider = providerInfo.Provider;
      switch (provider) {
        case StorageProvider.MySql:
          divider = providerInfo.Info.StorageVersion < StorageProviderVersion.MySql56 ? 10000000 : 10;
          break;
        case StorageProvider.Firebird:
          divider = 1000;
          break;
        case StorageProvider.PostgreSql:
          divider = 10;
          break;
        default:
          divider = null;
          break;
      }

      if (!divider.HasValue) {
        return origin;
      }
      var ticks = origin.Ticks;
      var newTicks = ticks - (ticks % divider.Value);
      return new TimeOnly(newTicks);
    }

    public static void AddValueRow(this SqlInsert insert, in (SqlColumn column, SqlExpression value) first, params (SqlColumn column, SqlExpression value)[] additional)
    {
      var additional1 = additional ?? Array.Empty<(SqlColumn,SqlExpression)>();
      var row = new Dictionary<SqlColumn, SqlExpression>(1 + additional1.Length);
      row.Add(first.column, first.value);
      foreach (var keyValue in additional1) {
        row.Add(keyValue.column, keyValue.value);
      }
      insert.ValueRows.Add(row);
    }
#endif
  }
}