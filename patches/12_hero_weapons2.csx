// QoL: 7 more hero WEAPONS. Switch: global.qol_hero (see 10_hero_classes_weapons.csx).
//
// The weapon slots are used up (melee 0..25 and ranged 26..32 all have a weapon since 10_hero_classes_weapons.csx), and the
// game has one spare icon (frame 33, a bow). So this patch ALSO adds new icons: six weapons are drawn below as small pixel
// grids (every cell is 2x2 pixels, like the game's own art), packed into a new 128x128 texture page, and appended to the
// sprite sh_wap_eq as frames 34..39. The icons are plain white silhouettes, which is how the game stores all of its weapon art.
//
// Slots: ids 0..25 melee, 26..33 ranged ("hero_wp >= 26" used to mean ranged; that test is replaced everywhere by
// "26..33"), 34..39 new melee. The two pickers (wap_choose, forge_wap_choose) get a third melee row for 34..39 and their
// ranged section moves down one row; every array of the game that was sized for 33 weapons is extended.
//
//   id  name               unlocks at   effect (u = forge upgrades of that weapon)
//   33  Warbow (ranged)    level 20     +120 damage, +40 evasion
//   34  Katana (sword)     level 7      +36 damage, +10 evasion; the hero gets tired 50% slower
//   35  Venom fang         level 11     +20 damage; every hit also deals 6% (+1%/u) of the monster's max HP
//   36  Executioner's axe  level 14     +60 damage; kills any monster below 25% (+2%/u) HP outright (counts as an axe)
//   37  War scythe         level 16     +55 damage; heals 5% (+1%/u) of the damage dealt on every hit
//   38  Claymore (sword)   level 19     +140 damage, +15 armor
//   39  Halberd            level 24     +180 damage; after a kill the next monster takes 20% (+3%/u) of your damage
//
// Plug-in points: hg_wap (stats + tooltip), the two pickers (unlock, size, drawing), is_sword / is_axe (the shields that
// boost swords and axes), random_wap (weapon 20), the battle tick (the four special effects), the arrays that hold forge
// levels (main Create / Alarm 2), and the five places that tested "ranged" with a weapon number.
//
// The art is generated at install time, so the repository holds no image files and no game data.

using ImageMagick;
using UndertaleModLib.Util;

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string S(string text) => "\"" + text.Replace("\"", "\\\"") + "\"";

// ---------------------------------------------------------------------------------------------------------------------
// 1) THE ART. Each weapon: the grid (# = pixel), where it goes in the 32x48 frame (tx, ty = top-left of the frame's picture).
//    The game's weapons are held with the grip at x = 27; one-handed ones end at y = 28 and the long ones at y = 40.
// ---------------------------------------------------------------------------------------------------------------------
var art = new (string[] grid, int tx, int ty)[]
{
    // 34 Katana: a slightly curved blade, a guard and a long grip (8 x 40, grip centre at x = 3)
    (new[] {
        "..#.", "..#.", "..#.", ".##.", ".#..", ".#..", ".#..", ".#..", ".#..", ".#..",
        ".#..", ".#..", ".#..", "####", ".#..", ".#..", ".#..", ".#..", ".##.", "..#." }, 24, 0),
    // 35 Venom fang: a short notched dagger (6 x 20)
    (new[] {
        ".#.", ".#.", "##.", ".#.", ".#.", ".#.", "###", ".#.", ".#.", ".#." }, 24, 8),
    // 36 Executioner's axe: a big blade on the left of a long shaft (12 x 38, shaft centre at x = 11)
    (new[] {
        ".....#", "..#..#", ".###.#", "######", "######", "######", ".#####", "..####", "...#.#", ".....#",
        ".....#", ".....#", ".....#", ".....#", ".....#", ".....#", ".....#", ".....#", ".....#" }, 16, 2),
    // 37 War scythe: a wide crescent blade on a long pole (14 x 38, pole centre at x = 13)
    (new[] {
        "..#####", ".######", "###..##", "##....#", "#.....#", "......#", "......#", "......#", "......#", "......#",
        "......#", "......#", "......#", "......#", "......#", "......#", "......#", "......#", "......#" }, 14, 2),
    // 38 Claymore: a broad two-handed sword (8 x 36, grip centre at x = 4)
    (new[] {
        ".##.", ".##.", ".##.", ".##.", ".##.", ".##.", ".##.", ".##.", ".##.", ".##.",
        ".##.", ".##.", ".##.", "####", ".##.", ".##.", ".##.", "####" }, 23, 4),
    // 39 Halberd: a spike, an axe blade and a hook on a long pole (14 x 38, pole centre at x = 7)
    (new[] {
        "...#...", "...#...", "..###..", "...#...", "...###.", ".#.####", "##.####", ".#.###.", "...#...", "...#...",
        "...#...", "...#...", "...#...", "...#...", "...#...", "...#...", "...#...", "...#...", "...#..." }, 20, 2),
};
if (Data.Sprites.ByName("sh_wap_eq").Textures.Count != 34)
    throw new Exception("sh_wap_eq does not have 34 frames - is this patch being applied twice?");

