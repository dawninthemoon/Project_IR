
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ReadPivotProcessor
{
    private Color _pivotAnchorColor;

    private bool _find = false;
    private Vector2Int _findPivot = Vector2Int.zero;
    private Texture2D _targetTexture;


    private int _textureWidth;
    private int _textureHeight;
    
    public ReadPivotProcessor()
    {
        _find = false;
    }

    public void start(Texture2D targetTexture, Color pivotColor)
    {
        if(targetTexture.isReadable == false)
        {
            DebugUtil.assert(false,"target texture is none readable");
            return;
        }

        _targetTexture = targetTexture;
        _pivotAnchorColor = pivotColor;
        _find = false;
        _findPivot = new Vector2Int(-1,-1);

        _textureWidth = _targetTexture.width;
        _textureHeight = _targetTexture.height;

        for(int y = 0; y <= _textureHeight; ++y)
        {
            for(int x = 0; x < _textureWidth; ++x)
            {
                Debug.Log(_targetTexture.GetPixel(x,y));
                if(MathEx.equals(_targetTexture.GetPixel(x,y), _pivotAnchorColor, Mathf.Epsilon) == true)
                {
                    
                    _find = true;
                    _findPivot.x = x;
                    _findPivot.y = y;

                    return;
                }
            }
        }
    }

    public bool getPivot(out Vector2Int pivot)
    {
        pivot = _findPivot;
        return _find;
    }
}

public class MovementDataExporter
{
    private static MovementDataExporter window;    
    private Sprite[] _rawSpriteData;

    public void readAndExportData(string filePath, Color pivotAnchorColor)
    {
        _rawSpriteData = ResourceContainerEx.Instance().GetSpriteAll(filePath);

        if(_rawSpriteData == null)
        {
            DebugUtil.assert(false,"sprite does not exists");
            return;
        }

        ReadPivotProcessor processor = new ReadPivotProcessor();

        Vector2Int realPivot = Vector2Int.zero;
        Vector2Int prevRealPivot = Vector2Int.zero;
        Vector3 totalMove = Vector3.zero;
        Vector2 prevPivot = Vector2.zero;
        Vector2 normalizedPivot = Vector2.zero;

        MovementGraphData[] movementGraphDatas = new MovementGraphData[_rawSpriteData.Length];

        for(int i = 0; i < _rawSpriteData.Length; ++i)
        {
            if(_rawSpriteData[i] == null)
            {
                DebugUtil.assert(false,"sprite is null");
                return;
            }

            processor.start(_rawSpriteData[i].texture,pivotAnchorColor);

            prevRealPivot = realPivot;
            if(processor.getPivot(out realPivot) == false)
            {
                DebugUtil.assert(false,"pivot not found : {0}",_rawSpriteData[i].texture.name);
                return;
            }
            else
            {
                prevPivot = normalizedPivot;
                normalizedPivot = pivotIndexToTextureSpace(_rawSpriteData[i].texture,realPivot);
                ChangePivot(_rawSpriteData[i].texture,normalizedPivot);

                MovementGraphData movementData = new MovementGraphData();
                movementData._index = i;

                if(i != 0)
                {
                    Vector2Int pivotDiff = realPivot - prevRealPivot;

                    movementData._moveFactor = new Vector3((float)pivotDiff.x, (float)pivotDiff.y) * 0.01f;
                    totalMove = totalMove + movementData._moveFactor;
                    movementData._totalMoveFactor = totalMove;
                }

                movementGraphDatas[i] = movementData;
            }
        }

        SaveToScriptableObject(movementGraphDatas, _rawSpriteData[0].name, filePath);
    }

    public void SaveToScriptableObject(MovementGraphData[] graphDatas, string fileName, string path)
    {
        MovementGraph asset = ScriptableObject.CreateInstance<MovementGraph>();

        asset.setData(graphDatas, fileName);

        AssetDatabase.CreateAsset(asset, "Assets/" + path + fileName + ".asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    public Vector2 pivotIndexToTextureSpace(Texture2D texture, Vector2Int pivot)
    {
        return new Vector2(((float)pivot.x + 0.5f) / (float)texture.width, ((float)pivot.y + 0.5f) / (float)texture.height);
    }


    public void ChangePivot(Texture2D texture, Vector2 pivot)
    {
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
    
        ti.spritePivot = pivot;
        TextureImporterSettings texSettings = new TextureImporterSettings();
        ti.ReadTextureSettings(texSettings);
        texSettings.spriteAlignment = (int)SpriteAlignment.Custom;
        ti.SetTextureSettings(texSettings);
        ti.SaveAndReimport();
    }
}
#endif