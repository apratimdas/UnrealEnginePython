// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;
using System.Collections.Generic;

public class UnrealEnginePython : ModuleRules
{

    // leave this string as empty for triggering auto-discovery of python installations...
    private string pythonHome = "";
    // otherwise specify the path of your python installation
    //private string pythonHome = "C:/Program Files/Python36";
    // this is an example for Homebrew on Mac 
    //private string pythonHome = "/usr/local/Cellar/python3/3.6.0/Frameworks/Python.framework/Versions/3.6/";
    // on Linux an include;libs syntax is expected:
    //private string pythonHome = "/usr/local/include/python3.6;/usr/local/lib/libpython3.6.so"


    private string[] windowsKnownPaths =
    {
        "C:/Program Files/Python36",
        "C:/Program Files/Python35",
        "C:/Python27",
        "C:/IntelPython35"
    };

    private string[] macKnownPaths =
    {
        "/Library/Frameworks/Python.framework/Versions/3.6",
        "/Library/Frameworks/Python.framework/Versions/3.5",
        "/Library/Frameworks/Python.framework/Versions/2.7",
    };

    private string[] linuxKnownIncludesPaths =
    {
        "/usr/local/include/python3.6",
        "/usr/local/include/python3.6m",
        "/usr/local/include/python3.5",
        "/usr/local/include/python3.5m",
        "/usr/local/include/python2.7",
        "/usr/include/python3.6",
        "/usr/include/python3.6m",
        "/usr/include/python3.5",
        "/usr/include/python3.5m",
        "/usr/include/python2.7",
    };

    private string[] linuxKnownLibsPaths =
    {
	    "/usr/local/lib/libpython3.6.so",
	    "/usr/local/lib/libpython3.6m.so",
	    "/usr/local/lib/x86_64-linux-gnu/libpython3.6.so",
	    "/usr/local/lib/x86_64-linux-gnu/libpython3.6m.so",
	    "/usr/local/lib/libpython3.5.so",
	    "/usr/local/lib/libpython3.5m.so",
	    "/usr/local/lib/x86_64-linux-gnu/libpython3.5.so",
	    "/usr/local/lib/x86_64-linux-gnu/libpython3.5m.so",
	    "/usr/local/lib/libpython2.7.so",
	    "/usr/local/lib/x86_64-linux-gnu/libpython2.7.so",
	    "/usr/lib/libpython3.6.so",
	    "/usr/lib/libpython3.6m.so",
	    "/usr/lib/x86_64-linux-gnu/libpython3.6.so",
	    "/usr/lib/x86_64-linux-gnu/libpython3.6m.so",
	    "/usr/lib/libpython3.5.so",
	    "/usr/lib/libpython3.5m.so",
	    "/usr/lib/x86_64-linux-gnu/libpython3.5.so",
	    "/usr/lib/x86_64-linux-gnu/libpython3.5m.so",
	    "/usr/lib/libpython2.7.so",
	    "/usr/lib/x86_64-linux-gnu/libpython2.7.so",
    };

    public UnrealEnginePython(TargetInfo Target)
    {

        PublicIncludePaths.AddRange(
            new string[] {
                "UnrealEnginePython/Public",
				// ... add public include paths required here ...
            }
            );


        PrivateIncludePaths.AddRange(
            new string[] {
                "UnrealEnginePython/Private",
				// ... add other private include paths required here ...
			}
            );


        PublicDependencyModuleNames.AddRange(
            new string[]
            {
                "Core",
                "Sockets",
                "Networking"
				// ... add other public dependencies that you statically link with here ...
			}
            );


        PrivateDependencyModuleNames.AddRange(
            new string[]
            {
                "CoreUObject",
                "Engine",
                "InputCore",
                "Slate",
                "SlateCore",
                "MovieScene",
                "LevelSequence",
                "HTTP",
                "UMG",
                "AppFramework"
				// ... add private dependencies that you statically link with here ...
			}
            );


        DynamicallyLoadedModuleNames.AddRange(
            new string[]
            {
				// ... add any modules that your module loads dynamically here ...
			}
            );


        if (UEBuildConfiguration.bBuildEditor)
        {
            PrivateDependencyModuleNames.AddRange(new string[]{
                "UnrealEd",
                "LevelEditor",
                "BlueprintGraph",
                "Projects",
                "Sequencer",
                "SequencerWidgets",
                "AssetTools",
                "LevelSequenceEditor",
                "MovieSceneTools",
                "MovieSceneTracks",
                "CinematicCamera",
                "EditorStyle",
                "GraphEditor",
                "UMGEditor",
                "AIGraph",
                "RawMesh",
                "DesktopWidgets",
            });
        }

        if ((Target.Platform == UnrealTargetPlatform.Win64) || (Target.Platform == UnrealTargetPlatform.Win32))
        {
            if (pythonHome == "")
            {
                pythonHome = DiscoverPythonPath(windowsKnownPaths);
                if (pythonHome == "")
                {
                    throw new System.Exception("Unable to find Python installation");
                }
            }
            System.Console.WriteLine("Using Python at: " + pythonHome);
            PublicIncludePaths.Add(pythonHome);
            string libPath = GetWindowsPythonLibFile(pythonHome);
            PublicLibraryPaths.Add(Path.GetDirectoryName(libPath));
            PublicAdditionalLibraries.Add(libPath);
        }
        else if (Target.Platform == UnrealTargetPlatform.Mac)
        {
            if (pythonHome == "")
            {
                pythonHome = DiscoverPythonPath(macKnownPaths);
                if (pythonHome == "")
                {
                    throw new System.Exception("Unable to find Python installation");
                }
            }
            System.Console.WriteLine("Using Python at: " + pythonHome);
            PublicIncludePaths.Add(pythonHome);
            string libPath = GetMacPythonLibFile(pythonHome);
            PublicLibraryPaths.Add(Path.GetDirectoryName(libPath));
            PublicDelayLoadDLLs.Add(libPath);
            Definitions.Add(string.Format("UNREAL_ENGINE_PYTHON_ON_MAC"));
        }
        else if (Target.Platform == UnrealTargetPlatform.Linux)
        {
            if (pythonHome == "")
            {
		string includesPath = DiscoverLinuxPythonIncludesPath();
		if (includesPath == null)
		{
			throw new System.Exception("Unable to find Python includes, please add a search path to linuxKnownIncludesPaths");
		}
		string libsPath = DiscoverLinuxPythonLibsPath();
		if (libsPath == null)
		{
			throw new System.Exception("Unable to find Python libs, please add a search path to linuxKnownLibsPaths");
		}
                PublicIncludePaths.Add(includesPath);
                PublicAdditionalLibraries.Add(libsPath);
            }
            else
            {
		string []items = pythonHome.Split(';');
               	PublicIncludePaths.Add(items[0]);
               	PublicAdditionalLibraries.Add(items[1]);
            }
            Definitions.Add(string.Format("UNREAL_ENGINE_PYTHON_ON_LINUX"));
        }

    }

