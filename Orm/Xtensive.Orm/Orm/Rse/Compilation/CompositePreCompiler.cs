// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Compilation
{
  public sealed class CompositePreCompiler : IPreCompiler
  {
    public readonly IReadOnlyList<IPreCompiler> Items;

    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      var provider = rootProvider;
      foreach (var item in Items)
        provider = item.Process(provider);
      return provider;
    }


    // Constructors

    public CompositePreCompiler(params IPreCompiler[] preCompilers)
    {
      Items = preCompilers;
    }
  }
}
