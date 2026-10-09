// QoL: three new religious SERVANTS and a RELIC for each of the five new gods of 14_new_religions.csx.
// Switch: global.qol_div ("Divine extras"). OFF hides the new servants and pauses their (and the relics') effects.
//
// SERVANTS. The game has Guru, Missioner and Chaplain (ids 0..2). You pay faith to summon one; it has two perks that you buy
// with holiness (each a repeatable level; perk 1 gets x10 dearer per level, perk 2 x100). Effects are checks of SlugaVar(id, 1|2).
//   id  servant     summon cost (faith)   perk 1                                  perk 2
//   3   Oracle      300 million           Visions: +10% hero experience           Prophecy: -4% monsters to capture a tile (max -40%)
//   4   Treasurer   8 billion             Tithes: +10% gold per second            Vaults: +30% gold for abdication
//   5   Architect   60 billion            Blueprints: -5% worker cost (max -50%)  Masonry: +20% wheat, wood and stone production
// The servant picture is frame number = id of the sprite sluj_guru: the Oracle uses frame 3, which the game has but never uses;
// the Treasurer (a money bag) and the Architect (a drafting compass) are drawn below as frames 4 and 5. The "choose a servant"
// screen is redrawn with two rows of three.
//
// RELICS. The game has one relic per religion (ids 2..6 for the five old gods; bought with faith, upgradable for 10B faith x the
// level once you have sailed). The five new gods get ids 10..14, shown in the "Religion Relics" part of the Relics window:
//   10  Merchant's Seal   10B faith  +20 gold per second           upgrades: +[LVL]0% to total gold production
//   11  Sun Crown         10B faith  +25% faith production         upgrades: +[LVL]0% to total faith production
//   12  Storm Banner      10B faith  +15% hero damage              upgrades: +[LVL]0% to hero damage
//   13  Night Veil        10B faith  +2 ritual power               upgrades: +[LVL] to ritual automation
//   14  Turtle Shell      10B faith  +10% to all production       upgrades: +[LVL]0% to total wheat, wood and stone production
// Their art is new: a 64x64 tile with three frames (normal, hovered, owned) each, drawn from the gods' emblems.
//
// The strip of owned relics on the main screen draws a 38x38 picture per relic from the sprite src_all (one frame per relic id).
// It had no frames for the three ritual relics of 13_divine_extras.csx (ids 7..9), so those showed a wrong picture; frames 7..14
// are added here for them and for the five new relics.

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
// 1) ART
// =====================================================================================================================
// -- shape helpers (a shape returns 0 = empty, 1 = the main colour, 2 = black detail, 3 = the accent colour) --
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
bool InRoundRect(double x, double y, double size, double radius)
{
    if (x < 0 || y < 0 || x >= size || y >= size) return false;
    double cx = x < radius ? radius : (x >= size - radius ? size - radius - 1 : x);
    double cy = y < radius ? radius : (y >= size - radius ? size - radius - 1 : y);
    return (x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius;
}

// -- the five gods' emblems (128 x 128 space), the same shapes as in 14_new_religions.csx --
int EmbScales(double x, double y)
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
int EmbSun(double x, double y)
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
int EmbStorm(double x, double y)
{
    var bolt = new (double, double)[] { (72, 52), (50, 90), (64, 90), (52, 122), (94, 78), (77, 78), (90, 52) };
    if (InPoly(x, y, bolt)) return 3;
    if (Dist(x, y, 40, 58) <= 18 || Dist(x, y, 64, 44) <= 22 || Dist(x, y, 90, 56) <= 17) return 1;
    if (y >= 56 && y <= 74 && x >= 38 && x <= 94) return 1;
    return 0;
}
int EmbMoon(double x, double y)
{
    if (Star4(x, y, 96, 36, 11) || Star4(x, y, 110, 76, 8) || Star4(x, y, 84, 102, 6)) return 3;
    if (Dist(x, y, 56, 64) <= 42 && Dist(x, y, 77, 54) > 34) return 1;
    return 0;
}
int EmbTurtle(double x, double y)
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
        if (Math.Abs(x - 62) <= 1.3 && ee <= 0.95) return 2;
        if (Math.Abs(ee - 0.40) <= 0.03) return 2;
        if (Math.Abs(ee - 0.74) <= 0.025) return 2;
        return 1;
    }
    if (y > 90 && y <= 99 && ex * ex <= 1) return 1;
    return 0;
}
// -- the three ritual relics' icons of 13_divine_extras.csx (64 x 64 space): a sun, a coin and a sword --
int Rel64Sun(double x, double y)
{
    double dx = x - 31.5, dy = y - 31.5, d = Math.Sqrt(dx * dx + dy * dy);
    if (d <= 10.5) return 1;
    for (int k = 0; k < 8; k++)
    {
        double a = k * Math.PI / 4, ux = Math.Cos(a), uy = Math.Sin(a);
        double proj = dx * ux + dy * uy, perp = Math.Abs(-dx * uy + dy * ux);
        if (proj >= 14 && proj <= 24 && perp <= (k % 2 == 0 ? 2.6 : 1.6)) return 1;
    }
    return 0;
}
int Rel64Coin(double x, double y)
{
    double dx = x - 31.5, dy = y - 31.5, d = Math.Sqrt(dx * dx + dy * dy), m = Math.Abs(dx) + Math.Abs(dy);
    if (m <= 7) return 1;
    if (m <= 10) return 0;
    if (d >= 13 && d <= 15) return 0;
    return d <= 19 ? 1 : 0;
}
int Rel64Sword(double x, double y)
{
    if (y >= 6 && y <= 36)
    {
        double half = y < 14 ? 0.6 + (y - 6) * 0.2 : 2.2;
        if (Math.Abs(x - 31.5) <= half) return 1;
    }
    if (y >= 37 && y <= 40 && x >= 21 && x <= 42) return 1;
    if (y >= 41 && y <= 51 && x >= 30 && x <= 33) return 1;
    double px = x - 31.5, py = y - 55;
    return px * px + py * py <= 11 ? 1 : 0;
}
// -- the two new servant pictures (128 x 128 space, white): a money bag and a drafting compass --
int SvTreasurer(double x, double y)
{
    double m = Math.Abs(x - 64) + Math.Abs(y - 90);
    if ((x - 64) * (x - 64) / 1764.0 + (y - 88) * (y - 88) / 1296.0 <= 1)             // the round bag
    {
        if (m <= 9) return 0;                                           // a gem cut out of the bag
        double d = Dist(x, y, 64, 90);
        if (d >= 19 && d <= 22.5) return 0;                             // a coin ring cut out of it
        return 1;
    }
    var funnel = new (double, double)[] { (34, 16), (94, 16), (78, 44), (74, 58), (54, 58), (50, 44) };
    if (InPoly(x, y, funnel) && y >= 17 + 2.5 * Math.Sin((x - 64) * 0.5))             // the ruffled neck of the bag
        return (y >= 46 && y <= 49.5) ? 0 : 1;                                        // with a cord tied around it
    return 0;
}
int SvArchitect(double x, double y)
{
    double dh = Dist(x, y, 64, 26);
    if (dh <= 12) return dh <= 5.5 ? 0 : 1;                                           // the hinge ring
    if (Math.Abs(x - 64) <= 3.5 && y >= 2 && y <= 16) return 1;                       // the handle
    if (SegDist(x, y, 64, 36, 24, 118) <= 7 || SegDist(x, y, 64, 36, 104, 118) <= 7) return 1;       // the legs
    if (SegDist(x, y, 42, 80, 86, 80) <= 3.6) return 1;                               // the cross bar
    return 0;
}

