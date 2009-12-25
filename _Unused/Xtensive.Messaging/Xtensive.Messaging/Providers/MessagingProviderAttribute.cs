// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;

namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Indicates that class is messaging provider and specifies protocol for associate with the class.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  [Serializable]
  public sealed class MessagingProviderAttribute: Attribute
  {
    private readonly string protocol;

    /// <summary>
    /// Gets or sets the protocol name associated with
    /// <see cref="IMessagingProvider"/>.
    /// </summary>
    public string Protocol
    {
      get { return protocol; }
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="protocol">The name of the protocol the provider supports.</param>
    public MessagingProviderAttribute(string protocol)
    {
      this.protocol = protocol;
    }
  }
}