// QoL: new GODS (pantheons), RELICS, MUTATIONS and EMPIRE bonuses.
// Switch: global.qol_div (Options > QoL Features > "Divine extras"). Default ON. OFF hides the new entries and pauses their
// effects; what you already chose stays chosen (and spent), and comes back when you switch ON.
//
// GODS: the pantheon window is built from arrays (pan_N, panI = display order, panC cost, panN name, panU colour, panT text),
// so twelve new gods are added at the end of the list (ids 11..22). Their effects are checks of MyPan(id) in the right places.
//   id  god                    cost (faith)   effect
//   11  Goddess of the Harvest 50,000         +25% wheat, wood and stone production
//   12  God of Commerce        250,000        +50% gold per second, +25% gold for abdication
//   13  Goddess of Fortune     1,200,000      -15% cost of all workers
//   14  Goddess of the Moon    8,000,000      +3 ritual power, +3 ritual automation
//   15  God of Time            60,000,000     -30% hiring time in all buildings
//   16  God of the Forge       400,000,000    heroes get +25% HP, damage and armor
//   17  God of the Deep        2,500,000      +50% stone production
//   18  Goddess of the Sea     15,000,000     +1 gold per second per ship
//   19  God of Wisdom          120,000,000    +50% hero experience, -20% hero level-up cost
//   20  God of Rites           700,000,000    -30% ritual cost, +30 seconds ritual duration
//   21  Goddess of Devotion    4,000,000,000  +25% to all production while a ritual is running
//   22  God of Gold            20,000,000,000 x2 gold per second, +50% gold for abdication
//
// RITUALS: three new ritual types in the ritual window (the game had six; the circle of icons simply gets nine). Each is a bit
// of main.o_type, so costs, auto-casting, saving and offline catch-up already work for them. New 64x64 icons (on / off) are
// drawn below and appended to the game's ritual sprites sRit / sRitOff as frames 6..8.
//   6   Gold ritual     @x gold production during the ritual   (@ = 5 + ritual power, like the game's own wheat/wood/stone rituals)
//   7   Faith ritual    @x faith production during the ritual (holiness not counted)
//   8   Wisdom ritual   @x hero experience from kills during the ritual
//
// RELICS: three new "ritual relics" (RELIC[7..9], bought with faith like the first two) on a second row of the relic window.
// Their 64x64 art (three frames each: normal, hovered, owned) is drawn below and added to the game as new sprites.
//   7   Relic of Abundance     20B faith      +25% wheat, wood, stone and faith production
//   8   Relic of Fortune       40B faith      +10 gold per second, +25% gold for abdication
//   9   Relic of Valor         80B faith      heroes +25% HP and damage, -25% hero level-up cost
//
// MUTATIONS: three more cards (ids 12..14) on a fifth row of the mutation window (the window gets taller). As in the game,
// mutations are only available on other planets, or with the Amber Shop's extra mutation.
//   12  Giant heart            x3 hero HP and damage
//   13  Gilded blood           x4 gold per second, +25% gold for abdication
//   14  Chimera                +5% to all production per tile (on this continent)
//
// EMPIRE: new rows (columns 9 and up), five levels each (cost 1, 2, 3, 4, 10 points, like all rows). They are open from the
// start (no spent-points requirement). The window cannot hold more than the game's nine rows, so it gets TWO TABS: "Empires I"
// is the game's own window, untouched (nine rows, the Cosmos lock, the planet bonus); "Empires II" holds the new rows, six of
// them, from the same code. The "all rows maxed -> Cosmos" rule still looks at the first nine rows only. The two Empires of
// this file are rows 9 and 10; rows 11 to 14 are in 15_new_empires.csx.
//   9   Mongolian: heroes +20% HP and damage | -20% monsters to capture a tile | -30% hero level-up cost |
//                  every kill gives 0.1 gold per tile you own | 2x experience from kills
//   10  Persian:   +20% faith | +1 ritual power and automation | -10% cost of all workers | +25% gold for abdication |
//                  +15% to all production
//
// Effects are spread over: main Step (production), get_compen (abdication gold), getCost (workers), HireRabs + Land Step
// (hiring time), AfterShlem (hero stats), LandBattle (monsters per tile, kill gold, experience), Land Step (hero level cost).

using ImageMagick;
using UndertaleModLib.Util;

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string G(string ru, string en) => "g(\"" + ru + "\", \"" + en + "\")";
const string DIV = "global.qol_div";

// 1) Setting, read from the "opt" ini ([QOL] div, default ON) at the very top of main Create.
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    @"VERSION = 167;
if (!variable_global_exists(""qol_div""))
{
    ini_open(""opt"");
    global.qol_div = ini_read_real(""QOL"", ""div"", 1);
    ini_close();
}");

