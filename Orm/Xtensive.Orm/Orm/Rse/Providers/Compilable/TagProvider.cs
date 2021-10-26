// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Edgar Isajanyan
// Created:    2021.09.13

using System;
using Xtensive.Core;


namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Injects SQL comment for query identification
  /// </summary>
  [Serializable]
  public sealed class TagProvider : UnaryProvider
  {
    public readonly string Tag;

    // Constructors

    public TagProvider(CompilableProvider source, string tag)
      : base(ProviderType.Tag, source)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmptyOrWhiteSpace(tag, "tag");
      Tag = tag;
      Initialize();
    }
  }
}