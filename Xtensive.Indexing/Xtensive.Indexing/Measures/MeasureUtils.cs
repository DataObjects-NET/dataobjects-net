// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.15

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Indexing.Resources;
using System.Linq;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// <see cref="IMeasure{TItem,TResult}"/>-related utilities.
  /// </summary>
  /// <typeparam name="TItem">Type of measurable item.</typeparam>
  public static class MeasureUtils<TItem>
  {
    /// <summary>
    /// Ensures that the specified arguments are not null.
    /// </summary>
    /// <param name="first">The first.</param>
    /// <param name="second">The second.</param>
    public static void EnsureMeasurementsAreNotNull(IMeasure<TItem> first, IMeasure<TItem> second)
    {
      ArgumentValidator.EnsureArgumentNotNull(first, "first");
      ArgumentValidator.EnsureArgumentNotNull(second, "second");
    }

    /// <summary>
    /// Ensures that the both measurements have value.
    /// </summary>
    /// <param name="first">The first.</param>
    /// <param name="second">The second.</param>
    public static void EnsureMeasurementsHaveValue(IMeasure<TItem> first, IMeasure<TItem> second)
    {
      if (!first.HasResult && !second.HasResult)
        throw new InvalidOperationException(Strings.ExBothMeasurementsHaveNoValue);
    }

    /// <summary>
    /// Ensures that the measurement has value.
    /// </summary>
    /// <param name="measurement">The measurement.</param>
    /// <param name="name">The name of the measurement.</param>
    public static void EnsureMeasurementHasValue(IMeasure<TItem> measurement, string name)
    {
      if (!measurement.HasResult)
        throw new InvalidOperationException(String.Format(Strings.ExMeasurementMustHaveValue, name));
    }

    /// <summary>
    /// Gets the measurement results.
    /// </summary>
    /// <param name="measureResults">The measurements.</param>
    /// <param name="names">The names of measures.</param>
    /// <returns>The <see cref="Array"/> of measurement results.</returns>
    public static object[] GetMeasurements(IMeasureResultSet<TItem> measureResults, params string[] names)
    {
      ArgumentValidator.EnsureArgumentNotNull(names, "names");

      var results = new object[names.Length];
      for (int i = 0; i < names.Length; i++)
        results[i + 1] = measureResults[names[i]].Result;

      return results;
    }

    /// <summary>
    /// Adds one set of measure results to another one.
    /// </summary>
    /// <param name="current">Current measurements results.</param>
    /// <param name="appliedResults">Second measurements results.</param>
    /// <returns><see langword="true"/> if all measureResults were completed successfully 
    /// (<see cref="IMeasure{TItem}.HasResult"/> is <see langword="true"/>), 
    /// otherwise - <see langword="false"/>.</returns>
    public static bool BatchAdd(IMeasureResultSet<TItem> current, IMeasureResultSet<TItem> appliedResults)
    {
      ArgumentValidator.EnsureArgumentNotNull(current, "current");
      ArgumentValidator.EnsureArgumentNotNull(appliedResults, "appliedResults");

      if (current.Count != appliedResults.Count)
        throw new ArgumentException(Strings.MeasuresAndMeasurementsHaveDifferentAmountOfItems);

      bool success = true;

      for (int i = 0, count = (int)current.Count; i < count; i++)
        if (!current[i].AddWith(appliedResults[i]))
          success = false;

      return success;
    }

    /// <summary>
    /// Subtracts the one set of measure results from another one.
    /// </summary>
    /// <param name="current">Measure results to subtract from.</param>
    /// <param name="appliedResults">Second measurements set.</param>
    /// <returns><see langword="true"/> if all measureResults were completed successfully 
    /// (<see cref="IMeasure{TItem}.HasResult"/> is <see langword="true"/>), 
    /// otherwise - <see langword="false"/>.</returns>
    public static bool BatchSubtract(IMeasureResultSet<TItem> current, IMeasureResultSet<TItem> appliedResults)
    {
      ArgumentValidator.EnsureArgumentNotNull(current, "current");
      ArgumentValidator.EnsureArgumentNotNull(appliedResults, "appliedResults");

      if (current.Count != appliedResults.Count)
        throw new ArgumentException(Strings.MeasuresAndMeasurementsHaveDifferentAmountOfItems);

      bool success = true;

      for (int i = 0, count = (int)current.Count; i < count; i++)
        if (!current[i].SubtractWith(appliedResults[i]))
          success = false;

      return success;
    }

    /// <summary>
    /// Recalculates the measure results that have no value.
    /// </summary>
    /// <param name="measureResults">The measure results.</param>
    /// <param name="enumerable">The enumerable.</param>
    /// <returns><see langword="true"/> if all measure results were completed successfully 
    /// (<see cref="IMeasure{TItem}.HasResult"/> is <see langword="true"/>), 
    /// otherwise - <see langword="false"/>.</returns>
    public static bool BatchRecalculate(IMeasureResultSet<TItem> measureResults, IEnumerable<TItem> enumerable)
    {
      ArgumentValidator.EnsureArgumentNotNull(measureResults, "measureResults");
      ArgumentValidator.EnsureArgumentNotNull(enumerable, "enumerable");

      bool success = true;
      var measureToCalculate = measureResults.Where(measure => !measure.HasResult).ToList();
      foreach (IMeasure<TItem> measure in measureToCalculate)
        foreach (TItem item in enumerable)
          success &= measure.Add(item);
      return success;
    }

    /// <summary>
    /// Calculates measurements for provided <paramref name="enumerable"/>.
    /// </summary>
    /// <param name="measures">Measures to calculate.</param>
    /// <param name="enumerable">Elements to calculate measure for.</param>
    public static IMeasureResultSet<TItem> BatchCalculate(IMeasureSet<TItem> measures, IEnumerable<TItem> enumerable)
    {
      ArgumentValidator.EnsureArgumentNotNull(measures, "measures");
      ArgumentValidator.EnsureArgumentNotNull(enumerable, "enumerable");

      var results = new MeasureResultSet<TItem>(measures);
      foreach (IMeasure<TItem> measure in results)
        foreach (TItem item in enumerable)
          measure.Add(item);

      return results;
    }

    /// <summary>
    /// Calculates measurement for provided <paramref name="enumerable"/> and returns new measure.
    /// </summary>
    /// <param name="measure">Measure to calculate.</param>
    /// <param name="enumerable">Elements to calculate measure for.</param>
    public static IMeasure<TItem> BatchCalculate(IMeasure<TItem> measure, IEnumerable<TItem> enumerable)
    {
      ArgumentValidator.EnsureArgumentNotNull(measure, "measure");
      ArgumentValidator.EnsureArgumentNotNull(enumerable, "enumerable");

      IMeasure<TItem> result = measure.CreateNew();
      foreach (TItem item in enumerable)
        result.Add(item);
      return result;
    }

    /// <summary>
    /// Returns an <see cref="IMeasureSet{TItem}" /> for provided <paramref name="measures"/> by their <paramref name="names"/>.
    /// </summary>
    /// <param name="measures">The source measures.</param>
    /// <param name="names">Names of measures.</param>
    public static IMeasureSet<TItem> GetMeasures(IMeasureSet<TItem> measures, params string[] names)
    {
      ArgumentValidator.EnsureArgumentNotNull(names, "names");

      MeasureSet<TItem> result = new MeasureSet<TItem>();
      for (int i = 0; i < names.Length; i++) {
        string measureName = names[i];
        var measure = measures[measureName];
        if (measure==null)
          throw new ArgumentOutOfRangeException(string.Format("names[{0}]", i), String.Format(Strings.MeasureWithTheNameWasNotFound, measureName));
        result.Add(measure);
      }
      return result;
    }
  }
}