// =====================================================================================================================
// 2) GODS
// =====================================================================================================================
var gods = new (int id, string nameRu, string nameEn, double cost, string color, string textRu, string textEn)[]
{
    (11, "Богиня урожая",   "Goddess of the Harvest", 50000,     "merge_color(c_lime, c_yellow, 0.4)",
        "+25% к производству зерна, дерева и камня",              "+25% wheat, wood and stone production"),
    (12, "Бог торговли",    "God of Commerce",        250000,    "merge_color(c_yellow, c_orange, 0.3)",
        "+50% к золоту в секунду#+25% золота за отречение",       "+50% gold production per second#+25% gold for abdication"),
    (13, "Богиня удачи",    "Goddess of Fortune",     1200000,   "merge_color(c_fuchsia, c_white, 0.6)",
        "-15% к стоимости всех рабочих",                          "-15% cost of all workers"),
    (14, "Богиня луны",     "Goddess of the Moon",    8000000,   "merge_color(c_aqua, c_white, 0.45)",
        "+3 к силе обряда#+3 к автоматизации обрядов",            "+3 ritual power#+3 ritual automation"),
    (15, "Бог времени",     "God of Time",            60000000,  "merge_color(c_ltgray, c_blue, 0.3)",
        "-30% ко времени найма во всех зданиях",                  "-30% hiring time in all buildings"),
    (16, "Бог кузницы",     "God of the Forge",       400000000, "merge_color(c_orange, c_red, 0.4)",
        "Герои получают +25% к HP, урону и броне",                "Heroes get +25% HP, damage and armor"),
    (17, "Бог недр",        "God of the Deep",        2500000,   "merge_color(c_gray, c_aqua, 0.3)",
        "+50% к производству камня",                              "+50% stone production"),
    (18, "Богиня морей",    "Goddess of the Sea",     15000000,  "merge_color(c_blue, c_aqua, 0.5)",
        "+1 золота в секунду за каждый корабль",                  "+1 gold per second per ship"),
    (19, "Бог мудрости",    "God of Wisdom",          120000000, "merge_color(c_olive, c_white, 0.5)",
        "+50% к опыту героев#-20% к стоимости повышения уровня героя", "+50% hero experience#-20% hero level up cost"),
    (20, "Бог обрядов",     "God of Rites",           700000000, "merge_color(c_purple, c_fuchsia, 0.5)",
        "-30% к стоимости обряда#+30 секунд к длительности обряда",   "-30% ritual cost#+30 seconds to ritual duration"),
    (21, "Богиня преданности", "Goddess of Devotion", 4000000000, "merge_color(c_white, c_aqua, 0.4)",
        "+25% ко всему производству во время обряда",             "+25% to all production while a ritual is running"),
    (22, "Бог золота",      "God of Gold",            20000000000, "merge_color(c_yellow, c_white, 0.15)",
        "x2 к золоту в секунду#+50% золота за отречение",         "x2 gold per second#+50% gold for abdication"),
};
// pan_N is the number of gods in the list: the new ones are only counted while the switch is ON.
importGroup.QueueFindReplace("gml_Object_main_Create_0", "pan_N = 11;", "pan_N = 11 + (12 * " + DIV + ");");
var sbG = new System.Text.StringBuilder("panI[10] = 9;\n");
foreach (var gd in gods)
{
    sbG.Append("panI[" + gd.id + "] = " + gd.id + ";\n");
    sbG.Append("panC[" + gd.id + "] = " + gd.cost + ";\n");
    sbG.Append("panN[" + gd.id + "] = " + G(gd.nameRu, gd.nameEn) + ";\n");
    sbG.Append("panU[" + gd.id + "] = " + gd.color + ";\n");
    sbG.Append("panT[" + gd.id + "] = " + G(gd.textRu, gd.textEn) + ";\n");
}
importGroup.QueueFindReplace("gml_Object_main_Create_0", "panI[10] = 9;", sbG.ToString());

// =====================================================================================================================
// 3) RELICS: art + data + window
// =====================================================================================================================
// Three tiles x three frames, 64 x 64 each, packed into a 256 x 256 page. A tile is a rounded square with a border, like the
// game's own relic art; the icon is drawn from simple shapes.
(byte r, byte g, byte b)[] relicColor = { (255, 170, 40), (255, 230, 70), (140, 185, 255) };
string[] relicSprite = { "relic_abundance", "relic_fortune", "relic_valor" };

