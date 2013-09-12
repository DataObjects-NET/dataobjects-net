// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Text;

namespace Xtensive.Orm.Weaver
{
  [Serializable]
  public sealed class ProcessorMessage
  {
    private string projectId;
    private MessageLocation location;
    private MessageType type;
    private string messageText;
    private string messageCode;

    public string ProjectId
    {
      get { return projectId; }
      set { projectId = value; }
    }

    public MessageLocation Location
    {
      get { return location; }
      set { location = value; }
    }

    public MessageType Type
    {
      get { return type; }
      set { type = value; }
    }

    public string MessageCode
    {
      get { return messageCode; }
      set { messageCode = value; }
    }

    public string MessageText
    {
      get { return messageText; }
      set { messageText = value; }
    }

    public override string ToString()
    {
      var resultBuilder = new StringBuilder();
      if (location!=null)
        resultBuilder.Append(location);
      resultBuilder.AppendFormat("{0} {1}: {2}", GetMessageTypeName(type), messageCode, messageText);
      return resultBuilder.ToString();
    }

    private static string GetMessageTypeName(MessageType type)
    {
      switch (type) {
      case MessageType.Warning:
        return "warning";
      case MessageType.Error:
        return "error";
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }
  }
}