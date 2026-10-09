// QoL: four more EMPIRE bonuses with game-changing perks (rows 11..14 of the Empire window; they are on its second tab, "Empires II",
// together with the Mongolian and Persian rows of 13_divine_extras.csx). Each row has five levels that cost 1, 2, 3, 4 and 10
// points, like every row of the game; the fifth level is the big one. Switch: global.qol_div ("Divine extras"): OFF hides the second
// tab and pauses the effects.
//
//   11  Babylonian   (production)
//       1  +50% to all production (wheat, wood, stone, faith)
//       2  -30% cost of all workers
//       3  Hanging Gardens: 25% of the wheat, wood and stone production is also added to each of the other two
//       4  gold for abdication x2
//       5  Tower of Babel: x3 to all production
//   12  Mayan        (rituals)
//       1  +5 ritual power
//       2  -50% ritual cost
//       3  Long Count: rituals last twice as long (the cost does not change)
//       4  +5 ritual automation
//       5  End of Days: rituals cost no faith at all
//   13  Viking       (ships and sailing)
//       1  Longships: ships produce twice as much wheat, wood, stone and gold
//       2  +10 gold per second from every ship
//       3  sailing away needs 50% fewer ships
//       4  Berserkers: heroes get +100% HP and damage
//       5  Ragnarok: sailing away gives double Fame
//   14  Indian       (workers)
//       1  every hire brings 2 extra workers
//       2  -50% hiring time in all buildings
//       3  worker cost grows 3% slower per worker (15% -> 12%)
//       4  every new building starts with 50 workers (not the heroic ones)
//       5  Maharaja: every hire brings 8 more extra workers (10 in total)
//
// The extra workers per hire use the game's own spot (the Fame Shop's Open Enrollment): they are given in the wheat field, forest
// camp, farm, quarry, temple, school and shipyard.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string G(string ru, string en) => "g(\"" + ru + "\", \"" + en + "\")";
const string DIV = "global.qol_div";

// ---------------------------------------------------------------------------------------------------------------------
// Names and level texts
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueFindReplace("gml_Object_main_Create_0", "emp_name[10] = \"Persian\";",
    "emp_name[10] = \"Persian\";\nemp_name[11] = \"Babylonian\";\nemp_name[12] = \"Mayan\";\nemp_name[13] = \"Viking\";\nemp_name[14] = \"Indian\";");
