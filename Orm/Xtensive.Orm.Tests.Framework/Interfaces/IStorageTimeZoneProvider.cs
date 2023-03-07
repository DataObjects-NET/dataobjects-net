// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Orm.Tests
{
  public interface IStorageTimeZoneProvider
  {
    public TimeSpan TimeZoneOffset { get; }
  }
}