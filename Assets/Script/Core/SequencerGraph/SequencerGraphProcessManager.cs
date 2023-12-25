using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequencerGraphProcessManager
{
    private SimplePool<SequencerGraphProcessor> _sequencerGrpahProcessorPool = new SimplePool<SequencerGraphProcessor>();
    private List<SequencerGraphProcessor> _activeProcessorList = new List<SequencerGraphProcessor>();

    private GameEntityBase _ownerEntity = null;

    public SequencerGraphProcessManager(GameEntityBase ownerEntity)
    {
        _ownerEntity = ownerEntity;
    }

    public void initialize()
    {
        clearSequencerGraphProcessManager();
    }

    public void progress(float deltaTime)
    {
        for(int index = 0; index < _activeProcessorList.Count;)
        {
            SequencerGraphProcessor processor = _activeProcessorList[index];            
#if UNITY_EDITOR
            if(processor.isValid() == false)
            {
                DebugUtil.assert(false,"프로세서가 있는데 데이터가 없다. 통보 요망");
                continue;
            }
#endif
            processor.progress(deltaTime);
            if(processor.isSequencerEnd())
            {
                _activeProcessorList.RemoveAt(index);
                _sequencerGrpahProcessorPool.enqueue(processor);
            }
            else
            {
                ++index;
            }
        }
    }

    public SequencerGraphProcessor startSequencerFromStage(string sequencerKey, StagePointData currentPoint, List<SequencerGraphProcessor.SpawnedCharacterEntityInfo> pointCharacters, GameEntityBase targetEntity,List<MarkerItem> markerList, bool includePlayer)
    {
        SequencerGraphProcessor processor = _sequencerGrpahProcessorPool.dequeue();
        processor.clearSequencerGraphProcessor();
        processor.startSequencerFromStage(sequencerKey,currentPoint,pointCharacters,_ownerEntity,targetEntity,markerList,includePlayer);

        _activeProcessorList.Add(processor);

        return processor;
    }

    public SequencerGraphProcessor startSequencerClean(string sequencerKey, GameEntityBase targetEntity,bool includePlayer)
    {
        SequencerGraphProcessor processor = _sequencerGrpahProcessorPool.dequeue();
        processor.clearSequencerGraphProcessor();
        processor.startSequencer(sequencerKey,_ownerEntity,targetEntity,includePlayer);

        _activeProcessorList.Add(processor);

        return processor;
    }

    public void addSequencerSignal(string signal)
    {
        foreach(var processor in _activeProcessorList)
        {
            processor.addSignal(signal);
        }
    }

    public void clearSequencerGraphProcessManager()
    {
        foreach(var processor in _activeProcessorList)
        {
            processor.stopSequencer();
            processor.clearSequencerGraphProcessor();
            _sequencerGrpahProcessorPool.enqueue(processor);
        }

        _activeProcessorList.Clear();
    }
}