bool InRoundRect(double x, double y, double size, double radius)
{
    if (x < 0 || y < 0 || x >= size || y >= size) return false;
    double cx = x < radius ? radius : (x >= size - radius ? size - radius - 1 : x);
    double cy = y < radius ? radius : (y >= size - radius ? size - radius - 1 : y);
    return (x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius;
}
// icon shapes, in tile coordinates (0..63); the centre is (32, 32)
bool IconAbundance(int x, int y)   // a sun: a disk with eight rays
{
    double dx = x - 31.5, dy = y - 31.5, d = Math.Sqrt(dx * dx + dy * dy);
    if (d <= 10.5) return true;
    for (int k = 0; k < 8; k++)
    {
        double a = k * Math.PI / 4, ux = Math.Cos(a), uy = Math.Sin(a);
        double proj = dx * ux + dy * uy, perp = Math.Abs(-dx * uy + dy * ux);
        if (proj >= 14 && proj <= 24 && perp <= (k % 2 == 0 ? 2.6 : 1.6)) return true;
    }
    return false;
}
bool IconFortune(int x, int y)     // a coin: a disk with a groove ring and a gem in the middle
{
    double dx = x - 31.5, dy = y - 31.5, d = Math.Sqrt(dx * dx + dy * dy);
    double m = Math.Abs(dx) + Math.Abs(dy);
    if (m <= 7) return true;                    // the gem
    if (m <= 10) return false;                  // a dark outline around it
    if (d >= 13 && d <= 15) return false;       // the groove ring
    return d <= 19;
}
bool IconValor(int x, int y)       // a sword pointing up
{
    if (y >= 6 && y <= 36)
    {
        double half = y < 14 ? 0.6 + (y - 6) * 0.2 : 2.2;
        if (Math.Abs(x - 31.5) <= half) return true;
    }
    if (y >= 37 && y <= 40 && x >= 21 && x <= 42) return true;            // guard
    if (y >= 41 && y <= 51 && x >= 30 && x <= 33) return true;            // grip
    double px = x - 31.5, py = y - 55;
    return px * px + py * py <= 11;                                          // pommel
}
Func<int, int, bool>[] relicIcon = { IconAbundance, IconFortune, IconValor };

const int RelicPage = 256;
var relicAtlas = new MagickImage(MagickColors.Transparent, RelicPage, RelicPage);
{
    using var px = relicAtlas.GetPixels();
    for (int t = 0; t < 3; t++)
    {
        var col = relicColor[t];
        for (int f = 0; f < 3; f++)
        {
            int ox = 2 + f * 66, oy = 2 + t * 66;
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    if (!InRoundRect(x, y, 64, 9)) continue;
                    bool border = !InRoundRect(x - 2, y - 2, 60, 7);
                    bool icon = !border && relicIcon[t](x, y);
                    byte r, g, b;
                    if (border)
                    {
                        if (f == 1) { r = 255; g = 255; b = 255; }
                        else if (f == 2) { r = (byte)(col.r * 0.55); g = (byte)(col.g * 0.55); b = (byte)(col.b * 0.55); }
                        else { r = col.r; g = col.g; b = col.b; }
                    }
                    else if (icon)
                    {
                        double k = f == 2 ? 0.7 : 1.0;
                        r = (byte)(col.r * k); g = (byte)(col.g * k); b = (byte)(col.b * k);
                    }
                    else
                    {
                        double k = f == 2 ? 0.22 : 0.0;       // owned: a dark tint of the relic's colour
                        r = (byte)(col.r * k); g = (byte)(col.g * k); b = (byte)(col.b * k);
                    }
                    px.SetPixel(ox + x, oy + y, new byte[] { r, g, b, 255 });
                }
        }
    }
}
string tmpRelic = Path.Combine(Path.GetTempPath(), "territory_idle_mod_relics.png");
relicAtlas.Write(tmpRelic, MagickFormat.Png32);
UndertaleEmbeddedTexture relicPage = new();
relicPage.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
using (MagickImage bgra = TextureWorker.ReadBGRAImageFromFile(tmpRelic))
    relicPage.TextureData.Image = GMImage.FromMagickImage(bgra).ConvertToPng();
Data.EmbeddedTextures.Add(relicPage);
File.Delete(tmpRelic);

