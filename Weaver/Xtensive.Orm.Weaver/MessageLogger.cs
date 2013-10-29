// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Xtensive.Orm.Weaver
{
  internal sealed class MessageLogger
  {
    private static readonly Dictionary<MessageCode, string> Messages;

    private readonly IMessageWriter writer;
    private readonly string projectId;

    public void Write(MessageCode code)
    {
      Write(code, null, null);
    }

    public void Write(MessageCode code, MessageLocation location)
    {
      Write(code, null, location);
    }

    public void Write(MessageCode code, string extraInformation)
    {
      Write(code, extraInformation, null);
    }

    public void Write(MessageCode code, string extraInformation, MessageLocation location)
    {
      string description;
      if (!Messages.TryGetValue(code, out description))
        throw new ArgumentOutOfRangeException("code");

      var messageCode = string.Format(CultureInfo.InvariantCulture, "XW{0:0000}", (int) code);
      var messageText = string.IsNullOrEmpty(extraInformation)
        ? description
        : string.Format("{0}: {1}", description, extraInformation);

      var message = new ProcessorMessage {
        ProjectId = projectId,
        MessageCode = messageCode,
        MessageText = messageText,
        Location = location,
        Type = GetMessageType(code),
      };

      writer.Write(message);
    }

    private static MessageType GetMessageType(MessageCode code)
    {
      var value = (int) code;
      return value >= 1000 ? MessageType.Warning : MessageType.Error;
    }

    private static void RegisterMessage(MessageCode code, string description)
    {
      Messages.Add(code, description);
    }

    public MessageLogger(string projectId, IMessageWriter writer)
    {
      if (writer==null)
        throw new ArgumentNullException("writer");
      this.writer = writer;
      this.projectId = projectId;
    }

    static MessageLogger()
    {
      Messages = new Dictionary<MessageCode, string>();

      RegisterMessage(MessageCode.ErrorInternal, "Internal error");
      RegisterMessage(MessageCode.ErrorInputFileIsNotFound, "Input file is not found");
      RegisterMessage(MessageCode.ErrorStrongNameKeyIsNotFound, "Strong name key file is not found");
      RegisterMessage(MessageCode.ErrorUnableToLocateOrmAssembly, "Unable to locate Xtensive.Orm assembly");
      RegisterMessage(MessageCode.ErrorUnableToFindReferencedAssembly, "Unable to find referenced assembly");
      RegisterMessage(MessageCode.ErrorUnableToRemoveBackingField, "Unable to remove backing field");
      RegisterMessage(MessageCode.ErrorEntityLimitIsExceeded,
        "Number of persistent types in assembly exceeds the maximal available types per assembly for Community Edition. " +
        "Consider upgrading to any commercial edition of DataObjects.Net.");
      RegisterMessage(MessageCode.ErrorLicenseIsInvalid, "DataObjects.Net license is invalid.");
      RegisterMessage(MessageCode.ErrorSubscriptionExpired,
        "Your subscription expired and is not valid for this release of DataObjects.Net.");

      RegisterMessage(MessageCode.WarningDebugSymbolsFileIsNotFound, "Debug symbols file is not found");
      RegisterMessage(MessageCode.WarningReferencedAssemblyFileIsNotFound, "Referenced assembly file is not found");
    }
  }
}