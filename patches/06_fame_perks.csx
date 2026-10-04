// QoL: effects of the Fame Shop perks. Each hook multiplies by (global.fs_lv[k] * global.qol_fame), so with
// the "Fame Shop" switch OFF every perk is inactive (purchases are kept).
// Perk index k = branch * 7 + slot. Max level is 5 unless noted. Level L costs base * L perk points, with
// base 4 / 7 / 10 / 15 / 25 for tree rows 0 / 1 / 2 / 3 / 4 (capstone).
//
//  PROSPERITY  0 Fertile Fields      +25%/lvl wheat production
//              1 Lumber Boom         +25%/lvl wood production
//              2 Quarry Masters      +25%/lvl stone production
//              3 Golden Touch        +30%/lvl gold income
//              4 Royal Treasury      +25%/lvl gold from abdication
//              5 Land Grants         -12%/lvl gold cost of new tiles
//              6 Golden Age          +12%/lvl production of ALL resources (wheat, wood, stone, faith)
//  INDUSTRY    7 Quick Hands         +30%/lvl worker hiring speed
//              8 Fair Wages          -10%/lvl cost of hiring workers
//              9 Seed Stock          +1000/lvl wheat, wood and stone at the start of every new game
//             10 Pilgrim's Provisions +1000/lvl faith at the start of every new game
//             11 Open Enrollment     +1 extra worker per hire per lvl (max 3; not heroes)
//             12 Economies of Scale  -0.6%/lvl growth of the cost of each next worker (base 1.15 -> 1.12)
//             13 Guild Charter       +1 more extra worker per hire per lvl (max 2; not heroes)
//  VALOR      14 Veteran's Resolve   -8%/lvl monsters needed per tile battle
//             15 Mighty Blows        +25%/lvl hero damage
//             16 Battle Wisdom       +25%/lvl hero experience
//             17 Thick Skin          -10%/lvl damage taken by the hero
//             18 Swift Strikes       +20%/lvl hero attack speed
//             19 Nimble Feet         +4%/lvl chance to dodge
//             20 Warlord's Banner    +15%/lvl hero damage AND experience
//  DEVOTION   21 Long Rituals        +10 s/lvl ritual duration
//             22 Frugal Rites        -10%/lvl ritual cost
//             23 Divine Favor        +25%/lvl holiness gain
//             24 Faithful Hearts     +25%/lvl faith production
//             25 Ritual Power        +2/lvl ritual power (the ritual production multiplier is 5 + power)
//             26 Ritual Echo         +2/lvl ritual automation (more rituals in a row)
//             27 Apotheosis          +20%/lvl production of ALL resources while a ritual is running

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// effective level of perk k (0 when the Fame Shop switch is off)
string F(int k) => "(global.fs_lv[" + k + "] * global.qol_fame)";

// ---- 0-3, 6, 24: production. Just before the per-step income is added, so the top bar, rituals and offline
//      progress all see it. d_gold is also the "gold bonus %" used by the abdication payout, so it helps there too.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "var SP = 30;",
    "d_zern *= ((1 + (0.25 * " + F(0) + ")) * (1 + (0.12 * " + F(6) + ")));\n" +
    "d_les *= ((1 + (0.25 * " + F(1) + ")) * (1 + (0.12 * " + F(6) + ")));\n" +
    "d_kam *= ((1 + (0.25 * " + F(2) + ")) * (1 + (0.12 * " + F(6) + ")));\n" +
    "d_ver *= ((1 + (0.25 * " + F(24) + ")) * (1 + (0.12 * " + F(6) + ")));\n" +
    "d_gold *= (1 + (0.3 * " + F(3) + "));\n" +
    "var SP = 30;");

// ---- 27: Apotheosis, inside the "ritual is running" block (right after the ritual timer ticks).
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "o_time -= ti;",
    "o_time -= ti;\n" +
    "d_zern *= (1 + (0.2 * " + F(27) + "));\n" +
    "d_les *= (1 + (0.2 * " + F(27) + "));\n" +
    "d_kam *= (1 + (0.2 * " + F(27) + "));\n" +
    "d_ver *= (1 + (0.2 * " + F(27) + "));");

// ---- 5: tile cost (shown in the tooltip and charged by the same variable).
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "kle_cost2 = kle_cost;",
    "kle_cost2 = kle_cost * (1 - (0.12 * " + F(5) + "));");

// ---- 4: abdication gold (the tooltip and the real payout both use get_compen).
importGroup.QueueFindReplace(
    "gml_Script_get_compen",
    "return floor(d / 10);",
    "d *= (1 + (0.25 * " + F(4) + "));\nreturn floor(d / 10);");

// ---- 23: holiness. holy_get is rebuilt every step, so scaling it here cannot compound; only gains are scaled.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "if (holy_get > 0)",
    "if (holy_get > 0)\n{\n    holy_get *= (1 + (0.25 * " + F(23) + "));\n}\nif (holy_get > 0)");

// ---- 25, 26: ritual power (rit_SILA) and ritual automation (rit_power), both rebuilt every step.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "rit_SILA = MyRel(2, 2)",
    "rit_SILA = (2 * " + F(25) + ") + MyRel(2, 2)");
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "+ (2 * MyTri(2, 1)) + (main.muta[2] * 5);",
    "+ (2 * MyTri(2, 1)) + (main.muta[2] * 5) + (2 * " + F(26) + ");");