// Pack them side by side into a 128 x 128 transparent image (2 px gap) and remember where each one went.
const int PageSize = 128;
var atlas = new MagickImage(MagickColors.Transparent, PageSize, PageSize);
var source = new (int x, int y, int w, int h)[art.Length];
{
    using var pixels = atlas.GetPixels();
    int px = 2;
    for (int i = 0; i < art.Length; i++)
    {
        int w = art[i].grid[0].Length * 2, h = art[i].grid.Length * 2;
        source[i] = (px, 2, w, h);
        for (int r = 0; r < art[i].grid.Length; r++)
            for (int c = 0; c < art[i].grid[r].Length; c++)
                if (art[i].grid[r][c] == '#')
                    for (int dy = 0; dy < 2; dy++)
                        for (int dx = 0; dx < 2; dx++)
                            pixels.SetPixel(px + c * 2 + dx, 2 + r * 2 + dy, new byte[] { 255, 255, 255, 255 });
        px += w + 2;
    }
}
// Same route as the official import script: write a PNG, read it back as BGRA and store it as the texture's image.
string tmpPng = Path.Combine(Path.GetTempPath(), "territory_idle_mod_weapons.png");
atlas.Write(tmpPng, MagickFormat.Png32);
UndertaleEmbeddedTexture page = new();
page.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
using (MagickImage bgra = TextureWorker.ReadBGRAImageFromFile(tmpPng))
    page.TextureData.Image = GMImage.FromMagickImage(bgra).ConvertToPng();
Data.EmbeddedTextures.Add(page);
File.Delete(tmpPng);

var sprite = Data.Sprites.ByName("sh_wap_eq");
for (int i = 0; i < art.Length; i++)
{
    UndertaleTexturePageItem item = new();
    item.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
    item.SourceX = (ushort)source[i].x;
    item.SourceY = (ushort)source[i].y;
    item.SourceWidth = (ushort)source[i].w;
    item.SourceHeight = (ushort)source[i].h;
    item.TargetX = (ushort)art[i].tx;
    item.TargetY = (ushort)art[i].ty;
    item.TargetWidth = (ushort)source[i].w;
    item.TargetHeight = (ushort)source[i].h;
    item.BoundingWidth = (ushort)sprite.Width;
    item.BoundingHeight = (ushort)sprite.Height;
    item.TexturePage = page;
    Data.TexturePageItems.Add(item);
    UndertaleSprite.TextureEntry entry = new();
    entry.Texture = item;
    sprite.Textures.Add(entry);
}

