

public static partial class MessageTitles
{

    public const ushort     customTitle_start           = 0xff00;

    public const ushort     system_registerRequest      = 0x0100;
    public const ushort     system_deregisterRequest    = 0x0101;
    public const ushort     system_onTriggerEnter       = 0x0102;
    public const ushort     system_onTriggerExit        = 0x0103;
    public const ushort     system_pauseUpdate          = 0x0104;
    public const ushort     system_playUpdate           = 0x0105;
    public const ushort     system_updateFrame          = 0x0106;

    //게임 시작 알림
    public const ushort     game_gameStart              = 0x0200;
    //게임 오버 알림
    public const ushort     game_gameOver               = 0x0201;
    //맵 변경 알림
    public const ushort     game_mapChange              = 0x0202;
    //타겟 텔레포트
    public const ushort     game_teleportTarget         = 0x0203;
    public const ushort     game_stageEnd               = 0x0204;


    public const ushort     effect_spawnEffect          = 0x0300;

    public const ushort     entity_setTarget            = 0x0400;
    public const ushort     entity_searchNearest        = 0x0401;
    public const ushort     entity_spawnCharacter       = 0x0402;
    public const ushort     entity_stopUpdate           = 0x0403;
    public const ushort     entity_setTimeScale         = 0x0404;
    public const ushort     entity_searchNearestQuick   = 0x0401;

}