// -- colours of the eight new relic icons (main, accent), ids 7..14 --
((byte r, byte g, byte b) main, (byte r, byte g, byte b) accent)[] relicPal =
{
    ((255, 170, 40), (255, 170, 40)),     // 7 sun (Relic of Abundance)
    ((255, 230, 70), (255, 230, 70)),     // 8 coin (Relic of Fortune)
    ((140, 185, 255), (140, 185, 255)),   // 9 sword (Relic of Valor)
    ((255, 200, 60), (255, 245, 170)),    // 10 scales
    ((255, 130, 40), (255, 220, 120)),    // 11 sun
    ((120, 150, 255), (255, 235, 90)),    // 12 storm
    ((190, 170, 255), (255, 245, 170)),   // 13 moon
    ((70, 190, 150), (200, 240, 210)),    // 14 turtle
};
// the shapes in 64 x 64 space: the ritual relics' own, and the gods' emblems scaled to ~52 px
Func<double, double, int> Fit(Func<double, double, int> emb) => (x, y) => emb((x - 32) * 2.45 + 64, (y - 32) * 2.45 + 64);
Func<double, double, int>[] relicShape =
{
    Rel64Sun, Rel64Coin, Rel64Sword,
    Fit(EmbScales), Fit(EmbSun), Fit(EmbStorm), Fit(EmbMoon), Fit(EmbTurtle),
};

