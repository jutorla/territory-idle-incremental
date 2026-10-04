// QoL: effects of the Fame Shop perks (32 perks, 4 branches). Each hook multiplies by
// (global.fs_lv[k] * global.qol_fame), so with the "Fame Shop" switch OFF every perk is inactive (purchases are kept).
// Perk index k = branch * 8 + slot. Max level 5 unless noted. Level L costs base * L perk points,
// base 5 / 8 / 12 / 16 / 40 for root / first / second / third step / keystone.
//
// Design rules (see FAME_SHOP_V2_PROPOSAL.md): no new automation, and nothing that duplicates an Amber Shop item
// (timelapse, pantheon->heritage, double hero stats, 1.5x battle speed, permanent +25% production, starting
// resources, 99 workers on construction, extra mutation). The Amber Shop's own code is not touched; in particular the
// shared get_compen() script is NOT modified, because the paid "gold for abdication" item uses it.
//
//  INDUSTRY    0 Quick Hands          +25%/lvl worker hiring speed
//              1 Fair Wages           -10%/lvl cost of hiring workers
//              2 Economies of Scale   -0.6%/lvl growth of the cost of each next worker (base 1.15 -> 1.12)
//              3 Open Enrollment      +1 extra worker per hire per lvl (max 3; production buildings only)
//              4 Master Craftsmen     +3%/lvl output of a building per 20 workers in it (up to 200)
//              5 Imperial Ambition    +0.4%/lvl total production per tile owned
//              6 Renowned Workshops   +0.8%/lvl * sqrt(Fame) total production
//              7 Foremen (keystone)   1%/lvl of wheat+wood+stone+faith income is added to gold income (max 3)
//  CONQUEST    8 Surveyors' Guild     new-tile price growth x10 -> x(10 - 0.5/lvl)
//              9 Veteran's Resolve    -8%/lvl monsters needed per tile battle
//             10 Battle Wisdom        +25%/lvl hero experience
//             11 Plunder              each monster killed gives 2%/lvl of one second of wheat/wood/stone/faith income
//             12 Phoenix              +1 hero revive per battle per lvl (max 3)
//             13 Spoils of War        conquering a tile pays 20%/lvl of the gold price of the next tile
//             14 Land Deeds           buying a tile refunds 10%/lvl of its gold price
//             15 Cleave (keystone)    each kill counts as +1 extra kill per lvl toward the tile (max 2 = x3)
//  DEVOTION   16 Long Rituals         +10 s/lvl ritual duration
//             17 Frugal Rites         -10%/lvl ritual cost
//             18 Ritual Power         +2/lvl ritual power (ritual production multiplier is 5 + power)
//             19 Apotheosis           +20%/lvl production of ALL resources while a ritual is running
//             20 Devout Scholars      -10%/lvl holiness cost of religion perks
//             21 Divine Favor         +25%/lvl holiness gain
//             22 Ritual Devotion      +0.1%/lvl total production per ritual performed (up to 200)
//             23 Timeless Rites (key) -25%/lvl of the cost increase every ritual adds (max 4 = cost never rises)
//  LEGACY     24 Royal Treasury       +25%/lvl gold when abdicating
//             25 Heirloom             keep 8%/lvl of wheat, wood, stone and faith when abdicating (max 40%)
//             26 Blueprints           restore buildings after abdication: price 5M / 2M / 500K / 100K / free
//             27 Continental Spirit   +3%/lvl total production per continent visited
//             28 Shipwrights' Guild   -10%/lvl ships needed to sail away
//             29 Famous Voyage        +10%/lvl Fame when sailing away (max 3)
//             30 Merchant Fleet       +0.01%/lvl total production per ship built
//             31 Legend of Fame (key) each Fame point gives +2%/lvl more abdication gold (base is +3% per point) (max 3)

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// effective level of perk k (0 when the Fame Shop switch is off)
string F(int k) => "(global.fs_lv[" + k + "] * global.qol_fame)";

// ======================================================================================================
// INDUSTRY
// ======================================================================================================

