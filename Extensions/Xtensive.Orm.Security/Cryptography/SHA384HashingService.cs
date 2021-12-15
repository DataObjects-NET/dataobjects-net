// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Cryptography;
using Xtensive.IoC;

namespace Xtensive.Orm.Security.Cryptography
{
  /// <summary>
  /// Implementation of <see cref="IHashingService"/> with SHA384 algorithm.
  /// </summary>
  [Service(typeof (IHashingService), Singleton = true, Name = "sha384")]
  public class SHA384HashingService : GenericHashingService
  {
    /// <inheritdoc/>
    protected override HashAlgorithm GetHashAlgorithm()
    {
      return SHA384.Create();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SHA384HashingService"/> class.
    /// </summary>
    public SHA384HashingService()
    {
    }
  }
}