    private string DiscoverPythonPath(string[] knownPaths)
    {
        // insert the PYTHONHOME content as the first known path
        List<string> paths = new List<string>(knownPaths);
        string environmentPath = System.Environment.GetEnvironmentVariable("PYTHONHOME");
        if (!string.IsNullOrEmpty(environmentPath))
            paths.Insert(0, environmentPath);

        foreach (string path in paths)
        {
            string headerFile = Path.Combine(path, "include", "Python.h");
            if (File.Exists(headerFile))
            {
                return path;
            }
            // this is mainly useful for OSX
            headerFile = Path.Combine(path, "Headers", "Python.h");
            if (File.Exists(headerFile))
            {
                return path;
            }
        }
        return "";
    }

    private string DiscoverLinuxPythonIncludesPath()
    {
        foreach (string path in linuxKnownIncludesPaths)
        {
            string headerFile = Path.Combine(path, "Python.h");
            if (File.Exists(headerFile))
            {
                return path;
            }
        }
        return null;
    }

    private string DiscoverLinuxPythonLibsPath()
    {
        foreach (string path in linuxKnownLibsPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    private string GetMacPythonLibFile(string basePath)
    {
        // first try with python3
        for (int i = 9; i >= 0; i--)
        {
            string fileName = string.Format("libpython3.{0}.dylib", i);
            string fullPath = Path.Combine(basePath, "lib", fileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
            fileName = string.Format("libpython3.{0}m.dylib", i);
            fullPath = Path.Combine(basePath, "lib", fileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        // then python2
        for (int i = 9; i >= 0; i--)
        {
            string fileName = string.Format("libpython2.{0}.dylib", i);
            string fullPath = Path.Combine(basePath, "lib", fileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
            fileName = string.Format("libpython2.{0}m.dylib", i);
            fullPath = Path.Combine(basePath, "lib", fileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new System.Exception("Invalid Python installation, missing .dylib files");
    }

    private string GetWindowsPythonLibFile(string basePath)
    {
        // just for usability, report if the pythonHome is not in the system path
        string[] allPaths = System.Environment.GetEnvironmentVariable("PATH").Split(';');
        // this will transform the slashes in backslashes...
        string checkedPath = Path.GetFullPath(basePath);
        if (checkedPath.EndsWith("\\"))
        {
            checkedPath = checkedPath.Remove(checkedPath.Length - 1);
        }
        bool found = false;
        foreach (string item in allPaths)
        {
            if (item == checkedPath || item == checkedPath + "\\")
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            System.Console.WriteLine("[WARNING] Your Python installation is not in the system PATH environment variable, very probably the plugin will fail to load");
        }
        // first try with python3
        for (int i = 9; i >= 0; i--)
        {
            string fileName = string.Format("python3{0}.lib", i);
            string fullPath = Path.Combine(basePath, "libs", fileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        // then python2
        for (int i = 9; i >= 0; i--)
        {
            string fileName = string.Format("python2{0}.lib", i);
            string fullPath = Path.Combine(basePath, "libs", fileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new System.Exception("Invalid Python installation, missing .lib files");
    }
}