// New sprites, shaped like the game's own relic sprites (64 x 64, origin in the centre, axis-aligned mask).
var template = Data.Sprites.ByName("src_PP");
for (int t = 0; t < 3; t++)
{
    if (Data.Sprites.ByName(relicSprite[t]) != null)
        throw new Exception(relicSprite[t] + " already exists - is this patch being applied twice?");
    var spr = new UndertaleSprite();
    spr.Name = Data.Strings.MakeString(relicSprite[t]);
    spr.Width = template.Width; spr.Height = template.Height;
    spr.MarginLeft = template.MarginLeft; spr.MarginRight = template.MarginRight;
    spr.MarginTop = template.MarginTop; spr.MarginBottom = template.MarginBottom;
    spr.Transparent = template.Transparent; spr.Smooth = template.Smooth; spr.Preload = template.Preload;
    spr.BBoxMode = template.BBoxMode; spr.SepMasks = template.SepMasks;
    spr.OriginX = template.OriginX; spr.OriginY = template.OriginY;
    for (int f = 0; f < 3; f++)
    {
        UndertaleTexturePageItem item = new();
        item.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
        item.SourceX = (ushort)(2 + f * 66); item.SourceY = (ushort)(2 + t * 66);
        item.SourceWidth = 64; item.SourceHeight = 64;
        item.TargetX = 0; item.TargetY = 0; item.TargetWidth = 64; item.TargetHeight = 64;
        item.BoundingWidth = 64; item.BoundingHeight = 64;
        item.TexturePage = relicPage;
        Data.TexturePageItems.Add(item);
        UndertaleSprite.TextureEntry entry = new();
        entry.Texture = item;
        spr.Textures.Add(entry);
    }
    foreach (var m in template.CollisionMasks)
    {
        var mask = new UndertaleSprite.MaskEntry();
        mask.Data = (byte[])m.Data.Clone();
        mask.Width = m.Width;
        mask.Height = m.Height;
        spr.CollisionMasks.Add(mask);
    }
    Data.Sprites.Add(spr);
}

// Data in main Create: RELICN, the state, the sprites (by name; they exist now), the tooltips.
importGroup.QueueFindReplace("gml_Object_main_Create_0", "RELICN = 7;", "RELICN = 10;");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "RELIC[6] = 0;", "RELIC[6] = 0;\nRELIC[7] = 0;\nRELIC[8] = 0;\nRELIC[9] = 0;");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "RELS[6] = 140;",
    "RELS[6] = 140;\nRELS[7] = relic_abundance;\nRELS[8] = relic_fortune;\nRELS[9] = relic_valor;");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "rcL_info[0] = \"\";",
    "rc_info[7] = " + G("Стоимость: 20B веры#+25% к итоговому производству зерна, дерева, камня и веры", "Cost: 20B faith#+25% to total wheat, wood, stone and faith production") + ";\n" +
    "rc_info[8] = " + G("Стоимость: 40B веры#+10 золота в секунду#+25% золота за отречение", "Cost: 40B faith#+10 gold per second#+25% gold for abdication") + ";\n" +
    "rc_info[9] = " + G("Стоимость: 80B веры#Герои получают +25% HP и урона#-25% к стоимости повышения уровня героя", "Cost: 80B faith#Heroes get +25% HP and damage#-25% hero level up cost") + ";\n" +
    "rcL_info[7] = \"\";\nrcL_info[8] = \"\";\nrcL_info[9] = \"\";\n" +
    "rcL_info[0] = \"\";");

