// QoL: four SPECIAL BUILDINGS, a new category with its own tile menu ("Bld. Special", the fifth button of an empty tile).
// They have requirements you must meet to build them, you can have only ONE of each, and they give very powerful bonuses.
// Switch: global.qol_nb (Options > QoL Features > "Extra buildings"), the same as 09 and 17. OFF hides the menu and pauses
// the bonuses (buildings that are already built stay, and keep their workers).
//
//   BLD  name           requirements (to build; the bonus also works only while they hold)    workers      bonus (base x2, plus per worker)
//   29   Hermitage      NO Temple and NO Cathedral anywhere                                    Hermits      wheat, wood, stone production x2, +5% per hermit
//   30   Imperial Mint  NO Academy, and at least 20 tiles                                      Minters      gold per second and gold for abdication x2, +5% per minter
//   31   Citadel        an Academy, a Training hall and a Forge                                Soldiers     hero HP and damage x2, +4% per soldier
//   32   Grand Sanctum  a Temple, a Cathedral and at least 30 monks                           Acolytes     faith production x2, +5% per acolyte, rituals last 60 s longer
//
// How it plugs in (see 09 and 17 for the machinery they share: BLD is the Var index + 1, and the hiring, the saving, Spd,
// Clear and the auto-hire checkbox are generic):
//   * main Create/Step: four flags per building, recomputed every step from the building counts Var.bcnt (the game keeps them
//     up to date when something is built, cleared or loaded):  sp_ok = the requirements hold,  sp_can = and none is built or
//     being built yet (so you may build it),  sp_on = it is built and the requirements hold (so its bonus works).
//   * Land Draw: the "Bld. Special" button (the Build / Stone / Heroic / Marine buttons are moved up a little to make room for it),
//     the menu page (men = 9) and the tile faces. Buttons you cannot press are dark red.
//   * Land Step: the page switch, the requirements in the tooltip with [OK] / [X], and the block that refuses the build.
//   * Bonuses: main Step (production), get_compen (abdication), AfterShlem (hero) and the ritual window (duration).
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
string D(double v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

// ---------------------------------------------------------------------------------------------------------------------
// The table. Index k = 0..3; Var index = 28 + k; BLD = 29 + k.
// ---------------------------------------------------------------------------------------------------------------------
const int N = 4;
const int V0 = 28;
string[] nameRu  = { "Скит", "Монетный двор", "Цитадель", "Святилище" };
string[] nameEn  = { "Hermitage", "Imperial Mint", "Citadel", "Grand Sanctum" };
string[] hireRu  = { "Нанять отшельника", "Нанять чеканщика", "Нанять воина", "Нанять служку" };
string[] hireEn  = { "Hire a hermit", "Hire a minter", "Hire a soldier", "Hire an acolyte" };
string[] nounRu  = { "Отшельники: ", "Чеканщики: ", "Воины: ", "Служки: " };
string[] nounEn  = { "Hermits: ", "Minters: ", "Soldiers: ", "Acolytes: " };
string[] infoRu  =
{
    "Скит вдали от храмов.#Вся добыча зерна, дерева и камня x2#и еще +5% за каждого отшельника.#Только один. Нет храмов и соборов.",
    "Императорский монетный двор.#Золото в секунду и за отречение x2#и еще +5% за каждого чеканщика.#Только один. Нужно 20 клеток, нет академий.",
    "Крепость для вашего героя.#Здоровье и урон героя x2#и еще +4% за каждого воина.#Только один. Нужны академия, зал и кузница.",
    "Главное святилище вашей веры.#Добыча веры x2, +5% за каждого служку,#ритуалы длятся на 60 сек дольше.#Только один. Нужны храм, собор и 30 монахов.",
};
string[] infoEn  =
{
    "A retreat far from the temples.#All wheat, wood and stone production x2,#and +5% more for every hermit.#Only one. No Temples and no Cathedrals.",
    "The imperial mint.#Gold per second and for abdication x2,#and +5% more for every minter.#Only one. Needs 20 tiles and no Academies.",
    "A fortress for your hero.#Hero HP and damage x2,#and +4% more for every soldier.#Only one. Needs an Academy, a Training hall and a Forge.",
    "The holiest place of your faith.#Faith production x2, +5% for every acolyte,#and rituals last 60 s longer.#Only one. Needs a Temple, a Cathedral and 30 monks.",
};
//                      wheat     wood      gold      stone
double[][] cost =
{
    new double[] {     20e9,     20e9,       5e6,     20e9 },       // Hermitage
    new double[] {     50e9,        0,     200e6,     50e9 },       // Imperial Mint
    new double[] {        0,    100e9,     100e6,    100e9 },       // Citadel
    new double[] {     80e9,     80e9,      50e6,     80e9 },       // Grand Sanctum
};
//                      wheat        wood   seconds
double[][] hire =
{
    new double[] {      2e9,        0,    8 },                      // hermits
    new double[] {      4e9,        0,   10 },                      // minters
    new double[] {      6e9,        0,   10 },                      // soldiers
    new double[] {      3e9,        0,   12 },                      // acolytes
};
string[] cc =
{
    "merge_color(c_lime, c_aqua, 0.3)",
    "merge_color(c_yellow, c_orange, 0.1)",
    "merge_color(c_ltgray, c_blue, 0.25)",
    "merge_color(c_fuchsia, c_white, 0.4)",
};
// the bonus line on the tile (shown as soon as the building stands, the base bonus does not need workers)
string[] effect =
{
    "\"x\" + string(2 * (1 + (0.05 * Var.rcnt[28]))) + " + G(" добыча зерна, дерева и камня", " wheat, wood and stone production"),
    "\"x\" + string(2 * (1 + (0.05 * Var.rcnt[29]))) + " + G(" золото в секунду и за отречение", " gold per second and for abdication"),
    "\"x\" + string(2 * (1 + (0.04 * Var.rcnt[30]))) + " + G(" здоровье и урон героя", " hero HP and damage"),
    "\"x\" + string(2 * (1 + (0.05 * Var.rcnt[31]))) + " + G(" добыча веры, ритуалы +60 сек", " faith production, rituals +60 s"),
};
// Requirements, for the tooltip: { Russian, English, the number shown "(you have N)", the condition that must hold }.
string[][][] req =
{
    new[] {
        new[] { "Нет храмов", "No Temples", "Var.bcnt[4]", "Var.bcnt[4] == 0" },
        new[] { "Нет соборов", "No Cathedrals", "Var.bcnt[9]", "Var.bcnt[9] == 0" },
    },
    new[] {
        new[] { "Нет академий", "No Academies", "Var.bcnt[6]", "Var.bcnt[6] == 0" },
        new[] { "Клеток не меньше 20", "At least 20 tiles", "main.tile_num", "main.tile_num >= 20" },
    },
    new[] {
        new[] { "Есть академия", "An Academy", "Var.bcnt[6]", "Var.bcnt[6] > 0" },
        new[] { "Есть тренировочный зал", "A Training hall", "Var.bcnt[7]", "Var.bcnt[7] > 0" },
        new[] { "Есть кузница", "A Forge", "Var.bcnt[11]", "Var.bcnt[11] > 0" },
    },
    new[] {
        new[] { "Есть храм", "A Temple", "Var.bcnt[4]", "Var.bcnt[4] > 0" },
        new[] { "Есть собор", "A Cathedral", "Var.bcnt[9]", "Var.bcnt[9] > 0" },
        new[] { "Монахов не меньше 30", "At least 30 monks", "Var.rcnt[4]", "Var.rcnt[4] >= 30" },
    },
};
// the same requirements as one condition (main Step), by building
string[] reqAll =
{
    "(Var.bcnt[4] == 0 && Var.bcnt[9] == 0)",
    "(Var.bcnt[6] == 0 && tile_num >= 20)",
    "(Var.bcnt[6] > 0 && Var.bcnt[7] > 0 && Var.bcnt[11] > 0)",
    "(Var.bcnt[4] > 0 && Var.bcnt[9] > 0 && Var.rcnt[4] >= 30)",
};
string[] spriteName = { "tt_qol_hermitage", "tt_qol_mint", "tt_qol_citadel", "tt_qol_sanctum" };

// ---------------------------------------------------------------------------------------------------------------------
// 1) ART: four 32 x 32 sprites (drawn by SpecialIcons at the bottom of this file) on a new texture page.
// ---------------------------------------------------------------------------------------------------------------------
var page0 = new MagickImage(MagickColors.Transparent, 256, 256);
{
    using var px = page0.GetPixels();
    IconCanvas[] icons = SpecialIcons.All();
    for (int k = 0; k < N; k++)
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                uint v = icons[k].px[y * 32 + x];
                if (v == 0) continue;
                px.SetPixel(2 + k * 34 + x, 2 + y, new byte[] { (byte)(v >> 16), (byte)(v >> 8), (byte)v, 255 });
            }
}
string tmp = Path.Combine(Path.GetTempPath(), "territory_idle_mod_special.png");
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
    item.SourceX = (ushort)(2 + k * 34); item.SourceY = 2;
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
// 2) Var Create: the buildings and their workers (after 17, so BLDN is 28 here and the new entries are numbered 28..31).
// ---------------------------------------------------------------------------------------------------------------------
var sbB = new System.Text.StringBuilder();
for (int k = 0; k < N; k++)
{
    for (int c = 0; c < 4; c++) sbB.Append("cost[BLDN, " + c + "] = " + D(cost[k][c]) + ";\n");
    sbB.Append("bcnt[BLDN] = 0;\nbspd[BLDN] = 0;\n");
    sbB.Append("binfo[BLDN] = " + G(infoRu[k], infoEn[k]) + ";\n");
    sbB.Append("BLDN++;\n");
}
importGroup.QueueFindReplace("gml_Object_Var_Create_0", "rcost[0, 0] = 10;", sbB.ToString() + "rcost[0, 0] = 10;");

