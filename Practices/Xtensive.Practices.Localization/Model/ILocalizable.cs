// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.22

namespace Xtensive.Practices.Localization.Model
{
  /// <summary>
  /// Defines localization contract.
  /// </summary>
  /// <typeparam name="T">Entity</typeparam>
  public interface ILocalizable<T> where T : Localization
  {
    ///<summary>
    /// Set of localizations.
    ///</summary>
    LocalizationSet<T> Localizations { get; }
  }
}