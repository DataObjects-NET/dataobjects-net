// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.25

using System.IO;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// <see cref="Stream"/> serializer contract.
  /// </summary>
  public interface ISerializer : ISerializer<Stream>
  {
  }
}