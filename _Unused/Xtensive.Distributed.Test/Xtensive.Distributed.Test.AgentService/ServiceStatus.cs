// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.23

namespace Xtensive.Distributed.Test.AgentService
{
  public struct ServiceStatus
  {
    public int serviceType;
    public int currentState;
    public int controlsAccepted;
    public int win32ExitCode;
    public int serviceSpecificExitCode;
    public int checkPoint;
    public int waitHint;
  }
}