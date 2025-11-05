// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.08

using System;

namespace Xtensive.Sql.Drivers.Firebird
{
  internal static class Constants
  {
    public const string DefaultSchemaName = ""; // "Firebird";

    // cannot use "FFF" cause it may lead to empty string for fractions part.
    public const string DateTimeFormatString = @"''\'yyyy\.MM\.dd HH\:mm\:ss\.fff\'''";
    public const string DateFormatString = @"''\'yyyy\.MM\.dd\'''";
    public const string TimeFormatString = @"''\'HH\:mm\:ss\.ffff\'''";
  }
}