using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceProjectileEntity : ProjectileEntityBase
{
    public string graphName;

    public override void initialize()
    {
        base.initialize();
        setData(ProjectileManager._instance.getProjectileGraphData(graphName));
        shot(transform.position);

    }
}