// ---- 0: hiring speed. Base hiring time of the live hiring code (Land step) and of the ritual auto-hire (HireRabs).
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs" })
{
    importGroup.QueueFindReplace(entry,
        "nspd = Var.rtime[r_i] * power(1.1, rab);",
        "nspd = Var.rtime[r_i] * power(1.1, rab) / (1 + (0.25 * " + F(0) + "));");
    importGroup.QueueFindReplace(entry,
        "nspd = Var.rtime[r_i] * power(1.05, rab);",
        "nspd = Var.rtime[r_i] * power(1.05, rab) / (1 + (0.25 * " + F(0) + "));");
}

// ---- 1, 2: worker hiring cost (getCost is used by both the live and the ritual hiring code).
//      The cost grows by 15% per worker; Economies of Scale lowers that growth, which matters more the more workers you have.
importGroup.QueueFindReplace(
    "gml_Script_getCost",
    "var n = argument0 * power(1.15, argument1 - argument2);",
    "var n = argument0 * power(1.15 - (0.006 * " + F(2) + "), argument1 - argument2) * (1 - (0.1 * " + F(1) + "));");

// ---- 3: extra workers per hire. A hire completes in three places (live, ritual auto-hire, offline catch-up).
//      Only production workers get the bonus: buildings 1-6 and 9; not the hero (7), training (8), rest (10/11) or forge (12).
string massHire =
    "rab += 1;\n" +
    "if (BLD < 7 || BLD == 9)\n" +
    "{\n" +
    "    rab += " + F(3) + ";\n" +
    "}";
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs", "gml_Script_Forw" })
    importGroup.QueueFindReplace(entry, "rab += 1;", massHire);

// ---- 4: Master Craftsmen. The four income lines of a building all start with "doh = rab * (".
importGroup.QueueFindReplace(
    "gml_Object_Land_Step_0",
    "doh = rab * (",
    "doh = rab * (1 + (0.03 * " + F(4) + " * min(10, floor(rab / 20)))) * (");

// ---- 5, 6, 22, 27, 30: production that scales with progress, and 7: Foremen. Just before the per-step income is added,
//      so the top bar, rituals and offline progress all see it. The five scaling perks are added together into one bonus.
//      Foremen is NOT put into d_gold: d_gold is also used as a percentage by the abdication payout. It has its own
//      variable (d_fgold, gold per second) which Step and Forw both add to the gold.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "var SP = 30;",
    "var _fsb = (0.004 * " + F(5) + " * tile_num) + (0.008 * " + F(6) + " * sqrt(empire_ppp)) + (0.03 * " + F(27) + " * CONT) + (0.0001 * " + F(30) + " * Var.rcnt[8]) + (0.001 * " + F(22) + " * min(o_cnt, 200));\n" +
    "d_zern *= (1 + _fsb);\n" +
    "d_les *= (1 + _fsb);\n" +
    "d_kam *= (1 + _fsb);\n" +
    "d_ver *= (1 + _fsb);\n" +
    "d_fgold = 0.01 * " + F(7) + " * (d_zern + d_les + d_kam + d_ver);\n" +
    "var SP = 30;");
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "global.gold += (d_gold / SP);",
    "global.gold += (d_gold / SP);\nglobal.gold += (d_fgold / SP);");
// timelapses, offline progress and the instant ritual finish go through Forw, which adds d_gold itself
importGroup.QueueFindReplace("gml_Script_Forw", "if (d_gold)", "if (d_gold + d_fgold)");
importGroup.QueueFindReplace("gml_Script_Forw", "(d_gold * om)", "((d_gold + d_fgold) * om)");
importGroup.QueueFindReplace("gml_Script_Forw", "(d_gold * delta)", "((d_gold + d_fgold) * delta)");

// ======================================================================================================
// CONQUEST
// ======================================================================================================

// ---- 8: new-tile price growth (price at load, after every purchase, and after an Empire perk recalculates it).
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "kle_cost = power(10, global.klet - global.battled - global.klet_vera);",
    "kle_cost = power(10 - (0.5 * " + F(8) + "), global.klet - global.battled - global.klet_vera);");