var sbW = new System.Text.StringBuilder();
for (int k = 0; k < N; k++)
{
    int v = V0 + k;
    sbW.Append("rcost[" + v + ", 0] = " + D(hire[k][0]) + ";\n");
    sbW.Append("rcost[" + v + ", 1] = " + D(hire[k][1]) + ";\n");
    sbW.Append("rtime[" + v + "] = " + D(hire[k][2]) + ";\n");
    sbW.Append("rcnt[" + v + "] = 0;\nrcntmax[" + v + "] = 0;\n");
    sbW.Append("rname[" + v + "] = " + G(nounRu[k], nounEn[k]) + ";\n");
}
importGroup.QueueFindReplace("gml_Object_Var_Create_0", "vrag_spr[0] = 73;", sbW.ToString() + "vrag_spr[0] = 73;");

// ---------------------------------------------------------------------------------------------------------------------
// 3) The flags (main). sp_ok: the requirements hold. sp_pend: one is being built right now. sp_can: you may build it.
//    sp_on: it stands and the requirements hold, so the bonus works (and the option is ON).
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueAppend("gml_Object_main_Create_0",
    "\nfor (var _sc = 0; _sc < 4; _sc++)\n{\n    sp_ok[_sc] = 0;\n    sp_pend[_sc] = 0;\n    sp_can[_sc] = 0;\n    sp_on[_sc] = 0;\n}\n");

