// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.10.19

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  public class PerformanceResultContainer : Dictionary<string, Dictionary<string, long>>
  {
    private const string DoubleValueFormatTemplate = "##.0000000";

    public override string ToString()
    {
      var stringBuilder = new StringBuilder();

      foreach (var counter in this) {
        using (MakeMeasuresRegion(counter.Key.IsNullOrEmpty() ? "Default" : string.Format("-{0}-", counter.Key), stringBuilder)) {
          foreach (var point in counter.Value) {
            var value = AdaptToReadableFrom(point.Value, out var measure);
            _ = stringBuilder.AppendLine(string.Format("{0}:{1} {2}", point.Key, value.ToString(DoubleValueFormatTemplate), measure));
          }
        }
      }
      return stringBuilder.ToString();
    }

    private IDisposable MakeMeasuresRegion(string nodeName, StringBuilder output)
    {
      _ = output.AppendLine(string.Format("----------------------{0}-------------------------", nodeName));
      var disposable = new Disposable(
        (state) => output.AppendLine("------------------------end---------------------------"));
      return disposable;
    }

    private double AdaptToReadableFrom(long bytes, out Measure measure)
    {
      measure = Measure.Kilobytes;
      var kilobytes = (double)bytes / 1024;
      if (kilobytes < 1024) {
        return kilobytes;
      }
      measure = Measure.Megabytes;
      var megabytes = kilobytes / 1024;
      if (megabytes < 1024) {
        return megabytes;
      }
      measure = Measure.Gigabytes;
      return megabytes / 1024;
    }
  }
}
