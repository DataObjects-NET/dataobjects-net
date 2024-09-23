// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.


namespace Xtensive.Orm.Configuration.Options
{
  internal interface IIdentifyableOptions
  {
    /// <summary>
    /// Identifier of options instance in collections where uniqueness is required.
    /// </summary>
    object Identifier { get; }
  }
}