// The window: three flags, a second row of ritual relics (when the switch is ON), and the purchases.
importGroup.QueueAppend("gml_Object_HRelic_Create_0", "\nbR4 = 0;\nbR5 = 0;\nbR6 = 0;\n");
importGroup.QueueFindReplace(
    "gml_Object_HRelic_Draw_64",
    "bR2 = DrawRelic(1, rx2, ry2);",
    @"bR2 = DrawRelic(1, rx2, ry2);
bR4 = 0;
bR5 = 0;
bR6 = 0;
if (global.qol_div)
{
    bR4 = DrawRelic(7, cX - 92, y1 + 232);
    bR5 = DrawRelic(8, cX, y1 + 232);
    bR6 = DrawRelic(9, cX + 92, y1 + 232);
}");
var relicCost = new[] { 20000000000.0, 40000000000.0, 80000000000.0 };
var sbR = new System.Text.StringBuilder("\n");
for (int t = 0; t < 3; t++)
{
    int id = 7 + t;
    sbR.Append("if (bR" + (4 + t) + " && main.RELIC[" + id + "] == 0 && main.ver >= " + relicCost[t] + ")\n{\n");
    sbR.Append("    main.ver -= " + relicCost[t] + ";\n    main.RELIC[" + id + "] = 1;\n");
    sbR.Append("    p = instance_create(" + (t == 0 ? "cX - 92" : t == 1 ? "cX" : "cX + 92") + ", y1 + 232, relic_eff);\n");
    sbR.Append("    p.sprite_index = main.RELS[" + id + "];\n    instance_destroy();\n}\n");
}
importGroup.QueueAppend("gml_Object_HRelic_Mouse_53", sbR.ToString());

// =====================================================================================================================
// 4) MUTATIONS: three cards on a fifth row; the window grows from 640 to 710 px.
// =====================================================================================================================
importGroup.QueueFindReplace("gml_Object_main_Create_0", "muta_max = 12;", "muta_max = 15;");
importGroup.QueueFindReplace("gml_Object_MutagenOB_Create_0", "y2 = 720;", "y2 = 790;");
importGroup.QueueFindReplace("gml_Object_MutagenOB_Create_0", "info[11] = g(",
    "info[12] = " + G("x3 HP и урон героя", "x3 hero HP and damage") + ";\n" +
    "info[13] = " + G("x4 золота в секунду#+25% золота за отречение", "x4 gold per second#+25% gold for abdication") + ";\n" +
    "info[14] = " + G("+5% ко всему производству#за каждую клетку", "+5% to all production#for each tile on this continent") + ";\n" +
    "info[11] = g(");
importGroup.QueueFindReplace("gml_Object_MutagenOB_Draw_64",
    "DrawMKARD(x1 + 20 + 490, y1 + 80 + 345, 11);",
    @"DrawMKARD(x1 + 20 + 490, y1 + 80 + 345, 11);
if (global.qol_div)
{
    DrawMKARD(x1 + 25, y1 + 80 + 460, 12);
    DrawMKARD(x1 + 20 + 245, y1 + 80 + 460, 13);
    DrawMKARD(x1 + 20 + 490, y1 + 80 + 460, 14);
}");

// =====================================================================================================================
// 5) EMPIRE: more rows and a second tab. emp_c / emp_info are sized by emp_max; the save/load/reset loops ran over 9 rows.
// =====================================================================================================================
importGroup.QueueFindReplace("gml_Object_main_Create_0", "emp_max = 9;", "emp_max = 15;");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "emp_name[8] = \"German\";", "emp_name[8] = \"German\";\nemp_name[9] = \"Mongolian\";\nemp_name[10] = \"Persian\";");
var emp = new (int row, int lvl, string ru, string en)[]
{
    (9, 0, "Герои получают +20% к HP и урону.", "Heroes get +20% HP and damage."),
    (9, 1, "-20% монстров, нужных для захвата клетки.", "-20% monsters to defeat to capture a tile."),
    (9, 2, "-30% к стоимости повышения уровня героя.", "-30% hero level up cost."),
    (9, 3, "Каждое убийство монстра дает 0.1 золота#за каждую вашу клетку.", "Every monster kill gives 0.1 gold#for each tile you own."),
    (9, 4, "2x опыта за убийства.", "2x experience from kills."),
    (10, 0, "+20% к производству веры.", "+20% faith production."),
    (10, 1, "+1 к силе обряда#+1 к автоматизации обрядов", "+1 ritual power#+1 ritual automation"),
    (10, 2, "-10% к стоимости всех рабочих.", "-10% cost of all workers."),
    (10, 3, "+25% золота за отречение.", "+25% gold for abdication."),
    (10, 4, "+15% ко всему производству#(зерно, дерево, камень, вера).", "+15% to all production#(wheat, wood, stone, faith)."),
};
var sbE = new System.Text.StringBuilder();
foreach (var e in emp) sbE.Append("emp_info[" + e.row + ", " + e.lvl + "] = " + G(e.ru, e.en) + ";\n");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "txt_perk[1] = \"IRONMAN#+25% armor.#Can use golden items for free.\";",
    sbE.ToString() + "txt_perk[1] = \"IRONMAN#+25% armor.#Can use golden items for free.\";");

foreach (string entry in new[] { "gml_Object_main_Alarm_11", "gml_Object_main_Alarm_1", "gml_Object_main_Alarm_2", "gml_Object_Empire_Mouse_53" })
    importGroup.QueueFindReplace(entry, "for (var i = 0; i < 9; i++)", "for (var i = 0; i < 15; i++)");

