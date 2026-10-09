// QoL: TEN MORE BUILDINGS: five for the sea ("Bld. Marine" menu, coastal tiles only) and five of other types.
// Switch: global.qol_nb (Options > QoL Features > "Extra buildings"), the same as 09_new_buildings.csx. OFF hides the buttons
// and pauses every bonus below (buildings that are already built stay, and keep their workers).
//
//   BLD  menu             name            hired workers   effect (counted over EVERY tile of that type, like the Windmill)
//   19   Build 2          Sawmill         Sawyers         +2% wood production on all forest camps per sawyer
//   20   Build 2          Tavern          Barkeepers      +2% hero experience per barkeeper
//   21   Stone 2          Observatory     Astronomers     +1 second ritual duration per astronomer
//   22   Stone 2          Bank            Bankers         +1% gold for abdication per banker
//   23   Heroic 2         Barracks        Recruits        +0.5% hero HP per recruit
//   24   Marine           Harbor          Dockers         +1% to everything the ships produce per docker
//   25   Marine           Fishery         Fishermen       +1% wheat production (fields and ships) per fisherman
//   26   Marine           Lighthouse      Keepers         +2% fame per keeper when you sail away (fame = +3% gold for abdication each)
//   27   Marine 2         Sail Loft       Sailmakers      every 100 sailmakers halve the ship price
//   28   Marine 2         Naval Academy   Marines         +0.5% hero HP and damage per marine
//
// How it plugs in is explained in 09_new_buildings.csx (this patch uses the same machinery: the building number BLD is the
// Var index + 1; the tile faces, the hiring and the saving are generic). What is new here:
//   * The Marine menu (men = 4) had only the Shipyard. It now has four buttons and a "More >" for a second page (men = 8).
//   * The second pages of Build / Stone / Heroic (men = 5, 6, 7) get a third and fourth slot.
//   * The icons are new: ten 32 x 32 pictures drawn at the bottom of this file, one new sprite each (tt_qol_*).
//   * Effects are hooked into: Land Step (Sawmill, ship price), main Step (Harbor, Fishery wheat, the Lighthouse's fame in the
//     Sail Away text), main Alarm 11 (the Lighthouse's fame when it is paid), LandBattle (Tavern), the ritual window
//     (Observatory), get_compen (Bank), AfterShlem (Barracks, Naval Academy).
//   * The Empire perk "every new building starts with 50 workers (not the heroic ones)" now really leaves out ALL heroic
//     buildings: the game's Academy / Training hall / Hotel / Forge and the new Arena, Watchtower, Barracks, Naval Academy.
//
// All numbers live in the table below, in one place, so they are easy to retune.
//
// UNINSTALLING: a save that contains these buildings cannot be loaded by the unmodified game. Clear them (or abdicate)
// and switch the option off before running Uninstall.bat. The README says so.

using System.Collections.Generic;
using ImageMagick;
using UndertaleModLib.Util;

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string G(string ru, string en) => "g(\"" + ru + "\", \"" + en + "\")";

