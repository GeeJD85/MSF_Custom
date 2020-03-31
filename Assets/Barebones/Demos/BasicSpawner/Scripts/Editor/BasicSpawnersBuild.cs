using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Barebones.MasterServer.Examples.BasicSpawner
{
    public class BasicSpawnersBuild
    {
        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner/All")]
        private static void BuildBoth()
        {
            BuildMasterForWindows();
            BuildSpawnerForWindows();
            BuildRoomForWindows();
            BuildClientForWindows();
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner/Master Server")]
        private static void BuildMasterForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawner", "MasterServer");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Barebones/Demos/BasicSpawner/Scenes/MasterServer/MasterServer.unity" },
                locationPathName = Path.Combine(buildFolder, "MasterServer.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("@echo off\n");
                arguments.Append("start \"Basic Spawner - Master Server\" ");
                arguments.Append("MasterServer.exe ");
                arguments.Append($"{Msf.Args.Names.StartMaster} ");
                arguments.Append($"{Msf.Args.Names.MasterIp} {Msf.Args.MasterIp} ");

                File.WriteAllText(Path.Combine(buildFolder, "Start Master Server.bat"), arguments.ToString());

                Debug.Log("Master Server build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Master Server build failed");
            }
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner/Spawner")]
        private static void BuildSpawnerForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawner", "Spawner");
            string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "BasicSpawner", "Room", "Room.exe");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Barebones/Demos/BasicSpawner/Scenes/Spawner/Spawner.unity" },
                locationPathName = Path.Combine(buildFolder, "Spawner.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("@echo off\n");
                arguments.Append("start \"Basic Spawner - Spawner\" ");
                arguments.Append("Spawner.exe ");
                arguments.Append($"{Msf.Args.Names.StartSpawner} ");
                arguments.Append($"{Msf.Args.Names.StartClientConnection} ");
                arguments.Append($"{Msf.Args.Names.MasterIp} {Msf.Args.MasterIp} ");
                arguments.Append($"{Msf.Args.Names.MasterPort} {Msf.Args.MasterPort} ");
                arguments.Append($"{Msf.Args.Names.DontSpawnInBatchmode} ");
                arguments.Append($"{Msf.Args.Names.RoomExecutablePath} {roomExePath} ");

                File.WriteAllText(Path.Combine(buildFolder, "Start Spawner.bat"), arguments.ToString());

                Debug.Log("Spawner build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Spawner build failed");
            }
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner/Room")]
        private static void BuildRoomForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawner", "Room");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/Barebones/Demos/BasicSpawner/Scenes/Room/Room.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Room.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Room build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Room build failed");
            }
        }

        [MenuItem("Tools/MSF/Build/Demos/Basic Spawner/Client")]
        private static void BuildClientForWindows()
        {
            string buildFolder = Path.Combine("Builds", "BasicSpawner", "Client");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/Barebones/Demos/BasicSpawner/Scenes/Client/Client.unity",
                    "Assets/Barebones/Demos/BasicSpawner/Scenes/Room/Room.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Client.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Client build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Client build failed");
            }
        }
    }
}