var sbS = new System.Text.StringBuilder("tile_num = instance_number(Land);\n");
for (int k = 0; k < N; k++) sbS.Append("sp_ok[" + k + "] = " + reqAll[k] + ";\nsp_pend[" + k + "] = 0;\n");
sbS.Append("with (Land)\n{\n    if (_BLD >= 29 && _BLD <= 32)\n    {\n        other.sp_pend[_BLD - 29] = 1;\n    }\n}\n");
sbS.Append("for (var _sk = 0; _sk < 4; _sk++)\n{\n");
sbS.Append("    sp_can[_sk] = (sp_ok[_sk] && Var.bcnt[28 + _sk] == 0 && !sp_pend[_sk] && global.qol_nb);\n");
sbS.Append("    sp_on[_sk] = (sp_ok[_sk] && Var.bcnt[28 + _sk] > 0 && global.qol_nb);\n}\n");
importGroup.QueueFindReplace("gml_Object_main_Step_0", "tile_num = instance_number(Land);", sbS.ToString());

// ---------------------------------------------------------------------------------------------------------------------
// 4) Land Create / Draw: the menu.
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueAppend("gml_Object_Land_Create_0", "\nbms = 0;\n");

// Tile menu (men = 0): five buttons now. They are 38 apart instead of 40 so that all five fit into the 199 px tile.
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "bm = DrawBtn(x + 10, y + 10, g(\"Построить\", \"Build\"));",
    "bm = DrawBtn(x + 10, y + 10, g(\"Построить\", \"Build\"));\nbms = 0;\nif (global.qol_nb)\n{\n    bms = DrawBtn(x + 10, y + 10 + 152, g(\"Пст. Особое\", \"Bld. Special\"));\n}");
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "bmk = DrawBtn(x + 10, y + 10 + 40, g(\"Пст. Каменное\", \"Bld. Stone\"));",
    "bmk = DrawBtn(x + 10, y + 10 + 38, g(\"Пст. Каменное\", \"Bld. Stone\"));");
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "bmh = DrawBtn(x + 10, y + 10 + 80, g(\"Пст. Геройск\", \"Bld. Heroic\"));",
    "bmh = DrawBtn(x + 10, y + 10 + 76, g(\"Пст. Геройск\", \"Bld. Heroic\"));");
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "bmw = DrawBtn(x + 10, y + 10 + 120, g(\"Пст. Морское\", \"Bld. Marine\"));",
    "bmw = DrawBtn(x + 10, y + 10 + 114, g(\"Пст. Морское\", \"Bld. Marine\"));");