// Renders one tile (size x size) of a relic into the atlas. frame: 0 normal, 1 hovered (white border), 2 owned (dim).
void PutTile(IPixelCollection<byte> px, int ox, int oy, int size, double radius, int border, int frame, int idx)
{
    var pal = relicPal[idx];
    var shape = relicShape[idx];
    double k = frame == 2 ? 0.7 : 1.0;
    for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            if (!InRoundRect(x, y, size, radius)) continue;
            bool edge = !InRoundRect(x - border, y - border, size - 2 * border, Math.Max(1, radius - border));
            double r, g, b;
            if (edge)
            {
                if (frame == 1) { r = g = b = 255; }
                else { double e = frame == 2 ? 0.55 : 1.0; r = pal.main.r * e; g = pal.main.g * e; b = pal.main.b * e; }
            }
            else
            {
                double bg = frame == 2 ? 0.22 : 0.0;
                double br = pal.main.r * bg, bgc = pal.main.g * bg, bb = pal.main.b * bg;
                int n1 = 0, n2 = 0, n3 = 0;
                for (int sy = 0; sy < 2; sy++)
                    for (int sx = 0; sx < 2; sx++)
                    {
                        int s = shape((x + 0.25 + sx * 0.5) * 64.0 / size, (y + 0.25 + sy * 0.5) * 64.0 / size);
                        if (s == 1) n1++; else if (s == 2) n2++; else if (s == 3) n3++;
                    }
                int n = n1 + n2 + n3;
                r = (br * (4 - n) + (n1 * pal.main.r + n3 * pal.accent.r) * k) / 4.0;
                g = (bgc * (4 - n) + (n1 * pal.main.g + n3 * pal.accent.g) * k) / 4.0;
                b = (bb * (4 - n) + (n1 * pal.main.b + n3 * pal.accent.b) * k) / 4.0;
            }
            px.SetPixel(ox + x, oy + y, new byte[] { (byte)r, (byte)g, (byte)b, 255 });
        }
}
void PutSilhouette(IPixelCollection<byte> px, int ox, int oy, Func<double, double, int> shape)
{
    for (int y = 0; y < 128; y++)
        for (int x = 0; x < 128; x++)
        {
            int n = 0;
            for (int sy = 0; sy < 3; sy++)
                for (int sx = 0; sx < 3; sx++)
                    if (shape(x + 0.17 + sx * 0.33, y + 0.17 + sy * 0.33) == 1) n++;
            if (n > 0) px.SetPixel(ox + x, oy + y, new byte[] { 255, 255, 255, (byte)(255 * n / 9) });
        }
}

