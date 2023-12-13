[System.Serializable]
public class AttackPresetData
{
    public string               _name;
    public float                _attackRadius;
    public float                _attackAngle;
    public float                _attackStartDistance;
    public float                _attackRayRadius = 0f;

    public UnityEngine.Vector3  _pushVector;

    public CommonMaterial       _attackMaterial;
}
