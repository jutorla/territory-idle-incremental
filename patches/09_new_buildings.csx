// QoL: six NEW BUILDINGS, two in each of the tile's Build / Bld. Stone / Bld. Heroic menus ("More >" opens page 2).
// Switch: global.qol_nb (Options > QoL Features > "Extra buildings"). Default ON. OFF hides the buttons and pauses every
// bonus below (buildings that are already built stay, and keep their workers).
//
//   BLD  menu      name         hired workers   effect (all counted over EVERY tile of that type, like the Farm)
//   13   Build     Windmill     Millers         +2% wheat production on all wheat fields per miller
//   14   Build     Market       Merchants       +1 gold per second per 10 merchants (gold also raises the abdication payout)
//   15   Stone     Monolith     Keepers         +0.5% to ALL production (wheat, wood, stone, faith) per keeper
//   16   Stone     Library      Scribes         +2% hiring speed in every building per scribe (no limit)
//   17   Heroic    Arena        Gladiators      each academy hero gains 0.5 exp per second per hero level per gladiator
//   18   Heroic    Watchtower   Watchmen        every 100 watchmen halve the monsters needed to capture a tile (no limit)
//
// How they plug into the game (the game has no building list; everything is keyed by the number BLD = Var index + 1):
//   * Var (Create): cost / bcnt / bspd / binfo and rcost / rtime / rcnt / rcntmax / rname get entries 12..17 and BLDN = 18.
//     Everything generic already loops over Var.BLDN (the per-tile arrays, saving the speed-ups, the welcome-back summary),
//     and the hiring, saving, clearing and the "Spd" amber button work from those arrays, so they work unchanged.
//   * Land Create/Draw/Step: the menu pages (men = 5, 6, 7 are page 2 of Build / Stone / Heroic), the tile "faces" (same
//     layout as the game's own buildings), the production multipliers on doh, and the Arena experience.
//   * main Step: the Market's gold. HireRabs + Land Step: the Library. LandBattle + empire_pick_s: the Watchtower.
//   * The free workers that the game gives to "most" buildings (Empire +10 workers, the mutation's +500) are NOT given to
//     the new ones, so a first-built Monolith cannot start with 500 keepers.
//
// All numbers live in the table below, in one place, so they are easy to retune.
//
// UNINSTALLING: a save that contains these buildings cannot be loaded by the unmodified game. Clear them (or abdicate)
// and switch the option off before running Uninstall.bat. The README says so.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string G(string ru, string en) => "g(\"" + ru + "\", \"" + en + "\")";

