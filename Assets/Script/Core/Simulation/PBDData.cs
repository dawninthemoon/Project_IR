using UnityEngine;

public class PBDParticleData
{
    public Vector3 _position;
    public Vector3 _prevPosition;
    public float _invMass;

    public void initialize(Vector3 position, float invMass)
    {
        _position = position;
        _prevPosition = position;
        _invMass = invMass;
    }
}

public class PBDDistanceConstraintData
{
    public PBDParticleData _particle1 = null;
    public PBDParticleData _particle2 = null;
    
    public float _restLength = 0f;
    public float _stiffness = 1f;

    public void initialize(PBDParticleData particle1, PBDParticleData particle2, float restLength, float stiffness)
    {
        _particle1 = particle1;
        _particle2 = particle2;
        _restLength = restLength;
        _stiffness = stiffness;
    }

    public void solve()
    {
        Vector3 delta = _particle1._position - _particle2._position;
        float deltaLength = delta.magnitude;
        float diff = (deltaLength - _restLength) / deltaLength;
        Vector3 correction = delta * (0.5f * _stiffness * diff);

        if(_particle1._invMass > 0f)
            _particle1._position -= correction * _particle1._invMass;
        if(_particle2._invMass > 0f)
            _particle2._position += correction * _particle2._invMass;
    }
}