importGroup.QueueFindReplace("gml_Object_kle_b_Mouse_4", "main.kle_cost *= 10;", "main.kle_cost *= (10 - (0.5 * " + F(8) + "));");
importGroup.QueueFindReplace("gml_Object_kle_b_Mouse_4", "main.kle_cost *= 9;", "main.kle_cost *= (9 - (0.5 * " + F(8) + "));");
importGroup.QueueFindReplace(
    "gml_Script_empire_pick_s",
    "main.kle_cost = power(9, global.klet - global.battled - global.klet_vera);",
    "main.kle_cost = power(9 - (0.5 * " + F(8) + "), global.klet - global.battled - global.klet_vera);");

// ---- 14: Land Deeds. Part of the tile price comes back when you buy a tile.
importGroup.QueueFindReplace(
    "gml_Object_kle_b_Mouse_4",
    "global.gold -= main.kle_cost2;",
    "global.gold -= main.kle_cost2;\nglobal.gold += (main.kle_cost2 * 0.1 * " + F(14) + ");");

// ---- 9: monsters needed per tile battle (every place that computes need_kills, so the value stays consistent).
string kills80  = "need_kills = 200 + (200 * global.battled) + (80 * sqr(global.battled));";
string kills100 = "need_kills = 200 + (200 * global.battled) + (100 * sqr(global.battled));";
string Kills(string original) => "need_kills = ceil((" + original.Substring("need_kills = ".Length).TrimEnd(';') + ") * (1 - (0.08 * " + F(9) + ")));";
foreach (string entry in new[] { "gml_Object_LandBattle_Create_0", "gml_Object_LandBattle_Other_10" })
{
    importGroup.QueueFindReplace(entry, kills80, Kills(kills80));
    importGroup.QueueFindReplace(entry, kills100, Kills(kills100));
}
importGroup.QueueFindReplace("gml_Script_empire_pick_s", kills80, Kills(kills80));

// ---- 10: hero experience per kill.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "* (1 + (3 * main.muta[9]));",
    "* (1 + (3 * main.muta[9])) * (1 + (0.25 * " + F(10) + "));");

// ---- 15 (Cleave) and 11 (Plunder): the kill counter. A kill counts as 1 + Cleave kills; Plunder pays resources per kill.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "kills++;",
    "kills += (1 + " + F(15) + ");\n" +
    "if (" + F(11) + " > 0)\n" +
    "{\n" +
    "    main.zern += (main.d_zern * 0.02 * " + F(11) + ");\n" +
    "    main.les += (main.d_les * 0.02 * " + F(11) + ");\n" +
    "    main.kam += (main.d_kam * 0.02 * " + F(11) + ");\n" +
    "    main.ver += (main.d_ver * 0.02 * " + F(11) + ");\n" +
    "}");

// ---- 12: Phoenix, extra revives per battle (the game compares the revive allowance with the revives already used).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "(MyRel(3, 3) + obor) > revive",
    "(MyRel(3, 3) + obor + " + F(12) + ") > revive");

// ---- 13: Spoils of War. When a tile is won, the next tile's price (main.kle_cost) is partly paid in gold.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "global.battled++;",
    "global.battled++;\nglobal.gold += (main.kle_cost * 0.2 * " + F(13) + ");");

// ======================================================================================================
// DEVOTION
// ======================================================================================================

// ---- 16: ritual duration.
importGroup.QueueFindReplace(
    "gml_Object_HRit_Other_10",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2));",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2)) + (10 * " + F(16) + ");");

// ---- 17: ritual cost, in the ritual window (this ritual, the next one, and the multi-cast total) and in tmp_rit_cost.
string cheaper = "((100 - (10 * " + F(17) + ")) * rk)";
string cheaper2 = "((100 - (10 * " + F(17) + ")) * rk2)";
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "(100 * rk)", cheaper);
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "(100 * rk2)", cheaper2);
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", "(100 * rk)", cheaper);

