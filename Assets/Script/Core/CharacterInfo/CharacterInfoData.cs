
[System.Serializable]
public class CharacterInfoData
{
    public string       _displayName;
    public string       _actionGraphPath;
    public string       _aiGraphPath;
    public string       _statusName;

    public float        _characterWidth;
    public float        _characterHeight;
    public float        _headUpOffset;

    public SearchIdentifier _searchIdentifer = SearchIdentifier.Enemy;
    public CommonMaterial   _defaultMaterial = CommonMaterial.Skin;
}
