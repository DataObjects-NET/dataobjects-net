// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.08

using System;
using System.Globalization;
using Xtensive.Messaging.Providers;
using Xtensive.Messaging.Resources;
using Xtensive.PluginManager;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Gets messaging poviders from <see cref="PluginManager"/> 
  /// </summary>
  internal static class MessagingProviderManager
  {
    private static readonly PluginManager<MessagingProviderAttribute> pluginManager =
      new PluginManager<MessagingProviderAttribute>(typeof (IMessagingProvider), Environment.CurrentDirectory);

    /// <summary>
    /// Gets <see cref="IMessagingProvider"/> from <see cref="PluginManager"/>. Method is thread-safe.
    /// </summary>
    /// <param name="info"><see cref="EndPointInfo"/> with protocol information</param>
    /// <returns>Instance of <see cref="IMessagingProvider"/> for <paramref name="info"/></returns>
    public static IMessagingProvider GetMessagingProvider(EndPointInfo info)
    {
      lock (pluginManager) {
        Type protocolType = pluginManager[new MessagingProviderAttribute(info.Protocol)];
        if (protocolType==null)
          throw new MessagingException(
            string.Format(CultureInfo.CurrentCulture, Strings.ExPluginForProtocolNotFound, info.Protocol, Environment.CurrentDirectory));
        var provider = (IMessagingProvider)Activator.CreateInstance(protocolType);
        return provider;
      }
    }
  }
}