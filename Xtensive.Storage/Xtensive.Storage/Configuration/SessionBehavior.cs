// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.18

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// 
  /// </summary>
  public enum SessionBehavior
  {
    /// <summary>
    /// Session options are optimized for server-side usage. <see cref="SessionOptions"/> will be set to 
    /// <see cref="SessionOptions.AutoPersist"/> | <see cref="SessionOptions.Transactional"/> | <see cref="SessionOptions.AutoShortenTransactions"/>.
    /// </summary>
    Server = 0,
    /// <summary>
    /// Session options are optimized for client-side usage. <see cref="SessionOptions"/> will be set to 
    /// <see cref="SessionOptions.AutoShortenTransactions"/>.
    /// </summary>
    Client
  }
}