// The special page (men = 9): the four buildings; a button you cannot press right now is dark red.
var sbP = new System.Text.StringBuilder("else if (men == 9)\n{\n    var _sc = 0;\n");
for (int k = 0; k < N; k++)
{
    sbP.Append("    _sc = 0;\n    if (!main.sp_can[" + k + "])\n    {\n        _sc = c_maroon;\n    }\n");
    sbP.Append("    b[" + (V0 + k) + "] = DrawBtnPrg(x + 10, y + 10 + " + (40 * k) + ", " + G(nameRu[k], nameEn[k]) + ", bld_P[" + (V0 + k) + "], _sc);\n");
}
sbP.Append("    draw_set_font(font1);\n    bms = 0;\n    nb_more = 0;\n    nb_back = 0;\n");
sbP.Append("    b_cc = DrawBtnSml(x + 99, y + 176, g(\"Отмена\", \"Cancel\"));\n}\n");
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "else if (men == 8)", sbP.ToString() + "else if (men == 8)");

// The tile faces: in front of the first face of 17 (so in front of the shared Spd / Clear / autohire controls, see 09).
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
    sbF.Append("    draw_set_color(c_yellow);\n    draw_rectangle(x + 3, y + 3, (x + w) - 3, (y + h) - 3, true);\n");   // the gold frame of a special building
    sbF.Append("    draw_set_color(cc);\n    draw_set_font(font0);\n");
    sbF.Append("    draw_sprite(" + spriteName[k] + ", 0, x + 19, y + 19);\n");
    sbF.Append("    name = " + G(nameRu[k], nameEn[k]) + ";\n");
    sbF.Append("    draw_text(x + 41, y + 7, name);\n");
    sbF.Append("    draw_set_font(font1);\n");
    sbF.Append("    b[0] = DrawBtnPrg(x + 10, y + 40, " + G(hireRu[k], hireEn[k]) + ", rab_P, cr);\n");
    sbF.Append("    draw_set_font(font0);\n");
    sbF.Append("    draw_set_color(cc);\n");
    sbF.Append("    if (rab)\n    {\n");
    sbF.Append("        draw_text(x + 5, y + 76, " + G(nounRu[k], nounEn[k]) + " + string(rab));\n");
    sbF.Append("    }\n");
    sbF.Append("    draw_text_ext(x + 5, y + 92, " + effect[k] + ", 14, w - 9);\n");
    sbF.Append("    if (!global.qol_nb)\n    {\n");
    sbF.Append("        draw_set_color(c_gray);\n");
    sbF.Append("        draw_text_ext(x + 5, y + 134, g(\"(выключено в настройках QoL)\", \"(switched off in QoL options)\"), 14, w - 9);\n");
    sbF.Append("    }\n");
    sbF.Append("    else if (!main.sp_ok[" + k + "])\n    {\n");
    sbF.Append("        draw_set_color(c_red);\n");
    sbF.Append("        draw_text_ext(x + 5, y + 134, g(\"Требования не выполнены: бонус не действует\", \"Requirements not met: bonus paused\"), 14, w - 9);\n");
    sbF.Append("    }\n");
    sbF.Append("}\n");
}
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "if (BLD == 19)", sbF.ToString() + "if (BLD == 19)");

