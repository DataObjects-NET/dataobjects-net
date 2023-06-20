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
    /// Cuts down resolution of <see cref="DateTime"/> value if needed.
    /// </summary>
    /// <param name="origin">The value to fix.</param>
    /// <param name="provider">Type of provider.</param>
    /// <returns>New value with less resolution if <paramref name="provider"/> requires it or untouched <paramref name="origin"/> if the provider doesn't</returns>
    public static DateTime FixDateTimeForProvider(this DateTime origin, StorageProviderInfo providerInfo)
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
      return new DateTime(newTicks);
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
  }
}