// Options menu: "QoL Features..." page with ON/OFF toggles for the QoL mods.
//
// The in-game menu (gamemenu) is a list of buttons btn[0..N-1]; the page is identified by T
// (0 = main, 1 = reset, 2 = save/load to PC, 10 = window size). Sub-pages are made by spawning a
// new gamemenu, setting btn/N/T and NZ = 1 (ignore clicks for a few steps), then destroying
// the old one. We add page T = 20:
//   btn[0] Ritual finish   -> global.qol_rit
//   btn[1] Speed button    -> global.qol_spd  (turning it off also resets the speed to 1x)
//   btn[2] Fast fights     -> global.qol_btl
//   btn[3] Fame Shop       -> global.qol_fame (hides the button and disables every perk effect)
//   btn[4] Amber upgrades... -> opens page T = 21 (below): one switch per Amber Shop item, global.qol_ab[0..8]; see 07_amber_effects.csx
//   btn[5] 5-tile Empire pts -> global.qol_emp5 (Empire points for every 5 tiles instead of 15; see 08_empire_rate.csx.
//                             The points are recounted at once. Switching OFF after spending more than the 15-tile
//                             rate gives resets the Empire perks for free, with a save + restart like the Empire reset)
//   btn[6] Extra buildings -> global.qol_nb (six new buildings in the tile menus; see 09_new_buildings.csx)
//   btn[7] Hero extras     -> global.qol_hero (20 new weapons, 5 shields, 8 helmets and 7 hero classes; see patches 10, 11, 12)
//   btn[8] Divine extras   -> global.qol_div (6 gods, 3 relics, 3 mutations, 2 empire rows; see 13_divine_extras.csx)
//   btn[9] < Back          -> main menu
// Page T = 21 "Amber upgrades" (opened from btn[4]):
//   btn[0]      All upgrades   -> sets every item ON (or all OFF when they are all ON already)
//   btn[1..9]   one switch per Amber Shop item -> global.qol_ab[0..8]
//   btn[10]     < Back         -> the QoL page
// Toggles keep the menu open (we exit before the game's alarm[5] closes it) and are saved to the
// "opt" ini ([QOL] section). Clicking outside the buttons closes the menu as usual.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// ---- Amber upgrades page (T = 21): the GML is generated from this table. ----
// { label RU, label EN, what it does RU, what it does EN }, in the order of global.qol_ab[0..8].
string[][] abItems =
{
    new[] { "+25% зерна: ", "+25% wheat: ", "Постоянный бонус +25% к добыче зерна.", "Your permanent +25% wheat production." },
    new[] { "+25% дерева: ", "+25% wood: ", "Постоянный бонус +25% к добыче дерева.", "Your permanent +25% wood production." },
    new[] { "+25% камня: ", "+25% stone: ", "Постоянный бонус +25% к добыче камня.", "Your permanent +25% stone production." },
    new[] { "+25% веры: ", "+25% faith: ", "Постоянный бонус +25% к добыче веры.", "Your permanent +25% faith production." },
    new[] { "Старт. ресурсы: ", "Start res +1000: ", "+1000 зерна, дерева и камня в начале каждой новой игры.", "+1000 wheat, wood and stone at the start of every new game." },
    new[] { "Старт. вера: ", "Start faith: ", "+1000 веры в начале каждой новой игры.", "+1000 faith at the start of every new game." },
    new[] { "99 рабочих: ", "99 workers: ", "Поля, лесные лагеря и каменоломни получают#99 рабочих при постройке.", "Wheat fields, forest camps and quarries get#99 workers when they are built." },
    new[] { "Скорость боя 1.5x: ", "1.5x fight speed: ", "Усиление на один бой из магазина янтаря:#1.5x скорость боя, пока клетка не захвачена.#Пока выключено, оно не тратится.", "The one-battle boost from the Amber Shop:#1.5x battle speed until the tile is captured.#It is not used up while this is OFF." },
    new[] { "Герой x2 на бой: ", "2x hero stats: ", "Усиление на один бой из магазина янтаря:#удвоенные характеристики героя.#Пока выключено, оно не тратится.", "The one-battle boost from the Amber Shop:#double hero stats for the next battle.#It is not used up while this is OFF." },
};
string G(string ru, string en) => "g(\"" + ru + "\", \"" + en + "\")";