// ---------------------------------------------------------------------------------------------------------------------
// 2) THE WEAPONS. GML expressions use uuu (forge upgrades of this weapon).
// ---------------------------------------------------------------------------------------------------------------------
var W = new (int id, int lvl, bool ranged, string nameEn, string nameRu, string dmg, string uki, string shi, string extraEn, string extraRu)[]
{
    (33, 20, true,  "Warbow",             "Боевой лук",        "120 + (60 * uuu)", "40 + (7 * uuu)", null, null, null),
    (34, 7,  false, "Katana",             "Катана",            "36 + (50 * uuu)",  "10 + (3 * uuu)", null,
        "\"#The hero gets tired 50% slower\"",
        "\"#Герой устает на 50% медленнее\""),
    (35, 11, false, "Venom fang",         "Ядовитый клык",     "20 + (30 * uuu)",  null, null,
        "\"#Venom: every hit also deals \" + string(6 + uuu) + \"% of the monster's max HP\"",
        "\"#Яд: каждый удар наносит еще \" + string(6 + uuu) + \"% максимального HP монстра\""),
    (36, 14, false, "Executioner's axe",  "Топор палача",      "60 + (60 * uuu)",  null, null,
        "\"#Executes monsters below \" + string(25 + (2 * uuu)) + \"% HP\"",
        "\"#Добивает монстров с HP ниже \" + string(25 + (2 * uuu)) + \"%\""),
    (37, 16, false, "War scythe",         "Боевая коса",       "55 + (50 * uuu)",  null, null,
        "\"#Life drain: heals \" + string(5 + uuu) + \"% of the damage dealt\"",
        "\"#Похищение жизни: лечит \" + string(5 + uuu) + \"% нанесенного урона\""),
    (38, 19, false, "Claymore",           "Клеймор",           "140 + (70 * uuu)", null, "15 + (10 * uuu)", null, null),
    (39, 24, false, "Halberd",            "Алебарда",          "180 + (70 * uuu)", null, null,
        "\"#Cleave: after a kill the next monster takes \" + string(20 + (3 * uuu)) + \"% of your damage\"",
        "\"#Рассечение: после убийства следующий монстр получает \" + string(20 + (3 * uuu)) + \"% вашего урона\""),
};

// hg_wap: new cases in front of case 29 (same format as 10_hero_classes_weapons.csx). hlp: 0 apply, 1 Russian, 2 English.
var sbW = new System.Text.StringBuilder();
foreach (var w in W)
{
    string en = S(w.nameEn + "#Damage: +") + " + bers_bon_str(" + w.dmg + ", 1.5, beb_dbl)";
    string ru = S(w.nameRu + "#Урон: +") + " + bers_bon_str(" + w.dmg + ", 1.5, beb_dbl)";
    if (w.shi != null) { en += " + " + S("#Armor: +") + " + string(" + w.shi + ")"; ru += " + " + S("#Защита: +") + " + string(" + w.shi + ")"; }
    if (w.uki != null) { en += " + " + S("#Evasion chance: +") + " + string(" + w.uki + ")"; ru += " + " + S("#Шанс уклонения: +") + " + string(" + w.uki + ")"; }
    if (w.extraEn != null) { en += " + " + w.extraEn; ru += " + " + w.extraRu; }
    if (w.ranged) { en += " + " + S("#You won't be able to use shield with this weapon!"); ru += " + " + S("#С этим оружием нельзя будет выбрать щит!"); }
    sbW.Append("    case " + w.id + ":\n");
    sbW.Append("        if (hlp > 1)\n        {\n            return " + en + ";\n        }\n");
    sbW.Append("        if (hlp)\n        {\n            return " + ru + ";\n        }\n");
    sbW.Append("        hero.hero_DAM += bers_bon_val(" + w.dmg + ", 1.5, beb_dbl);\n");
    if (w.uki != null) sbW.Append("        hero.hero_UKL += " + w.uki + ";\n");
    if (w.shi != null) sbW.Append("        hero.hero_SHI += " + w.shi + ";\n");
    sbW.Append("        break;\n");
}
importGroup.QueueFindReplace("gml_Script_hg_wap", "case 29:", sbW.ToString() + "    case 29:");

// Swords (Katana, Claymore) and the axe: the game's shields that boost "swords" or "axes", and the Berserker, ask these.
importGroup.QueueFindReplace("gml_Script_is_sword", "case 7:", "case 7:\n    case 34:\n    case 38:");
importGroup.QueueFindReplace("gml_Script_is_axe", "argument0 == 19", "argument0 == 19 || argument0 == 36");

// Weapon 20 ("Chaos") may also turn into the new melee weapons.
importGroup.QueueFindReplace("gml_Script_random_wap", "22, 23, 24, 5, 10, 12, 14, 25)", "22, 23, 24, 5, 10, 12, 14, 25, 34, 35, 36, 37, 38, 39)");