// ---------------------------------------------------------------------------------------------------------------------
// The table. Index k = 0..9; Var index = 18 + k; BLD = 19 + k.
//   Cost = building price { wheat, wood, gold, stone }    Hire = worker price { wheat, wood }, seconds for the first worker
//   Page = men value of the page it appears on (5 Build 2, 6 Stone 2, 7 Heroic 2, 4 Marine, 8 Marine 2), Slot = 0..3
// ---------------------------------------------------------------------------------------------------------------------
const int N = 10;
const int V0 = 18;
string[] nameRu  = { "Лесопилка", "Таверна", "Обсерватория", "Банк", "Казармы", "Порт", "Рыбацкий пирс", "Маяк", "Парусный цех", "Мор. академия" };
string[] nameEn  = { "Sawmill", "Tavern", "Observatory", "Bank", "Barracks", "Harbor", "Fishery", "Lighthouse", "Sail Loft", "Naval Academy" };
string[] hireRu  = { "Нанять пильщика", "Нанять трактирщика", "Нанять астронома", "Нанять банкира", "Нанять новобранца", "Нанять докера", "Нанять рыбака", "Нанять смотрителя", "Нанять мастера", "Нанять морпеха" };
string[] hireEn  = { "Hire a sawyer", "Hire a barkeeper", "Hire an astronomer", "Hire a banker", "Hire a recruit", "Hire a docker", "Hire a fisherman", "Hire a keeper", "Hire a sailmaker", "Hire a marine" };
string[] nounRu  = { "Пильщики: ", "Трактирщики: ", "Астрономы: ", "Банкиры: ", "Новобранцы: ", "Докеры: ", "Рыбаки: ", "Смотрители: ", "Мастера: ", "Морпехи: " };
string[] nounEn  = { "Sawyers: ", "Barkeepers: ", "Astronomers: ", "Bankers: ", "Recruits: ", "Dockers: ", "Fishermen: ", "Keepers: ", "Sailmakers: ", "Marines: " };
string[] infoRu  =
{
    "Пильщики распиливают бревна:#+2% к добыче дерева на всех#лесных лагерях за каждого пильщика.",
    "Герои отдыхают и хвастаются в таверне:#+2% к опыту героя за каждого#трактирщика.",
    "Астрономы читают звезды:#+1 секунда к длительности ритуалов#за каждого астронома.",
    "Банкиры приумножают золото:#+1% золота за отречение за каждого#банкира.",
    "Новобранцы закаляют героя:#+0.5% к здоровью героя за каждого#новобранца.",
    "Докеры быстрее грузят корабли:#+1% ко всей добыче кораблей#за каждого докера.",
    "Рыбаки приносят улов:#+1% к добыче зерна (полей и кораблей)#за каждого рыбака.",
    "Маяк ведет флот домой:#+2% славы при отплытии#за каждого смотрителя.",
    "Мастера шьют лучшие паруса:#каждые 100 мастеров вдвое уменьшают#цену кораблей.",
    "Морпехи сопровождают героя:#+0.5% к здоровью и урону героя#за каждого морпеха.",
};
string[] infoEn  =
{
    "Sawyers cut the logs:#+2% wood production on all forest#camps for every sawyer.",
    "Heroes rest and boast in the tavern:#+2% hero experience for every#barkeeper.",
    "Astronomers read the stars:#+1 second to the duration of rituals#for every astronomer.",
    "Bankers make your gold grow:#+1% gold for abdication for every#banker.",
    "Recruits harden your hero:#+0.5% hero HP for every recruit.",
    "Dockers load the ships faster:#+1% to everything your ships#produce for every docker.",
    "Fishermen bring in the catch:#+1% wheat production (fields and#ships) for every fisherman.",
    "The beacon guides your fleet home:#+2% fame when you sail away#for every keeper.",
    "Sailmakers stitch better sails:#every 100 sailmakers halve the#ship price.",
    "Marines escort your hero:#+0.5% hero HP and damage for every#marine.",
};
//                      wheat     wood      gold      stone
double[][] cost =
{
    new double[] {      20000,     30000,        0,         0 },      // Sawmill
    new double[] {      40000,     40000,      200,         0 },      // Tavern
    new double[] {          0,    300000,     2000,    900000 },      // Observatory
    new double[] {          0,   1500000,    40000,   4000000 },      // Bank
    new double[] {     4e9,        2e9,          0,       4e9 },      // Barracks (heroic buildings cost billions)
    new double[] {          0,   3000000,     5000,   1000000 },      // Harbor
    new double[] {      60000,    150000,        0,         0 },      // Fishery
    new double[] {          0,    900000,     3000,   3500000 },      // Lighthouse
    new double[] {     250000,   1200000,     1000,         0 },      // Sail Loft
    new double[] {     6e9,        6e9,    2000000,       6e9 },      // Naval Academy
};
//                      wheat        wood   seconds
double[][] hire =
{
    new double[] {       6000,        0,    2 },                      // sawyers
    new double[] {      12000,        0,    3 },                      // barkeepers
    new double[] {     200000,        0,    6 },                      // astronomers
    new double[] {     600000,        0,    8 },                      // bankers
    new double[] {  600000000,        0,    7 },                      // recruits
    new double[] {          0,    80000,    1.5 },                    // dockers (paid in wood, like the ships)
    new double[] {      15000,        0,    2 },                      // fishermen
    new double[] {     400000,        0,    6 },                      // keepers
    new double[] {          0,   100000,    3 },                      // sailmakers (paid in wood)
    new double[] { 1200000000,        0,    9 },                      // marines
};
int[] page = { 5, 5, 6, 6, 7, 4, 4, 4, 8, 8 };
int[] slot = { 2, 3, 2, 3, 2, 1, 2, 3, 0, 1 };
// Face colours (cc = border and text, cb = background) and the effect line drawn on the tile.
string[] cc =
{
    "merge_color(c_green, c_orange, 0.4)",
    "merge_color(c_orange, c_yellow, 0.2)",
    "merge_color(c_fuchsia, c_blue, 0.5)",
    "merge_color(c_yellow, c_orange, 0.15)",
    "merge_color(c_red, c_orange, 0.25)",
    "merge_color(c_aqua, c_blue, 0.3)",
    "merge_color(c_aqua, c_lime, 0.25)",
    "merge_color(c_ltgray, c_red, 0.25)",
    "merge_color(c_yellow, c_ltgray, 0.45)",
    "merge_color(c_blue, c_aqua, 0.6)",
};
string[] effect =
{
    "\"+\" + string(2 * Var.rcnt[18]) + " + G("% дерева на всех лагерях", "% wood on all forest camps"),
    "\"+\" + string(2 * Var.rcnt[19]) + " + G("% опыта героя", "% hero experience"),
    "\"+\" + string(Var.rcnt[20]) + " + G(" сек к длительности ритуала", " s ritual duration"),
    "\"+\" + string(Var.rcnt[21]) + " + G("% золота за отречение", "% gold for abdication"),
    "\"+\" + string(0.5 * Var.rcnt[22]) + " + G("% здоровья героя", "% hero HP"),
    "\"+\" + string(Var.rcnt[23]) + " + G("% добычи кораблей", "% ship output"),
    "\"+\" + string(Var.rcnt[24]) + " + G("% добычи зерна", "% wheat production"),
    "\"+\" + string(2 * Var.rcnt[25]) + " + G("% славы при отплытии", "% fame for sailing"),
    "\"-\" + string(round(100 * (1 - power(0.993092, Var.rcnt[26])))) + " + G("% цены кораблей", "% ship price"),
    "\"+\" + string(0.5 * Var.rcnt[27]) + " + G("% здоровья и урона героя", "% hero HP and damage"),
};
string[] spriteName = new string[N];
for (int k = 0; k < N; k++) spriteName[k] = "tt_qol_" + nameEn[k].ToLower().Replace(" ", "_");