// ---------------------------------------------------------------------------------------------------------------------
// The table. Index k = 0..5; Var index = 12 + k; BLD = 13 + k.
//   Cost = building price { wheat, wood, gold, stone }    Hire = worker price { wheat, wood }, seconds for the first worker
//   Page = men value of the page it appears on (5 Build, 6 Stone, 7 Heroic), Slot = 0/1 (top / second button)
// ---------------------------------------------------------------------------------------------------------------------
string[] nameRu  = { "Мельница", "Рынок", "Монолит", "Библиотека", "Арена", "Башня стражи" };
string[] nameEn  = { "Windmill", "Market", "Monolith", "Library", "Arena", "Watchtower" };
string[] hireRu  = { "Нанять мельника", "Нанять торговца", "Нанять хранителя", "Нанять писца", "Нанять гладиатора", "Нанять дозорного" };
string[] hireEn  = { "Hire a miller", "Hire a merchant", "Hire a keeper", "Hire a scribe", "Hire a gladiator", "Hire a watchman" };
string[] nounRu  = { "Мельники: ", "Торговцы: ", "Хранители: ", "Писцы: ", "Гладиаторы: ", "Дозорные: " };
string[] nounEn  = { "Millers: ", "Merchants: ", "Keepers: ", "Scribes: ", "Gladiators: ", "Watchmen: " };
string[] infoRu  =
{
    "Мельники перемалывают зерно:#+2% к добыче зерна на всех#полях за каждого мельника.",
    "Торговцы приносят золото:#+1 золото в сек за каждых 10#торговцев (растет и бонус отречения).",
    "Хранители древнего монолита:#+0.5% ко всей добыче (зерно, дерево,#камень, вера) за каждого хранителя.",
    "Писцы записывают знания:#+2% к скорости найма во всех зданиях#за каждого писца.",
    "Гладиаторы тренируют героя:#каждый дает 0.5 опыта в сек#за каждый уровень героя.",
    "Дозорные выслеживают врагов:#каждые 100 дозорных вдвое уменьшают#число монстров на тайл.",
};
string[] infoEn  =
{
    "Millers grind your grain:#+2% wheat production on all wheat#fields for every miller.",
    "Merchants bring in gold:#+1 gold per second for every 10#merchants (also raises the abdication bonus).",
    "Keepers of an ancient monolith:#+0.5% to all production (wheat, wood,#stone, faith) for every keeper.",
    "Scribes record the knowledge:#+2% hiring speed in every building#for each scribe.",
    "Gladiators train your hero:#each one gives 0.5 exp per second#for every hero level.",
    "Watchmen scout the enemy:#every 100 watchmen halve the#monsters per tile.",
};
//                      wheat     wood      gold    stone
double[][] cost =
{
    new double[] {      8000,     8000,     0,      0 },            // Windmill
    new double[] {     20000,    20000,   100,      0 },            // Market (a little gold, like the Quarry)
    new double[] {         0,   200000,     0, 400000 },            // Monolith
    new double[] {    100000,   400000,     0, 1500000 },           // Library
    new double[] {3000000000, 3000000000,   0, 3000000000 },        // Arena (heroic buildings cost billions)
    new double[] {         0, 6000000000, 1000000, 6000000000 },    // Watchtower
};
//                      wheat        wood   seconds
double[][] hire =
{
    new double[] {       4000,        0,    2 },                    // millers
    new double[] {          0,     3000,    3 },                    // merchants (paid in wood)
    new double[] {      50000,        0,    4 },                    // keepers
    new double[] {     100000,        0,    5 },                    // scribes
    new double[] {  500000000,        0,    8 },                    // gladiators
    new double[] {  300000000,        0,    6 },                    // watchmen
};
int[] page = { 5, 5, 6, 6, 7, 7 };
int[] slot = { 0, 1, 0, 1, 0, 1 };
// Face colours (cc = border and text, cb = background), icon sprite + frame, and the effect line drawn on the tile.
string[] cc =
{
    "merge_color(c_orange, c_yellow, 0.35)",
    "merge_color(c_yellow, c_orange, 0.25)",
    "merge_color(c_ltgray, c_aqua, 0.3)",
    "merge_color(c_yellow, c_ltgray, 0.55)",
    "merge_color(c_red, c_orange, 0.35)",
    "merge_color(c_lime, c_gray, 0.5)",
};
string[] cb =
{
    "merge_color(cc, c_black, 0.8)",
    "merge_color(cc, c_black, 0.82)",
    "merge_color(cc, c_black, 0.85)",
    "merge_color(c_maroon, c_black, 0.75)",
    "merge_color(cc, c_black, 0.82)",
    "merge_color(cc, c_black, 0.85)",
};
string[] spr   = { "tt_wmill", "tt_tamo", "tt_mine", "tt_school", "tt_dodze", "tt_tamo" };
int[]    frame = { 0, 0, 1, 1, 0, 1 };
bool[]   plate = { false, false, false, false, true, false };   // the black belt needs a light plate behind it
string[] effect =
{
    "\"+\" + string(2 * Var.rcnt[12]) + " + G("% зерна на всех полях", "% wheat on all fields"),
    "\"+\" + string(floor(rab / 10)) + " + G(" золота в сек", " gold per sec"),
    "\"+\" + string(0.5 * Var.rcnt[14]) + " + G("% ко всей добыче", "% to all production"),
    "\"+\" + string(2 * Var.rcnt[15]) + " + G("% скорость найма везде", "% hiring speed everywhere"),
    "\"+\" + string(0.5 * Var.rcnt[16]) + " + G(" опыта героя/сек за ур.", " hero exp/sec per level"),
    "\"-\" + string(round(100 * (1 - power(0.993092, Var.rcnt[17])))) + " + G("% монстров на тайл", "% monsters per tile"),
};
const int N = 6;

