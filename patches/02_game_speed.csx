// QoL: game speed button: cycles 1x -> 2x -> 5x -> 10x -> 20x (right click goes back).
// Placed in the left column under the top bar, above the Gold panel.
// Toggle: global.qol_spd (Options > QoL Features). Turning it off resets the speed to 1x.
//
// How it works: the whole game is step based (production, timers, battles, alarms),
// so scaling room_speed speeds up everything consistently. Vsync is switched off
// while sped up (otherwise the monitor refresh rate would cap the speed), and the
// autosave alarm is stretched by the same factor so saves stay ~1 per real second.
//
// Globals: spd_i = selected index (0..4), spd_want = selected multiplier,
//          spd_m = multiplier currently applied (changed only in End Step).
// This script also initialises ALL QoL settings (qol_rit / qol_spd / qol_btl) from the
// "opt" ini file ([QOL] section, default ON), because main.Create is the earliest hook.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// 1) Create: initialise globals at the very top (before the early-exit error path).
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    @"VERSION = 167;
if (!variable_global_exists(""spd_i""))
{
    global.spd_i = 0;
    global.spd_want = 1;
    global.spd_m = 1;
    ini_open(""opt"");
    global.qol_rit = ini_read_real(""QOL"", ""rit"", 1);
    global.qol_spd = ini_read_real(""QOL"", ""spd"", 1);
    global.qol_btl = ini_read_real(""QOL"", ""btl"", 1);
    ini_close();
}");

// 2) Draw GUI: the button, tooltip and click handling (inserted before the final mouse_norm()).
importGroup.QueueFindReplace(
    "gml_Object_main_Draw_64",
    "mouse_norm();",
    @"if (global.qol_spd)
{
    draw_set_font(font1);
    draw_set_halign(fa_left);
    draw_set_valign(fa_top);
    var _spdbg = 0;
    if (global.spd_want > 1)
    {
        _spdbg = merge_color(c_navy, c_orange, 0.45);
    }
    var b_SPD = DrawBtnW(Xgold1 + 5, 94, g(""Скорость: x"", ""Speed: x"") + string(global.spd_want), 1, _spdbg);
    if (b_SPD)
    {
        main.tmp_upper = 2;
        mHelp(g(""Скорость игры: x"" + string(global.spd_want) + ""#ЛКМ: следующая скорость (1x, 2x, 5x, 10x, 20x)#ПКМ: предыдущая скорость#Реальная скорость зависит от мощности компьютера"", ""Game speed: x"" + string(global.spd_want) + ""#Left click: next speed (1x, 2x, 5x, 10x, 20x)#Right click: previous speed#Actual speed depends on how fast your PC can run the game""));
        var _spdd = 0;
        if (mouse_check_button_pressed(mb_left))
        {
            _spdd = 1;
        }
        if (mouse_check_button_pressed(mb_right))
        {
            _spdd = 4;
        }
        if (_spdd)
        {
            global.spd_i = (global.spd_i + _spdd) mod 5;
            global.spd_want = 1;
            if (global.spd_i == 1)
            {
                global.spd_want = 2;
            }
            if (global.spd_i == 2)
            {
                global.spd_want = 5;
            }
            if (global.spd_i == 3)
            {
                global.spd_want = 10;
            }
            if (global.spd_i == 4)
            {
                global.spd_want = 20;
            }
        }
    }
}
mouse_norm();");

// 3) End Step: apply the chosen speed (display_reset must not run inside a Draw event).
importGroup.QueueAppend(
    "gml_Object_main_Step_2",
    @"
if (global.spd_want != global.spd_m)
{
    if (global.spd_m == 1)
    {
        display_reset(0, false);
    }
    else if (global.spd_want == 1)
    {
        display_reset(0, true);
    }
    global.spd_m = global.spd_want;
}
if (room_speed != (31 * global.spd_m))
{
    room_speed = 31 * global.spd_m;
}");

// 4) Autosave: keep the real-time save interval constant regardless of speed.
importGroup.QueueFindReplace(
    "gml_Object_main_Alarm_1",
    "alarm[1] = 30 + sdt;",
    "alarm[1] = (30 + sdt) * global.spd_m;");

importGroup.Import();
