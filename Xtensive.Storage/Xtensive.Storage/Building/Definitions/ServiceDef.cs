// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.07

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Definitions
{
  [Serializable]
  public class ServiceDef : Node
  {
    private readonly Type underlyingType;

    /// <summary>
    /// Gets or sets the underlying system type.
    /// </summary>
    public Type UnderlyingType
    {
      get { return underlyingType; }
    }
    //    /// <summary>
    //    /// Performs additional custom processes before setting new name to this instance.
    //    /// </summary>
    //    /// <param name="oldValue">The name to replace with <paramref name="newValue"/>.</param>
    //    /// <param name="newValue">The new name of this instance.</param>
    //    /// <exception cref="InvalidOperationException">Service with the specified name already exists 
    //    /// in <see cref="Storage.Storage.Services"/>.</exception>
    //    protected override void OnNameChanging(string oldValue, string newValue)
    //    {
    //      base.OnNameChanging(oldValue, newValue);
    //      if (storage.Services[newValue] != null)
    //        throw new InvalidOperationException(
    //          string.Format(Xtensive.Storage.Resources.Strings.ExServiceWithNameAlreadyExistsInStorageInfoServicesCollection, newValue));
    //    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDef"/> class.
    /// </summary>
    /// <param name="type">The underlying type.</param>
    internal ServiceDef(Type type)
    {
      underlyingType = type;
    }
  }
}