// 1) Setting, read from the "opt" ini ([QOL] nb, default ON) at the very top of main Create.
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    @"VERSION = 167;
if (!variable_global_exists(""qol_nb""))
{
    ini_open(""opt"");
    global.qol_nb = ini_read_real(""QOL"", ""nb"", 1);
    ini_close();
}");

// 2) Var Create: the buildings (cost, counters, info text) go right before the worker table, the workers before the enemies.
var sbB = new System.Text.StringBuilder();
for (int k = 0; k < N; k++)
{
    sbB.Append("cost[BLDN, 0] = " + cost[k][0] + ";\n");
    sbB.Append("cost[BLDN, 1] = " + cost[k][1] + ";\n");
    sbB.Append("cost[BLDN, 2] = " + cost[k][2] + ";\n");
    sbB.Append("cost[BLDN, 3] = " + cost[k][3] + ";\n");
    sbB.Append("bcnt[BLDN] = 0;\nbspd[BLDN] = 0;\n");
    sbB.Append("binfo[BLDN] = " + G(infoRu[k], infoEn[k]) + ";\n");
    sbB.Append("BLDN++;\n");
}
importGroup.QueueFindReplace("gml_Object_Var_Create_0", "rcost[0, 0] = 10;", sbB.ToString() + "rcost[0, 0] = 10;");

var sbW = new System.Text.StringBuilder();
for (int k = 0; k < N; k++)
{
    int v = 12 + k;
    sbW.Append("rcost[" + v + ", 0] = " + hire[k][0] + ";\n");
    sbW.Append("rcost[" + v + ", 1] = " + hire[k][1] + ";\n");
    sbW.Append("rtime[" + v + "] = " + hire[k][2] + ";\n");
    sbW.Append("rcnt[" + v + "] = 0;\nrcntmax[" + v + "] = 0;\n");
    sbW.Append("rname[" + v + "] = " + G(nounRu[k], nounEn[k]) + ";\n");
}
importGroup.QueueFindReplace("gml_Object_Var_Create_0", "vrag_spr[0] = 73;", sbW.ToString() + "vrag_spr[0] = 73;");

// 3) Land Create: the flags of the "More >" and "< Back" buttons.
importGroup.QueueAppend("gml_Object_Land_Create_0", "\nnb_more = 0;\nnb_back = 0;\n");

// 4) Land Draw, menu: a "More >" button next to "Cancel" on pages 1..3, and the three second pages.
importGroup.QueueFindReplace(
    "gml_Object_Land_Draw_0",
    "b_cc = DrawBtnSml(x + 99, y + 176, g(\"Отмена\", \"Cancel\"));",
    @"b_cc = DrawBtnSml(x + 99, y + 176, g(""Отмена"", ""Cancel""));
nb_more = 0;
nb_back = 0;
if (global.qol_nb && men <= 3)
{
    nb_more = DrawBtnSml(x + 10, y + 176, g(""Еще >"", ""More >""));
}");

var sbP = new System.Text.StringBuilder();
for (int p = 5; p <= 7; p++)
{
    sbP.Append("else if (men == " + p + ")\n{\n");
    for (int k = 0; k < N; k++)
    {
        if (page[k] != p) continue;
        int v = 12 + k;
        sbP.Append("    b[" + v + "] = DrawBtnPrg(x + 10, y + 10 + " + (slot[k] * 40) + ", " + G(nameRu[k], nameEn[k]) + ", bld_P[" + v + "]);\n");
    }
    sbP.Append("    draw_set_font(font1);\n");
    sbP.Append("    nb_more = 0;\n");
    sbP.Append("    nb_back = DrawBtnSml(x + 10, y + 176, g(\"< Назад\", \"< Back\"));\n");
    sbP.Append("    b_cc = DrawBtnSml(x + 99, y + 176, g(\"Отмена\", \"Cancel\"));\n");
    sbP.Append("}\n");
}
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "else if (men == 4)", sbP.ToString() + "else if (men == 4)");

