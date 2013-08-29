// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.08.21

using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Xtensive.Core;
using Xtensive.Sql.Drivers.Sqlite.Resources;

namespace Xtensive.Sql.Drivers.Sqlite
{
  internal static class ProviderInitializer
  {
    private const string LibraryDirectory = "Native";
    private const string LibraryFileName = "SQLite.Interop.dll";
    private const string LibraryResourceNameFormat = @"Xtensive.Sql.Drivers.Sqlite.NativeModules.{0}_{1}.SQLite.Interop.dll";
    private const string LibraryMutexFormat = @"{0}_Native_{1}";

    private const string FrameworkName = "Net40";

    private static volatile bool IsInitialized;
    private static readonly object SyncRoot = new object();

    public static void Run(string nativeLibraryCacheFolder)
    {
      if (IsInitialized)
        return;
      lock (SyncRoot) {
        if (IsInitialized)
          return;
        IsInitialized = true;
        ExtractAndLoadNativeLibrary(nativeLibraryCacheFolder);
        RegisterCollations();
      }
    }

    private static Stream GetLibraryStream()
    {
      var resourceName = string.Format(LibraryResourceNameFormat, FrameworkName, IntPtr.Size * 8);
      var result = typeof (ProviderInitializer).Assembly.GetManifestResourceStream(resourceName);
      if (result==null)
        throw new InvalidOperationException(string.Format(Strings.ExResourceXIsMissing, resourceName));
      return result;
    }

    private static string GetLibraryHash()
    {
      using (var hashProvider = new SHA1Managed()) {
        hashProvider.Initialize();
        using (var stream = GetLibraryStream())
          hashProvider.ComputeHash(stream);
        var hash = hashProvider.Hash.Take(8).ToArray();
        return new StringBuilder().AppendHexArray(hash).ToString();
      }
    }

    private static string GetLibraryFileName(string nativeLibraryCacheFolder, string moduleHash)
    {
      string basePath;

      if (nativeLibraryCacheFolder!=null) {
        basePath = nativeLibraryCacheFolder;
      }
      else {
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(localApplicationData))
          throw new InvalidOperationException(Strings.ExLocalApplicationDataIsNotAvailableSetDomainConfiguratioNativeLibraryCacheFolder);
        basePath = Path.Combine(Path.Combine(localApplicationData, ThisAssembly.ProductName), LibraryDirectory);
      }

      return Path.Combine(Path.Combine(basePath, moduleHash), LibraryFileName);
    }

    private static void ExtractLibrary(string moduleFileName)
    {
      if (File.Exists(moduleFileName))
        return;

      var moduleDirName = Path.GetDirectoryName(moduleFileName);

      if (!Directory.Exists(moduleDirName))
        Directory.CreateDirectory(moduleDirName);

      try {
        using (var fileStream = new FileStream(moduleFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        using (var resourceStream = GetLibraryStream()) {
          var buffer = new byte[1024 * 32];
          int bytesRead;
          while ((bytesRead = resourceStream.Read(buffer, 0, buffer.Length)) > 0)
            fileStream.Write(buffer, 0, bytesRead);
        }
      }
      catch {
        File.Delete(moduleFileName);
        throw;
      }
    }

    private static void ExtractAndLoadNativeLibrary(string nativeLibraryCacheFolder)
    {
      var hash = GetLibraryHash();
      var moduleFileName = GetLibraryFileName(nativeLibraryCacheFolder, hash);
      var mutexName = string.Format(LibraryMutexFormat, ThisAssembly.ProductName, hash);

      using (var mutex = new Mutex(false, mutexName)) {
        mutex.WaitOne();
        try {
          ExtractLibrary(moduleFileName);
        }
        finally {
          mutex.ReleaseMutex();
        }
      }

      LoadNativeLibrary(moduleFileName);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    private static void LoadNativeLibrary(string moduleFileName)
    {
      const uint loadWithAlteredSearchPath = 0x00000008;
      var result = LoadLibraryEx(moduleFileName, IntPtr.Zero, loadWithAlteredSearchPath);
      if (result==IntPtr.Zero)
        throw new InvalidOperationException(string.Format(Strings.ExFailedToLoadNativeModuleX, moduleFileName));
    }

    private static void RegisterCollations()
    {
      // Register our helper collations

      var functions = typeof (DriverFactory).Assembly.GetTypes()
        .Where(t => typeof (SQLiteFunction).IsAssignableFrom(t));

      foreach (var f in functions)
        SQLiteFunction.RegisterFunction(f);
    }
  }
}