// ---------------------------------------------------------------------------------------------------------------------
// 1) ART: one 32 x 32 sprite per building (drawn by BuildingIcons below), on a new texture page.
// ---------------------------------------------------------------------------------------------------------------------
var page0 = new MagickImage(MagickColors.Transparent, 256, 256);
{
    using var px = page0.GetPixels();
    IconCanvas[] icons = BuildingIcons.All();
    for (int k = 0; k < N; k++)
    {
        int ox = 2 + (k % 5) * 34, oy = 2 + (k / 5) * 34;
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                uint v = icons[k].px[y * 32 + x];
                if (v == 0) continue;
                px.SetPixel(ox + x, oy + y, new byte[] { (byte)(v >> 16), (byte)(v >> 8), (byte)v, 255 });
            }
    }
}
string tmp = Path.Combine(Path.GetTempPath(), "territory_idle_mod_buildings2.png");
page0.Write(tmp, MagickFormat.Png32);
UndertaleEmbeddedTexture embPage = new();
embPage.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
using (MagickImage bgra = TextureWorker.ReadBGRAImageFromFile(tmp))
    embPage.TextureData.Image = GMImage.FromMagickImage(bgra).ConvertToPng();
Data.EmbeddedTextures.Add(embPage);
File.Delete(tmp);

var iconTemplate = Data.Sprites.ByName("tt_wheat");
for (int k = 0; k < N; k++)
{
    if (Data.Sprites.ByName(spriteName[k]) != null)
        throw new Exception(spriteName[k] + " already exists - is this patch being applied twice?");
    UndertaleTexturePageItem item = new();
    item.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
    item.SourceX = (ushort)(2 + (k % 5) * 34); item.SourceY = (ushort)(2 + (k / 5) * 34);
    item.SourceWidth = 32; item.SourceHeight = 32;
    item.TargetX = 0; item.TargetY = 0; item.TargetWidth = 32; item.TargetHeight = 32;
    item.BoundingWidth = 32; item.BoundingHeight = 32;
    item.TexturePage = embPage;
    Data.TexturePageItems.Add(item);

    var spr = new UndertaleSprite();
    spr.Name = Data.Strings.MakeString(spriteName[k]);
    spr.Width = iconTemplate.Width; spr.Height = iconTemplate.Height;
    spr.MarginLeft = iconTemplate.MarginLeft; spr.MarginRight = iconTemplate.MarginRight;
    spr.MarginTop = iconTemplate.MarginTop; spr.MarginBottom = iconTemplate.MarginBottom;
    spr.Transparent = iconTemplate.Transparent; spr.Smooth = iconTemplate.Smooth; spr.Preload = iconTemplate.Preload;
    spr.BBoxMode = iconTemplate.BBoxMode; spr.SepMasks = iconTemplate.SepMasks;
    spr.OriginX = iconTemplate.OriginX; spr.OriginY = iconTemplate.OriginY;
    UndertaleSprite.TextureEntry entry = new();
    entry.Texture = item;
    spr.Textures.Add(entry);
    foreach (var m in iconTemplate.CollisionMasks)
    {
        var mask = new UndertaleSprite.MaskEntry();
        mask.Data = (byte[])m.Data.Clone(); mask.Width = m.Width; mask.Height = m.Height;
        spr.CollisionMasks.Add(mask);
    }
    Data.Sprites.Add(spr);
}

// ---------------------------------------------------------------------------------------------------------------------
// 2) Var Create: the buildings (cost, counters, info text) go right before the worker table, the workers before the enemies.
//    This runs after 09, so BLDN is 18 here and the new entries are numbered 18..27.
// ---------------------------------------------------------------------------------------------------------------------
var sbB = new System.Text.StringBuilder();
for (int k = 0; k < N; k++)
{
    for (int c = 0; c < 4; c++) sbB.Append("cost[BLDN, " + c + "] = " + cost[k][c].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ";\n");
    sbB.Append("bcnt[BLDN] = 0;\nbspd[BLDN] = 0;\n");
    sbB.Append("binfo[BLDN] = " + G(infoRu[k], infoEn[k]) + ";\n");
    sbB.Append("BLDN++;\n");
}
importGroup.QueueFindReplace("gml_Object_Var_Create_0", "rcost[0, 0] = 10;", sbB.ToString() + "rcost[0, 0] = 10;");

var sbW = new System.Text.StringBuilder();
for (int k = 0; k < N; k++)
{
    int v = V0 + k;
    sbW.Append("rcost[" + v + ", 0] = " + hire[k][0].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ";\n");
    sbW.Append("rcost[" + v + ", 1] = " + hire[k][1].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ";\n");
    sbW.Append("rtime[" + v + "] = " + hire[k][2].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ";\n");
    sbW.Append("rcnt[" + v + "] = 0;\nrcntmax[" + v + "] = 0;\n");
    sbW.Append("rname[" + v + "] = " + G(nounRu[k], nounEn[k]) + ";\n");
}
importGroup.QueueFindReplace("gml_Object_Var_Create_0", "vrag_spr[0] = 73;", sbW.ToString() + "vrag_spr[0] = 73;");