// 5) Land Draw, the tile faces. They must be drawn BEFORE the shared "Spd" / "Clear" / "stop autohire" controls and the
//    amber icon (the game draws those once, after the last of its own buildings): a face paints the whole tile, so one drawn
//    after them would hide them. So they go in right after the Shipyard (the game's last face). The anchor is the last line of
//    the Shipyard's block; our text closes that block and ends with an empty copy of its opening, so the original closing
//    braces still match. Same layout as the game's own buildings.
var sbF = new System.Text.StringBuilder("\n");
for (int k = 0; k < N; k++)
{
    int bld = 13 + k;
    sbF.Append("if (BLD == " + bld + ")\n{\n");
    sbF.Append("    cc = " + cc[k] + ";\n");
    sbF.Append("    cb = " + cb[k] + ";\n");
    sbF.Append("    cr = merge_color(cc, c_white, 0.2);\n");
    sbF.Append("    draw_set_color(cc);\n    draw_rectangle(x, y, x + w, y + h, false);\n");
    sbF.Append("    draw_set_color(cb);\n    draw_rectangle(x + a, y + a, (x + w) - a, (y + h) - a, false);\n");
    sbF.Append("    draw_set_color(cc);\n    draw_set_font(font0);\n");
    if (plate[k])
    {
        sbF.Append("    draw_set_color(c_ltgray);\n    draw_rectangle(x + 3, y + 3, x + 35, y + 35, false);\n    draw_set_color(cc);\n");
    }
    sbF.Append("    draw_sprite(" + spr[k] + ", " + frame[k] + ", x + 19, y + 19);\n");
    sbF.Append("    name = " + G(nameRu[k], nameEn[k]) + ";\n");
    sbF.Append("    draw_text(x + 41, y + 7, name);\n");
    sbF.Append("    draw_set_font(font1);\n");
    sbF.Append("    b[0] = DrawBtnPrg(x + 10, y + 40, " + G(hireRu[k], hireEn[k]) + ", rab_P, cr);\n");
    sbF.Append("    draw_set_font(font0);\n");
    sbF.Append("    if (rab)\n    {\n");
    sbF.Append("        draw_set_color(cc);\n");
    sbF.Append("        draw_text(x + 5, y + 76, " + G(nounRu[k], nounEn[k]) + " + string(rab));\n");
    sbF.Append("        draw_text_ext(x + 5, y + 92, " + effect[k] + ", 14, w - 9);\n");
    sbF.Append("    }\n");
    sbF.Append("    if (!global.qol_nb)\n    {\n");
    sbF.Append("        draw_set_color(c_gray);\n");
    sbF.Append("        draw_text_ext(x + 5, y + 130, g(\"(выключено в настройках QoL)\", \"(switched off in QoL options)\"), 14, w - 9);\n");
    sbF.Append("    }\n");
    sbF.Append("}\n");
}
string shipLast = "draw_text_ext(x + 5, y + 92, s, 15, w - 8);";
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", shipLast,
    shipLast + "\n    }\n}\n" + sbF.ToString() + "if (BLD == 9)\n{\n    if (rab)\n    {\n");

