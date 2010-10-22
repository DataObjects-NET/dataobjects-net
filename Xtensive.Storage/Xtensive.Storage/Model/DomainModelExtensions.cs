// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.24

using Xtensive.Storage.Model.Stored;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Extension methods related to <see cref="DomainModel"/>.
  /// </summary>
  public static class DomainModelExtensions
  {
    /// <summary>
    /// Converts speicified <see cref="DomainModel"/> to corresponding <see cref="StoredDomainModel"/>.
    /// </summary>
    /// <param name="model">The model to convert.</param>
    /// <returns>A result of conversion.</returns>
    public static StoredDomainModel ToStoredModel(this DomainModel model)
    {
      return new ConverterToStoredModel().Convert(model);
    }
  }
}