// ---------------------------------------------------------------------------------------------------------------------
// 5) Land Step: the page switch, the requirements in the tooltip, and the block that refuses a build whose requirements fail.
// ---------------------------------------------------------------------------------------------------------------------
// (a) the "Bld. Special" button opens page 9; like the other category flags, it is cleared while a page is open
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "bmw = 0;", "bmw = 0;\n        bms = 0;");
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "if (bmw)",
    "if (bms)\n    {\n        if (mouse_check_button_pressed(mb_left))\n        {\n            men = 9;\n        }\n    }\n    if (bmw)");

// (b) the tooltip of a special building's button: "Requirements:" with [OK] / [X], in front of the description. (The same line
//     is also in the game's free-first-farm branch, where i is never >= 28, so the extra text is never reached there.)
var sbT = new System.Text.StringBuilder("if (i >= 28 && i <= 31)\n{\n    var _sk = i - 28;\n");
sbT.Append("    tx += (\"##\" + g(\"Требования:\", \"Requirements:\"));\n");
for (int k = 0; k < N; k++)
{
    sbT.Append("    if (_sk == " + k + ")\n    {\n");
    foreach (var r in req[k])
    {
        sbT.Append("        tx += (\"#- \" + " + G(r[0], r[1]) + " + g(\" (у вас \", \" (you have \") + string(" + r[2] + ") + \"): \");\n");
        sbT.Append("        if (" + r[3] + ")\n        {\n            tx += \"[OK]\";\n        }\n        else\n        {\n            tx += \"[X]\";\n        }\n");
    }
    sbT.Append("    }\n");
}
sbT.Append("    tx += (\"#- \" + g(\"Только один (построено: \", \"Only one (built: \") + string(Var.bcnt[i]) + \"): \");\n");
sbT.Append("    if (Var.bcnt[i] == 0 && !main.sp_pend[_sk])\n    {\n        tx += \"[OK]\";\n    }\n    else\n    {\n        tx += \"[X]\";\n    }\n");
sbT.Append("}\n");
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "if (Var.binfo[i] != \"\")", sbT.ToString() + "if (Var.binfo[i] != \"\")");

// (c) the build itself: dflag is the game's own "may be built" flag (the Forge uses it for the great blacksmith)
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "var dflag = 1;",
    "var dflag = 1;\nif (i >= 28 && i <= 31 && !main.sp_can[i - 28])\n{\n    dflag = 0;\n}");

// (d) no free starting workers from the Indian Empire perk for special buildings either (they are not for the heroes' list
//     of 17 but they are not ordinary buildings: the first workers cost billions)
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "BLD != 23 && BLD != 28)", "BLD != 23 && BLD != 28 && BLD < 29)");