// 6) Land Step, menu: switching pages. The game leaves bm / bmk / bmh / bmw set after a category was opened (it re-reads
//    them on every click to stay on the same page), which would pull the page back, so they are cleared while a menu is
//    open. Pages are told apart by men: 5 = Build 2, 6 = Stone 2, 7 = Heroic 2 (page + 4).
importGroup.QueueFindReplace(
    "gml_Object_Land_Step_0",
    "if (bm)",
    @"if (men >= 5 && !global.qol_nb)
{
    men = 0;
}
if (men != 0)
{
    bm = 0;
    bmk = 0;
    bmh = 0;
    bmw = 0;
}
if (nb_more && mouse_check_button_pressed(mb_left))
{
    men += 4;
    nb_more = 0;
    for (i = 0; i < N; i++)
    {
        b[i] = 0;
    }
    exit;
}
if (nb_back && mouse_check_button_pressed(mb_left))
{
    men -= 4;
    nb_back = 0;
    for (i = 0; i < N; i++)
    {
        b[i] = 0;
    }
    exit;
}
if (bm)");

// 7) Land Step, production: multipliers on doh (it is recomputed from scratch every step, so nothing compounds).
//    Windmill -> wheat fields. Monolith -> wheat, wood, stone and faith buildings.
importGroup.QueueFindReplace(
    "gml_Object_Land_Step_0",
    "if (global._trial == 0 && BLD == 5)",
    @"if (global.qol_nb)
{
    if (BLD == 1)
    {
        doh *= (1 + (0.02 * Var.rcnt[12]));
    }
    if (BLD == 1 || BLD == 2 || BLD == 4 || BLD == 5)
    {
        doh *= (1 + (0.005 * Var.rcnt[14]));
    }
}
if (global._trial == 0 && BLD == 5)");

// 8) Land Step, Arena: every academy with a hero (and not retired) gains experience. hero_exp is a running total (the game
//    does not subtract it on a level-up), so this only moves the next level closer. 30 steps = 1 second.
importGroup.QueueFindReplace(
    "gml_Object_Land_Step_0",
    "if (clr_b)",
    @"if (BLD == 7 && rab > 0 && zahv == 0 && global.qol_nb)
{
    hero_exp += (((Var.rcnt[16] * 0.5) * (hero_lvl + 1)) / 30);
}
if (clr_b)");

// 9) Library: hiring time. The expression is in two places (the tile's own hiring and HireRabs, which is used by the
//    auto-hire ritual and by the offline catch-up). The Hotel (resting) keeps its own formula and is not changed.
string libOld = "power(1.05, rab)) / (1 + (0.25 * (global.fs_lv[0] * global.qol_fame)))";
string libNew = libOld + " / (1 + (0.02 * Var.rcnt[15] * global.qol_nb))";
importGroup.QueueFindReplace("gml_Object_Land_Step_0", libOld, libNew);
importGroup.QueueFindReplace("gml_Script_HireRabs", libOld, libNew);

// 10) No free starting workers for the new buildings (Empire +10 workers per building, and the mutation's +500).
importGroup.QueueFindReplace(
    "gml_Object_Land_Step_0",
    "if (BLD != 7 && BLD != 8 && BLD != 11 && BLD != 12)",
    "if (BLD != 7 && BLD != 8 && BLD != 11 && BLD != 12 && BLD < 13)");

// 11) Market gold, added with the other building incomes in main Step.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "if (myst)",
    @"if (BLD == 14 && global.qol_nb)
{
    other.d_gold += floor(rab / 10);
}
if (myst)");

// 12) Watchtower: fewer monsters per tile. The Fame Shop patch (06) already wraps every place that sets need_kills in
//     "* (1 - 0.08 * Veteran's Resolve)"; we add our factor next to it (the same expression in all three places).
string killOld = "(1 - (0.08 * (global.fs_lv[9] * global.qol_fame)))";
string killNew = "(" + killOld + " * power(0.993092, Var.rcnt[17] * global.qol_nb))";
importGroup.QueueFindReplace("gml_Object_LandBattle_Create_0", killOld, killNew);
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", killOld, killNew);
importGroup.QueueFindReplace("gml_Script_empire_pick_s", killOld, killNew);

importGroup.Import();
