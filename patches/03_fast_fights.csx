// QoL: faster fights - chain one-shot kills.
// Toggle: global.qol_btl (Options > QoL Features).
//
// LandBattle.Alarm_0 is the fight timer: each firing runs one attack tick
// (event_user(0)), and a kill also sets wait = 1 so the next tick is a pause.
// After a tick that killed a monster we now keep ticking right away, up to 100
// kills per firing, as long as the hero's damage one-shots the next monster.
// Every extra tick is the game's own attack code, so exp, dodging, counter damage,
// kill bonuses and tile completion all behave exactly as before - only the waiting
// between kills is skipped.
//
// The chain stops at the first monster the hero cannot one-shot, when the hero dies
// (ranen), or when the tile is finished (kills >= need_kills; the tick destroys
// this instance, so we must not tick again).

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Alarm_0",
    "event_user(0);",
    @"var _k0 = kills;
event_user(0);
if (global.qol_btl && kills > _k0)
{
    var _ck = 1;
    while (_ck < 100 && kills < need_kills && !ranen && O.hero_HP > 0)
    {
        var _dm = O.hero_DAM;
        if (O.hero_wp == 6)
        {
            if (O.hero_ss == 4)
            {
                _dm = O.hero_HP * 5;
            }
            else
            {
                _dm = O.hero_HP;
            }
        }
        var _heal = 0;
        if (vrag_DAM == 6)
        {
            _heal = 6;
        }
        if (_dm < (vrag_HP + _heal))
        {
            break;
        }
        var _kb = kills;
        wait = 0;
        event_user(0);
        if (kills == _kb)
        {
            break;
        }
        _ck += 1;
    }
}");

importGroup.Import();