// ---------------------------------------------------------------------------------------------------------------------
// 3) Land Draw, menus. Build 2 / Stone 2 / Heroic 2 get more slots; Marine gets its own page layout (with "More >") and a
//    second page. The "More >" / "< Back" buttons flip men by 4 (see 09), so Marine 1 <-> Marine 2 is men 4 <-> 8.
// ---------------------------------------------------------------------------------------------------------------------
string Btn(int k) => "    b[" + (V0 + k) + "] = DrawBtnPrg(x + 10, y + 10 + " + (slot[k] * 40) + ", " + G(nameRu[k], nameEn[k]) + ", bld_P[" + (V0 + k) + "]);\n";
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "b[13] = DrawBtnPrg(x + 10, y + 10 + 40, g(\"Рынок\", \"Market\"), bld_P[13]);",
    "b[13] = DrawBtnPrg(x + 10, y + 10 + 40, g(\"Рынок\", \"Market\"), bld_P[13]);\n" + Btn(0) + Btn(1));
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "b[15] = DrawBtnPrg(x + 10, y + 10 + 40, g(\"Библиотека\", \"Library\"), bld_P[15]);",
    "b[15] = DrawBtnPrg(x + 10, y + 10 + 40, g(\"Библиотека\", \"Library\"), bld_P[15]);\n" + Btn(2) + Btn(3));
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "b[17] = DrawBtnPrg(x + 10, y + 10 + 40, g(\"Башня стражи\", \"Watchtower\"), bld_P[17]);",
    "b[17] = DrawBtnPrg(x + 10, y + 10 + 40, g(\"Башня стражи\", \"Watchtower\"), bld_P[17]);\n" + Btn(4));

var sbM = new System.Text.StringBuilder();
sbM.Append("else if (men == 4 && global.qol_nb)\n{\n");
sbM.Append("    b[8] = DrawBtnPrg(x + 10, y + 10, g(\"Верфь\", \"Shipyard\"), bld_P[8]);\n");
sbM.Append(Btn(5)).Append(Btn(6)).Append(Btn(7));
sbM.Append("    draw_set_font(font1);\n    b_cc = DrawBtnSml(x + 99, y + 176, g(\"Отмена\", \"Cancel\"));\n");
sbM.Append("    nb_back = 0;\n    nb_more = DrawBtnSml(x + 10, y + 176, g(\"Еще >\", \"More >\"));\n}\n");
sbM.Append("else if (men == 8)\n{\n");
sbM.Append(Btn(8)).Append(Btn(9));
sbM.Append("    draw_set_font(font1);\n    nb_more = 0;\n    nb_back = DrawBtnSml(x + 10, y + 176, g(\"< Назад\", \"< Back\"));\n");
sbM.Append("    b_cc = DrawBtnSml(x + 99, y + 176, g(\"Отмена\", \"Cancel\"));\n}\n");
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "else if (men == 4)", sbM.ToString() + "else if (men == 4)");

// ---------------------------------------------------------------------------------------------------------------------
// 4) Land Draw, the tile faces (same layout as the game's own buildings and as the ones from 09). Like those, they go in front of
//    the shared "Spd" / "Clear" / "stop autohire" controls (see 09), so they are put in front of the first face of 09.
// ---------------------------------------------------------------------------------------------------------------------
var sbF = new System.Text.StringBuilder("\n");
for (int k = 0; k < N; k++)
{
    int bld = V0 + 1 + k;
    sbF.Append("if (BLD == " + bld + ")\n{\n");
    sbF.Append("    cc = " + cc[k] + ";\n");
    sbF.Append("    cb = merge_color(cc, c_black, 0.8);\n");
    sbF.Append("    cr = merge_color(cc, c_white, 0.2);\n");
    sbF.Append("    draw_set_color(cc);\n    draw_rectangle(x, y, x + w, y + h, false);\n");
    sbF.Append("    draw_set_color(cb);\n    draw_rectangle(x + a, y + a, (x + w) - a, (y + h) - a, false);\n");
    sbF.Append("    draw_set_color(cc);\n    draw_set_font(font0);\n");
    sbF.Append("    draw_sprite(" + spriteName[k] + ", 0, x + 19, y + 19);\n");
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
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "if (BLD == 13)", sbF.ToString() + "if (BLD == 13)");

// ---------------------------------------------------------------------------------------------------------------------
// 5) Effects.
// ---------------------------------------------------------------------------------------------------------------------
// Sawmill (Var 18): wood production of the forest camps. Same trick as in 09: the original block is the Windmill's
// "if (BLD == 1)"; our line closes it and opens the forest camps' block, whose closing brace is the original one.
importGroup.QueueFindReplace("gml_Object_Land_Step_0",
    "doh *= (1 + (0.02 * Var.rcnt[12]));",
    "doh *= (1 + (0.02 * Var.rcnt[12]));\n}\nif (BLD == 2)\n{\n    doh *= (1 + (0.02 * Var.rcnt[18]));");

// Sail Loft (Var 26): ships (BLD 9) are paid in wood, so the price is set in the wood branch. HireRabs (the auto-hire
// ritual and the offline catch-up) has the same line.
string shipOld = "ncost = getCost(Var.rcost[r_i, 1], rab, pseudo);";
string shipNew = shipOld + "\nif (BLD == 9)\n{\n    ncost *= power(0.993092, Var.rcnt[26] * global.qol_nb);\n}";
importGroup.QueueFindReplace("gml_Object_Land_Step_0", shipOld, shipNew);
importGroup.QueueFindReplace("gml_Script_HireRabs", shipOld, shipNew);

// The Empire perk "every new building starts with 50 workers (not the heroic ones)": leave out ALL heroic buildings.
importGroup.QueueFindReplace("gml_Object_Land_Step_0",
    "if (MyEmp(14, 3) && global.qol_div && BLD != 7 && BLD != 8 && BLD != 11 && BLD != 12)",
    "if (MyEmp(14, 3) && global.qol_div && BLD != 7 && BLD != 8 && BLD != 11 && BLD != 12 && BLD != 17 && BLD != 18 && BLD != 23 && BLD != 28)");

