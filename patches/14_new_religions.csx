// QoL: five new RELIGION gods (the "Religion" window: Chicken, Stone, Nature, Oni and Frog are ids 0..4; these are 5..9),
// each with 3 perks. Switch: global.qol_div ("Divine extras"). OFF takes them out of the god list and pauses their effects.
//
// How a religion works in the game: you choose one god; it produces holiness (a per-second income from the things it likes,
// multiplied by 5 during its ritual type) and holiness buys three perks, each a repeatable LEVEL whose price grows x10 (perk 1),
// x100 (perk 2) and x1000 (perk 3) per level. The effects are checks of MyRel(god, 1..3) = the level of that perk.
//
//   id  god              likes (holiness per second)                                  x5 during
//   5   Merchant god     +1 per ship, +5 per market, +1 per gold per second           the gold ritual
//   6   Sun god          +3 per temple, +5 per cathedral                              the faith ritual
//   7   Storm god        +4 per academy, +3 per training hall, +3 per forge, +0.5 per kill
//   8   Night goddess    +1 per ritual performed, +25 while a ritual is running       the wisdom ritual
//   9   World turtle     +2 per tile, +20 per continent visited
//
//   id  perk 1                          perk 2                              perk 3
//   5   +20% gold per second            +25% gold for abdication            +5 gold per second per tile
//   6   +25% faith production           +10 seconds ritual duration         +1 revive per hero battle
//   7   +10% hero damage                -2% monsters per tile (max -40%)    +25% hero HP
//   8   -10% ritual cost (max -90%)     +1 ritual automation                +50% hero experience
//   9   +0.5% production per tile       -5% hiring time everywhere          +20% production
//
// Each god needs a picture. The five emblems (128 x 128, like the game's own small god pictures) are drawn below: a scale, a
// sun, a storm cloud with a bolt, a crescent moon and a turtle. They are added to the game as new sprites on a new texture
// page. The big dark picture behind the "choose a god" screen is the same emblem drawn four times larger and very dark.
//
// Not done for the new gods: the Divine Trials (they have per-god rules and goals) and the god's own relic. Their buttons are
// hidden, and the Relics window shows only the two ritual relics rows for them.

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

// =====================================================================================================================
// 1) THE EMBLEMS
// =====================================================================================================================
// A shape function returns, for a point of the 128 x 128 picture: 0 = empty, 1 = the god's colour, 2 = black (detail),
// 3 = the accent colour. Pixels are sampled 3 x 3 times for smooth edges.
double Dist(double x, double y, double cx, double cy) => Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
double SegDist(double x, double y, double ax, double ay, double bx, double by)
{
    double vx = bx - ax, vy = by - ay, wx = x - ax, wy = y - ay;
    double t = Math.Max(0, Math.Min(1, (wx * vx + wy * vy) / (vx * vx + vy * vy)));
    return Dist(x, y, ax + t * vx, ay + t * vy);
}
bool InPoly(double x, double y, (double x, double y)[] p)
{
    bool inside = false;
    for (int i = 0, j = p.Length - 1; i < p.Length; j = i++)
        if ((p[i].y > y) != (p[j].y > y) && x < (p[j].x - p[i].x) * (y - p[i].y) / (p[j].y - p[i].y) + p[i].x)
            inside = !inside;
    return inside;
}
bool Star4(double x, double y, double cx, double cy, double r)
    => Math.Sqrt(Math.Abs(x - cx) / r) + Math.Sqrt(Math.Abs(y - cy) / r) <= 1;

