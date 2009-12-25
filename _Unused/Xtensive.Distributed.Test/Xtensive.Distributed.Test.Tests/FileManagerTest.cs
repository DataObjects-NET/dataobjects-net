// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.11

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Xtensive.Distributed.Test.Tests.RemoteAssembly;

namespace Xtensive.Distributed.Test.Tests
{
  [TestFixture]
  public class FileManagerTest
  {
    public const string ServerUrl = "tcp://127.0.0.1:37091/Server";
    private readonly string sourceFolder = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "Source");
    private readonly string targetFolder = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "Target");
    private readonly string downloadFolder = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "Download");

    [Test]
    public void UploadTest()
    {
      if (Directory.Exists(sourceFolder))
        Directory.Delete(sourceFolder, true);
      if (Directory.Exists(targetFolder))
        Directory.Delete(targetFolder, true);
      Directory.CreateDirectory(sourceFolder);
      Directory.CreateDirectory(targetFolder);
      byte[] buffer = new byte[1000];
      Random random = new Random();
      for (int i = 0; i < 10; i++) {
        using (
          FileStream fs = new FileStream(Path.Combine(sourceFolder, Guid.NewGuid() + ".bin"), FileMode.Create)) {
          for (int j = 0; j < 100; j++) {
            random.NextBytes(buffer);
            fs.Write(buffer, 0, buffer.Length);
          }
          fs.Flush();
          fs.Close();
        }
      }
      using (new Server(ServerUrl)) {
        using (new Agent(ServerUrl, targetFolder)) {
          Thread.Sleep(TimeSpan.FromSeconds(3));
          Client client = new Client(ServerUrl);
          AgentInfo[] availableAgents = client.Agents;
          Assert.Greater(availableAgents.Length, 0);
          Task<ConsoleTest> task = client.CreateTask<ConsoleTest>();
          task.FileManager.Upload(sourceFolder, "");
          string[] directories = Directory.GetDirectories(targetFolder);
          Assert.AreEqual(1, directories.Length);
          Assert.AreEqual(Directory.GetFileSystemEntries(sourceFolder).Length, Directory.GetFileSystemEntries(directories[0]).Length);
        }
      }
      Directory.Delete(sourceFolder, true);
      Directory.Delete(targetFolder, true);
    }

    [Test]
    public void RecursiveUpload()
    {
      if (Directory.Exists(sourceFolder))
        Directory.Delete(sourceFolder, true);
      if (Directory.Exists(targetFolder))
        Directory.Delete(targetFolder, true);
      Directory.CreateDirectory(sourceFolder);
      Directory.CreateDirectory(targetFolder);
      byte[] buffer = new byte[1000];
      Random random = new Random();
      for (int currentFolder = 0; currentFolder < 10; currentFolder++) {
        string folderName = Path.Combine(sourceFolder, Guid.NewGuid() + ".folder");
        Directory.CreateDirectory(folderName);
        for (int i = 0; i < 10; i++) {
          using (
            FileStream fs = new FileStream(Path.Combine(folderName, Guid.NewGuid() + ".bin"), FileMode.Create)) {
            for (int j = 0; j < 100; j++) {
              random.NextBytes(buffer);
              fs.Write(buffer, 0, buffer.Length);
            }
            fs.Flush();
            fs.Close();
          }
        }
      }
      using (new Server(ServerUrl)) {
        using (new Agent(ServerUrl, targetFolder)) {
          Thread.Sleep(TimeSpan.FromSeconds(3));
          Client client = new Client(ServerUrl);
          AgentInfo[] availableAgents = client.Agents;
          Assert.Greater(availableAgents.Length, 0);
          Task<ConsoleTest> task = client.CreateTask<ConsoleTest>();
          task.FileManager.Upload(sourceFolder, "");
        }
      }
      Directory.Delete(sourceFolder, true);
      Directory.Delete(targetFolder, true);
    }

    [Test]
    public void Download()
    {
      if (Directory.Exists(sourceFolder))
        Directory.Delete(sourceFolder, true);
      if (Directory.Exists(targetFolder))
        Directory.Delete(targetFolder, true);
      if (Directory.Exists(downloadFolder))
        Directory.Delete(downloadFolder, true);
      Directory.CreateDirectory(sourceFolder);
      Directory.CreateDirectory(targetFolder);
      Directory.CreateDirectory(downloadFolder);
      byte[] buffer = new byte[1000];
      Random random = new Random();
      for (int currentFolder = 0; currentFolder < 10; currentFolder++) {
        string folderName = Path.Combine(sourceFolder, Guid.NewGuid() + ".folder");
        Directory.CreateDirectory(folderName);
        for (int i = 0; i < 10; i++) {
          using (
            FileStream fs = new FileStream(Path.Combine(folderName, Guid.NewGuid() + ".bin"), FileMode.Create)) {
            for (int j = 0; j < 100; j++) {
              random.NextBytes(buffer);
              fs.Write(buffer, 0, buffer.Length);
            }
            fs.Flush();
            fs.Close();
          }
        }
      }
      using (new Server(ServerUrl)) {
        using (new Agent(ServerUrl, targetFolder)) {
          Thread.Sleep(TimeSpan.FromSeconds(3));
          Client client = new Client(ServerUrl);
          AgentInfo[] availableAgents = client.Agents;
          Assert.Greater(availableAgents.Length, 0);
          Task<ConsoleTest> task = client.CreateTask<ConsoleTest>();
          task.FileManager.Upload(sourceFolder, "");
          task.FileManager.Download(downloadFolder, "");
        }
      }
      Directory.Delete(sourceFolder, true);
      Directory.Delete(targetFolder, true);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void FileAlreadyExistsWhileUpload()
    {
      if (Directory.Exists(sourceFolder))
        Directory.Delete(sourceFolder, true);
      if (Directory.Exists(targetFolder))
        Directory.Delete(targetFolder, true);
      Directory.CreateDirectory(sourceFolder);
      Directory.CreateDirectory(targetFolder);
      byte[] buffer = new byte[1000];
      Random random = new Random();
      for (int i = 0; i < 10; i++)
      {
        using (
          FileStream fs = new FileStream(Path.Combine(sourceFolder, Guid.NewGuid() + ".bin"), FileMode.Create))
        {
          for (int j = 0; j < 100; j++)
          {
            random.NextBytes(buffer);
            fs.Write(buffer, 0, buffer.Length);
          }
          fs.Flush();
          fs.Close();
        }
      }
      using (new Server(ServerUrl))
      {
        using (new Agent(ServerUrl, targetFolder))
        {
          Thread.Sleep(TimeSpan.FromSeconds(3));
          Client client = new Client(ServerUrl);
          AgentInfo[] availableAgents = client.Agents;
          Assert.Greater(availableAgents.Length, 0);
          Task<ConsoleTest> task = client.CreateTask<ConsoleTest>();
          task.FileManager.Upload(sourceFolder, "");
          task.FileManager.Upload(sourceFolder, "");
        }
      }
      Directory.Delete(sourceFolder, true);
      Directory.Delete(targetFolder, true);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void FolderAlreadyExistsWhileUpload()
    {
      if (Directory.Exists(sourceFolder))
        Directory.Delete(sourceFolder, true);
      if (Directory.Exists(targetFolder))
        Directory.Delete(targetFolder, true);
      Directory.CreateDirectory(sourceFolder);
      Directory.CreateDirectory(targetFolder);
      for (int i = 0; i < 10; i++)
      {
        Directory.CreateDirectory(Path.Combine(sourceFolder, Guid.NewGuid() + ".folder"));
      }
      using (new Server(ServerUrl))
      {
        using (new Agent(ServerUrl, targetFolder))
        {
          Thread.Sleep(TimeSpan.FromSeconds(3));
          Client client = new Client(ServerUrl);
          AgentInfo[] availableAgents = client.Agents;
          Assert.Greater(availableAgents.Length, 0);
          Task<ConsoleTest> task = client.CreateTask<ConsoleTest>();
          task.FileManager.Upload(sourceFolder, "");
          task.FileManager.Upload(sourceFolder, "");
        }
      }
      Directory.Delete(sourceFolder, true);
      Directory.Delete(targetFolder, true);
    }

  }
}