// ---------------------------------------------------------------------------------------------------------------------
// 3) "Ranged" used to be "weapon number >= 26". Now it is 26..33, because 34..39 are melee.
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueFindReplace("gml_Object_LandBattle_Draw_0", "O.hero_wp >= 26", "(O.hero_wp >= 26 && O.hero_wp < 34)");
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "if (hero_wp >= 26)", "if (hero_wp >= 26 && hero_wp < 34)");
importGroup.QueueFindReplace("gml_Object_Land_Draw_0", "hero_wp < 26", "(hero_wp < 26 || hero_wp >= 34)");
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "hero_ss == -1 && hero_wp < 26", "hero_ss == -1 && (hero_wp < 26 || hero_wp >= 34)");
importGroup.QueueFindReplace("gml_Object_Land_Step_0", "hero_ss != -1 || hero_wp >= 26", "hero_ss != -1 || (hero_wp >= 26 && hero_wp < 34)");
importGroup.QueueFindReplace("gml_Script_AfterShlem", "if (O.hero_wp >= 26)", "if (O.hero_wp >= 26 && O.hero_wp < 34)");

// Forge levels are kept for 33 weapons (0..32): extend the two loops that reset / load them to 0..65.
importGroup.QueueFindReplace("gml_Object_main_Create_0", "wap_upgrade[i] = 0;", "wap_upgrade[i] = 0;\nwap_upgrade[i + 33] = 0;");
importGroup.QueueFindReplace("gml_Object_main_Alarm_2",
    "wap_upgrade[i] = ini_read_real(\"WAP\", \"uuu\" + string(i), 0);",
    "wap_upgrade[i] = ini_read_real(\"WAP\", \"uuu\" + string(i), 0);\nwap_upgrade[i + 33] = ini_read_real(\"WAP\", \"uuu\" + string(i + 33), 0);");

// ---------------------------------------------------------------------------------------------------------------------
// 4) The pickers: unlock by hero level (before the "free for Ronin" check), the "New!" tag, the new arrays, a third melee row
//    for 34..39 and the ranged section one row lower. The window grows by one row.
// ---------------------------------------------------------------------------------------------------------------------
var sbAllow = new System.Text.StringBuilder();
var sbNew = new System.Text.StringBuilder();
foreach (var w in W)
{
    sbAllow.Append("if (global.qol_hero && O.hero_lvl >= " + w.lvl + ")\n{\n    allow[" + w.id + "] = 1;\n}\n");
    sbNew.Append("    if (global.qol_hero && O.hero_lvl == " + w.lvl + ")\n    {\n        new[" + w.id + "] = 1;\n    }\n");
}
foreach (string entry in new[] { "gml_Object_wap_choose", "gml_Object_forge_wap_choose" })
{
    bool forge = entry.Contains("forge");
    importGroup.QueueFindReplace(entry + "_Alarm_1", "if (O.hero_CLASS == 3 || O.hero_super == 1)", sbAllow.ToString() + "if (O.hero_CLASS == 3 || O.hero_super == 1)");
    importGroup.QueueFindReplace(entry + "_Alarm_1", "var nw = O.hero_new - 1;", "var nw = O.hero_new - 1;\n" + sbNew.ToString());

    importGroup.QueueFindReplace(entry + "_Create_0", "N2 = 33;", "N2 = 34;");
    importGroup.QueueFindReplace(entry + "_Create_0", forge ? "y2 = 511;" : "y2 = 610;", forge ? "y2 = 593;" : "y2 = 675;");
    importGroup.QueueAppend(entry + "_Create_0", "\nfor (var i = 34; i < 40; i++)\n{\n    allow[i] = 0;\n    gold[i] = 0;\n    aura[i] = 0;\n    new[i] = 0;\n}\n");

    // Draw: the melee loop runs over 32 positions (26 + 6); positions >= 26 show weapons 34..39.
    importGroup.QueueFindReplace(entry + "_Draw_64", "for (var i = 0; i < N1; i++)", "for (var ii = 0; ii < (N1 + 6); ii++)");
    importGroup.QueueFindReplace(entry + "_Draw_64",
        "var _x = x1 + 40 + ((i % 13) * 65);",
        "var i = ii;\nif (ii >= N1)\n{\n    i = 34 + (ii - N1);\n}\nvar _x = x1 + 40 + ((ii % 13) * 65);");
    if (forge)
    {
        importGroup.QueueFindReplace(entry + "_Draw_64", "var _y = y1 + 80 + ((i div 13) * 82);", "var _y = y1 + 80 + ((ii div 13) * 82);");
        importGroup.QueueFindReplace(entry + "_Draw_64", "_y + 13 + ((i div 13) * 5)", "_y + 13 + ((ii div 13) * 5)");
        importGroup.QueueFindReplace(entry + "_Draw_64", "draw_text(cX, y1 + 236 + 9,", "draw_text(cX, y1 + 236 + 9 + 82,");
        importGroup.QueueFindReplace(entry + "_Draw_64", "var _y = y1 + 296 + 9 + ((k div 13) * 65);", "var _y = y1 + 296 + 9 + 82 + ((k div 13) * 65);");
    }
    else
    {
        importGroup.QueueFindReplace(entry + "_Draw_64", "var _y = y1 + 80 + ((i div 13) * 65);", "var _y = y1 + 80 + ((ii div 13) * 65);");
        importGroup.QueueFindReplace(entry + "_Draw_64", "draw_text(cX, y1 + 236,", "draw_text(cX, y1 + 301,");
        importGroup.QueueFindReplace(entry + "_Draw_64", "var _y = y1 + 296 + ((k div 13) * 65);", "var _y = y1 + 361 + ((k div 13) * 65);");
    }
}