// ---------------------------------------------------------------------------------------------------------------------
// 6) The bonuses.
// ---------------------------------------------------------------------------------------------------------------------
// Production (main Step, with the other multipliers): Hermitage (wheat, wood, stone), Mint (gold), Sanctum (faith).
importGroup.QueueFindReplace("gml_Object_main_Step_0", "if (main.muta[3])",
    @"if (sp_on[0])
{
    var _sm = 2 * (1 + (0.05 * Var.rcnt[28]));
    d_zern *= _sm;
    d_les *= _sm;
    d_kam *= _sm;
}
if (sp_on[1])
{
    d_gold *= (2 * (1 + (0.05 * Var.rcnt[29])));
}
if (sp_on[3])
{
    d_ver *= (2 * (1 + (0.05 * Var.rcnt[31])));
}
if (main.muta[3])");

// Gold for abdication: Mint.
importGroup.QueueFindReplace("gml_Script_get_compen", "d *= (1 + (0.01 * Var.rcnt[21]));",
    "d *= (1 + (0.01 * Var.rcnt[21]));\n    if (main.sp_on[1])\n    {\n        d *= (2 * (1 + (0.05 * Var.rcnt[29])));\n    }");

// Hero HP and damage: Citadel.
importGroup.QueueFindReplace("gml_Script_AfterShlem", "O.hero_DAM = ceil(O.hero_DAM * (1 + (0.005 * Var.rcnt[27])));",
    "O.hero_DAM = ceil(O.hero_DAM * (1 + (0.005 * Var.rcnt[27])));\n    if (main.sp_on[2])\n    {\n        O.hero_HP = ceil(O.hero_HP * (2 * (1 + (0.04 * Var.rcnt[30]))));\n        O.hero_DAM = ceil(O.hero_DAM * (2 * (1 + (0.04 * Var.rcnt[30]))));\n    }");

// Ritual duration: Sanctum, +60 s.
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", "(Var.rcnt[20] * global.qol_nb);",
    "(Var.rcnt[20] * global.qol_nb) + (60 * main.sp_on[3]);");

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


// Icons of the four SPECIAL buildings (uses IconCanvas from icons.cs).
public static class SpecialIcons
{
    static uint C(int r, int g, int b) { return IconCanvas.C(r, g, b); }

    // Hermitage: a hermit's hut with a glowing window, a pine and the moon
    public static IconCanvas Hut()
    {
        IconCanvas k = new IconCanvas();
        uint wall = C(214, 200, 165), wallDk = C(165, 150, 118), roof = C(150, 98, 52), roofHi = C(200, 145, 75), door = C(85, 55, 35), glow = C(255, 225, 110);
        k.Disc(25.5, 6.5, 4.4, C(250, 245, 200));                                      // moon: a disc with a bite taken out
        k.Disc(27.5, 5.3, 3.8, 0);
        k.Poly(new double[] { 26, 28, 28.5, 18, 31, 28 }, C(45, 110, 60));            // pine (right)
        k.Poly(new double[] { 25, 22, 28.5, 12, 32, 22 }, C(55, 130, 70));
        k.Rect(28, 28, 29, 30.5, C(90, 60, 35));
        k.Rect(5, 17, 22, 28.5, wall);                                                  // walls
        k.Rect(19.5, 17, 22, 28.5, wallDk);
        k.Poly(new double[] { 3, 18, 13.5, 7, 24, 18 }, roof);                          // roof
        k.Poly(new double[] { 3, 18, 13.5, 7, 13.5, 18 }, roofHi);
        k.Rect(17.5, 6, 19.5, 11.5, C(120, 120, 130));                                  // chimney
        k.Rect(10.5, 21, 15.5, 28.5, door);                                             // door
        k.Rect(7, 19.5, 9.5, 22, glow);                                                 // window
        k.Rect(17, 20, 20, 23, glow);
        k.Rect(2, 28.5, 31, 30.5, C(80, 130, 70));                                      // ground
        k.Outline(C(35, 25, 20));
        return k;
    }

    // Imperial Mint: a gold coin stamped with a crown, and a hammer
    public static IconCanvas Mint()
    {
        IconCanvas k = new IconCanvas();
        uint gold = C(250, 205, 55), rim = C(200, 140, 25), hi = C(255, 240, 150), dk = C(150, 100, 15);
        k.Disc(13.5, 15.5, 12.4, rim);
        k.Disc(13.5, 15.5, 10.8, gold);
        k.Ring(13.5, 15.5, 8.6, 9.6, hi);
        // crown
        k.Poly(new double[] { 7, 19.5, 6.5, 10.5, 10, 13.5, 13.5, 8.5, 17, 13.5, 20.5, 10.5, 20, 19.5 }, dk);
        k.Rect(7, 19.5, 20, 21.5, dk);
        k.Disc(6.6, 10.2, 1.2, hi); k.Disc(13.5, 8.2, 1.2, hi); k.Disc(20.4, 10.2, 1.2, hi);
        // hammer: wooden handle, steel head
        k.Line(15, 30, 27, 18, 3.4, C(150, 100, 55));
        k.Line(15.6, 29.4, 26.4, 18.6, 1.0, C(205, 150, 85));
        k.Line(22.5, 13.5, 30.5, 21.5, 6.5, C(160, 168, 185));
        k.Line(23, 14, 30, 21, 2.0, C(215, 222, 235));
        k.Outline(C(70, 45, 5));
        return k;
    }