int EmbScales(double x, double y)       // the Merchant god
{
    if (Dist(x, y, 64, 20) <= 9) return Dist(x, y, 64, 20) <= 4 ? 2 : 1;
    if (Math.Abs(x - 64) <= 3 && y >= 28 && y <= 104) return 1;
    if (y >= 98 && y <= 112 && Math.Abs(x - 64) <= 18 + (y - 98) * 1.6) return 1;
    if (y >= 32 && y <= 40 && x >= 12 && x <= 116) return 1;
    foreach (double cx in new[] { 24.0, 104.0 })
    {
        if (SegDist(x, y, cx, 40, cx - 18, 84) <= 1.8 || SegDist(x, y, cx, 40, cx + 18, 84) <= 1.8) return 1;
        double d = Dist(x, y, cx, 84);
        if (y >= 84 && d <= 22) return (d >= 15 && d <= 17.5) ? 2 : 1;
    }
    return 0;
}
int EmbSun(double x, double y)          // the Sun god
{
    double dx = x - 64, dy = y - 64, d = Math.Sqrt(dx * dx + dy * dy);
    if (d <= 26) return (d >= 16 && d <= 19) ? 2 : 1;
    for (int k = 0; k < 12; k++)
    {
        double a = k * Math.PI / 6, ux = Math.Cos(a), uy = Math.Sin(a);
        double proj = dx * ux + dy * uy, perp = Math.Abs(-dx * uy + dy * ux), len = k % 2 == 0 ? 60 : 50;
        if (proj >= 32 && proj <= len && perp <= 5.2 * (1 - (proj - 32) / (len - 32)) + 0.5) return 1;
    }
    return 0;
}
int EmbStorm(double x, double y)        // the Storm god: a cloud with a bolt
{
    var bolt = new (double, double)[] { (72, 52), (50, 90), (64, 90), (52, 122), (94, 78), (77, 78), (90, 52) };
    if (InPoly(x, y, bolt)) return 3;
    if (Dist(x, y, 40, 58) <= 18 || Dist(x, y, 64, 44) <= 22 || Dist(x, y, 90, 56) <= 17) return 1;
    if (y >= 56 && y <= 74 && x >= 38 && x <= 94) return 1;
    return 0;
}
int EmbMoon(double x, double y)         // the Night goddess: a crescent and stars
{
    if (Star4(x, y, 96, 36, 11) || Star4(x, y, 110, 76, 8) || Star4(x, y, 84, 102, 6)) return 3;
    if (Dist(x, y, 56, 64) <= 42 && Dist(x, y, 77, 54) > 34) return 1;
    return 0;
}
int EmbTurtle(double x, double y)       // the World turtle
{
    if (Dist(x, y, 113, 69) <= 2.6) return 2;
    if (Dist(x, y, 108, 74) <= 12) return 1;
    if (y >= 72 && y <= 90 && x >= 92 && x <= 104) return 1;
    foreach (double lx in new[] { 32.0, 54.0, 78.0, 98.0 })
        if ((x - lx) * (x - lx) / 100.0 + (y - 104) * (y - 104) / 150.0 <= 1 && y >= 92) return 1;
    if (InPoly(x, y, new (double, double)[] { (10, 90), (26, 80), (26, 98) })) return 1;
    double ex = (x - 62) / 46, ey = (y - 90) / 40;
    if (y <= 92 && ex * ex + ey * ey <= 1)
    {
        double ee = ex * ex + ey * ey;
        if (Math.Abs(x - 62) <= 1.3 && ee <= 0.95) return 2;                  // the seam along the shell
        if (Math.Abs(ee - 0.40) <= 0.03) return 2;                            // a ring of plates
        if (Math.Abs(ee - 0.74) <= 0.025) return 2;
        return 1;
    }
    if (y > 90 && y <= 99 && ex * ex <= 1) return 1;                          // the belly
    return 0;
}
Func<double, double, int>[] emblem = { EmbScales, EmbSun, EmbStorm, EmbMoon, EmbTurtle };
// (god colour, accent colour) of each emblem
((byte r, byte g, byte b) main, (byte r, byte g, byte b) accent)[] palette =
{
    ((255, 200, 60), (255, 245, 170)),
    ((255, 130, 40), (255, 220, 120)),
    ((120, 150, 255), (255, 235, 90)),
    ((190, 170, 255), (255, 245, 170)),
    ((70, 190, 150), (200, 240, 210)),
};
string[] emblemSprite = { "rel_merchant", "rel_sun", "rel_storm", "rel_night", "rel_turtle" };

