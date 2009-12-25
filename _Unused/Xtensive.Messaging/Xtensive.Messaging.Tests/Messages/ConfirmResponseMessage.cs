// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.11

using System;
using Xtensive.Messaging;

namespace Xtensive.Messaging.Tests
{
  /// <summary>
  /// Represents response message to <see cref="Sender.Ask(object)"/> method call than this call need only confirmation about successful
  /// execution on remote host. No additional data provided along with message. 
  /// </summary>
  [Serializable]
  public class ConfirmResponseMessage: ResponseMessage 
  {
  }
}