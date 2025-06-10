using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

[InitializeOnLoad]
public class LaunchConifersInstaller
{
    static LaunchConifersInstaller()
    {
        EditorApplication.update += Update;
    }


    static void Update()
    {
        EditorApplication.update -= Update;

        if( !EditorApplication.isPlayingOrWillChangePlaymode )
        {
            ConifersInstaller.Init();
        }
    }
}