var emblemAtlas = new MagickImage(MagickColors.Transparent, 512, 512);
{
    using var px = emblemAtlas.GetPixels();
    for (int t = 0; t < 5; t++)
    {
        int ox = 2 + (t % 3) * 130, oy = 2 + (t / 3) * 130;
        var pal = palette[t];
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                int n1 = 0, n2 = 0, n3 = 0;
                for (int sy = 0; sy < 3; sy++)
                    for (int sx = 0; sx < 3; sx++)
                    {
                        int s = emblem[t](x + 0.17 + sx * 0.33, y + 0.17 + sy * 0.33);
                        if (s == 1) n1++; else if (s == 2) n2++; else if (s == 3) n3++;
                    }
                int n = n1 + n2 + n3;
                if (n == 0) continue;
                double r = (n1 * pal.main.r + n3 * pal.accent.r) / (double)n;
                double g = (n1 * pal.main.g + n3 * pal.accent.g) / (double)n;
                double b = (n1 * pal.main.b + n3 * pal.accent.b) / (double)n;
                px.SetPixel(ox + x, oy + y, new byte[] { (byte)r, (byte)g, (byte)b, (byte)(255 * n / 9) });
            }
    }
}
string tmpEmb = Path.Combine(Path.GetTempPath(), "territory_idle_mod_gods.png");
emblemAtlas.Write(tmpEmb, MagickFormat.Png32);
UndertaleEmbeddedTexture embPage = new();
embPage.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
using (MagickImage bgra = TextureWorker.ReadBGRAImageFromFile(tmpEmb))
    embPage.TextureData.Image = GMImage.FromMagickImage(bgra).ConvertToPng();
Data.EmbeddedTextures.Add(embPage);
File.Delete(tmpEmb);

var embTemplate = Data.Sprites.ByName("sRs_golem");          // a 128 x 128 god picture with its origin in the middle
for (int t = 0; t < 5; t++)
{
    if (Data.Sprites.ByName(emblemSprite[t]) != null)
        throw new Exception(emblemSprite[t] + " already exists - is this patch being applied twice?");
    var spr = new UndertaleSprite();
    spr.Name = Data.Strings.MakeString(emblemSprite[t]);
    spr.Width = embTemplate.Width; spr.Height = embTemplate.Height;
    spr.MarginLeft = embTemplate.MarginLeft; spr.MarginRight = embTemplate.MarginRight;
    spr.MarginTop = embTemplate.MarginTop; spr.MarginBottom = embTemplate.MarginBottom;
    spr.Transparent = embTemplate.Transparent; spr.Smooth = embTemplate.Smooth; spr.Preload = embTemplate.Preload;
    spr.BBoxMode = embTemplate.BBoxMode; spr.SepMasks = embTemplate.SepMasks;
    spr.OriginX = embTemplate.OriginX; spr.OriginY = embTemplate.OriginY;
    UndertaleTexturePageItem item = new();
    item.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
    item.SourceX = (ushort)(2 + (t % 3) * 130); item.SourceY = (ushort)(2 + (t / 3) * 130);
    item.SourceWidth = 128; item.SourceHeight = 128;
    item.TargetX = 0; item.TargetY = 0; item.TargetWidth = 128; item.TargetHeight = 128;
    item.BoundingWidth = 128; item.BoundingHeight = 128;
    item.TexturePage = embPage;
    Data.TexturePageItems.Add(item);
    UndertaleSprite.TextureEntry entry = new();
    entry.Texture = item;
    spr.Textures.Add(entry);
    foreach (var m in embTemplate.CollisionMasks)
    {
        var mask = new UndertaleSprite.MaskEntry();
        mask.Data = (byte[])m.Data.Clone();
        mask.Width = m.Width;
        mask.Height = m.Height;
        spr.CollisionMasks.Add(mask);
    }
    Data.Sprites.Add(spr);
}