// ---- 7: hiring speed. Base hiring time of the live hiring code (Land step) and of the ritual auto-hire (HireRabs).
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs" })
{
    importGroup.QueueFindReplace(entry,
        "nspd = Var.rtime[r_i] * power(1.1, rab);",
        "nspd = Var.rtime[r_i] * power(1.1, rab) / (1 + (0.3 * " + F(7) + "));");
    importGroup.QueueFindReplace(entry,
        "nspd = Var.rtime[r_i] * power(1.05, rab);",
        "nspd = Var.rtime[r_i] * power(1.05, rab) / (1 + (0.3 * " + F(7) + "));");
}

// ---- 8, 12: worker hiring cost (getCost is used by both the live and the ritual hiring code).
//      The cost grows by 15% per worker; Economies of Scale lowers that growth, which matters more the more workers you have.
importGroup.QueueFindReplace(
    "gml_Script_getCost",
    "var n = argument0 * power(1.15, argument1 - argument2);",
    "var n = argument0 * power(1.15 - (0.006 * " + F(12) + "), argument1 - argument2) * (1 - (0.1 * " + F(8) + "));");

// ---- 11, 13: extra workers per hire. A hire completes in three places (live, ritual auto-hire, offline catch-up).
//      Only production workers get the bonus: buildings 1-6 and 9; not the hero (7), training (8), rest (10/11) or forge (12).
string massHire =
    "rab += 1;\n" +
    "if (BLD < 7 || BLD == 9)\n" +
    "{\n" +
    "    rab += (" + F(11) + " + " + F(13) + ");\n" +
    "}";
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs", "gml_Script_Forw" })
    importGroup.QueueFindReplace(entry, "rab += 1;", massHire);

// ---- 9, 10: starting resources of every new game (loading a fresh game, and the values written at abdication).
importGroup.QueueFindReplace("gml_Object_main_Alarm_2", "+ main.pps_res)", "+ main.pps_res + (1000 * " + F(9) + "))");
importGroup.QueueFindReplace("gml_Object_main_Alarm_2", "+ main.pps_ver)", "+ main.pps_ver + (1000 * " + F(10) + "))");
importGroup.QueueFindReplace("gml_Script__tri_cave", "+ main.pps_res)", "+ main.pps_res + (1000 * " + F(9) + "))");
importGroup.QueueFindReplace("gml_Script__tri_cave", "+ main.pps_ver)", "+ main.pps_ver + (1000 * " + F(10) + "))");

// ---- 14: monsters needed per tile battle (every place that computes need_kills, so the value stays consistent).
string kills80  = "need_kills = 200 + (200 * global.battled) + (80 * sqr(global.battled));";
string kills100 = "need_kills = 200 + (200 * global.battled) + (100 * sqr(global.battled));";
string Kills(string original) => "need_kills = ceil((" + original.Substring("need_kills = ".Length).TrimEnd(';') + ") * (1 - (0.08 * " + F(14) + ")));";
foreach (string entry in new[] { "gml_Object_LandBattle_Create_0", "gml_Object_LandBattle_Other_10" })
{
    importGroup.QueueFindReplace(entry, kills80, Kills(kills80));
    importGroup.QueueFindReplace(entry, kills100, Kills(kills100));
}
importGroup.QueueFindReplace("gml_Script_empire_pick_s", kills80, Kills(kills80));

// ---- 15, 20: hero damage. Applied where the monster takes the hit, and in the fast-fights chain check (03_fast_fights.csx)
//      so that "can the hero one-shot the next monster" uses the same number.
string heroDmg = "(1 + (0.25 * " + F(15) + ") + (0.15 * " + F(20) + "))";
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "vrag_HP -= O.hero_DAM;",
    "vrag_HP -= ceil(O.hero_DAM * " + heroDmg + ");");
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Alarm_0",
    "var _heal = 0;",
    "_dm = ceil(_dm * " + heroDmg + ");\nvar _heal = 0;");

// ---- 16, 20: hero experience per kill.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "* (1 + (3 * main.muta[9]));",
    "* (1 + (3 * main.muta[9])) * (1 + (0.25 * " + F(16) + ") + (0.15 * " + F(20) + "));");

// ---- 17: damage taken (both the shield branch and the HP branch compute "dam" from the monster's damage).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "var dam = vrag_DAM;",
    "var dam = ceil(vrag_DAM * (1 - (0.1 * " + F(17) + ")));");

// ---- 19: dodge chance.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "((O.hero_UKL - UST) * uklon_2)",
    "((O.hero_UKL - UST + (4 * " + F(19) + ")) * uklon_2)");

// ---- 18: attack speed. The fight timer (normal and the amber speed-up variant).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Alarm_0",
    "alarm[0] = max(2, 20 / kd);",
    "alarm[0] = max(2, 20 / (kd * (1 + (0.2 * " + F(18) + "))));");
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Alarm_0",
    "alarm[0] = max(3, 30 / kd);",
    "alarm[0] = max(3, 30 / (kd * (1 + (0.2 * " + F(18) + "))));");

// ---- 21: ritual duration.
importGroup.QueueFindReplace(
    "gml_Object_HRit_Other_10",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2));",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2)) + (10 * " + F(21) + ");");

// ---- 22: ritual cost, in the ritual window (this ritual, the next one, and the multi-cast total) and in tmp_rit_cost.
string cheaper = "((100 - (10 * " + F(22) + ")) * rk)";
string cheaper2 = "((100 - (10 * " + F(22) + ")) * rk2)";
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "(100 * rk)", cheaper);
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "(100 * rk2)", cheaper2);
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", "(100 * rk)", cheaper);

importGroup.Import();