// Harbor (Var 23): added with the other building incomes in main Step, in front of the Market's line. It adds a share of
// what this tile's ships (BLD 9) already produce, with the same expressions as the game.
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "if (BLD == 14 && global.qol_nb)",
    @"if (BLD == 9 && global.qol_nb && Var.rcnt[23] > 0)
{
    var _hb = 0.01 * Var.rcnt[23];
    other.d_zern += ((rab * 1000 * (1 + (main.pp_zern * global.qol_ab[0] * 0.01))) * _hb);
    other.d_les += ((rab * 100 * (1 + (main.pp_les * global.qol_ab[1] * 0.01))) * _hb);
    other.d_kam += ((rab * 10 * (1 + (main.pp_kam * global.qol_ab[2] * 0.01))) * _hb);
    other.d_gold += ((rab * (1 + MyEmp(6, 2))) * _hb);
}
if (BLD == 14 && global.qol_nb)");

// Fishery (Var 24): +1% to all wheat (fields, ships and the rest), with the game's other wheat multipliers in main Step.
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "if (main.muta[3])",
    "if (global.qol_nb)\n{\n    d_zern *= (1 + (0.01 * Var.rcnt[24]));\n}\nif (main.muta[3])");

// Lighthouse (Var 25): +2% fame per keeper when you sail away. Fame is "empire_ppp += ceil(tile_num * ...)" in main Alarm 11
// (where it is paid) and in main Step (the text of the Sail Away button); the Viking Empire's factor is in both places, and
// ours goes right behind it.
string fameOld = "(1 + (MyEmp(13, 4) * global.qol_div))";
string fameNew = fameOld + " * (1 + (0.02 * Var.rcnt[25] * global.qol_nb))";
importGroup.QueueFindReplace("gml_Object_main_Alarm_11", fameOld, fameNew);
importGroup.QueueFindReplace("gml_Object_main_Step_0", fameOld, fameNew);

// Tavern (Var 19): hero experience per kill (class_k is the multiplier the game and the other patches build up).
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10",
    "if (SlugaVar(3, 1) && global.qol_div)",
    "if (global.qol_nb)\n{\n    class_k *= (1 + (0.02 * Var.rcnt[19]));\n}\nif (SlugaVar(3, 1) && global.qol_div)");

// Observatory (Var 20): ritual duration, computed in the ritual window.
importGroup.QueueFindReplace("gml_Object_HRit_Other_10",
    "(10 * MyRel(6, 2) * global.qol_div);",
    "(10 * MyRel(6, 2) * global.qol_div) + (Var.rcnt[20] * global.qol_nb);");

// Bank (Var 21): gold for abdication.
importGroup.QueueFindReplace("gml_Script_get_compen",
    "if (all_myst)",
    "if (global.qol_nb)\n{\n    d *= (1 + (0.01 * Var.rcnt[21]));\n}\nif (all_myst)");

// Barracks (Var 22: HP) and Naval Academy (Var 27: HP and damage), in front of the Training hall's HP line.
importGroup.QueueFindReplace("gml_Script_AfterShlem",
    "if (Var.rcnt[7])",
    @"if (global.qol_nb)
{
    O.hero_HP = ceil(O.hero_HP * (1 + (0.005 * Var.rcnt[22])));
    O.hero_HP = ceil(O.hero_HP * (1 + (0.005 * Var.rcnt[27])));
    O.hero_DAM = ceil(O.hero_DAM * (1 + (0.005 * Var.rcnt[27])));
}
if (Var.rcnt[7])");

importGroup.Import();

// =====================================================================================================================
// The icons (32 x 32, drawn with simple shapes).
// =====================================================================================================================
public class IconCanvas
{
    public const int S = 32;
    public uint[] px = new uint[S * S];       // 0xAARRGGBB, 0 = empty

    public static uint C(int r, int g, int b) { return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | (uint)b; }
    public static uint Mix(uint a, uint b, double t)
    {
        int ar = (int)((a >> 16) & 255), ag = (int)((a >> 8) & 255), ab = (int)(a & 255);
        int br = (int)((b >> 16) & 255), bg = (int)((b >> 8) & 255), bb = (int)(b & 255);
        return C((int)(ar + (br - ar) * t), (int)(ag + (bg - ag) * t), (int)(ab + (bb - ab) * t));
    }
    public static uint Dark(uint c, double t) { return Mix(c, C(0, 0, 0), t); }
    public static uint Light(uint c, double t) { return Mix(c, C(255, 255, 255), t); }

    public void Set(int x, int y, uint c) { if (x >= 0 && y >= 0 && x < S && y < S) px[y * S + x] = c; }
    public uint Get(int x, int y) { if (x >= 0 && y >= 0 && x < S && y < S) return px[y * S + x]; return 0; }

    // generic: paint every pixel whose centre satisfies the predicate
    public delegate bool Pred(double x, double y);
    public void Fill(Pred p, uint c)
    {
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
                if (p(x + 0.5, y + 0.5)) px[y * S + x] = c;
    }
    public void Rect(double x0, double y0, double x1, double y1, uint c)
    {
        Fill(delegate(double x, double y) { return x >= x0 && x <= x1 && y >= y0 && y <= y1; }, c);
    }
    public void Disc(double cx, double cy, double r, uint c)
    {
        Fill(delegate(double x, double y) { return (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r; }, c);
    }
    public void Ring(double cx, double cy, double r0, double r1, uint c)
    {
        Fill(delegate(double x, double y) { double d = (x - cx) * (x - cx) + (y - cy) * (y - cy); return d >= r0 * r0 && d <= r1 * r1; }, c);
    }
    public void Ellipse(double cx, double cy, double rx, double ry, uint c)
    {
        Fill(delegate(double x, double y) { double u = (x - cx) / rx, v = (y - cy) / ry; return u * u + v * v <= 1; }, c);
    }
    public static double SegD(double x, double y, double ax, double ay, double bx, double by)
    {
        double vx = bx - ax, vy = by - ay, wx = x - ax, wy = y - ay;
        double t = Math.Max(0, Math.Min(1, (wx * vx + wy * vy) / (vx * vx + vy * vy)));
        double px2 = ax + t * vx, py2 = ay + t * vy;
        return Math.Sqrt((x - px2) * (x - px2) + (y - py2) * (y - py2));
    }
    public void Line(double ax, double ay, double bx, double by, double th, uint c)
    {
        Fill(delegate(double x, double y) { return SegD(x, y, ax, ay, bx, by) <= th / 2; }, c);
    }
    public void Poly(double[] xy, uint c)
    {
        int n = xy.Length / 2;
        Fill(delegate(double x, double y)
        {
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = xy[2 * i], yi = xy[2 * i + 1], xj = xy[2 * j], yj = xy[2 * j + 1];
                if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) inside = !inside;
            }
            return inside;
        }, c);
    }
    // a 1 px dark outline around everything drawn so far
    public void Outline(uint c)
    {
        uint[] copy = (uint[])px.Clone();
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                if (copy[y * S + x] != 0) continue;
                bool near = false;
                for (int dy = -1; dy <= 1 && !near; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx != 0 && dy != 0) continue;
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < S && ny < S && copy[ny * S + nx] != 0) { near = true; break; }
                    }
                if (near) px[y * S + x] = c;
            }
    }
}