var emp = new (int row, int lvl, string ru, string en)[]
{
    (11, 0, "+50% ко всему производству#(зерно, дерево, камень, вера).", "+50% to all production#(wheat, wood, stone, faith)."),
    (11, 1, "-30% к стоимости всех рабочих.", "-30% cost of all workers."),
    (11, 2, "ВИСЯЧИЕ САДЫ#25% производства зерна, дерева и камня#также добавляется к каждому из двух других ресурсов.", "HANGING GARDENS#25% of the wheat, wood and stone production#is also added to each of the other two."),
    (11, 3, "Золото за отречение x2.", "Gold for abdication x2."),
    (11, 4, "ВАВИЛОНСКАЯ БАШНЯ#x3 ко всему производству#(зерно, дерево, камень, вера).", "TOWER OF BABEL#x3 to all production#(wheat, wood, stone, faith)."),
    (12, 0, "+5 к силе обряда.", "+5 ritual power."),
    (12, 1, "-50% к стоимости обряда.", "-50% ritual cost."),
    (12, 2, "ДОЛГИЙ СЧЕТ#Обряды длятся вдвое дольше#(стоимость не меняется).", "LONG COUNT#Rituals last twice as long#(the cost does not change)."),
    (12, 3, "+5 к автоматизации обрядов.", "+5 ritual automation."),
    (12, 4, "КОНЕЦ СВЕТА#Обряды не стоят веры.", "END OF DAYS#Rituals cost no faith at all."),
    (13, 0, "ДРАККАРЫ#Корабли производят вдвое больше#зерна, дерева, камня и золота.", "LONGSHIPS#Ships produce twice as much#wheat, wood, stone and gold."),
    (13, 1, "+10 золота в секунду от каждого корабля.", "+10 gold per second from every ship."),
    (13, 2, "Для отплытия нужно на 50% меньше кораблей.", "Sailing away needs 50% fewer ships."),
    (13, 3, "БЕРСЕРКИ#Герои получают +100% к HP и урону.", "BERSERKERS#Heroes get +100% HP and damage."),
    (13, 4, "РАГНАРЕК#Отплытие дает вдвое больше славы.", "RAGNAROK#Sailing away gives double Fame."),
    (14, 0, "Каждый найм дает 2 дополнительных рабочих.", "Every hire brings 2 extra workers."),
    (14, 1, "-50% ко времени найма во всех зданиях.", "-50% hiring time in all buildings."),
    (14, 2, "Стоимость рабочих растет медленнее:#на 3% меньше за каждого рабочего (15% -> 12%).", "Worker cost grows slower:#3% less per worker (15% -> 12%)."),
    (14, 3, "Каждое новое здание начинает с 50 рабочими#(кроме героических зданий).", "Every new building starts with 50 workers#(except the heroic buildings)."),
    (14, 4, "МАХАРАДЖА#Каждый найм дает еще 8 дополнительных рабочих#(всего 10).", "MAHARAJA#Every hire brings 8 more extra workers#(10 in total)."),
};
var sbE = new System.Text.StringBuilder();
foreach (var e in emp) sbE.Append("emp_info[" + e.row + ", " + e.lvl + "] = " + G(e.ru, e.en) + ";\n");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "txt_perk[1] = \"IRONMAN#+25% armor.#Can use golden items for free.\";",
    sbE.ToString() + "txt_perk[1] = \"IRONMAN#+25% armor.#Can use golden items for free.\";");

// ---------------------------------------------------------------------------------------------------------------------
// Production (main Step, in the gated block of the divine extras): Babylonian 1, 3, 5 and Viking 1, 2
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "if (MyPan(21) && o_time > 0)",
    @"if (MyEmp(11, 0))
{
    d_zern *= 1.5;
    d_les *= 1.5;
    d_kam *= 1.5;
    d_ver *= 1.5;
}
if (MyEmp(11, 2))
{
    var _ea = d_zern;
    var _eb = d_les;
    var _ec = d_kam;
    d_zern += (0.25 * (_eb + _ec));
    d_les += (0.25 * (_ea + _ec));
    d_kam += (0.25 * (_ea + _eb));
}
if (MyEmp(11, 4))
{
    d_zern *= 3;
    d_les *= 3;
    d_kam *= 3;
    d_ver *= 3;
}
if (MyEmp(13, 0))
{
    var _sh = Var.rcnt[8];
    d_zern += (1000 * _sh);
    d_les += (100 * _sh);
    d_kam += (10 * _sh);
    d_gold += (_sh * (1 + MyEmp(6, 2)));
}
if (MyEmp(13, 1))
{
    d_gold += (10 * Var.rcnt[8]);
}
if (MyPan(21) && o_time > 0)");

// Gold for abdication: Babylonian 4.
importGroup.QueueFindReplace("gml_Script_get_compen", "if (MyPan(22))", "if (MyEmp(11, 3))\n{\n    d *= 2;\n}\nif (MyPan(22))");

// Worker cost (getCost): Babylonian 2 (cheaper) and Indian 3 (the 15% growth per worker becomes 12%).
importGroup.QueueFindReplace("gml_Script_getCost", "power(1.15 - (0.006 * (global.fs_lv[2] * global.qol_fame))",
    "power(1.15 - (0.006 * (global.fs_lv[2] * global.qol_fame)) - (0.03 * MyEmp(14, 2) * " + DIV + ")");
importGroup.QueueFindReplace("gml_Script_getCost", "(1 - (0.1 * MyEmp(10, 2) * global.qol_div));",
    "(1 - (0.1 * MyEmp(10, 2) * global.qol_div)) * (1 - (0.3 * MyEmp(11, 1) * " + DIV + "));");

