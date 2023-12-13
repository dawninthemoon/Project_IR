using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanmakuManager : Singleton<DanmakuManager>
{
    private DanmakuGraph        _danmakuGraph;

    public void initialize()
    {
        _danmakuGraph = new DanmakuGraph();
        _danmakuGraph.initialize(null);
    }

    public void process(float deltaTime)
    {
        _danmakuGraph.process(deltaTime);

    }

    public void release()
    {
        _danmakuGraph.release();
    }

    public void addDanmaku(string path, Vector3 position, Vector3 offset, bool useFlip, SearchIdentifier searchIdentifier)
    {
        _danmakuGraph.addDanmakuGraph(path, position, offset, useFlip, searchIdentifier);
    }
}
