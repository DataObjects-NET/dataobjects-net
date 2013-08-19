// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Globalization;

namespace Xtensive.Orm.Weaver
{
  [Serializable]
  public sealed class ProcessorMessage
  {
    private string projectId;
    private string file;
    private int line;
    private int column;
    private MessageType type;
    private string messageText;
    private string messageCode;

    public string ProjectId
    {
      get { return projectId; }
      set { projectId = value; }
    }

    public string File
    {
      get { return file; }
      set { file = value; }
    }

    public int Line
    {
      get { return line; }
      set { line = value; }
    }

    public int Column
    {
      get { return column; }
      set { column = value; }
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
      return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): {3} {4}: {5}",
        file, line, column, type.ToString().ToLowerInvariant(), messageCode, messageText);
    }
  }
}