// ---------------------------------------------------------------------------------------------------------------------
// 5) The battle tick (LandBattle Other_10): the four special effects.
// ---------------------------------------------------------------------------------------------------------------------
//    Katana: a dodge makes the hero less tired (the game's own "ust_" factor of one dodge).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "var ust_ = 1;",
    "var ust_ = 1;\nif (O.hero_wp == 34)\n{\n    ust_ *= 0.5;\n}");
//    Venom fang (a share of the monster's max HP on every hit), Executioner's axe (finishing blow), War scythe (life drain):
//    right after the hit, in front of the Warhammer's check from 10_hero_classes_weapons.csx.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "if (O.hero_wp == 10 && random(100) < (20 + (2 * main.wap_upgrade[10])))",
    @"if (O.hero_wp == 35)
{
    vrag_HP -= ceil((Var.vrag_hp[mon[m_id]] + (monLV * 110)) * (0.06 + (0.01 * main.wap_upgrade[35])));
}
if (O.hero_wp == 36 && vrag_HP > 0 && vrag_HP < ceil((Var.vrag_hp[mon[m_id]] + (monLV * 110)) * (0.25 + (0.02 * main.wap_upgrade[36]))))
{
    vrag_HP = 0;
}
if (O.hero_wp == 37)
{
    O.hero_HP += max(1, ceil(O.hero_DAM * (0.05 + (0.01 * main.wap_upgrade[37]))));
}
if (O.hero_wp == 10 && random(100) < (20 + (2 * main.wap_upgrade[10])))");
//    Halberd: on a kill the NEXT monster (already created at that point) takes a share of the hero's damage.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "if (O.hero_wp == 29 && !UST)",
    @"if (O.hero_wp == 39)
{
    vrag_HP -= ceil(O.hero_DAM * (0.2 + (0.03 * main.wap_upgrade[39])));
}
if (O.hero_wp == 29 && !UST)");

// ---------------------------------------------------------------------------------------------------------------------
// 6) The forge shows a picture of the weapon being upgraded: sprite sh_forge has one frame per weapon (35 frames, indexed by
//    weapon number + 1). The new weapons have no frame there, so they show their own icon, enlarged.
// ---------------------------------------------------------------------------------------------------------------------
importGroup.QueueFindReplace("gml_Object_Land_Draw_0",
    "draw_sprite(sh_forge, var3, XX, YY + 75);",
    "if (var3 <= 34)\n{\n    draw_sprite(sh_forge, var3, XX, YY + 75);\n}\nelse\n{\n    draw_sprite_ext(sh_wap_eq, var3 - 1, XX, YY + 75, 2, 2, 0, c_white, 1);\n}");
importGroup.QueueFindReplace("gml_Object_ForgeW_Draw_64",
    "draw_sprite(sh_forge, O.var3, cX, y1 + 75);",
    "if (O.var3 <= 34)\n{\n    draw_sprite(sh_forge, O.var3, cX, y1 + 75);\n}\nelse\n{\n    draw_sprite_ext(sh_wap_eq, O.var3 - 1, cX, y1 + 75, 2, 2, 0, c_white, 1);\n}");

importGroup.Import();