// =====================================================================================================================
// 2) THE GODS' DATA (HRel Create: the arrays the religion window reads for the god G)
// =====================================================================================================================
var gods = new (int id, string spr, string clr, string nameRu, string nameEn, string likesRu, string likesEn,
    string[] perkRu, string[] perkEn, string[] helpRu, string[] helpEn, double[] cost)[]
{
    (5, "rel_merchant", "merge_color(c_yellow, c_orange, 0.35)", "Бог торговцев", "Merchant god",
        "Любит:#Торговлю и море#+1 святости в сек за каждый корабль#+5 святости в сек за каждый рынок#+1 святости в сек за каждую монету золота в сек#x5 к получению святости во время 'золотого' ритуала",
        "Likes:#Trade and the sea#+1 holiness per second per each ship#+5 holiness per second per each market#+1 holiness per second per each gold per second#x5 holiness gain during 'gold' ritual",
        new[] { "Сокровищница", "Торговые пути", "Золотое касание" },
        new[] { "Hoard", "Trade routes", "Golden touch" },
        new[] { "+20% к производству золота в секунду", "+25% золота за отречение", "+5 золота в секунду за каждую клетку" },
        new[] { "+20% gold production per second", "+25% gold for abdication", "+5 gold per second for each tile" },
        new[] { 5000.0, 150000.0, 3000000.0 }),
    (6, "rel_sun", "merge_color(c_orange, c_yellow, 0.25)", "Бог солнца", "Sun god",
        "Любит:#Свет и веру#+3 святости в сек за каждый храм#+5 святости в сек за каждый собор#x5 к получению святости во время ритуала 'веры'",
        "Likes:#Light and faith#+3 holiness per second per each temple#+5 holiness per second per each cathedral#x5 holiness gain during 'faith' ritual",
        new[] { "Сияние", "Вечный рассвет", "Возрождение" },
        new[] { "Radiance", "Eternal dawn", "Rebirth" },
        new[] { "+25% к производству веры", "+10 секунд к длительности обряда", "+1 воскрешение героя в каждой битве" },
        new[] { "+25% faith production", "+10 seconds to the duration of the ritual", "+1 revive for the hero in every battle" },
        new[] { 4000.0, 100000.0, 2000000.0 }),
    (7, "rel_storm", "merge_color(c_blue, c_aqua, 0.35)", "Бог бури", "Storm god",
        "Любит:#Войну и героев#+4 святости в сек за каждую академию#+3 святости в сек за каждый тренировочный зал#+3 святости в сек за каждую кузницу#+0.5 святости когда герой убивает монстра",
        "Likes:#War and heroes#+4 holiness per second per each academy#+3 holiness per second per each training hall#+3 holiness per second per each forge#+0.5 holiness when a monster is killed",
        new[] { "Гром", "Волчья стая", "Альфа" },
        new[] { "Thunder", "Wolf pack", "Alpha" },
        new[] { "+10% к урону героя", "-2% монстров для захвата клетки (макс. -40%)", "+25% к HP героя" },
        new[] { "+10% hero damage", "-2% monsters to capture a tile (max. -40%)", "+25% hero HP" },
        new[] { 100.0, 5000.0, 200000.0 }),
    (8, "rel_night", "merge_color(c_fuchsia, c_blue, 0.55)", "Богиня ночи", "Night goddess",
        "Любит:#Обряды и тайны#+1 святости в сек за каждый проведенный обряд#+25 святости в сек, пока идет обряд#x5 к получению святости во время ритуала 'мудрости'",
        "Likes:#Rituals and secrets#+1 holiness per second per each ritual performed#+25 holiness per second while a ritual is running#x5 holiness gain during 'wisdom' ritual",
        new[] { "Предвидение", "Ночное зрение", "Знание" },
        new[] { "Foresight", "Night vision", "Lore" },
        new[] { "-10% к стоимости обряда (макс. -90%)", "+1 к автоматизации обрядов", "+50% к опыту героя" },
        new[] { "-10% ritual cost (max. -90%)", "+1 to rituals automation", "+50% to hero experience" },
        new[] { 6000.0, 80000.0, 1500000.0 }),
    (9, "rel_turtle", "merge_color(c_aqua, c_green, 0.4)", "Мировая черепаха", "World turtle",
        "Любит:#Землю#+2 святости в сек за каждую клетку#+20 святости в сек за каждый посещенный континент",
        "Likes:#The land#+2 holiness per second per each tile#+20 holiness per second per each continent visited",
        new[] { "Терпение", "Древнее ремесло", "Мать-земля" },
        new[] { "Patience", "Ancient craft", "Mother Earth" },
        new[] { "+0.5% к производству за каждую клетку (на этом континенте)", "-5% ко времени найма во всех зданиях", "+20% ко всему производству" },
        new[] { "+0.5% production for each tile (on this continent)", "-5% hiring time in all buildings", "+20% to all production" },
        new[] { 8000.0, 120000.0, 2500000.0 }),
};
var sbH = new System.Text.StringBuilder();
foreach (var gd in gods)
{
    int i = gd.id;
    sbH.Append("god[" + i + "] = " + gd.spr + ";\n");
    sbH.Append("godc[" + i + "] = " + gd.spr + ";\n");
    sbH.Append("gstr[" + i + "] = " + G(gd.nameRu, gd.nameEn) + ";\n");
    sbH.Append("clr[" + i + "] = " + gd.clr + ";\n");
    sbH.Append("txt[" + i + "] = " + G(gd.likesRu, gd.likesEn) + ";\n");
    for (int p = 0; p < 3; p++)
    {
        sbH.Append("rst" + (p + 1) + "[" + i + "] = " + G(gd.perkRu[p], gd.perkEn[p]) + ";\n");
        sbH.Append("cost" + (p + 1) + "[" + i + "] = " + gd.cost[p] + ";\n");
        sbH.Append("rshelp" + (p + 1) + "[" + i + "] = " + G(gd.helpRu[p], gd.helpEn[p]) + ";\n");
    }
}
importGroup.QueueFindReplace("gml_Object_HRel_Create_0", "var mnf = mmstr(global.MYST_TIMER);", sbH.ToString() + "var mnf = mmstr(global.MYST_TIMER);");

