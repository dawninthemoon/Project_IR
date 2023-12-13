using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class FileDebugger
{
    public static void OpenFileWithCursor(string filePath, int lineNumber)
    {
        string vscodePath = "code";
        string command = $"{vscodePath} -g {filePath}:{lineNumber}";

        Process.Start("cmd.exe", $"/c {command}");
    }

    public static int findLine(string filePath, string targetString)
    {
        var lines = File.ReadLines(filePath);
        int line = 1;
        foreach(var item in lines)
        {
            if(item.Contains(targetString))
                break;
            ++line;
        }

        return line;
    }
}
