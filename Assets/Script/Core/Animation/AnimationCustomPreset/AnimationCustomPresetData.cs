[System.Serializable]
public class AnimationCustomPresetData
{
    public float[]      _duration = null;
    public string[]     _effectFrameEvent = null;
    public int          _playCount = 0;

    public float getTotalDuration()
    {
        if(_duration == null)
            return 0f;
            
        float total = 0f;
        for(int index = 0; index < _duration.Length; ++index)
            total += _duration[index];

        return total;
    }
}
