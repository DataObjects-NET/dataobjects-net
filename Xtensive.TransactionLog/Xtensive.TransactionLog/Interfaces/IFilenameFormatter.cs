// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.31

namespace Xtensive.TransactionLog
{
  /// <summary>
  /// Describes formatter for keys that can convert key to file system compatible name and restore key from file system name.
  /// </summary>
  /// <typeparam name="TKey">Type of key.</typeparam>
  public interface IFileNameFormatter<TKey>
  {
    /// <summary>
    /// Restores key from string.
    /// </summary>
    /// <param name="value">String representation of key.</param>
    /// <returns><typeparamref name="TKey"/> object.</returns>
    TKey RestoreFromString(string value);

    /// <summary>
    /// Stores key to string.
    /// </summary>
    /// <param name="key">Key to represent as file system compatible name.</param>
    /// <returns>File system compatible name of key.</returns>
    string SaveToString(TKey key);
  }
}