public static class BuildingIcons
{
    static uint C(int r, int g, int b) { return IconCanvas.C(r, g, b); }

    // Harbor: an anchor
    public static IconCanvas Anchor()
    {
        IconCanvas k = new IconCanvas();
        uint steel = C(120, 175, 230), hi = C(190, 225, 255), sh = C(60, 100, 160);
        k.Line(16, 8, 16, 26, 3.2, steel);
        k.Line(10, 11.5, 22, 11.5, 2.8, steel);
        k.Ring(16, 5.2, 1.8, 4.2, steel);
        // the curved arms: a lower half ring around (16,17)
        k.Fill(delegate(double x, double y) { double d = Math.Sqrt((x - 16) * (x - 16) + (y - 17) * (y - 17)); return y >= 17 && d >= 8.2 && d <= 11.2; }, steel);
        // flukes
        k.Poly(new double[] { 1.5, 14.5, 8.5, 16.8, 5.2, 21 }, steel);
        k.Poly(new double[] { 30.5, 14.5, 23.5, 16.8, 26.8, 21 }, steel);
        // shading: light left edge, dark right edge
        k.Line(15, 9, 15, 25, 1, hi);
        k.Line(9.6, 11, 17, 11, 1, hi);
        k.Line(17.2, 12.5, 17.2, 25, 1, sh);
        k.Outline(C(15, 30, 55));
        return k;
    }

    // Fishery: a fish
    public static IconCanvas Fish()
    {
        IconCanvas k = new IconCanvas();
        uint body = C(245, 150, 50), belly = C(255, 215, 150), fin = C(220, 100, 40), dk = C(150, 60, 25);
        k.Poly(new double[] { 22, 16, 30.5, 8.5, 28.5, 16, 30.5, 23.5 }, fin);       // tail
        k.Poly(new double[] { 11, 10.5, 16, 4.5, 20, 11 }, fin);                      // back fin
        k.Poly(new double[] { 13, 21, 16, 26, 19.5, 21 }, fin);                       // belly fin
        k.Ellipse(13.5, 16, 11.5, 6.8, body);
        k.Ellipse(13.5, 19.2, 9.5, 3.2, belly);
        k.Line(8.8, 11.2, 8.8, 20.8, 1.2, dk);                                        // gill
        k.Disc(5.8, 14.2, 2.1, C(255, 255, 255));
        k.Disc(5.4, 14.4, 1.0, C(20, 20, 30));
        k.Line(2.5, 17.5, 5, 18.3, 1, dk);                                            // mouth
        for (int i = 0; i < 3; i++) k.Line(15.5 + i * 3.1, 12.5, 15.5 + i * 3.1, 18.5, 0.9, dk); // scales
        k.Outline(C(60, 25, 10));
        return k;
    }

    // Lighthouse: red and white striped tower with a lit lantern
    public static IconCanvas Lighthouse()
    {
        IconCanvas k = new IconCanvas();
        uint red = C(215, 55, 45), white = C(245, 245, 240), yel = C(255, 225, 70), roof = C(70, 70, 90), rock = C(120, 110, 100);
        k.Poly(new double[] { 9, 29.5, 12.5, 11, 19.5, 11, 23, 29.5 }, white);
        for (int band = 0; band < 3; band++)
        {
            double y0 = 14 + band * 6.2, y1 = y0 + 3.2;
            k.Fill(delegate(double x, double y)
            {
                if (y < y0 || y > y1) return false;
                double f = (y - 11) / 18.5, half = 3.5 + f * 3.5 + 0.2;
                return Math.Abs(x - 16) <= half;
            }, red);
        }
        k.Rect(12, 6.5, 20, 10.8, yel);                                               // lantern
        k.Rect(15.5, 6.5, 16.5, 10.8, C(200, 150, 30));
        k.Rect(11, 10.8, 21, 12, roof);                                               // gallery
        k.Poly(new double[] { 16, 1.5, 11.5, 6.5, 20.5, 6.5 }, roof);
        k.Rect(14.6, 21, 17.4, 25, C(60, 70, 110));                                   // door
        k.Rect(6, 29.5, 26, 31.5, rock);
        k.Line(10.5, 8, 2.5, 4.5, 1.2, C(255, 240, 150));                             // light beams
        k.Line(21.5, 8, 29.5, 4.5, 1.2, C(255, 240, 150));
        k.Line(10.5, 9, 2, 9.5, 1.2, C(255, 240, 150));
        k.Line(21.5, 9, 30, 9.5, 1.2, C(255, 240, 150));
        k.Outline(C(25, 25, 40));
        return k;
    }

