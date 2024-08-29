// Copyright (C) 2007-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Configuration.Options
{
  internal interface IIdentifyableOptions
  {
    object Identifier { get; }
  }

  internal interface IValidatableOptions
  {
    /// <summary>
    /// Performs validation of properties
    /// </summary>
    void Validate();
  }

  internal interface IHasDatabaseOption : IIdentifyableOptions
  {
    string Database { get; set; }

    object GetMappedIdentifier(IDictionary<string, DatabaseOptions> databaseMap);
  }

}
