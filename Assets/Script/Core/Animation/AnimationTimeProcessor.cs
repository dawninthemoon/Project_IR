public class AnimationTimeProcessor
{

    private AnimationCustomPresetData _customPresetData = null;

    private float       _framePerSecond = 0f;
    private float       _frameToTime = 0f;
    private float       _animationTime = 0f;

    private float       _animationStartTime = 0f;
    private float       _animationEndTime = 0f;

    private bool        _isLoop = false;
    private bool        _isEnd = false;

    private bool        _isLoopedThisFrame = false;

    private int         _animationLoopCount = 0;
    private int         _totalLoopCountPerFrame;

    private float       _currentAnimationTime = 0f;
    private int         _currentIndex = 0;
    private int         _endIndex = 0;

    private float       _prevAnimationTime = 0f;
    private int         _prevIndex = 0;

    private float       _prevAnimationTotalPlayTime = 0f;
    private float       _animationTotalPlayTime = 0f;
    private float       _animationSpeed = 1f;


    private float       _frameDurationStack = 0f;
    private float       _actionDuration = -1f;




    public bool isValid()
    {
        //return _frameCount != 0;
        return true;
    }

    public void initialize()
    {
        _customPresetData = null;
        _frameDurationStack = 0f;
        _actionDuration = -1f;

        _isEnd = false;
        _isLoop = false;
        _isLoopedThisFrame = false;

        _animationLoopCount = 0;

        _currentAnimationTime = 0f;
        _currentIndex = 0;
        _endIndex = 0;

        _prevAnimationTime = 0f;
        _prevIndex = 0;

        _prevAnimationTotalPlayTime = 0f;
        _animationTotalPlayTime = 0f;

        _framePerSecond = 0f;
        _frameToTime = 0f;

        _animationTime = 0f;

        _animationStartTime = 0f;
        _animationEndTime = 0f;

        _totalLoopCountPerFrame = 0;
        _animationSpeed = 1f;
    }

    public bool updateTime(float deltaTime)
    {
        DebugUtil.assert(isValid(),"프레임 카운트가 0입니다. 통보 요망");
        
        _totalLoopCountPerFrame = 0;

        if(_isEnd == true)
            return true;

        if(_animationSpeed == 0f)
        {
            DebugUtil.assert(false, "애니메이션 속도는 0이 될 수 없습니다.");
            _animationSpeed = 1f;
        }

        deltaTime *= _animationSpeed;

        _prevAnimationTime = _currentAnimationTime;
        _prevIndex = _currentIndex;

        _prevAnimationTotalPlayTime = _animationTotalPlayTime;

        _currentAnimationTime += deltaTime;
        _animationTotalPlayTime += deltaTime;

        _isLoopedThisFrame = false;

        if(_customPresetData != null)
            return updateTime2(deltaTime);

        _isEnd = CurrentAnimationIsEndInner();

        if((_isLoop == true || _animationLoopCount > 0) && _isEnd == true)
        {
            _isEnd = false;

            while(_prevAnimationTime >= _animationEndTime)
            {
                _prevAnimationTime -= _animationTime;
            }

            while(_currentAnimationTime >= _animationEndTime)
            {
                if(_animationLoopCount > 0 && --_animationLoopCount <= 0)
                {
                    _isEnd = true;
                    _isLoop = false;
                    _currentAnimationTime = _animationTime;
                }
                else
                {
                    ++_totalLoopCountPerFrame;
                    _currentAnimationTime -= _animationTime;
                }
            }
            
            _isLoopedThisFrame = true;
            setAnimationSpeed(1f);
        }
        else if(_isEnd == true)
        {
            _currentAnimationTime = _animationEndTime;
        }
            
        _currentIndex = getIndexInner();

        return _isEnd;
    }

    public bool updateTime2(float deltaTime)
    {
        bool forceEnd = false;
        if(_actionDuration != -1f && _animationTotalPlayTime >= _actionDuration)
        {
            _currentAnimationTime = _actionDuration - ((float)((int)(_animationTotalPlayTime * (1f / _currentAnimationTime))) * _animationEndTime);
            _prevAnimationTotalPlayTime = _actionDuration;
            _animationTotalPlayTime = _actionDuration;
            forceEnd = true;
        }

        for(int index = _currentIndex; index < _endIndex; ++index)
        {
            _currentIndex = index;
            if(_currentAnimationTime < _frameDurationStack + _customPresetData._duration[index])
                break;

            _frameDurationStack += _customPresetData._duration[index];
        }

        _isEnd = CurrentAnimationIsEndInner();

        if((_isLoop || _animationLoopCount > 0) && _isEnd == true)
        {
            _isEnd = false;
            while(_prevAnimationTime >= _animationEndTime)
            {
                _prevAnimationTime -= _animationTime;
            }

            while(_currentAnimationTime >= _animationEndTime)
            {
                _currentAnimationTime -= _animationTime;
                ++_totalLoopCountPerFrame;
                if(_animationLoopCount > 0 && --_animationLoopCount <= 0)
                {
                    _isEnd = true;
                    _isLoop = false;

                    _currentAnimationTime = _animationTime;

                    break;
                }
            }

            _isLoopedThisFrame = _isLoop || _animationLoopCount != 0;
            _frameDurationStack = 0f;

            float stack = _customPresetData._duration[0];
            for(int index = 0; index < _endIndex; ++index)
            {
                _currentIndex = index;

                if(_currentAnimationTime < stack)
                    break;
                
                _frameDurationStack += _customPresetData._duration[index];
                stack += _customPresetData._duration[index];
            }

            setAnimationSpeed(1f);
        }
        else if(_isEnd)
        {
            _currentIndex = _endIndex - 1;
            _currentAnimationTime = _animationEndTime;
        }

        if(forceEnd)
        {
            _isEnd = true;
            return true;
        }

        _isEnd &= _actionDuration == -1f;

        return _isEnd;
    }

    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc()
    {
        MoveValuePerFrameFromTimeDesc desc;
        desc.currentNormalizedTime = getCurrentNormalizedTime();
        desc.prevNormalizedTime = getPrevNormalizedTime();
        desc.loopCount = _customPresetData != null ? 0 : getTotalLoopCount();

        return desc;
    }

    public int getCurrentIndex()
    {
        return _currentIndex;
    }

    public int getEndIndex()
    {
        return _endIndex;
    }

    private int getIndexInner()
    {
        int currentIndex = (int)(_currentAnimationTime / _frameToTime);
        return _currentAnimationTime >= _animationEndTime ? currentIndex - 1 : currentIndex;
    }

    public int getTotalLoopCount()
    {
        return _totalLoopCountPerFrame;
    }

    public float getCurrentAnimationTime()
    {
        return _currentAnimationTime;
    }

    public float getAnimationDuration() {return _animationTime;}

    public float getAnimationTotalPlayTime()
    {
        return _animationTotalPlayTime;
    }

    public float getCurrentNormalizedTime()
    {
        if(_isEnd)
            return 1f;
        
        if(_customPresetData != null)
            return _animationTotalPlayTime * (1f / _actionDuration);
            
        return (_currentAnimationTime - _animationStartTime) * (1f /_animationTime);
    }

    public float getCurrentAnimationNormalizedTime()
    {
        if(_isEnd)
            return 1f;
            
        return (_currentAnimationTime - _animationStartTime) * (1f /_animationTime);
    }

    public float getPrevNormalizedTime()
    {
        if(_prevAnimationTime == _currentAnimationTime && _isEnd)
            return 1f;
        
        if(_customPresetData != null)
            return _prevAnimationTotalPlayTime * (1f / _actionDuration);

        return (_prevAnimationTime - _animationStartTime) * (1f / _animationTime);
    }

    public float getCurrentFrame()
    {
        if(_customPresetData == null)
            return _currentAnimationTime / _frameToTime;
        else
            return (float)_currentIndex + ((_currentAnimationTime - _frameDurationStack) * (1f / _customPresetData._duration[_currentIndex]));
    }

    public void setFrameToTime(float frame)
    {
        _currentAnimationTime = frame * _frameToTime;
        updateTime(0f);
    }

    public float getAnimationSpeed()
    {
        return _animationSpeed;
    }
    public void setAnimationSpeed(float speed)
    {
        _animationSpeed = speed;
    }

    public void setLoop(bool isLoop) 
    {
        _isLoop = isLoop;
    }

    public bool isLoop() {return _isLoop;}

    public void setLoopCount(int loopCount)
    {
        _animationLoopCount = loopCount;
    }

    public void setFrame(float startFrame, float endFrame, float fps)
    {
        _framePerSecond = fps;
        _frameToTime = 1f / _framePerSecond;

        _animationStartTime = startFrame * _frameToTime;
        _animationEndTime = endFrame * _frameToTime;
        _animationTime = _animationEndTime - _animationStartTime;
    }

    public void setCustomPresetData(AnimationCustomPresetData customPresetData)
    {
        if(customPresetData == null)
            return;

        _customPresetData = customPresetData;
        _animationStartTime = 0f;
        _animationEndTime = customPresetData.getTotalDuration();
        _animationTime = _animationEndTime;

        _endIndex = customPresetData._duration.Length;

        setLoop(customPresetData._playCount == 0);
        setLoopCount(customPresetData._playCount);
        setAnimationSpeed(1f);
        setFrameToTime(0f);

        _framePerSecond = 0f;
        _frameToTime = 0f;
    }

    public void setActionDuration(float duration)
    {
        _actionDuration = duration;
    }

    public float frameToTime(float frame)
    {
        return frame * _frameToTime;
    }

    public bool isLoopedThisFrame()
    {
        return _isLoopedThisFrame;
    }

    public bool isEnd()
    {
        return _isEnd;
    }

    private bool CurrentAnimationIsEndInner()
    {
        return _animationTime <= _currentAnimationTime;
    }

}

