// QoL: Amber upgrades ON/OFF, one switch per item (Options > QoL Features > Amber upgrades...). Default ON = the game exactly as before.
//
// When an item is OFF, its LASTING effect stops applying. Nothing is deleted or refunded: your Amber balance and every
// purchase stay as they are, and switching ON again brings the effect back. The Amber Shop's own code (items, prices,
// purchase handling) is not changed; only the places where the game READS those effects are gated by global.qol_ab[k]
// (0 or 1):
//
//   k  item                                          where the game reads it
//   0  +25% wheat production                         main.pp_zern  (income, and the tile tooltip)
//   1  +25% wood production                          main.pp_les
//   2  +25% stone production                         main.pp_kam
//   3  +25% faith production                         main.pp_ver
//   4  +1000 starting wheat, wood, stone             main.pps_res  (new game start and the values written at abdication)
//   5  +1000 starting faith                          main.pps_ver
//   6  99 workers when a building is built           main.pp_99
//   7  1.5x battle speed until the tile is captured  global.S_BTL  (not consumed while OFF)
//   8  double hero stats for the next battle         global.D_BTL  (not consumed while OFF)
//
// Not switched: one-off items (timelapse, gold for abdication, pantheon to heritage) have no lasting effect, and
// mutations that were already chosen stay (the extra mutation point is left alone).
// Things already granted stay too (for example workers that a building already received).
//
// Settings: the "opt" ini, [QOL] ab0..ab8. Before 1.6.0 there was a single switch [QOL] amb; it is used as the default
// of every item, so a player who had it OFF keeps everything OFF.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string AB(int k) => "global.qol_ab[" + k + "]";

// 1) Settings, read from the "opt" ini at the very top of main Create.
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    @"VERSION = 167;
if (!variable_global_exists(""qol_ab""))
{
    ini_open(""opt"");
    var _abold = ini_read_real(""QOL"", ""amb"", 1);
    for (var _abi = 0; _abi < 9; _abi++)
    {
        global.qol_ab[_abi] = ini_read_real(""QOL"", ""ab"" + string(_abi), _abold);
    }
    ini_close();
}");

// 2) Permanent production bonuses: every place that READS them (income per step, the tile tooltip, the extra income
//    lines in main Step). The Amber Shop's purchase code writes them and is not touched.
string[] prod = { "pp_zern", "pp_les", "pp_kam" };
for (int k = 0; k < prod.Length; k++)
{
    string v = prod[k];
    importGroup.QueueFindReplace("gml_Object_Land_Step_0", "main." + v, "(main." + v + " * " + AB(k) + ")");
    importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "main." + v, "(main." + v + " * " + AB(k) + ")");
    importGroup.QueueFindReplace("gml_Object_main_Step_0", "main." + v, "(main." + v + " * " + AB(k) + ")");
}
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "main.pp_ver", "(main.pp_ver * " + AB(3) + ")");

// 3) 99 workers on construction.
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "main.pp_99", "(main.pp_99 * " + AB(6) + ")");

// 4) Starting resources and faith: new-game defaults (main Alarm 2) and the values written at abdication (_tri_cave).
foreach (string entry in new[] { "gml_Object_main_Alarm_2", "gml_Script__tri_cave" })
{
    importGroup.QueueFindReplace(entry, "+ main.pps_res)", "+ (main.pps_res * " + AB(4) + "))");
    importGroup.QueueFindReplace(entry, "+ main.pps_ver)", "+ (main.pps_ver * " + AB(5) + "))");
}

// 5) One-battle boosts. S_BTL: the speed-up (fight timer, offline fight steps, the hero animation speed) and the reset
//    when the tile is captured (it is only consumed while the effect is ON). D_BTL: the check that doubles the hero
//    stats also consumes the boost, so gating the check keeps it for later.
importGroup.QueueFindReplace("gml_Object_LandBattle_Alarm_0", "if (global.S_BTL)", "if (global.S_BTL * " + AB(7) + ")");
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_20", "if (global.S_BTL)", "if (global.S_BTL * " + AB(7) + ")");
importGroup.QueueFindReplace("gml_Object_LandBattle_Draw_0", "(0.025 * global.S_BTL)", "(0.025 * global.S_BTL * " + AB(7) + ")");
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "global.S_BTL = 0;", "if (" + AB(7) + ")\n{\n    global.S_BTL = 0;\n}");
importGroup.QueueFindReplace("gml_Script_AfterShlem", "if (global.D_BTL > 0)", "if (global.D_BTL > 0 && " + AB(8) + ")");

importGroup.Import();
