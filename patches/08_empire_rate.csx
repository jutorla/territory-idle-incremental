// QoL: "5-tile Empire points" ON/OFF switch (Options > QoL Features). Default OFF = the game exactly as before.
//
// Empire points are normally given for every 15 tiles. When ON, they are given for every 5 tiles instead.
//
// How the game works: the points are not stored. After every load, main Step walks the counter up one step at a time:
//   if (tile_num + empire_ppp >= empire_progress) { empire_points += 1 + PLANET; empire_progress += 15; }
// where tile_num = tiles on this continent and empire_ppp = tiles from earlier continents (this is what the mod calls
// Fame). So the number of points you have is always floor((tile_num + empire_ppp) / 15) * (1 + PLANET), and the points
// you spent are saved separately (emp_c), so the rate can be changed at any time without touching the save:
//
//   main Create   empire_progress starts at the rate (the first point comes after 5 tiles instead of 15)
//   main Step     the step added to empire_progress is the rate; "if" becomes "while" so that after a load (or after
//                 switching the option) all points are caught up in one frame instead of one point per frame
//   main Step     can_empire (the Empire panel appears) needs the first point's worth of tiles, so 5 instead of 15
//   texts         "per each 15 tiles" in the Empire panel and in the first-point popup show the real number
//
// Switching in the menu (04_qol_menu.csx) recounts the points at once. Switching OFF after spending more points than
// the 15-tile rate gives would leave the player overspent, so in that case the Empire perks are reset for free (no
// reset charge, no Amber) with the same save + restart that the Empire window's own reset uses.
//
// Not changed: Fame, the Fame Shop, and the "Progress" counter (it simply counts to the next point).

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// Tiles per Empire point: 15 normally, 5 when global.qol_emp5 is 1.
const string RATE = "(15 - (10 * global.qol_emp5))";

// 1) Setting, read from the "opt" ini ([QOL] emp5, default OFF) at the very top of main Create.
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    @"VERSION = 167;
if (!variable_global_exists(""qol_emp5""))
{
    ini_open(""opt"");
    global.qol_emp5 = ini_read_real(""QOL"", ""emp5"", 0);
    ini_close();
}");

// 2) The counter starts at the rate.
importGroup.QueueFindReplace("gml_Object_main_Create_0", "empire_progress = 15;", "empire_progress = " + RATE + ";");

// 3) Step: panel unlock, catch-up loop and the step of the counter.
importGroup.QueueFindReplace("gml_Object_main_Step_0", "if (tile_num >= 15 || CONT)", "if (tile_num >= " + RATE + " || CONT)");
importGroup.QueueFindReplace("gml_Object_main_Step_0", "if ((tile_num + empire_ppp) >= empire_progress)", "while ((tile_num + empire_ppp) >= empire_progress)");
importGroup.QueueFindReplace("gml_Object_main_Step_0", "empire_progress += 15;", "empire_progress += " + RATE + ";");

// 4) Texts: "per each 15 tiles" (Empire panel, both languages and both the Earth and other-planet versions in main
//    Draw GUI, and the first-point popup in main Step).
foreach (string entry in new[] { "gml_Object_main_Draw_64", "gml_Object_main_Step_0" })
{
    importGroup.QueueFindReplace(entry, "15 клеток", "\" + string" + RATE + " + \" клеток");
    importGroup.QueueFindReplace(entry, "15 tiles", "\" + string" + RATE + " + \" tiles");
}

importGroup.Import();