// -- the page (512 x 512): 5 relic tiles x 3 frames (64 px), 8 mini icons (38 px), 2 servant pictures (128 px) --
//    relic tile of god t (0..4), frame f: (2 + f * 66, 2 + t * 66)
//    mini icon of relic id 7..14 (k = id - 7):  (210 + (k % 4) * 40, 2 + (k / 4) * 40)
//    servant pictures:                         (210, 90) and (340, 90)
var page = new MagickImage(MagickColors.Transparent, 512, 512);
{
    using var px = page.GetPixels();
    for (int t = 0; t < 5; t++)
        for (int f = 0; f < 3; f++)
            PutTile(px, 2 + f * 66, 2 + t * 66, 64, 9, 2, f, 3 + t);
    for (int k = 0; k < 8; k++)
        PutTile(px, 210 + (k % 4) * 40, 2 + (k / 4) * 40, 38, 5, 1, 0, k);
    PutSilhouette(px, 210, 90, SvTreasurer);
    PutSilhouette(px, 340, 90, SvArchitect);
}
string tmp = Path.Combine(Path.GetTempPath(), "territory_idle_mod_relics2.png");
page.Write(tmp, MagickFormat.Png32);
UndertaleEmbeddedTexture embPage = new();
embPage.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
using (MagickImage bgra = TextureWorker.ReadBGRAImageFromFile(tmp))
    embPage.TextureData.Image = GMImage.FromMagickImage(bgra).ConvertToPng();
Data.EmbeddedTextures.Add(embPage);
File.Delete(tmp);

UndertaleTexturePageItem NewItem(int sx, int sy, int w, int h, int bw, int bh)
{
    UndertaleTexturePageItem item = new();
    item.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
    item.SourceX = (ushort)sx; item.SourceY = (ushort)sy;
    item.SourceWidth = (ushort)w; item.SourceHeight = (ushort)h;
    item.TargetX = 0; item.TargetY = 0; item.TargetWidth = (ushort)w; item.TargetHeight = (ushort)h;
    item.BoundingWidth = (ushort)bw; item.BoundingHeight = (ushort)bh;
    item.TexturePage = embPage;
    Data.TexturePageItems.Add(item);
    return item;
}
void AddFrame(UndertaleSprite spr, UndertaleTexturePageItem item)
{
    UndertaleSprite.TextureEntry entry = new();
    entry.Texture = item;
    spr.Textures.Add(entry);
}

// the five new relic sprites (shaped like the game's own relic sprites: 64 x 64, origin in the middle)
string[] relicSprite = { "rrel_merchant", "rrel_sun", "rrel_storm", "rrel_night", "rrel_turtle" };
var relTemplate = Data.Sprites.ByName("src_PP");
for (int t = 0; t < 5; t++)
{
    if (Data.Sprites.ByName(relicSprite[t]) != null)
        throw new Exception(relicSprite[t] + " already exists - is this patch being applied twice?");
    var spr = new UndertaleSprite();
    spr.Name = Data.Strings.MakeString(relicSprite[t]);
    spr.Width = relTemplate.Width; spr.Height = relTemplate.Height;
    spr.MarginLeft = relTemplate.MarginLeft; spr.MarginRight = relTemplate.MarginRight;
    spr.MarginTop = relTemplate.MarginTop; spr.MarginBottom = relTemplate.MarginBottom;
    spr.Transparent = relTemplate.Transparent; spr.Smooth = relTemplate.Smooth; spr.Preload = relTemplate.Preload;
    spr.BBoxMode = relTemplate.BBoxMode; spr.SepMasks = relTemplate.SepMasks;
    spr.OriginX = relTemplate.OriginX; spr.OriginY = relTemplate.OriginY;
    for (int f = 0; f < 3; f++) AddFrame(spr, NewItem(2 + f * 66, 2 + t * 66, 64, 64, 64, 64));
    foreach (var m in relTemplate.CollisionMasks)
    {
        var mask = new UndertaleSprite.MaskEntry();
        mask.Data = (byte[])m.Data.Clone(); mask.Width = m.Width; mask.Height = m.Height;
        spr.CollisionMasks.Add(mask);
    }
    Data.Sprites.Add(spr);
}
// the mini icons: frames 7..14 of src_all (frame number = relic id)
var srcAll = Data.Sprites.ByName("src_all");
if (srcAll.Textures.Count != 7)
    throw new Exception("src_all does not have 7 frames - is this patch being applied twice?");
