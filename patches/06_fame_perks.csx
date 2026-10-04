// QoL: effects of the Fame Shop perks. Each hook multiplies by (global.fs_lv[k] * global.qol_fame), so with
// the "Fame Shop" switch OFF every perk is inactive (purchases are kept). Perk index k = branch * 4 + slot:
//
//   Prosperity  0 Bountiful Harvest  +10%/lvl wheat, wood, stone production
//               1 Faithful Hearts    +10%/lvl faith production
//               2 Royal Treasury     +10%/lvl gold from abdication
//               3 Land Grants        -8%/lvl gold cost of new tiles
//   Industry    4 Quick Hands        +15%/lvl worker hiring speed
//               5 Fair Wages         -6%/lvl cost of hiring workers
//               6 Seed Stock         +300/lvl wheat, wood, stone at the start of every new game
//               7 Pilgrim's Provisions +300/lvl faith at the start of every new game
//   Valor       8 Veteran's Resolve  -6%/lvl monsters needed per tile battle
//               9 Battle Wisdom      +12%/lvl hero experience
//              10 Long Rituals       +6 s/lvl ritual duration
//              11 Frugal Rites       -6%/lvl ritual cost
//
// Max level is 5 for every perk. Level L costs (4 / 8 / 14) * L perk points for tier 1 / 2 / 3 perks.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// effective level of perk k (0 when the Fame Shop switch is off)
string F(int k) => "(global.fs_lv[" + k + "] * global.qol_fame)";

// 0, 1: production. Just before the per-step income is added, so the top bar, rituals and offline progress all see it.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "var SP = 30;",
    "d_zern *= (1 + (0.1 * " + F(0) + "));\n" +
    "d_les *= (1 + (0.1 * " + F(0) + "));\n" +
    "d_kam *= (1 + (0.1 * " + F(0) + "));\n" +
    "d_ver *= (1 + (0.1 * " + F(1) + "));\n" +
    "var SP = 30;");

// 3: tile cost (shown in the tooltip and charged by the same variable).
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "kle_cost2 = kle_cost;",
    "kle_cost2 = kle_cost * (1 - (0.08 * " + F(3) + "));");

// 2: abdication gold (the tooltip and the real payout both use get_compen).
importGroup.QueueFindReplace(
    "gml_Script_get_compen",
    "return floor(d / 10);",
    "d *= (1 + (0.1 * " + F(2) + "));\nreturn floor(d / 10);");

// 4: hiring speed. Base hiring time of the live hiring code (Land step) and of the ritual auto-hire (HireRabs).
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs" })
{
    importGroup.QueueFindReplace(entry,
        "nspd = Var.rtime[r_i] * power(1.1, rab);",
        "nspd = Var.rtime[r_i] * power(1.1, rab) / (1 + (0.15 * " + F(4) + "));");
    importGroup.QueueFindReplace(entry,
        "nspd = Var.rtime[r_i] * power(1.05, rab);",
        "nspd = Var.rtime[r_i] * power(1.05, rab) / (1 + (0.15 * " + F(4) + "));");
}

// 5: worker hiring cost (getCost is used by both the live and the ritual hiring code).
importGroup.QueueFindReplace(
    "gml_Script_getCost",
    "var n = argument0 * power(1.15, argument1 - argument2);",
    "var n = argument0 * power(1.15, argument1 - argument2) * (1 - (0.06 * " + F(5) + "));");

// 6, 7: starting resources of every new game (loading a fresh game, and the values written at abdication).
importGroup.QueueFindReplace("gml_Object_main_Alarm_2", "+ main.pps_res)", "+ main.pps_res + (300 * " + F(6) + "))");
importGroup.QueueFindReplace("gml_Object_main_Alarm_2", "+ main.pps_ver)", "+ main.pps_ver + (300 * " + F(7) + "))");
importGroup.QueueFindReplace("gml_Script__tri_cave", "+ main.pps_res)", "+ main.pps_res + (300 * " + F(6) + "))");
importGroup.QueueFindReplace("gml_Script__tri_cave", "+ main.pps_ver)", "+ main.pps_ver + (300 * " + F(7) + "))");

// 8: monsters needed per tile battle (every place that computes need_kills, so the value stays consistent).
string kills80  = "need_kills = 200 + (200 * global.battled) + (80 * sqr(global.battled));";
string kills100 = "need_kills = 200 + (200 * global.battled) + (100 * sqr(global.battled));";
string Kills(string original) => "need_kills = ceil((" + original.Substring("need_kills = ".Length).TrimEnd(';') + ") * (1 - (0.06 * " + F(8) + ")));";
foreach (string entry in new[] { "gml_Object_LandBattle_Create_0", "gml_Object_LandBattle_Other_10" })
{
    importGroup.QueueFindReplace(entry, kills80, Kills(kills80));
    importGroup.QueueFindReplace(entry, kills100, Kills(kills100));
}
importGroup.QueueFindReplace("gml_Script_empire_pick_s", kills80, Kills(kills80));

// 9: hero experience per kill.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "* (1 + (3 * main.muta[9]));",
    "* (1 + (3 * main.muta[9])) * (1 + (0.12 * " + F(9) + "));");

// 10: ritual duration.
importGroup.QueueFindReplace(
    "gml_Object_HRit_Other_10",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2));",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2)) + (6 * " + F(10) + ");");

// 11: ritual cost, in the ritual window (this ritual, the next one, and the multi-cast total) and in tmp_rit_cost.
string cheaper = "((100 - (6 * " + F(11) + ")) * rk)";
string cheaper2 = "((100 - (6 * " + F(11) + ")) * rk2)";
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "(100 * rk)", cheaper);
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "(100 * rk2)", cheaper2);
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", "(100 * rk)", cheaper);

importGroup.Import();