// ---- 23: Timeless Rites. The cost of a ritual grows with the number of rituals performed (cnt); the perk scales that
//      count down, so at level 4 every ritual costs the base price. Same places as the cost above.
string slow = "(1 - (0.25 * " + F(23) + "))";
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "var cnt = main.o_cnt - main.o_pseudo;", "var cnt = (main.o_cnt - main.o_pseudo) * " + slow + ";");
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "var rk2 = 1 + sqr(cnt + 1);", "var rk2 = 1 + sqr(cnt + " + slow + ");");
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "rk2 += (1 + cnt + 1);", "rk2 += (1 + cnt + " + slow + ");");
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "cnt = (main.o_cnt - main.o_pseudo) + u;", "cnt = ((main.o_cnt - main.o_pseudo) + u) * " + slow + ";");
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", "var cnt = main.o_cnt - main.o_pseudo;", "var cnt = (main.o_cnt - main.o_pseudo) * " + slow + ";");

// ---- 18: ritual power (rit_SILA), rebuilt every step.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "rit_SILA = MyRel(2, 2)",
    "rit_SILA = (2 * " + F(18) + ") + MyRel(2, 2)");

// ---- 19: Apotheosis, inside the "ritual is running" block (right after the ritual timer ticks).
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "o_time -= ti;",
    "o_time -= ti;\n" +
    "d_zern *= (1 + (0.2 * " + F(19) + "));\n" +
    "d_les *= (1 + (0.2 * " + F(19) + "));\n" +
    "d_kam *= (1 + (0.2 * " + F(19) + "));\n" +
    "d_ver *= (1 + (0.2 * " + F(19) + "));");

// ---- 20: Devout Scholars. Religion perks (and the servant perks) cost holiness; cheaper in the tooltip and when bought.
importGroup.QueueFindReplace("gml_Object_HRel_Step_0", "if (bR1)", "k *= (1 - (0.1 * " + F(20) + "));\nif (bR1)");
importGroup.QueueFindReplace("gml_Object_HRel_Step_0", "if (bsR1)", "sk *= (1 - (0.1 * " + F(20) + "));\nif (bsR1)");
importGroup.QueueFindReplace("gml_Object_HRel_Other_12", "if (bR1 && main.holy >= (cost1[G] * k))", "k *= (1 - (0.1 * " + F(20) + "));\nif (bR1 && main.holy >= (cost1[G] * k))");
importGroup.QueueFindReplace("gml_Object_HRel_Other_12", "if (bsR1 && main.holy >= (h_cost[main.SLUGA, 0] * sk))", "sk *= (1 - (0.1 * " + F(20) + "));\nif (bsR1 && main.holy >= (h_cost[main.SLUGA, 0] * sk))");

// ---- 21: holiness. holy_get is rebuilt every step, so scaling it here cannot compound; only gains are scaled.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "if (holy_get > 0)",
    "if (holy_get > 0)\n{\n    holy_get *= (1 + (0.25 * " + F(21) + "));\n}\nif (holy_get > 0)");

// ======================================================================================================
// LEGACY
// ======================================================================================================

// ---- 24 (Royal Treasury) and 31 (Legend of Fame): the abdication payout, at the REAL abdication call site only.
//      get_compen() itself stays untouched because the Amber Shop's paid "gold for abdication" item also calls it.
//      Legend raises the per-Fame bonus from (0.03 + relic) to (0.03 + relic + 0.02 * level): the payout is scaled by the ratio.
string baseFame = "(1 + ((0.03 + (MyRel(4, 2) * 0.01)) * empire_ppp))";
string legendFame = "(1 + (((0.03 + (MyRel(4, 2) * 0.01)) + (0.02 * " + F(31) + ")) * empire_ppp))";
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "var co = get_compen();",
    "var co = get_compen();\nco = floor(co * (1 + (0.25 * " + F(24) + ")) * (" + legendFame + " / " + baseFame + "));");
// show the new per-Fame bonus in the abdication tooltip and the top bar
importGroup.QueueFindReplace("gml_Object_main_Step_0", "(3 + MyRel(4, 2))", "(3 + MyRel(4, 2) + (2 * " + F(31) + "))");
importGroup.QueueFindReplace("gml_Object_main_Draw_64", "(3 + MyRel(4, 2))", "(3 + MyRel(4, 2) + (2 * " + F(31) + "))");

