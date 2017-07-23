using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class TouchOscAddress : MonoBehaviour {
    public static TouchOscAddress Instance;

	// Use this for initialization
	void Start () {
		if (Instance == null)
        {
            Instance = this;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Load specified file
    /// </summary>
    /// <param name="_fileName"></param>
    /// <param name="fileExt"></param>
    /// <returns></returns>
    public string Load(string _fileName, string fileExt)
    {
        string fileName = GetFilePath(_fileName, fileExt);

        if (!File.Exists(fileName))
        { return null; }

        byte[] result;
        try
        {
            using (FileStream SourceStream = File.Open(fileName, FileMode.Open))
            {
                result = new byte[SourceStream.Length];
                SourceStream.Read(result, 0, (int)SourceStream.Length);
                return System.Text.Encoding.ASCII.GetString(result);
            }
        }
        catch
        {
            throw new System.Exception("Could not load: " + fileName);
        }
    }

    /// <summary>
    /// Save specified file`
    /// </summary>
    /// <param name="_file"></param>
    /// <param name="_fileExt"></param>
    public void Save(string _file, string _fileExt, string _line)
    {
        string path = GetFilePath(_file, _fileExt);
        byte[] line = Encoding.ASCII.GetBytes(_line);

        // Write the string to a file.
        try
        {
            UnityEngine.Windows.File.WriteAllBytes(path, line);
        }
        catch
        {
            throw new System.Exception("Could not save to file: " + GetFilePath(_file, _fileExt));
        }
    }

    /// <summary>
    /// Return the file path
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string GetFilePath(string fileName, string fileExt)
    {
        return Path.Combine(Application.persistentDataPath, fileName + fileExt);
    }
}
