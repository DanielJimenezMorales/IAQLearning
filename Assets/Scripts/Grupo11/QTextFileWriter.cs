using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class QTextFileWriter
{
    public QTextFileWriter() { }

    public void CreateText(string filePath, string content)
    {
        string path = Application.dataPath + filePath;

        if(!File.Exists(path))
        {
            File.WriteAllText(path, content);
        }
        else
        {
            File.Delete(path);

            File.WriteAllText(path, content);
        }
    }
}
