// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.27

using System;
using System.Diagnostics;

namespace Xtensive.Storage
{
  /// <summary>
  /// Enumerates possible <see cref="DisconnectedState.VersionsProvider"/> selection modes.
  /// </summary>
  public enum VersionsProviderType
  {
    /// <summary>
    /// Default mode.
    /// The same as <see cref="Session"/>.
    /// </summary>
    Default = Session,
    /// <summary>
    /// <see cref="Storage.Session"/>, to which changes are applied, is version provider.
    /// </summary>
    Session = 0x0,
    /// <summary>
    /// <see cref="DisconnectedState"/> instance itself is version provider.
    /// </summary>
    DisconnectedState = 0x1,
    /// <summary>
    /// <see cref="DisconnectedState.VersionsProvider"/> is specified manually.
    /// </summary>
    Other = 0x100,
  }
}