// 3) Choosing a god: the arrows skip the Frog until it is unlocked and walk on to 5..9 (only while the switch is ON).
importGroup.QueueFindReplace("gml_Object_HRel_Mouse_53", "G++;",
    "G++;\nif (G == 4 && !dopper)\n{\n    G = 5;\n}\nif (G == 5 && !" + DIV + ")\n{\n    G = 10;\n}");
importGroup.QueueFindReplace("gml_Object_HRel_Mouse_53", "if (G > (3 + dopper))", "if (G > 9 || (G > (3 + dopper) && !" + DIV + "))");
importGroup.QueueFindReplace("gml_Object_HRel_Mouse_53", "G--;", "G--;\nif (G == 4 && !dopper)\n{\n    G = 3;\n}");
importGroup.QueueFindReplace("gml_Object_HRel_Mouse_53", "G = 3 + dopper;", "G = 3 + dopper;\nif (" + DIV + ")\n{\n    G = 9;\n}");
// The big dark picture behind the god (the game has a 512 x 512 one for each of its gods): the emblem, 4x larger, very dark.
importGroup.QueueFindReplace("gml_Object_HRel_Draw_64", "draw_sprite(god[G], 0, cX, cY);",
    "if (G < 5)\n{\n    draw_sprite(god[G], 0, cX, cY);\n}\nelse\n{\n    draw_sprite_ext(godc[G], 0, cX, cY, 4, 4, 0, merge_color(c_black, c_white, 0.14), 1);\n}");
// Divine Trials: not for the new gods (their buttons stay hidden).
importGroup.QueueFindReplace("gml_Object_HRel_Draw_64", "if (global._trial == -1)", "if (global._trial == -1 && G < 5)");
// The god's own relic: the Relics window has none for the new gods.
importGroup.QueueFindReplace("gml_Object_HRelic_Draw_64", "if (main.cur_rel >= 0)", "if (main.cur_rel >= 0 && main.cur_rel < 5)");

// =====================================================================================================================
// 4) HOLINESS: what each new god likes. The game's chain ends with "else if (cur_rel == 4) { ... }"; ours continue it and leave
//    the last block open for the game's own closing brace.
// =====================================================================================================================
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "holy_get += ((Var.bcnt[5] * 3) + (Var.bcnt[6] * 3));",
    @"holy_get += ((Var.bcnt[5] * 3) + (Var.bcnt[6] * 3));
}
else if (cur_rel == 5 && global.qol_div)
{
    holy_get += (Var.rcnt[8] + (Var.bcnt[13] * 5) + d_gold);
    if (o_time > 0 && bit_test(o_type, 6))
    {
        holy_get *= 5;
    }
}
else if (cur_rel == 6 && global.qol_div)
{
    holy_get += ((Var.bcnt[4] * 3) + (Var.bcnt[9] * 5));
    if (o_time > 0 && bit_test(o_type, 7))
    {
        holy_get *= 5;
    }
}
else if (cur_rel == 7 && global.qol_div)
{
    holy_get += ((Var.bcnt[6] * 4) + (Var.bcnt[7] * 3) + (Var.bcnt[11] * 3));
}
else if (cur_rel == 8 && global.qol_div)
{
    holy_get += o_cnt;
    if (o_time > 0)
    {
        holy_get += 25;
    }
    if (o_time > 0 && bit_test(o_type, 8))
    {
        holy_get *= 5;
    }
}
else if (cur_rel == 9 && global.qol_div)
{
    holy_get += ((tile_num * 2) + (CONT * 20));");
// The Storm god also likes every kill (like the Oni).
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "if (MyRel(3, 0))",
    "if (MyRel(7, 0) && " + DIV + ")\n{\n    main.holy += 0.5;\n}\nif (MyRel(3, 0))");