// ---- 29: Famous Voyage. More Fame when sailing away (and the same number in the sail-away tooltip).
string fameGain = "ceil(tile_num * (1 + (0.1 * " + F(29) + ")))";
importGroup.QueueFindReplace("gml_Object_main_Alarm_11", "empire_ppp += tile_num;", "empire_ppp += " + fameGain + ";");
importGroup.QueueFindReplace("gml_Object_main_Step_0", "string(tile_num) + \" славы, что даст", "string(" + fameGain + ") + \" славы, что даст");
importGroup.QueueFindReplace("gml_Object_main_Step_0", "string(tile_num) + \" fame, which will give", "string(" + fameGain + ") + \" fame, which will give");
importGroup.QueueFindReplace("gml_Object_main_Step_0", "string(tile_num * 3)", "string(" + fameGain + " * 3)");

// ---- 28: Shipwrights' Guild. Ships needed to sail away.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "var need_ships = 500 + min(1000, CONT * 25);",
    "var need_ships = ceil((500 + min(1000, CONT * 25)) * (1 - (0.1 * " + F(28) + ")));");

// ---- 25: Heirloom. On every abdication, keep a share of the stock. _tri_cave() (called right before the restart)
//      writes the start values of the next game into "game1"; we add our share on top of whatever it wrote
//      (or of the normal start value). Runs inside the abdication code, where self is main.
string heirloom =
    "_tri_cave();\n" +
    "if (global.qol_fame * global.fs_lv[25] > 0)\n" +
    "{\n" +
    "    var _hk = 0.08 * global.fs_lv[25];\n" +
    "    ini_open(\"game1\");\n" +
    "    ini_write_string(\"RES\", \"zern\", string(ini_read_real(\"RES\", \"zern\", 100 + (500 * RELIC[3]) + pps_res) + (zern * _hk)));\n" +
    "    ini_write_string(\"RES\", \"les\", string(ini_read_real(\"RES\", \"les\", (500 * RELIC[2]) + pps_res) + (les * _hk)));\n" +
    "    ini_write_string(\"RES\", \"kam\", string(ini_read_real(\"RES\", \"kam\", 0 + pps_res) + (kam * _hk)));\n" +
    "    ini_write_string(\"RES\", \"ver\", string(ini_read_real(\"RES\", \"ver\", 0 + pps_ver) + (ver * _hk)));\n" +
    "    ini_close();\n" +
    "}";
importGroup.QueueFindReplace("gml_Object_main_Alarm_11", "_tri_cave();", heirloom);

// ---- 26: Blueprints. The game's own "restore buildings after abdication" feature (an Empire perk, 5,000,000 gold) is
//      switched on by the perk too, with a price that drops with the level. The prompt only appears when a saved
//      building list ("nem") exists, so nobody pays for restoring nothing.
importGroup.QueueFindReplace("gml_Object_main_Alarm_11", "if (MyEmp(8, 4))", "if (MyEmp(8, 4) || (" + F(26) + " > 0))");
importGroup.QueueFindReplace("gml_Object_main_Alarm_2", "if (MyEmp(8, 4))", "if (MyEmp(8, 4) || (" + F(26) + " > 0))");
importGroup.QueueFindReplace("gml_Object_main_Alarm_2", "if (nflag)", "if (nflag && file_exists(\"nem\"))");
// (order matters: replace the literal 5000000 first, then insert the block that defines bpc with its own 5000000)
importGroup.QueueFindReplace("gml_Object_blu_re_Draw_64", "5000000", "bpc");
importGroup.QueueFindReplace(
    "gml_Object_blu_re_Draw_64",
    "g(\"Восстановить здания#(5 млн золота)\", \"Restore buildings#(5 million gold)\")",
    "g(\"Восстановить здания#(\" + kstr(bpc) + \" золота)\", \"Restore buildings#(\" + kstr(bpc) + \" gold)\")");
importGroup.QueueFindReplace(
    "gml_Object_blu_re_Draw_64",
    "a = 20;",
    "a = 20;\n" +
    "bpc = 5000000;\n" +
    "if (" + F(26) + " >= 2)\n{\n    bpc = 2000000;\n}\n" +
    "if (" + F(26) + " >= 3)\n{\n    bpc = 500000;\n}\n" +
    "if (" + F(26) + " >= 4)\n{\n    bpc = 100000;\n}\n" +
    "if (" + F(26) + " >= 5)\n{\n    bpc = 0;\n}");

importGroup.Import();
