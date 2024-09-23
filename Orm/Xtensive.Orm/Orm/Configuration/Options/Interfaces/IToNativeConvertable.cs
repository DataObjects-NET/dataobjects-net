// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm.Configuration.Options
{
  internal interface IToNativeConvertable<TNative>
  {
    /// <summary>
    /// Converts element of options pattern to native configuration it corresponds to (<typeparamref name="TNative"/>)
    /// </summary>
    /// <returns><typeparamref name="TNative"/> instance.</returns>
    TNative ToNative();
  }
}