var abMouse = new System.Text.StringBuilder();
abMouse.Append(@"if (T == 21)
{
    if (M == 10)
    {
        var _pb = instance_create(x, y, gamemenu);
        with (_pb)
        {
            N = 10;
            T = 20;
            NZ = 1;
        }
        io_clear();
        instance_destroy();
        exit;
    }
    if (M == 0)
    {
        var _abn = 0;
        for (var _abi = 0; _abi < 9; _abi++)
        {
            _abn += global.qol_ab[_abi];
        }
        var _abv = 1;
        if (_abn == 9)
        {
            _abv = 0;
        }
        for (var _abj = 0; _abj < 9; _abj++)
        {
            global.qol_ab[_abj] = _abv;
        }
    }
    else
    {
        global.qol_ab[M - 1] = !global.qol_ab[M - 1];
    }
    ini_open(""opt"");
    for (var _abk = 0; _abk < 9; _abk++)
    {
        ini_write_real(""QOL"", ""ab"" + string(_abk), global.qol_ab[_abk]);
    }
    ini_close();
    exit;
}
");
string AmberMouse = abMouse.ToString();

var abDraw = new System.Text.StringBuilder();
abDraw.Append(@"
if (T == 21)
{
    var _on2 = g(""ВКЛ"", ""ON"");
    var _off2 = g(""ВЫКЛ"", ""OFF"");
    var _abc = 0;
    for (var _abx = 0; _abx < 9; _abx++)
    {
        _abc += global.qol_ab[_abx];
    }
    var _sa = _off2;
    if (_abc == 9)
    {
        _sa = _on2;
    }
    else if (_abc > 0)
    {
        _sa = string(_abc) + ""/9"";
    }
    btn[0] = g(""Все апгрейды: "", ""All upgrades: "") + _sa;
");
for (int k = 0; k < abItems.Length; k++)
{
    abDraw.Append("    _sa = _off2;\n    if (global.qol_ab[" + k + "])\n    {\n        _sa = _on2;\n    }\n");
    abDraw.Append("    btn[" + (k + 1) + "] = " + G(abItems[k][0], abItems[k][1]) + " + _sa;\n");
}
abDraw.Append("    btn[10] = g(\"< Назад\", \"< Back\");\n}");
string AmberDraw = abDraw.ToString();

var abTips = new System.Text.StringBuilder();
abTips.Append("if (T == 21 && M == 0)\n{\n    mHelp(" + G(
    "Включает или выключает все эффекты магазина янтаря сразу.#Янтарь и покупки не теряются.",
    "Turns every Amber Shop effect ON or OFF at once.#Your Amber and purchases are never lost.") + ");\n}\n");
for (int k = 0; k < abItems.Length; k++)
{
    abTips.Append("if (T == 21 && M == " + (k + 1) + ")\n{\n    mHelp(" + G(
        abItems[k][2] + "#Янтарь и покупки не теряются.",
        abItems[k][3] + "#Your Amber and purchases are never lost.") + ");\n}\n");
}
string AmberTips = abTips.ToString();

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
        N = 10;
        T = 20;
        NZ = 1;
    }
    io_clear();
    instance_destroy();
    exit;
}
" + AmberMouse + @"if (T == 20)
{
    var _rst = 0;
    if (M == 9)
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
    if (M == 3)
    {
        global.qol_fame = !global.qol_fame;
    }
    if (M == 4)
    {
        var _pa = instance_create(x, y, gamemenu);
        with (_pa)
        {
            N = 11;
            T = 21;
            NZ = 1;
        }
        io_clear();
        instance_destroy();
        exit;
    }
    if (M == 5)
    {
        global.qol_emp5 = !global.qol_emp5;
        var _est = 15 - (10 * global.qol_emp5);
        main.empire_points = 0;
        main.empire_progress = _est;
        if (!global.qol_emp5 && main.empire_spend > (floor((main.tile_num + main.empire_ppp) / _est) * (1 + global.PLANET)))
        {
            for (var _ei = 0; _ei < 9; _ei++)
            {
                main.emp_c[_ei] = 0;
            }
            _rst = 1;
        }
    }
    if (M == 6)
    {
        global.qol_nb = !global.qol_nb;
    }
    if (M == 7)
    {
        global.qol_hero = !global.qol_hero;
    }
    if (M == 8)
    {
        global.qol_div = !global.qol_div;
        main.pan_N = 11 + (12 * global.qol_div);
    }
    ini_open(""opt"");
    ini_write_real(""QOL"", ""rit"", global.qol_rit);
    ini_write_real(""QOL"", ""spd"", global.qol_spd);
    ini_write_real(""QOL"", ""btl"", global.qol_btl);
    ini_write_real(""QOL"", ""fame"", global.qol_fame);
    ini_write_real(""QOL"", ""emp5"", global.qol_emp5);
    ini_write_real(""QOL"", ""nb"", global.qol_nb);
    ini_write_real(""QOL"", ""hero"", global.qol_hero);
    ini_write_real(""QOL"", ""div"", global.qol_div);
    ini_close();
    if (_rst)
    {
        with (main)
        {
            event_perform(ev_alarm, 1);
        }
        game_restart();
    }
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
    _s = _off;
    if (global.qol_fame)
    {
        _s = _on;
    }
    btn[3] = g(""Магазин славы: "", ""Fame Shop: "") + _s;
    btn[4] = g(""Апгрейды янтаря..."", ""Amber upgrades..."");
    _s = _off;
    if (global.qol_emp5)
    {
        _s = _on;
    }
    btn[5] = g(""Очки за 5 клеток: "", ""5-tile Empire pts: "") + _s;
    _s = _off;
    if (global.qol_nb)
    {
        _s = _on;
    }
    btn[6] = g(""Новые здания: "", ""Extra buildings: "") + _s;
    _s = _off;
    if (global.qol_hero)
    {
        _s = _on;
    }
    btn[7] = g(""Доп. герои: "", ""Hero extras: "") + _s;
    _s = _off;
    if (global.qol_div)
    {
        _s = _on;
    }
    btn[8] = g(""Божественное: "", ""Divine extras: "") + _s;
    btn[9] = g(""< Назад"", ""< Back"");
}" + AmberDraw);

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
if (T == 20 && M == 3)
{
    mHelp(g(""Кнопка 'Магазин славы' рядом с магазином янтаря:#каждое очко славы - это очко перка для#постоянных улучшений. При выключении кнопка скрыта,#а все перки не действуют (покупки сохраняются)."", ""'Fame Shop' button next to the Amber Shop:#every Fame point is also a perk point for#permanent upgrades. Turning it off hides the button#and disables all perks (your purchases are kept).""));
}
if (T == 20 && M == 4)
{
    mHelp(g(""Открывает список постоянных эффектов магазина янтаря:#можно отключать каждый по отдельности.#Янтарь и покупки не теряются. Разовые товары#(промотка, золото, наследие) не затрагиваются."", ""Opens the list of lasting Amber Shop effects:#you can switch each one off separately.#Your Amber and purchases are never lost. One-off items#(timelapse, gold, heritage) are not affected.""));
}
if (T == 20 && M == 5)
{
    mHelp(g(""Очки империи даются за каждые 5 клеток вместо 15.#Очки пересчитываются сразу, включая прошлые континенты.#Если выключить после того, как вы потратили больше очков,#чем даёт обычная скорость, бонусы империи бесплатно#сбрасываются, а игра перезапускается."", ""Empire points are given for every 5 tiles instead of 15.#Points are recounted at once, tiles from earlier#continents included. If you switch it off after spending#more than the normal rate gives, your Empire bonuses#are reset for free and the game restarts.""));
}
if (T == 20 && M == 6)
{
    mHelp(g(""16 новых зданий в меню тайла (кнопка 'Еще >',#морские - в меню 'Пст. Морское'): Мельница, Рынок,#Лесопилка, Таверна, Монолит, Библиотека, Обсерватория,#Банк, Арена, Башня стражи, Казармы, Порт, Рыбацкий пирс,#Маяк, Парусный цех, Мор. академия.#Плюс 4 особых здания (меню 'Пст. Особое'): Скит,#Монетный двор, Цитадель, Святилище.#При выключении кнопки скрыты, а бонусы не действуют#(построенные здания остаются).#Перед удалением мода очистите их клетки."", ""16 new buildings in the tile menus ('More >' button;#the sea ones are in Bld. Marine): Windmill, Market,#Sawmill, Tavern, Monolith, Library, Observatory,#Bank, Arena, Watchtower, Barracks, Harbor, Fishery,#Lighthouse, Sail Loft, Naval Academy.#Plus 4 special buildings (Bld. Special menu): Hermitage,#Imperial Mint, Citadel, Grand Sanctum.#Turning it off hides the buttons and pauses their bonuses#(buildings you already built stay).#Clear their tiles before you uninstall the mod.""));
}
if (T == 20 && M == 8)
{
    mHelp(g(""5 новых богов религии, 12 новых пантеонов, 3 реликвии, 3 мутации,#3 новых вида ритуала и 2 империи (Монгольская, Персидская).#При выключении они скрыты, а их эффекты не действуют.#Выбранное сохраняется (очки империи остаются потраченными)."", ""5 new religion gods, 12 new pantheons, 3 relics, 3 mutations,#3 new ritual types and 2 empires (Mongolian, Persian).#Turning it off hides them and pauses their effects.#Your choices are kept (spent Empire points stay spent).""));
}
if (T == 20 && M == 7)
{
    mHelp(g(""20 новых видов оружия, 5 щитов, 8 шлемов#и 7 новых классов героя (Рыцарь, Следопыт, Клирик,#Фехтовальщик, Чернокнижник, Мученик, Ученый).#При выключении новые вещи не открываются, а классы#скрыты (у героя они остаются до следующей клетки)."", ""20 new weapons, 5 shields, 8 helmets and 7 new#hero classes (Knight, Ranger, Cleric, Duelist, Warlock,#Martyr, Scholar).#Turning it off stops the new items from unlocking and#hides the classes (a hero keeps them until the next tile).""));
}
" + AmberTips + @"mouse_norm();");

// 5) The main menu's 'Steam Page' hover flag (row 7 of the main page) must not fire on our longer pages.
importGroup.QueueFindReplace(
    "gml_Object_gamemenu_Draw_64",
    "if (M == 7 && !instance_exists(steam_ob))",
    "if (M == 7 && T == 0 && !instance_exists(steam_ob))");

importGroup.Import();
