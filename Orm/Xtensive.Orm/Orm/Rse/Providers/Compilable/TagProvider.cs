// Copyright (C) 2003-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

    public TagProvider(CompilableProvider source, string tag) :
      base(ProviderType.Tag, source)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmptyOrWhiteSpace(tag, "tag");
      Tag = tag;
      Initialize();
    }
  }
}