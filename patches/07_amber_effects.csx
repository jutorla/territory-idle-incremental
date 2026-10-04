// QoL: "Amber effects" ON/OFF switch (Options > QoL Features). Default ON = the game exactly as before.
//
// When OFF, the LASTING effects of the Amber Shop stop applying. Nothing is deleted or refunded: your Amber balance and
// every purchase stay as they are, and switching ON again brings the effects back. The Amber Shop's own code (items,
// prices, purchase handling) is not changed; only the places where the game READS those effects are gated by
// global.qol_amb (0 or 1):
//
//   Permanent upgrades window
//     +25% wheat / wood / stone / faith production   main.pp_zern / pp_les / pp_kam / pp_ver  (income, and the tile tooltip)
//     +1000 starting wheat, wood, stone              main.pps_res  (new game start and the values written at abdication)
//     +1000 starting faith                           main.pps_ver
//     99 workers when a building is built            main.pp_99
//   One-battle boosts (Amber Shop)
//     1.5x battle speed until the tile is captured   global.S_BTL  (not consumed while OFF)
//     double hero stats for the next battle          global.D_BTL  (not consumed while OFF)
//
// Not switched: one-off items (timelapse, gold for abdication, pantheon to heritage) have no lasting effect, and
// mutations that were already chosen stay (the extra mutation point is left alone).
// Things already granted stay too (for example workers that a building already received).

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// 1) Setting, read from the "opt" ini ([QOL] amb, default ON) at the very top of main Create.
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    @"VERSION = 167;
if (!variable_global_exists(""qol_amb""))
{
    ini_open(""opt"");
    global.qol_amb = ini_read_real(""QOL"", ""amb"", 1);
    ini_close();
}");

// 2) Permanent production bonuses: every place that READS them (income per step, the tile tooltip, the extra income
//    lines in main Step). The Amber Shop's purchase code writes them and is not touched.
foreach (string v in new[] { "pp_zern", "pp_les", "pp_kam" })
{
    importGroup.QueueFindReplace("gml_Object_Land_Step_0", "main." + v, "(main." + v + " * global.qol_amb)");
    importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "main." + v, "(main." + v + " * global.qol_amb)");
    importGroup.QueueFindReplace("gml_Object_main_Step_0", "main." + v, "(main." + v + " * global.qol_amb)");
}
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "main.pp_ver", "(main.pp_ver * global.qol_amb)");

// 3) 99 workers on construction.
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "main.pp_99", "(main.pp_99 * global.qol_amb)");

// 4) Starting resources and faith: new-game defaults (main Alarm 2) and the values written at abdication (_tri_cave).
foreach (string entry in new[] { "gml_Object_main_Alarm_2", "gml_Script__tri_cave" })
{
    importGroup.QueueFindReplace(entry, "+ main.pps_res)", "+ (main.pps_res * global.qol_amb))");
    importGroup.QueueFindReplace(entry, "+ main.pps_ver)", "+ (main.pps_ver * global.qol_amb))");
}

// 5) One-battle boosts. S_BTL: the speed-up (fight timer, offline fight steps, the hero animation speed) and the reset
//    when the tile is captured (it is only consumed while the effect is ON). D_BTL: the check that doubles the hero
//    stats also consumes the boost, so gating the check keeps it for later.
importGroup.QueueFindReplace("gml_Object_LandBattle_Alarm_0", "if (global.S_BTL)", "if (global.S_BTL * global.qol_amb)");
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_20", "if (global.S_BTL)", "if (global.S_BTL * global.qol_amb)");
importGroup.QueueFindReplace("gml_Object_LandBattle_Draw_0", "(0.025 * global.S_BTL)", "(0.025 * global.S_BTL * global.qol_amb)");
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "global.S_BTL = 0;", "if (global.qol_amb)\n{\n    global.S_BTL = 0;\n}");
importGroup.QueueFindReplace("gml_Script_AfterShlem", "if (global.D_BTL > 0)", "if (global.D_BTL > 0 && global.qol_amb)");

importGroup.Import();