    // Naval Academy: a ship's wheel
    public static IconCanvas Wheel()
    {
        IconCanvas k = new IconCanvas();
        uint wood = C(205, 145, 65), hi = C(240, 190, 105), dk = C(120, 70, 30), brass = C(250, 215, 90);
        for (int i = 0; i < 8; i++)
        {
            double a = i * Math.PI / 4;
            double ex = 16 + Math.Cos(a) * 14.2, ey = 16 + Math.Sin(a) * 14.2;
            k.Line(16, 16, ex, ey, 2.4, wood);
            k.Disc(ex, ey, 2.0, wood);
        }
        k.Ring(16, 16, 7.4, 10.6, wood);
        k.Ring(16, 16, 9.6, 10.6, dk);
        k.Ring(16, 16, 7.4, 8.2, hi);
        k.Disc(16, 16, 4.0, brass);
        k.Disc(16, 16, 1.6, dk);
        k.Outline(C(50, 25, 10));
        return k;
    }

    // Sail Loft: a coil of rope
    public static IconCanvas Rope()
    {
        IconCanvas k = new IconCanvas();
        uint rope = C(215, 175, 105), dk = C(140, 100, 50), hi = C(245, 215, 150);
        k.Ring(16, 16, 4.2, 12.8, rope);
        // twisted strands: diagonal dark ticks along the ring
        k.Fill(delegate(double x, double y)
        {
            double d = Math.Sqrt((x - 16) * (x - 16) + (y - 16) * (y - 16));
            if (d < 4.2 || d > 12.8) return false;
            double ang = Math.Atan2(y - 16, x - 16);
            double t = Math.Sin(ang * 14 + d * 0.9);
            return t > 0.55;
        }, dk);
        k.Ring(16, 16, 4.2, 5.4, dk);
        k.Ring(16, 16, 11.8, 12.8, dk);
        k.Fill(delegate(double x, double y)
        {
            double d = Math.Sqrt((x - 16) * (x - 16) + (y - 16) * (y - 16));
            return d >= 7 && d <= 8.5 && x < 16 && y < 16 && Math.Sin(Math.Atan2(y - 16, x - 16) * 14 + d * 0.9) < 0.55;
        }, hi);
        k.Line(23, 25, 29.5, 30, 3.0, rope);                                          // loose end
        k.Line(24, 25.5, 29.5, 29.8, 0.9, dk);
        k.Outline(C(55, 35, 15));
        return k;
    }

    // Sawmill: a circular saw blade
    public static IconCanvas Saw()
    {
        IconCanvas k = new IconCanvas();
        uint steel = C(195, 200, 212), hi = C(235, 240, 250), dk = C(100, 105, 120);
        for (int i = 0; i < 14; i++)
        {
            double a = i * 2 * Math.PI / 14;
            double a2 = a + Math.PI / 14 * 1.7;
            double ax = 16 + Math.Cos(a) * 11, ay = 16 + Math.Sin(a) * 11;
            double bx = 16 + Math.Cos(a) * 15.2, by = 16 + Math.Sin(a) * 15.2;
            double cx = 16 + Math.Cos(a2) * 11, cy = 16 + Math.Sin(a2) * 11;
            k.Poly(new double[] { ax, ay, bx, by, cx, cy }, steel);
        }
        k.Disc(16, 16, 12, steel);
        k.Ring(16, 16, 8.2, 9.0, dk);
        for (int i = 0; i < 4; i++)
        {
            double a = i * Math.PI / 2 + 0.4;
            k.Disc(16 + Math.Cos(a) * 6.4, 16 + Math.Sin(a) * 6.4, 1.5, C(60, 62, 76));
        }
        k.Fill(delegate(double x, double y) { return x < 16 && y < 16 && x + y < 24 && x + y > 20 && Math.Sqrt((x - 16) * (x - 16) + (y - 16) * (y - 16)) > 9.5; }, hi);
        k.Disc(16, 16, 3.2, dk);
        k.Disc(16, 16, 1.4, C(30, 30, 40));
        k.Outline(C(35, 35, 50));
        return k;
    }

