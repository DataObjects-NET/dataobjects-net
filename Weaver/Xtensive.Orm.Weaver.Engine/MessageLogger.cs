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
    private static readonly Dictionary<MessageCode, Tuple<string, string>> Messages;

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
      Tuple<string, string> entry;
      if (!Messages.TryGetValue(code, out entry))
        throw new ArgumentOutOfRangeException("code");

      var messageText = string.IsNullOrEmpty(extraInformation)
        ? entry.Item2
        : string.Format("{0}: {1}", entry.Item2, extraInformation);

      var message = new ProcessorMessage {
        ProjectId = projectId,
        MessageCode = entry.Item1,
        MessageText = messageText,
        Location = location,
      };

      writer.Write(message);
    }

    private static void RegisterMessage(MessageCode code, string description)
    {
      var codeText = string.Format(CultureInfo.InvariantCulture, "XW{0:0000}", Messages.Count);
      Messages.Add(code, Tuple.Create(codeText, description));
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
      Messages = new Dictionary<MessageCode, Tuple<string, string>>();
      RegisterMessage(MessageCode.ErrorInputFileNotFound, "input file is not found");
    }
  }
}