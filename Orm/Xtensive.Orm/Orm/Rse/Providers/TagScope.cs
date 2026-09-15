// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  public readonly struct TagScope : IDisposable
  {
    private readonly List<string> tags;

    public void Dispose() =>
      tags.RemoveAt(tags.Count - 1);

    internal TagScope(List<string> tags, string tag)
    {
      ArgumentNullException.ThrowIfNull(tags);
      ArgumentNullException.ThrowIfNull(tag);
      (this.tags = tags).Add(tag);
    }
  }
}
