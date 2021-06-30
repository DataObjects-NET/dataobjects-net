// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Well-known databases which are used in tests
  /// </summary>
  public static class WellKnownDatabases
  {
    public const string SqlServerMasterDb = "master";

    /// <summary>
    /// Can be used in multi-database tests.
    /// </summary>
    public static class MultiDatabase
    {
      public const string MainDb = "DO-Tests";

      // Additional databases which can be used for multi-database tests
      public const string AdditionalDb1 = "DO-Tests-1";
      public const string AdditionalDb2 = "DO-Tests-2";
      public const string AdditionalDb3 = "DO-Tests-3";
      public const string AdditionalDb4 = "DO-Tests-4";
      public const string AdditionalDb5 = "DO-Tests-5";
      public const string AdditionalDb6 = "DO-Tests-6";
      public const string AdditionalDb7 = "DO-Tests-7";
      public const string AdditionalDb8 = "DO-Tests-8";
      public const string AdditionalDb9 = "DO-Tests-9";
      public const string AdditionalDb10 = "DO-Tests-10";
      public const string AdditionalDb11 = "DO-Tests-11";
      public const string AdditionalDb12 = "DO-Tests-12";
    }
  }
}
