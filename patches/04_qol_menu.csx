// Options menu: "QoL Features..." page with ON/OFF toggles for the three QoL mods.
//
// The in-game menu (gamemenu) is a list of buttons btn[0..N-1]; the page is identified by T
// (0 = main, 1 = reset, 2 = save/load to PC, 10 = window size). Sub-pages are made by spawning a
// new gamemenu, setting btn/N/T and NZ = 1 (ignore clicks for a few steps), then destroying
// the old one. We add page T = 20:
//   btn[0] Ritual finish   -> global.qol_rit
//   btn[1] Speed button    -> global.qol_spd  (turning it off also resets the speed to 1x)
//   btn[2] Fast fights     -> global.qol_btl
//   btn[3] < Back          -> main menu
// Toggles keep the menu open (we exit before the game's alarm[5] closes it) and are saved to the
// "opt" ini ([QOL] section). Clicking outside the buttons closes the menu as usual.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// 1) Create: add the entry at the end of the main list. Its index differs between the Steam and
//    non-Steam menus, so remember it in qolI and handle the click ourselves in Mouse_53.
importGroup.QueueAppend(
    "gml_Object_gamemenu_Create_0",
    @"
btn[N] = g(""QoL функции..."", ""QoL Features..."");
qolI = N;
N++;");

// 2) Mouse (global left pressed): open the page / handle toggles / go back.
importGroup.QueueFindReplace(
    "gml_Object_gamemenu_Mouse_53",
    "event_user(M);",
    @"if (T == 0 && M == qolI)
{
    var _p = instance_create(x, y, gamemenu);
    with (_p)
    {
        N = 4;
        T = 20;
        NZ = 1;
    }
    io_clear();
    instance_destroy();
    exit;
}
if (T == 20)
{
    if (M == 3)
    {
        var _p = instance_create(x, y, gamemenu);
        with (_p)
        {
            NZ = 1;
        }
        io_clear();
        instance_destroy();
        exit;
    }
    if (M == 0)
    {
        global.qol_rit = !global.qol_rit;
    }
    if (M == 1)
    {
        global.qol_spd = !global.qol_spd;
        if (!global.qol_spd)
        {
            global.spd_i = 0;
            global.spd_want = 1;
        }
    }
    if (M == 2)
    {
        global.qol_btl = !global.qol_btl;
    }
    ini_open(""opt"");
    ini_write_real(""QOL"", ""rit"", global.qol_rit);
    ini_write_real(""QOL"", ""spd"", global.qol_spd);
    ini_write_real(""QOL"", ""btl"", global.qol_btl);
    ini_close();
    exit;
}
event_user(M);");

// 3) Draw GUI: rebuild the page's labels every frame from the settings.
importGroup.QueueFindReplace(
    "gml_Object_gamemenu_Draw_64",
    "M = -1;",
    @"M = -1;
if (T == 20)
{
    var _on = g(""ВКЛ"", ""ON"");
    var _off = g(""ВЫКЛ"", ""OFF"");
    var _s = _off;
    if (global.qol_rit)
    {
        _s = _on;
    }
    btn[0] = g(""Завершение ритуала: "", ""Ritual finish: "") + _s;
    _s = _off;
    if (global.qol_spd)
    {
        _s = _on;
    }
    btn[1] = g(""Кнопка скорости: "", ""Speed button: "") + _s;
    _s = _off;
    if (global.qol_btl)
    {
        _s = _on;
    }
    btn[2] = g(""Быстрые бои: "", ""Fast fights: "") + _s;
    btn[3] = g(""< Назад"", ""< Back"");
}");

// 4) Draw GUI: hover tooltips for the page (M is the hovered button, set by the loop above).
importGroup.QueueFindReplace(
    "gml_Object_gamemenu_Draw_64",
    "mouse_norm();",
    @"if (T == 20 && M == 0)
{
    mHelp(g(""Нажмите на кнопку 'Обряд' во время ритуала,#чтобы завершить его мгновенно.#Вы получите всё производство за оставшееся время."", ""Click the 'Ritual' button while a ritual is running#to finish it instantly.#You get all the production for the remaining time.""));
}
if (T == 20 && M == 1)
{
    mHelp(g(""Добавляет кнопку скорости игры (x1/x2/x5/x10/x20)#в левой колонке под верхней панелью.#При выключении скорость сбрасывается до x1."", ""Adds a game speed button (x1/x2/x5/x10/x20)#to the left column under the top bar.#Turning it off resets the speed to x1.""));
}
if (T == 20 && M == 2)
{
    mHelp(g(""Если удар героя убивает следующего монстра,#бой продолжается сразу: до 100 убийств за раз#вместо одного за такт."", ""When the hero's hit kills the next monster, the fight#continues right away: up to 100 kills at once#instead of one per tick.""));
}
mouse_norm();");

importGroup.Import();
