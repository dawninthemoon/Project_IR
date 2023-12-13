using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class IOControl {

	public class INIDataInfo
	{
		public string title;
		public string data;

		public INIDataInfo(string t, string d) {title = t; data = d;}
	};

	public static void WriteStringToFile_NoMark(string[] str,string fileName, bool addPath = true)
	{
		string path = addPath ? PathForDocumentsFile(fileName) : fileName;
		FileStream file = new FileStream ( path, FileMode.Create, FileAccess.Write );
		StreamWriter sw = new StreamWriter( file );
		int line = str.Length;

		for(int i = 0; i < line; ++i)
		{
			sw.WriteLine(str[i]);
		}

		sw.Close();
		file.Close();
	}

    public static bool ReadRangeFromCSV(string text, int rowMin,int rowMax, int colMin, int colMax, out List<List<string>> data)
    {
        var row = text.Replace("\r",string.Empty).Split('\n');
        if(row.Length <= rowMax || row.Length <= rowMin)
        {
            Debug.Log("row range error");
            data = null;
            return false;
        }

        data = new List<List<string>>();
        rowMax = rowMax == -1 ? row.Length - 1 : rowMax;

        for(int i = rowMin; i <= rowMax; ++i)
        {
            var col = row[i].Split(',');
            
            if(row[i] == "")
                continue;

            if(col.Length <= colMax || col.Length <= colMax)
            {
                Debug.Log("len : " + col.Length + " min, max : " + colMin + "," + colMax + " column range error");
                data = null;
                return false;
            }

            var colList = new List<string>();
            data.Add(colList);
            colMax = colMax == -1 ? col.Length - 1 : colMax;

            for(int j = colMin; j <= colMax; ++j)
            {
                colList.Add(col[j]);
            }
        }

        return true;
    }

	public static string[] ReadStringFromFile(string fileName)
	{
		if(File.Exists(fileName))
		{
			FileStream file = new FileStream(fileName,FileMode.Open,FileAccess.Read);
			StreamReader st = new StreamReader(file);

			if(file == null || st == null)
			{
				Debug.Log("file does not exists");
				return null;
			}

			string[] s = null;
			s = st.ReadToEnd().Replace("\r",string.Empty).Split('\n');

			st.Close();
			file.Close();

			return s;
		}
		else
		{
			Debug.Log("file does not exists");
			return null;
		}
	}

	public static Dictionary<string,INIDataInfo[]> ReadiniFile(string fileName)
	{
		var data = ReadStringFromFile(fileName);
		if(data == null)
			return null;

		var blockList = new Dictionary<string,INIDataInfo[]>();
		var dataList = new List<INIDataInfo>();
		string title = "";

		foreach(var line in data)
		{
			if(line == "")
			{
				continue;
			}
			if(line[0] == '[')
			{
				if(title != "" && dataList.Count != 0)
				{
					blockList[title] = dataList.ToArray();
					dataList.Clear();
				}
				
				title = line.Substring(1,line.Length - 2);
				blockList.Add(title,null);
			}
			else
			{
				var split = line.Split('=');
				dataList.Add(new INIDataInfo(split[0],split[1]));
			}
		}

		if(title != "" && dataList.Count != 0)
		{
			blockList[title] = dataList.ToArray();
			dataList.Clear();
		}

		return blockList;
	}

	public static string ReadStringFromFile_NoSplit(string fileName)
	{
		string path = PathForDocumentsFile(fileName);

		if(File.Exists(path))
		{
			FileStream file = new FileStream(path,FileMode.Open,FileAccess.Read);
			StreamReader st = new StreamReader(file);

			if(file == null || st == null)
			{
				Debug.Log("file is does not exists");
				return null;
			}

			string s = null;
			s = st.ReadToEnd();

			st.Close();
			file.Close();

			return s;
		}
		else
		{
			Debug.Log("file is empty");
			return null;
		}
	}

	public static void getAllFileList(string path, string extension, ref List<DirectoryInfo> directoryInfoList, ref List<FileInfo> fileInfoList)
	{
		directoryInfoList.Clear();
		fileInfoList.Clear();

		DirectoryInfo di = new DirectoryInfo(path);
		foreach (DirectoryInfo directory in di.GetDirectories())
		{
			directoryInfoList.Add(directory);
		}
		foreach (FileInfo file in di.GetFiles())
		{
			if(file.Extension.ToLower() != extension)
				continue;

			fileInfoList.Add(file);
		}
	}

	public static void getFileList(string path, string extension, ref List<string> fileList)
	{
		fileList.Clear();

		DirectoryInfo di = new DirectoryInfo(PathForDocumentsFile(path));
		foreach (FileInfo file in di.GetFiles())
		{
			if(file.Extension.ToLower() != extension)
				continue;

			fileList.Add(file.Name);
		}
	}

	public static void getDirectoryList(string path, ref List<string> directoryList)
	{
		directoryList.Clear();

		DirectoryInfo di = new DirectoryInfo(PathForDocumentsFile(path));
		foreach (DirectoryInfo directory in di.GetDirectories())
		{
			directoryList.Add(directory.Name);
		}
	}

	public static string PathForDocumentsFile(string str)
	{
		string path = "";
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			
		}
		else if(Application.platform == RuntimePlatform.Android)
		{
			path = Application.persistentDataPath;
			path = path.Substring(0,path.LastIndexOf('/'));
		}
		else
		{
			path = Application.dataPath;
			path = path.Substring(0,path.LastIndexOf('/'));
		}

		return Path.Combine(path,str);
	}
}
