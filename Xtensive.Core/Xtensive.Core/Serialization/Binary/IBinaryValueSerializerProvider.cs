// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Implementation of <see cref="IValueSerializerProvider{TStream}"/> for binary (de)serializing.
  /// </summary>
  public interface IBinaryValueSerializerProvider : IValueSerializerProvider<Stream> {}
}