// The window. `tab` is 0 ("Empires I", the game's own rows, drawn by the game's own loop, which is told to draw nothing while
// tab 1 is shown) or 1 ("Empires II", rows 9..14, 50 px apart). The two tab buttons sit on the line of the "Total points" text,
// left and right of it; "Empires II" (and the second tab) exist only while Divine extras is ON. The new rows are open from the
// start (the game's own rows 7 to 9 stay locked behind 20 / 60 / 100 spent points).
importGroup.QueueAppend("gml_Object_Empire_Create_0", "\ntab = 0;\nbT1 = 0;\nbT2 = 0;\n");
importGroup.QueueFindReplace("gml_Object_Empire_Draw_64", "var cosmoflag = 1;",
    @"bT1 = 0;
bT2 = 0;
if (!global.qol_div)
{
    tab = 0;
}
else
{
    var _t1 = g(""Империи I"", ""Empires I"");
    var _t2 = g(""Империи II"", ""Empires II"");
    bT1 = DrawBtnRel(x1 + 14, y1 + 36, _t1, 0, merge_color(uU, c_yellow, tab == 0));
    bT2 = DrawBtnRel((x2 - string_width(_t2)) - 38, y1 + 36, _t2, 0, merge_color(uU, c_yellow, tab == 1));
}
draw_set_color(uU);
draw_set_halign(fa_left);
var cosmoflag = 1;");
importGroup.QueueFindReplace("gml_Object_Empire_Draw_64", "for (var i = 0; i < 9; i++)", "for (var i = 0; i < (9 * (1 - tab)); i++)");
importGroup.QueueFindReplace("gml_Object_Empire_Draw_64", "if (!main.COSMOS)",
    @"if (tab == 1)
{
    draw_set_color(uU);
    for (var i = 9; i < 15; i++)
    {
        draw_empire(x1 + 25, y1 + 96 + (50 * (i - 9)), i);
    }
}
if (!main.COSMOS && tab == 0)");
importGroup.QueueFindReplace("gml_Object_Empire_Draw_64", "if (global.PLANET)", "if (global.PLANET && tab == 0)");
importGroup.QueueFindReplace("gml_Object_Empire_Mouse_53", "if (bres && main.empire_respec > 0)",
    "if (bT1)\n{\n    tab = 0;\n}\nif (bT2 && " + DIV + ")\n{\n    tab = 1;\n}\nif (bres && main.empire_respec > 0)");

// =====================================================================================================================
// 6) THE EFFECTS (all multiplied / gated by the switch)
// =====================================================================================================================
// Production: in front of the Fame Shop's gold-from-resources line, after every other multiplier of the step.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "d_fgold = 0.01 * (global.fs_lv[7] * global.qol_fame) * (d_zern + d_les + d_kam + d_ver);",
    @"if (global.qol_div)
{
    if (MyPan(11))
    {
        d_zern *= 1.25;
        d_les *= 1.25;
        d_kam *= 1.25;
    }
    if (MyPan(12))
    {
        d_gold *= 1.5;
    }
    if (MyPan(17))
    {
        d_kam *= 1.5;
    }
    if (MyPan(18))
    {
        d_gold += Var.rcnt[8];
    }
    if (MyPan(21) && o_time > 0)
    {
        d_zern *= 1.25;
        d_les *= 1.25;
        d_kam *= 1.25;
        d_ver *= 1.25;
    }
    if (MyPan(22))
    {
        d_gold *= 2;
    }
    if (RELIC[7])
    {
        d_zern *= 1.25;
        d_les *= 1.25;
        d_kam *= 1.25;
        d_ver *= 1.25;
    }
    if (RELIC[8])
    {
        d_gold += 10;
    }
    if (main.muta[13])
    {
        d_gold *= 4;
    }
    if (main.muta[14])
    {
        var _chim = 1 + (0.05 * tile_num);
        d_zern *= _chim;
        d_les *= _chim;
        d_kam *= _chim;
        d_ver *= _chim;
    }
    if (MyEmp(10, 0))
    {
        d_ver *= 1.2;
    }
    if (MyEmp(10, 4))
    {
        d_zern *= 1.15;
        d_les *= 1.15;
        d_kam *= 1.15;
        d_ver *= 1.15;
    }
}
d_fgold = 0.01 * (global.fs_lv[7] * global.qol_fame) * (d_zern + d_les + d_kam + d_ver);");

// Gold for abdication (the game computes it in get_compen, with self = main).
importGroup.QueueFindReplace(
    "gml_Script_get_compen",
    "if (main.muta[10])",
    @"if (global.qol_div)
{
    if (MyPan(12))
    {
        d *= 1.25;
    }
    if (MyPan(22))
    {
        d *= 1.5;
    }
    if (main.RELIC[8])
    {
        d *= 1.25;
    }
    if (main.muta[13])
    {
        d *= 1.25;
    }
    if (MyEmp(10, 3))
    {
        d *= 1.25;
    }
}
if (main.muta[10])");

// Cost of every worker (one function, getCost): Goddess of Fortune and the Persian empire.
importGroup.QueueFindReplace(
    "gml_Script_getCost",
    "* (1 - (0.1 * (global.fs_lv[1] * global.qol_fame)));",
    "* (1 - (0.1 * (global.fs_lv[1] * global.qol_fame))) * (1 - (0.15 * MyPan(13) * " + DIV + ")) * (1 - (0.1 * MyEmp(10, 2) * " + DIV + "));");

// Ritual power (rit_SILA) and ritual automation (rit_power) are recomputed every step.
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "(2 * MyTri(2, 1)) + (main.muta[2] * 5);",
    "(2 * MyTri(2, 1)) + (main.muta[2] * 5) + (3 * MyPan(14) * " + DIV + ") + (MyEmp(10, 1) * " + DIV + ");");
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "tial_les + (main.muta[2] * 5);",
    "tial_les + (main.muta[2] * 5) + (3 * MyPan(14) * " + DIV + ") + (MyEmp(10, 1) * " + DIV + ");");

// Hiring time: the God of Time. The factor is appended to the Library's (09_new_buildings.csx) in both places.
string libTail = " / (1 + (0.02 * Var.rcnt[15] * global.qol_nb))";
string timeTail = libTail + " / (1 + (0.43 * MyPan(15) * " + DIV + "))";
importGroup.QueueFindReplace("gml_Object_Land_Step_0", libTail, timeTail);
importGroup.QueueFindReplace("gml_Script_HireRabs", libTail, timeTail);

// Hero stats (AfterShlem, in front of the one-battle boost like the game's own class changes): the God of the Forge, the
// Relic of Valor, the Giant heart mutation and the Mongolian empire.
importGroup.QueueFindReplace(
    "gml_Script_AfterShlem",
    "if (global.D_BTL > 0 && global.qol_ab[8])",
    @"if (global.qol_div)
{
    if (MyPan(16))
    {
        O.hero_HP = ceil(O.hero_HP * 1.25);
        O.hero_DAM = ceil(O.hero_DAM * 1.25);
        O.hero_SHI = ceil(O.hero_SHI * 1.25);
    }
    if (main.RELIC[9])
    {
        O.hero_HP = ceil(O.hero_HP * 1.25);
        O.hero_DAM = ceil(O.hero_DAM * 1.25);
    }
    if (main.muta[12])
    {
        O.hero_HP = ceil(O.hero_HP * 3);
        O.hero_DAM = ceil(O.hero_DAM * 3);
    }
    if (MyEmp(9, 0))
    {
        O.hero_HP = ceil(O.hero_HP * 1.2);
        O.hero_DAM = ceil(O.hero_DAM * 1.2);
    }
}
if (global.D_BTL > 0 && global.qol_ab[8])");

// Monsters needed per tile (the Watchtower's factor, already in all three places): the Mongolian empire.
string killTail = "power(0.993092, Var.rcnt[17] * global.qol_nb)";
foreach (string entry in new[] { "gml_Object_LandBattle_Create_0", "gml_Object_LandBattle_Other_10", "gml_Script_empire_pick_s" })
    importGroup.QueueFindReplace(entry, killTail, killTail + " * (1 - (0.2 * MyEmp(9, 1) * " + DIV + "))");

// Hero level-up cost: the Relic of Valor and the Mongolian empire.
importGroup.QueueFindReplace("gml_Object_Land_Step_0",
    "var L_cst = 1000000 * sqr(hero_lvl + 1);",
    "var L_cst = 1000000 * sqr(hero_lvl + 1) * (1 - (0.25 * main.RELIC[9] * " + DIV + ")) * (1 - (0.3 * MyEmp(9, 2) * " + DIV + ")) * (1 - (0.2 * MyPan(19) * " + DIV + "));");

// Kills: gold (Mongolian) and double experience (Mongolian).
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "if (O.hero_wp == 39)",
    "if (MyEmp(9, 3) && " + DIV + ")\n{\n    global.gold += 0.1 * main.tile_num;\n}\nif (O.hero_wp == 39)");
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "if (MyTri(3, 0) && MyRel(3, 0))",
    "if (MyEmp(9, 4) && " + DIV + ")\n{\n    class_k *= 2;\n}\n" +
    "if (MyPan(19) && " + DIV + ")\n{\n    class_k *= 1.5;\n}\n" +
    "if (" + DIV + " && main.o_time > 0 && bit_test(main.o_type, 8))\n{\n    class_k *= (5 + main.rit_SILA);\n}\n" +
    "if (MyTri(3, 0) && MyRel(3, 0))");

