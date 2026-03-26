// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class DatabaseOptions : IIdentifyableOptions,
    IValidatableOptions,
    IToNativeConvertable<DatabaseConfiguration>,
    INamedOptionsCollectionElement
  {
    public object Identifier => Name;

    /// <summary>
    /// Logical database name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Physical database name.
    /// </summary>
    public string RealName { get; set; }

    /// <summary>
    /// Type ID minimal value
    /// for types mapped to this database.
    /// Default value is <see cref="TypeInfo.MinTypeId"/>.
    /// </summary>
    public int MinTypeId { get; set; } = TypeInfo.MinTypeId;

    /// <summary>
    /// Type ID maximal value
    /// for types mapped to this database.
    /// Default value is <see cref="int.MaxValue"/>.
    /// </summary>
    public int MaxTypeId { get; set; } = int.MaxValue;

    /// <inheritdoc/>
    /// <exception cref="ArgumentException"><see cref="Name"/> is empty or null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="MinTypeId"/> or <see cref="MaxTypeId"/> is not in valid range.</exception>
    public void Validate()
    {
      if (Name.IsNullOrEmpty())
        throw new ArgumentException(Strings.ExArgumentCannotBeEmptyString,"Name");
      if (MinTypeId < TypeInfo.MinTypeId)
        throw new ArgumentOutOfRangeException("MinTypeId", MinTypeId,
          string.Format(Strings.ExArgumentMustBeGreaterThatOrEqualX, TypeInfo.MinTypeId));
      if (MaxTypeId < TypeInfo.MinTypeId)
        throw new ArgumentOutOfRangeException("MaxTypeId", MinTypeId,
          string.Format(Strings.ExArgumentMustBeGreaterThatOrEqualX, TypeInfo.MinTypeId));
    }

    /// <inheritdoc />
    public DatabaseConfiguration ToNative()
    {
      Validate();

      return new DatabaseConfiguration(Name) {
        RealName = RealName,
        MinTypeId = MinTypeId,
        MaxTypeId = MaxTypeId,
      };
    }
  }
}