    // Tavern: a mug of beer
    public static IconCanvas Mug()
    {
        IconCanvas k = new IconCanvas();
        uint glass = C(250, 175, 40), hi = C(255, 225, 120), dk = C(190, 110, 20), foam = C(250, 248, 238), band = C(150, 90, 25);
        k.Ring(23.6, 18, 3.0, 6.4, C(235, 225, 205));                                  // handle
        k.Ring(23.6, 18, 3.0, 3.8, C(150, 140, 125));
        k.Rect(6.5, 9, 22, 28, glass);
        k.Rect(6.5, 9, 9.2, 28, hi);
        k.Rect(19.6, 9, 22, 28, dk);
        k.Rect(6.5, 24.5, 22, 28, band);
        k.Rect(10.5, 13, 11.6, 22, hi);
        k.Disc(9.2, 8.5, 3.6, foam);
        k.Disc(13.5, 7.2, 4.0, foam);
        k.Disc(18, 7.8, 3.8, foam);
        k.Disc(21, 9.4, 2.4, foam);
        k.Rect(6.5, 8.5, 22, 11, foam);
        k.Line(7, 11.2, 22, 11.2, 0.9, C(230, 215, 170));
        k.Outline(C(60, 35, 10));
        return k;
    }

    // Observatory: a telescope under the stars
    public static IconCanvas Telescope()
    {
        IconCanvas k = new IconCanvas();
        uint tube = C(75, 120, 215), tube2 = C(110, 155, 240), brass = C(250, 210, 80), leg = C(140, 100, 60), star = C(255, 240, 130);
        k.Line(15, 19, 9.5, 30, 2.2, leg);
        k.Line(15, 19, 21, 30, 2.2, leg);
        k.Line(15, 19, 15.5, 30, 2.0, leg);
        k.Line(5.5, 22.5, 15, 14, 8, tube);
        k.Line(15, 14, 24.5, 6.5, 5.2, tube2);
        k.Line(9.7, 19.3, 12.6, 16.6, 8.6, brass);                                      // brass joint
        k.Line(22.2, 8.3, 23.6, 7.2, 6.0, brass);
        k.Disc(26.5, 5.2, 3.0, C(190, 235, 255));
        k.Disc(26.2, 5.0, 1.2, C(255, 255, 255));
        k.Line(7.4, 21, 14, 14.7, 2.0, C(140, 180, 255));                               // highlight
        // stars
        k.Poly(new double[] { 4, 3, 5, 5.5, 7.5, 6.5, 5, 7.5, 4, 10, 3, 7.5, 0.5, 6.5, 3, 5.5 }, star);
        k.Poly(new double[] { 27.5, 17, 28.3, 19, 30.3, 19.8, 28.3, 20.6, 27.5, 22.6, 26.7, 20.6, 24.7, 19.8, 26.7, 19 }, star);
        k.Outline(C(20, 25, 60));
        return k;
    }

    // Bank: a stack of gold coins and a big coin
    public static IconCanvas Coins()
    {
        IconCanvas k = new IconCanvas();
        uint gold = C(245, 200, 50), top = C(255, 235, 130), side = C(195, 140, 25), dk = C(140, 95, 15);
        // stack of three
        for (int i = 0; i < 3; i++)
        {
            double cy = 25 - i * 5.4;
            k.Rect(3, cy, 17.4, cy + 3.4, side);
            k.Ellipse(10.2, cy + 3.4, 7.2, 3.0, side);
            k.Ellipse(10.2, cy, 7.2, 3.0, top);
            k.Ellipse(10.2, cy, 4.6, 1.7, gold);
        }
        // a big coin standing on the right
        k.Disc(22.5, 19.5, 8.6, side);
        k.Disc(22.5, 19.5, 7.4, gold);
        k.Ring(22.5, 19.5, 4.6, 5.8, top);
        k.Rect(21.5, 15.8, 23.6, 23.2, top);
        k.Rect(19.8, 17.4, 25.3, 18.4, top);
        k.Rect(19.8, 20.6, 25.3, 21.6, top);
        k.Fill(delegate(double x, double y) { return x < 22.5 && y < 19.5 && (x - 22.5) * (x - 22.5) + (y - 19.5) * (y - 19.5) >= 40 && (x - 22.5) * (x - 22.5) + (y - 19.5) * (y - 19.5) <= 54; }, C(255, 245, 190));
        k.Outline(C(80, 50, 5));
        return k;
    }

    // Barracks: a tent with a banner
    public static IconCanvas Tent()
    {
        IconCanvas k = new IconCanvas();
        uint red = C(205, 55, 50), hi = C(235, 105, 90), dk = C(130, 25, 30), cream = C(245, 225, 175);
        k.Poly(new double[] { 16, 7, 1.5, 28.5, 30.5, 28.5 }, red);
        k.Poly(new double[] { 16, 7, 16, 28.5, 30.5, 28.5 }, C(175, 40, 42));
        k.Poly(new double[] { 16, 7, 9.5, 20, 12.5, 20.5, 16, 14 }, hi);
        k.Poly(new double[] { 16, 15, 11.2, 28.5, 20.8, 28.5 }, dk);                   // doorway
        k.Line(16, 15, 16, 28.5, 0.8, C(80, 15, 20));
        k.Line(6.5, 24, 25.5, 24, 1.4, cream);                                          // trim band
        k.Line(16, 7, 16, 0.8, 1.2, C(150, 150, 160));                                  // pole
        k.Poly(new double[] { 16.6, 0.8, 24, 2.8, 16.6, 5.0 }, C(250, 215, 70));         // flag
        k.Rect(1, 28.5, 31, 30.5, C(95, 120, 60));
        k.Outline(C(45, 12, 15));
        return k;
    }

    public static IconCanvas[] All()
    {
        // the order of the buildings in the table at the top: Sawmill, Tavern, Observatory, Bank, Barracks, Harbor, Fishery, Lighthouse, Sail Loft, Naval Academy
        return new IconCanvas[] { Saw(), Mug(), Telescope(), Coins(), Tent(), Anchor(), Fish(), Lighthouse(), Rope(), Wheel() };
    }
}
