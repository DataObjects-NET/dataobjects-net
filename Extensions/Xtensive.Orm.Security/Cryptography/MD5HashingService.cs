// Copyright (C) 2011-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Cryptography;
using System.Text;
using Xtensive.IoC;

namespace Xtensive.Orm.Security.Cryptography
{
  /// <summary>
  /// Implementation of <see cref="IHashingService"/> with MD5 algorithm.
  /// </summary>
  [Service(typeof (IHashingService), Singleton = true, Name = "md5")]
  public class MD5HashingService : GenericHashingService
  {
    /// <inheritdoc/>
#pragma warning disable SYSLIB0021 // Type or member is obsolete
    // direct creation is more efficient than MD5.Create()
    protected override HashAlgorithm GetHashAlgorithm() => new MD5CryptoServiceProvider();
#pragma warning restore SYSLIB0021 // Type or member is obsolete

    /// <summary>
    /// Initializes a new instance of the <see cref="MD5HashingService"/> class.
    /// </summary>
    public MD5HashingService()
    {
    }
  }
}