// =====================================================================================================================
// 5) THE PERKS (every effect is multiplied by the level of the perk, and by the switch where it is not inside a gated block)
// =====================================================================================================================
// Production (inside the gated block of 13_divine_extras.csx): Merchant 1 and 3, Sun 1, Turtle 1 and 3.
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "if (MyPan(21) && o_time > 0)",
    @"if (MyRel(5, 1))
{
    d_gold *= (1 + (0.2 * MyRel(5, 1)));
}
if (MyRel(5, 3))
{
    d_gold += (5 * tile_num * MyRel(5, 3));
}
if (MyRel(6, 1))
{
    d_ver *= (1 + (0.25 * MyRel(6, 1)));
}
if (MyRel(9, 1) || MyRel(9, 3))
{
    var _tur = (1 + (0.005 * tile_num * MyRel(9, 1))) * (1 + (0.2 * MyRel(9, 3)));
    d_zern *= _tur;
    d_les *= _tur;
    d_kam *= _tur;
    d_ver *= _tur;
}
if (MyPan(21) && o_time > 0)");
// Gold for abdication: Merchant 2.
importGroup.QueueFindReplace("gml_Script_get_compen", "if (MyPan(22))",
    "if (MyRel(5, 2))\n{\n    d *= (1 + (0.25 * MyRel(5, 2)));\n}\nif (MyPan(22))");
// Ritual: duration (Sun 2), cost (Night 1) and automation (Night 2).
importGroup.QueueFindReplace("gml_Object_HRit_Other_10",
    "(30 * MyPan(20) * global.qol_div);",
    "(30 * MyPan(20) * global.qol_div) + (10 * MyRel(6, 2) * global.qol_div);");
string ritCheap = "(1 - (0.3 * MyPan(20) * " + DIV + "))";
string ritCheaper = ritCheap + " * (1 - min(0.9, 0.1 * MyRel(8, 1) * " + DIV + "))";
importGroup.QueueFindReplace("gml_Object_HRit_Other_10", ritCheap, ritCheaper);
importGroup.QueueFindReplace("gml_Script_tmp_rit_cost", ritCheap, ritCheaper);
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "(2 * MyTri(2, 1)) + (main.muta[2] * 5) + (3 * MyPan(14) * global.qol_div) + (MyEmp(10, 1) * global.qol_div);",
    "(2 * MyTri(2, 1)) + (main.muta[2] * 5) + (3 * MyPan(14) * global.qol_div) + (MyEmp(10, 1) * global.qol_div) + (MyRel(8, 2) * global.qol_div);");
// Heroes (AfterShlem, inside the gated block): Storm 1 and 3.
importGroup.QueueFindReplace("gml_Script_AfterShlem", "if (MyEmp(9, 0))",
    "if (MyRel(7, 1))\n{\n    O.hero_DAM = ceil(O.hero_DAM * (1 + (0.1 * MyRel(7, 1))));\n}\nif (MyRel(7, 3))\n{\n    O.hero_HP = ceil(O.hero_HP * (1 + (0.25 * MyRel(7, 3))));\n}\nif (MyEmp(9, 0))");
// Monsters per tile: Storm 2 (next to the Mongolian empire's factor).
string killTail = "(1 - (0.2 * MyEmp(9, 1) * " + DIV + "))";
foreach (string entry in new[] { "gml_Object_LandBattle_Create_0", "gml_Object_LandBattle_Other_10", "gml_Script_empire_pick_s" })
    importGroup.QueueFindReplace(entry, killTail, killTail + " * (1 - min(0.4, 0.02 * MyRel(7, 2) * " + DIV + "))");
// Experience: Night 3. Revives: Sun 3.
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "if (MyPan(19) && global.qol_div)",
    "if (MyRel(8, 3) && " + DIV + ")\n{\n    class_k *= (1 + (0.5 * MyRel(8, 3)));\n}\nif (MyPan(19) && global.qol_div)");
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "(O.hero_CLASS == 11)) > revive",
    "(O.hero_CLASS == 11) + (MyRel(6, 3) * " + DIV + ")) > revive");
// Hiring time: Turtle 2 (next to the God of Time's factor, in both places).
string timeTail = " / (1 + (0.43 * MyPan(15) * " + DIV + "))";
foreach (string entry in new[] { "gml_Object_Land_Step_0", "gml_Script_HireRabs" })
    importGroup.QueueFindReplace(entry, timeTail, timeTail + " / (1 + (0.05 * MyRel(9, 2) * " + DIV + "))");

importGroup.Import();
