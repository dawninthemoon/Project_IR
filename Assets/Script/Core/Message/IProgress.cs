
interface IProgress
{
    void assign();
    void initialize();
    void progress(float deltaTime);
    void afterProgress(float deltaTime);
    void fixedProgress(float deltaTime);
    void release(bool disposeFromMaster);
}