// =====================================================================================================================
// 7) RITUALS: three new types in the ritual window
// =====================================================================================================================
// The window draws one icon per type in a circle (nRIT of them), from the sprites sRit (selected) and sRitOff, frame = type.
// The icons are drawn here: a white (selected) or grey (not selected) symbol in a round frame on black, 64 x 64 like the game's.
bool IconRitGold(int x, int y)      // a coin with a gem
{
    double dx = x - 31.5, dy = y - 31.5, d = Math.Sqrt(dx * dx + dy * dy), m = Math.Abs(dx) + Math.Abs(dy);
    if (m <= 6) return true;
    if (m <= 9) return false;
    if (d >= 10.5 && d <= 12.5) return false;
    return d <= 17;
}
bool IconRitFaith(int x, int y)     // a four-pointed star
{
    double dx = Math.Abs(x - 31.5), dy = Math.Abs(y - 31.5);
    return Math.Sqrt(dx) + Math.Sqrt(dy) <= 5.0;
}
bool IconRitWisdom(int x, int y)    // an open book: two pages with lines of text
{
    foreach (int side in new[] { -1, 1 })
    {
        double u = side < 0 ? 30.5 - x : x - 32.5;          // distance from the spine, 0 .. 19
        if (u < 0 || u > 19.5) continue;
        double top = 20 + 0.3 * u, bottom = 42 + 0.3 * u;
        if (y < top || y > bottom) continue;
        bool line = u >= 3 && u <= 16 && ((int)(y - top) % 5) == 3;
        return !line;
    }
    return false;
}
Func<int, int, bool>[] ritIcon = { IconRitGold, IconRitFaith, IconRitWisdom };

