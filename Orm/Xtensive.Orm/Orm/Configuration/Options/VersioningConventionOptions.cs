// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class VersioningConventionOptions : IToNativeConvertable<VersioningConvention>
  {
    /// <summary>
    /// Versioning policy for entities.
    /// Default value is <see cref="VersioningConvention.DefaultVersioningPolicy"/>
    /// </summary>
    public EntityVersioningPolicy EntityVersioningPolicy { get; set; } = VersioningConvention.DefaultVersioningPolicy;

    /// <summary>
    /// Value indicating that change of an <see cref="EntitySet{TItem}"/> owner version should be denied where possible.
    /// Default value is <see langword="false"/>
    /// </summary>
    public bool DenyEntitySetOwnerVersionChange { get; set; } = false;

    /// <inheritdoc />
    public VersioningConvention ToNative()
    {
      var result = new VersioningConvention {
        EntityVersioningPolicy = EntityVersioningPolicy,
        DenyEntitySetOwnerVersionChange = DenyEntitySetOwnerVersionChange
      };
      return result;
    }
  }
}