// ---------------------------------------------------------------------------------------------------------------------
// Rituals: Mayan 1 (power), 2 (cost), 3 (duration), 4 (automation), 5 (free)
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "tial_les + (main.muta[2] * 5) + (3 * MyPan(14) * global.qol_div) + (MyEmp(10, 1) * global.qol_div);",
    "tial_les + (main.muta[2] * 5) + (3 * MyPan(14) * global.qol_div) + (MyEmp(10, 1) * global.qol_div) + (5 * MyEmp(12, 0) * " + DIV + ");");
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "(MyRel(8, 2) * global.qol_div);",
    "(MyRel(8, 2) * global.qol_div) + (5 * MyEmp(12, 3) * " + DIV + ");");
string ritOld = "(1 - min(0.9, 0.1 * MyRel(8, 1) * global.qol_div))";
string ritNew = ritOld + " * (1 - (0.5 * MyEmp(12, 1) * " + DIV + ")) * (1 - (MyEmp(12, 4) * " + DIV + "))";
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", ritOld, ritNew);
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", ritOld, ritNew);
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "if (db == -1)",
    "if (MyEmp(12, 2) && " + DIV + ")\n{\n    RTM *= 2;\n}\nif (db == -1)");

// ---------------------------------------------------------------------------------------------------------------------
// Viking: heroes (4), ships for sailing (3), double Fame when sailing (5)
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueFindReplace("gml_Script_AfterShlem", "if (MyEmp(9, 0))",
    "if (MyEmp(13, 3))\n{\n    O.hero_HP = ceil(O.hero_HP * 2);\n    O.hero_DAM = ceil(O.hero_DAM * 2);\n}\nif (MyEmp(9, 0))");
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "(1 - (0.1 * (global.fs_lv[28] * global.qol_fame)))",
    "(1 - (0.1 * (global.fs_lv[28] * global.qol_fame))) * (1 - (0.5 * MyEmp(13, 2) * " + DIV + "))");
// The Fame you get for sailing is written in the payout (Alarm 11) and in the two confirmation texts (main Step); same expression.
string fameOld = "ceil(tile_num * (1 + (0.1 * (global.fs_lv[29] * global.qol_fame))))";
string fameNew = "ceil(tile_num * (1 + (0.1 * (global.fs_lv[29] * global.qol_fame))) * (1 + (MyEmp(13, 4) * " + DIV + ")))";
importGroup.QueueFindReplace("gml_Object_main_Alarm_11", fameOld, fameNew);
importGroup.QueueFindReplace("gml_Object_main_Step_0", fameOld, fameNew);

// ---------------------------------------------------------------------------------------------------------------------
// Indian: extra workers per hire (1, 5), hiring time (2), a start of 50 workers (4)
// ---------------------------------------------------------------------------------------------------------------------
string hireOld = "rab += (global.fs_lv[3] * global.qol_fame);";
string hireNew = "rab += ((global.fs_lv[3] * global.qol_fame) + (((2 * MyEmp(14, 0)) + (8 * MyEmp(14, 4))) * " + DIV + "));";
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs", "gml_Script_Forw" })
    importGroup.QueueFindReplace(entry, hireOld, hireNew);
string timeTail = " / (1 + (0.05 * MyRel(9, 2) * " + DIV + "))";
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs" })
    importGroup.QueueFindReplace(entry, timeTail, timeTail + " / (1 + (MyEmp(14, 1) * " + DIV + "))");
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "if (MyEmp(4, 0))",
    @"if (MyEmp(14, 3) && global.qol_div && BLD != 7 && BLD != 8 && BLD != 11 && BLD != 12)
{
    rab += 50;
    Var.rcnt[BLD - 1] += 50;
    Var.rcntmax[BLD - 1] = max(rab, Var.rcntmax[BLD - 1]);
}
if (MyEmp(4, 0))");

importGroup.Import();