    // Citadel: a castle with a banner
    public static IconCanvas Castle()
    {
        IconCanvas k = new IconCanvas();
        uint stone = C(150, 160, 178), stoneHi = C(190, 200, 215), stoneDk = C(100, 108, 125), red = C(210, 50, 50), door = C(55, 45, 50);
        k.Rect(2.5, 15, 29.5, 29.5, stone);                                             // curtain wall
        for (int i = 0; i < 5; i++) k.Rect(2.5 + i * 6.2, 12, 5.4 + i * 6.2, 15, stone);  // battlements
        k.Rect(11, 7, 21, 29.5, stone);                                                 // keep
        for (int i = 0; i < 3; i++) k.Rect(11 + i * 4.3, 4.2, 13.4 + i * 4.3, 7, stone);
        k.Rect(11, 7, 13, 29.5, stoneHi);
        k.Rect(19, 7, 21, 29.5, stoneDk);
        k.Rect(2.5, 15, 4, 29.5, stoneHi);
        k.Rect(28, 15, 29.5, 29.5, stoneDk);
        k.Poly(new double[] { 12.5, 29.5, 12.5, 23, 16, 20, 19.5, 23, 19.5, 29.5 }, door);   // gate
        k.Rect(15.2, 10, 16.8, 15, door);                                               // arrow slit
        k.Rect(6, 19, 7.2, 23, door); k.Rect(24.8, 19, 26, 23, door);
        for (int i = 0; i < 4; i++) k.Line(4 + i * 7.5, 26.2, 8 + i * 7.5, 26.2, 0.6, stoneDk);
        k.Line(16, 4.2, 16, 0.5, 1.2, C(150, 150, 160));                                // pole and flag
        k.Poly(new double[] { 16.6, 0.5, 25, 2.4, 16.6, 4.4 }, red);
        k.Outline(C(25, 28, 40));
        return k;
    }

    // Grand Sanctum: a radiant star above temple columns
    public static IconCanvas Sanctum()
    {
        IconCanvas k = new IconCanvas();
        uint gold = C(255, 215, 70), glow = C(255, 245, 190), white = C(235, 225, 250), shade = C(160, 140, 205), base_ = C(120, 100, 170);
        for (int i = 0; i < 8; i++)
        {
            double a = i * Math.PI / 4;
            k.Line(16, 9.5, 16 + Math.Cos(a) * 9.5, 9.5 + Math.Sin(a) * 9.5, i % 2 == 0 ? 2.4 : 1.4, gold);
        }
        k.Disc(16, 9.5, 5.2, gold);
        k.Disc(16, 9.5, 3.4, glow);
        k.Poly(new double[] { 4, 20, 16, 14.5, 28, 20 }, white);                        // pediment
        k.Poly(new double[] { 16, 14.5, 28, 20, 16, 20 }, shade);
        k.Rect(4, 20, 28, 21.5, shade);
        for (int i = 0; i < 4; i++)                                                      // columns
        {
            double cx = 6.5 + i * 6.3;
            k.Rect(cx - 1.6, 21.5, cx + 1.6, 28, white);
            k.Rect(cx + 0.4, 21.5, cx + 1.6, 28, shade);
        }
        k.Rect(3, 28, 29, 30.5, base_);
        k.Outline(C(40, 25, 70));
        return k;
    }

    public static IconCanvas[] All()
    {
        return new IconCanvas[] { Hut(), Mint(), Castle(), Sanctum() };
    }
}
