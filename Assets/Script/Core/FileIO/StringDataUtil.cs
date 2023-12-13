
public static class StringDataUtil
{
    public static float readFloat(string value)
    {
        if(value.Contains("Random_"))
        {
            string data = value.Replace("Random_","");
            string[] randomData = data.Split('^');
            if(randomData == null || randomData.Length != 2)
            {
                DebugUtil.assert(false,"invalid float data: {0}", value);
                return 0f;
            }

            return UnityEngine.Random.Range(float.Parse(randomData[0]),float.Parse(randomData[1]));
        }

        return float.Parse(value);
    }

    public static string cutFileNameFromFullPath(string fullPath)
    {
        return fullPath.Substring(0,fullPath.LastIndexOf('/'));
    }
}
