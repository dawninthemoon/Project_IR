using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDSimulation : MonoBehaviour
{
    private static Vector3 _kGravity = Vector3.down * 9.8f;
    private const float _kStiffness = 0.9f;
    private const int _kIterations = 10;
    
    public Vector3 _firstParticle = Vector3.zero;
    public Vector3 _windForce = Vector3.right;

    private List<PBDParticleData> _particles = new List<PBDParticleData>();
    private List<PBDDistanceConstraintData> _constraints = new List<PBDDistanceConstraintData>();


    public void Start()
    {
        createCloth(10,10,0.3f);
    }

    public void Update()
    {
        updateParticles(Time.deltaTime);
        drawParticles();
    }
    public void drawParticles()
    {
        foreach(var particle in _particles)
        {
            GizmoHelper.instance.drawCircle(particle._position,0.1f,3,Color.red);
        }
    }

    public void createCloth(int width, int height, float distance)
    {
        _particles.Clear();
        _constraints.Clear();

        for(int y = 0; y < height; ++y)
        {
            for(int x = 0; x < width; ++x)
            {
                Vector3 position = new Vector3((float)x * distance,(float)-y * distance, 0f);
                float invMass = y == 0 ? (x == 0 || x == width - 1 ? 0f : 1f) : 1f;
                PBDParticleData particle = new PBDParticleData();
                particle.initialize(position,invMass);

                _particles.Add(particle);
            }
        }

        for(int y = 0; y < height; ++y)
        {
            for(int x = 0; x < width; ++x)
            {
                PBDParticleData particle1 = _particles[y * width + x];
                if(x < width - 1)
                {
                    PBDParticleData particle2 = _particles[y * width + x + 1];
                    PBDDistanceConstraintData constraint = new PBDDistanceConstraintData();
                    constraint.initialize(particle1,particle2,distance,_kStiffness);

                    _constraints.Add(constraint);
                }
                if(y < height - 1)
                {
                    PBDParticleData particle2 = _particles[(y + 1) * width + x];
                    PBDDistanceConstraintData constraint = new PBDDistanceConstraintData();
                    constraint.initialize(particle1,particle2,distance,_kStiffness);

                    _constraints.Add(constraint);
                }
            }
        }
    }

    public void updateParticles(float deltaTime)
    {
        _particles[0]._position = _firstParticle;

        foreach(var particle in _particles)
        {
            if(particle._invMass > 0f)
            {
                Vector3 environmentForce = _windForce + _kGravity;
                particle._prevPosition = particle._position;
                particle._position += (particle._position - particle._prevPosition) * 0.99f + environmentForce * deltaTime;
            }
        }

        for(int index = 0; index < _kIterations; ++index)
        {
            foreach(var constraint in _constraints)
            {
                constraint.solve();
            }
        }
    }
}