for (int k = 0; k < 8; k++)
    AddFrame(srcAll, NewItem(210 + (k % 4) * 40, 2 + (k / 4) * 40, 38, 38, 38, 38));
// the servant pictures: frames 4 and 5 of sluj_guru (frame number = servant id; frame 3, unused by the game, is the Oracle)
var sluj = Data.Sprites.ByName("sluj_guru");
if (sluj.Textures.Count != 4)
    throw new Exception("sluj_guru does not have 4 frames - is this patch being applied twice?");
AddFrame(sluj, NewItem(210, 90, 128, 128, 128, 128));
AddFrame(sluj, NewItem(340, 90, 128, 128, 128, 128));

// =====================================================================================================================
// 2) SERVANTS: data, the "choose a servant" screen (two rows), and the effects
// =====================================================================================================================
var servants = new (int id, string nameRu, string nameEn, double cost, string[] capRu, string[] capEn, string[] txtRu, string[] txtEn, double[] hcost)[]
{
    (3, "Оракул", "Oracle", 300000000.0,
        new[] { "Видения", "Пророчество" }, new[] { "Visions", "Prophecy" },
        new[] { "+10% к опыту героя", "-4% монстров для захвата клетки (макс. -40%)" },
        new[] { "+10% hero experience", "-4% monsters to capture a tile (max. -40%)" },
        new[] { 2000.0, 50000.0 }),
    (4, "Казначей", "Treasurer", 8000000000.0,
        new[] { "Десятина", "Хранилища" }, new[] { "Tithes", "Vaults" },
        new[] { "+10% к производству золота в секунду", "+30% золота за отречение" },
        new[] { "+10% gold production per second", "+30% gold for abdication" },
        new[] { 5000.0, 80000.0 }),
    (5, "Зодчий", "Architect", 60000000000.0,
        new[] { "Чертежи", "Каменная кладка" }, new[] { "Blueprints", "Masonry" },
        new[] { "-5% к стоимости всех рабочих (макс. -50%)", "+20% к производству зерна, дерева и камня" },
        new[] { "-5% cost of all workers (max. -50%)", "+20% wheat, wood and stone production" },
        new[] { 8000.0, 120000.0 }),
};
var sbS = new System.Text.StringBuilder();
foreach (var s in servants)
{
    sbS.Append("s_name[" + s.id + "] = " + G(s.nameRu, s.nameEn) + ";\n");
    sbS.Append("s_cost[" + s.id + "] = " + s.cost + ";\n");
    for (int p = 0; p < 2; p++)
    {
        sbS.Append("s_cap[" + s.id + ", " + p + "] = " + G(s.capRu[p], s.capEn[p]) + ";\n");
        sbS.Append("s_txt[" + s.id + ", " + p + "] = " + G(s.txtRu[p], s.txtEn[p]) + ";\n");
        sbS.Append("h_cost[" + s.id + ", " + p + "] = " + s.hcost[p] + ";\n");
    }
}
importGroup.QueueFindReplace("gml_Object_HRel_Create_0", "x1 = 128;", sbS.ToString() + "x1 = 128;");