var ritAtlas = new MagickImage(MagickColors.Transparent, 256, 256);
{
    using var px = ritAtlas.GetPixels();
    for (int t = 0; t < 3; t++)
        for (int state = 0; state < 2; state++)               // 0 = selected (white), 1 = not selected (grey)
        {
            int ox = 2 + t * 66, oy = 2 + state * 66;
            byte c = state == 0 ? (byte)255 : (byte)150;
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    double dx = x - 31.5, dy = y - 31.5, d = Math.Sqrt(dx * dx + dy * dy);
                    if (d > 32) continue;
                    bool on = d >= 28.5 || ritIcon[t](x, y);
                    byte v = on ? c : (byte)0;
                    px.SetPixel(ox + x, oy + y, new byte[] { v, v, v, 255 });
                }
        }
}
string tmpRit = Path.Combine(Path.GetTempPath(), "territory_idle_mod_rituals.png");
ritAtlas.Write(tmpRit, MagickFormat.Png32);
UndertaleEmbeddedTexture ritPage = new();
ritPage.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
using (MagickImage bgra = TextureWorker.ReadBGRAImageFromFile(tmpRit))
    ritPage.TextureData.Image = GMImage.FromMagickImage(bgra).ConvertToPng();
Data.EmbeddedTextures.Add(ritPage);
File.Delete(tmpRit);
for (int state = 0; state < 2; state++)
{
    var rs = Data.Sprites.ByName(state == 0 ? "sRit" : "sRitOff");
    if (rs.Textures.Count != 6)
        throw new Exception(rs.Name.Content + " does not have 6 frames - is this patch being applied twice?");
    for (int t = 0; t < 3; t++)
    {
        UndertaleTexturePageItem item = new();
        item.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
        item.SourceX = (ushort)(2 + t * 66); item.SourceY = (ushort)(2 + state * 66);
        item.SourceWidth = 64; item.SourceHeight = 64;
        item.TargetX = 0; item.TargetY = 0; item.TargetWidth = 64; item.TargetHeight = 64;
        item.BoundingWidth = 64; item.BoundingHeight = 64;
        item.TexturePage = ritPage;
        Data.TexturePageItems.Add(item);
        UndertaleSprite.TextureEntry entry = new();
        entry.Texture = item;
        rs.Textures.Add(entry);
    }
}

// The window and the cost: nRIT, the tooltips, and the two loops that still ran over six types.
importGroup.QueueFindReplace("gml_Object_HRit_Create_0", "nRIT = 6;", "nRIT = 6 + (3 * " + DIV + ");");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "rit_txt[0] = g(",
    "rit_txt[6] = " + G("@x производства золота во время ритуала", "@x gold production during ritual") + ";\n" +
    "rit_txt[7] = " + G("@x производства веры во время ритуала", "@x faith production during ritual") + ";\n" +
    "rit_txt[8] = " + G("@x опыта героя за убийства во время ритуала", "@x hero experience from kills during ritual") + ";\n" +
    "rit_txt[0] = g(");
importGroup.QueueFindReplace("gml_Object_main_Step_0", "for (var i = 0; i < 6; i++)", "for (var i = 0; i < 9; i++)");
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", "for (i = 0; i < 6; i++)", "for (i = 0; i < 9; i++)");
// The God of Rites makes every ritual cheaper (both the window's calculation and the auto-cast price) and longer.
string ritBase = "(100 - (10 * (global.fs_lv[17] * global.qol_fame)))";
string ritCheap = "(" + ritBase + " * (1 - (0.3 * MyPan(20) * " + DIV + ")))";
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", ritBase, ritCheap);
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", ritBase, ritCheap);
importGroup.QueueFindReplace("gml_Object_HRit_Other_10",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2)) + (10 * (global.fs_lv[16] * global.qol_fame));",
    "RTM = 30 + (30 * MyPan(5)) + (10 * SlugaVar(0, 2)) + (10 * (global.fs_lv[16] * global.qol_fame)) + (30 * MyPan(20) * " + DIV + ");");
// The effects while the ritual runs (inside the game's "ritual is running" block, next to the wheat / wood / stone ones).
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "if (MyTri(1, 1))",
    @"if (global.qol_div)
{
    if (bit_test(o_type, 6))
    {
        d_gold *= (5 + main.rit_SILA);
    }
    if (bit_test(o_type, 7))
    {
        d_ver *= (5 + main.rit_SILA);
    }
}
if (MyTri(1, 1))");

importGroup.Import();