// The "choose a servant" screen, in front of the game's own (which stays, but is never reached). Same look as the game's, with
// the servants in two rows of three; the text moves down a row when there are more than three.
importGroup.QueueFindReplace("gml_Object_HRel_Draw_64", "if (slu_choose)",
    @"if (slu_choose)
{
    bG = 0;
    b_L = 0;
    b_R = 0;
    draw_set_color(c_black);
    draw_set_alpha(0.22);
    draw_rectangle(-1, -1, room_width + 1, room_height + 1, false);
    draw_set_alpha(1);
    var cU = merge_color(c_navy, c_black, 0.4);
    draw_set_color(cU);
    draw_rectangle(x1, y1, x2, y2, false);
    draw_set_color(c_aqua);
    draw_rectangle(x1, y1, x2, y2, true);
    draw_set_font(font0b);
    draw_set_halign(fa_center);
    draw_set_color(clr[G]);
    draw_text(cX, y1 + 10, g(""Выбрать служителя религии"", ""Choose a servant""));
    var w = 160;
    var u = 16777215;
    var sn = 3 + (3 * global.qol_div);
    for (var i = 0; i < sn; i++)
    {
        var sxx = x1 + 100 + (w * (i mod 3));
        var syy = y1 + 125 + (140 * (i div 3));
        if (point_in_rectangle(mouse.x, mouse.y, sxx - 66, syy - 66, sxx + 66, syy + 66))
        {
            draw_set_color(c_yellow);
            draw_rectangle(sxx - 66, syy - 66, sxx + 66, syy + 66, true);
            if (mouse_check_button_pressed(mb_left))
            {
                s_num = i;
            }
        }
        if (s_num == i)
        {
            u = 16777215;
            draw_set_color(c_white);
            draw_rectangle(sxx - 66, syy - 66, sxx + 66, syy + 66, true);
        }
        else
        {
            u = merge_color(c_gray, c_navy, 0.3);
        }
        draw_sprite_ext(sluj_guru, i, sxx, syy, 1, 1, 0, u, 1);
    }
    draw_set_color(c_white);
    var txt = s_name[s_num];
    txt += (g(""#Стоимость: "", ""#Cost: "") + kstr(s_cost[s_num]) + g("" веры"", "" faith""));
    txt += (""##-- "" + s_cap[s_num, 0] + "" --"" + g(""#Стоимость: "", ""#Cost: "") + kstr(h_cost[s_num, 0]) + g("" святости"", "" holiness"") + ""#"" + s_txt[s_num, 0]);
    txt += (""##-- "" + s_cap[s_num, 1] + "" --"" + g(""#Стоимость: "", ""#Cost: "") + kstr(h_cost[s_num, 1]) + g("" святости"", "" holiness"") + ""#"" + s_txt[s_num, 1]);
    draw_text(cX, y1 + 225 + (140 * (sn > 3)), txt);
    draw_set_halign(fa_left);
    draw_set_font(font1);
    bsc = DrawBtnRel(x2 - 110, y2 - 40, g(""Закрыть"", ""Close""), 0, 16776960);
    if (main.muta[5])
    {
        bsGET = DrawBtnRel(x1 + 22, y2 - 40, g(""Призвать (бесплатно)"", ""Summon (free)""), 0, 16776960);
    }
    else
    {
        bsGET = DrawBtnRel(x1 + 22, y2 - 40, g(""Призвать ("", ""Summon ("") + kstr(s_cost[s_num]) + g("" веры)"", "" faith)""), 0, 16776960);
    }
    exit;
}
if (slu_choose)");

// The effects. Oracle: experience (1) and monsters per tile (2); Treasurer: gold per second (1) and abdication gold (2);
// Architect: worker cost (1) and wheat, wood and stone production (2).
importGroup.QueueFindReplace("gml_Object_LandBattle_Other_10", "if (MyPan(19) && global.qol_div)",
    "if (SlugaVar(3, 1) && " + DIV + ")\n{\n    class_k *= (1 + (0.1 * SlugaVar(3, 1)));\n}\nif (MyPan(19) && global.qol_div)");
string killOld = "(1 - min(0.4, 0.02 * MyRel(7, 2) * global.qol_div))";
string killNew = killOld + " * (1 - min(0.4, 0.04 * SlugaVar(3, 2) * " + DIV + "))";
foreach (string entry in new[] { "gml_Object_LandBattle_Create_0", "gml_Object_LandBattle_Other_10", "gml_Script_empire_pick_s" })
    importGroup.QueueFindReplace(entry, killOld, killNew);
importGroup.QueueFindReplace("gml_Script_get_compen", "if (MyPan(22))",
    "if (SlugaVar(4, 2))\n{\n    d *= (1 + (0.3 * SlugaVar(4, 2)));\n}\nif (MyPan(22))");
importGroup.QueueFindReplace("gml_Script_getCost", "(1 - (0.3 * MyEmp(11, 1) * global.qol_div));",
    "(1 - (0.3 * MyEmp(11, 1) * global.qol_div)) * (1 - min(0.5, 0.05 * SlugaVar(5, 1) * " + DIV + "));");

// =====================================================================================================================
// 3) RELICS: data, window, purchases, effects
// =====================================================================================================================
var relics = new (int id, int god, string spr, string colour, string infoRu, string infoEn, string levelRu, string levelEn)[]
{
    (10, 5, "rrel_merchant", "merge_color(c_yellow, c_orange, 0.35)",
        "Стоимость: 10B веры#+20 золота в секунду", "Cost: 10B faith#+20 gold per second",
        "#+[LVL]0% к итоговому производству золота", "#+[LVL]0% to total gold production"),
    (11, 6, "rrel_sun", "merge_color(c_orange, c_yellow, 0.25)",
        "Стоимость: 10B веры#+25% к производству веры", "Cost: 10B faith#+25% faith production",
        "#+[LVL]0% к итоговому производству веры", "#+[LVL]0% to total faith production"),
    (12, 7, "rrel_storm", "merge_color(c_blue, c_aqua, 0.35)",
        "Стоимость: 10B веры#+15% к урону героя", "Cost: 10B faith#+15% hero damage",
        "#+[LVL]0% к урону героя", "#+[LVL]0% to hero damage"),
    (13, 8, "rrel_night", "merge_color(c_fuchsia, c_blue, 0.55)",
        "Стоимость: 10B веры#+2 к силе обряда", "Cost: 10B faith#+2 ritual power",
        "#+[LVL] к автоматизации обрядов", "#+[LVL] to ritual automation"),
    (14, 9, "rrel_turtle", "merge_color(c_aqua, c_green, 0.4)",
        "Стоимость: 10B веры#+10% ко всему производству", "Cost: 10B faith#+10% to all production",
        "#+[LVL]0% к итоговому производству зерна, дерева и камня", "#+[LVL]0% to total wheat, wood and stone production"),
};
importGroup.QueueFindReplace("gml_Object_main_Create_0", "RELICN = 10;", "RELICN = 15;");
importGroup.QueueFindReplace("gml_Object_main_Create_0", "RELIC[9] = 0;", "RELIC[9] = 0;\nRELIC[10] = 0;\nRELIC[11] = 0;\nRELIC[12] = 0;\nRELIC[13] = 0;\nRELIC[14] = 0;");
var sbR = new System.Text.StringBuilder();
foreach (var r in relics)
{
    sbR.Append("RELS[" + r.id + "] = " + r.spr + ";\n");
    sbR.Append("rc_info[" + r.id + "] = " + G(r.infoRu, r.infoEn) + ";\n");
    sbR.Append("rcL_info[" + r.id + "] = " + G(r.levelRu, r.levelEn) + ";\n");
}
importGroup.QueueFindReplace("gml_Object_main_Create_0", "rcL_info[0] = \"\";", sbR.ToString() + "rcL_info[0] = \"\";");

// The Relics window: the religion relic of a new god (the section is shown for ids 5..9 while the switch is ON; the chain of
// the game's own five continues with ours and leaves its last block open for the game's closing brace).
importGroup.QueueFindReplace("gml_Object_HRelic_Draw_64", "if (main.cur_rel >= 0 && main.cur_rel < 5)",
    "if (main.cur_rel >= 0 && (main.cur_rel < 5 || " + DIV + "))");
var sbW = new System.Text.StringBuilder("reld = 6;\n");
for (int k = 0; k < relics.Length; k++)
{
    var r = relics[k];
    sbW.Append("}\nif (MyRel(" + r.god + ", 0) && " + DIV + ")\n{\n    draw_set_color(" + r.colour + ");\n    ur = " + r.colour + ";\n    reld = " + r.id + ";\n");
}
importGroup.QueueFindReplace("gml_Object_HRelic_Draw_64", "reld = 6;", sbW.ToString());
var sbM = new System.Text.StringBuilder("\n");
foreach (var r in relics)
{
    sbM.Append("if (bR3 && MyRel(" + r.god + ", 0) && " + DIV + " && main.RELIC[" + r.id + "] == 0 && main.ver >= 10000000000)\n{\n");
    sbM.Append("    main.ver -= 10000000000;\n    main.RELIC[" + r.id + "] = 1;\n");
    sbM.Append("    p = instance_create(rx3, ry3, relic_eff);\n    p.sprite_index = main.RELS[" + r.id + "];\n    instance_destroy();\n}\n");
}
importGroup.QueueAppend("gml_Object_HRelic_Mouse_53", sbM.ToString());

// The effects. Each relic: its base effect at level 1, and its upgrade bonus at levels 2 and above (like the game's own relics).
importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "if (MyPan(21) && o_time > 0)",
    @"if (RELIC[10])
{
    d_gold += 20;
}
if (RELIC[10] > 1)
{
    d_gold *= (1 + (0.1 * RELIC[10]));
}
if (SlugaVar(4, 1))
{
    d_gold *= (1 + (0.1 * SlugaVar(4, 1)));
}
if (RELIC[11])
{
    d_ver *= 1.25;
}
if (RELIC[11] > 1)
{
    d_ver *= (1 + (0.1 * RELIC[11]));
}
if (RELIC[14])
{
    d_zern *= 1.1;
    d_les *= 1.1;
    d_kam *= 1.1;
    d_ver *= 1.1;
}
if (RELIC[14] > 1)
{
    d_zern *= (1 + (0.1 * RELIC[14]));
    d_les *= (1 + (0.1 * RELIC[14]));
    d_kam *= (1 + (0.1 * RELIC[14]));
}
if (SlugaVar(5, 2))
{
    d_zern *= (1 + (0.2 * SlugaVar(5, 2)));
    d_les *= (1 + (0.2 * SlugaVar(5, 2)));
    d_kam *= (1 + (0.2 * SlugaVar(5, 2)));
}
if (MyPan(21) && o_time > 0)");
importGroup.QueueFindReplace("gml_Script_AfterShlem", "if (MyEmp(9, 0))",
    "if (main.RELIC[12])\n{\n    O.hero_DAM = ceil(O.hero_DAM * 1.15);\n}\nif (main.RELIC[12] > 1)\n{\n    O.hero_DAM = ceil(O.hero_DAM * (1 + (0.1 * main.RELIC[12])));\n}\nif (MyEmp(9, 0))");
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "(5 * MyEmp(12, 0) * global.qol_div);",
    "(5 * MyEmp(12, 0) * global.qol_div) + (2 * (RELIC[13] > 0) * global.qol_div);");
importGroup.QueueFindReplace("gml_Object_main_Step_0",
    "(5 * MyEmp(12, 3) * global.qol_div);",
    "(5 * MyEmp(12, 3) * global.qol_div) + ((RELIC[13] > 1) * RELIC[13] * global.qol_div);");

importGroup.Import();
