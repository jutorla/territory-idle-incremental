// QoL: more hero WEAPONS (13) and CLASSES (3).
// Switch: global.qol_hero (Options > QoL Features > "Hero extras"). Default ON. OFF stops the new weapons from unlocking and
// hides the new classes from the class dialog; a hero that already has one keeps it until its next tile.
//
// WEAPONS. The game's weapon art (sprite sh_wap_eq, frames 0..33) already has icons for slots that have no weapon behind
// them: the picker draws those as dimmed, locked icons. We give them a weapon each. Slots 0..25 are the melee grid, 26..32
// the ranged row (no shield with ranged weapons); every array of the game is sized for 33 slots, so nothing is resized.
//
//   id  name              unlocks at   effect (u = forge upgrades of that weapon)
//   5   Scimitar (sword)  level 5      +24 damage, +15 evasion
//   15  Stiletto          level 8      +14 damage, +40 evasion
//   25  Trident           level 9      +30 damage; 20% (+3%/u) chance to hold the monster at bay (counts as a dodge)
//   31  Boomerang (rng)   level 9      +24 damage, +45 evasion
//   13  Scepter of Faith  level 10     +30 damage; +1 holiness (+1/u) per kill
//   10  Warhammer         level 12     +40 damage; 20% (+2%/u) crushing blow: triple damage
//   12  Mace              level 13     +38 damage, +40 HP
//   30  Longbow (rng)     level 13     +62 damage, +30 evasion
//   14  Thorn club        level 15     +45 damage; thorns: the monster takes 25% (+5%/u) of your damage when it hits you
//   16  Morning star      level 18     +75 damage, +20 armor
//   32  Crossbow (rng)    level 18     +95 damage, +25 evasion
//   18  Pike              level 21     +110 damage; +2 damage (+1/u) after every kill
//   7   Kris (sword)      level 23     +150 damage, +35 evasion
//
// Where they plug in: hg_wap (stats + the tooltip text, in both languages), the two pickers wap_choose / forge_wap_choose
// (unlock by hero level, the yellow "New!" tag), random_wap (weapon 20 may turn into the new melee ones), and the battle
// tick LandBattle Other_10 for the five special effects. The forge works for all of them because it only needs hg_wap.
//
// CLASSES (hero_CLASS 6, 7, 8; the unlock needs the best level reached with another class, which the game already saves for
// every class in [CLS]):
//   6 Knight (active)   unlock: level 12 with an Adventurer. Armor x2.5 (+2 per hero level), +15% HP, evasion always 0,
//                       every kill repairs 1 + level/4 armor (up to the starting armor).
//   7 Ranger (active)   unlock: level 15 with a Rogue. Ranged weapons: damage x2.2, evasion x1.5. Other weapons: damage x0.6.
//   8 Cleric (passive)  unlock: level 15 with a Ronin. 15 AAP per level up, +1 AAP per 20 kills, +1 holiness per kill.
//   9 Duelist (active)  unlock: level 15 with a Knight. Evasion x1.5, armor x0.5, every dodge is a riposte (60% damage).
//  10 Warlock (active)  unlock: level 15 with a Ranger. Damage x1.2, HP x0.8, every hit heals 4% of the damage dealt.
//  11 Martyr (passive)  unlock: level 15 with a Cleric. 10 AAP per level up, one extra revive in every battle.
//  12 Scholar (passive) unlock: level 12 with an Adventurer. 5 AAP per level up, experience from kills x3.
// The class dialog is made 60 px taller and its buttons 36 px apart to make room for all of them.

using System.Collections.Generic;
using System.Linq;

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string S(string text) => "\"" + text.Replace("\"", "\\\"") + "\"";
string G(string ru, string en) => "g(" + S(ru) + ", " + S(en) + ")";

// ---------------------------------------------------------------------------------------------------------------------
// The weapon table. GML expressions use uuu (forge upgrades of this weapon). Texts are GML string expressions.
// ---------------------------------------------------------------------------------------------------------------------
var W = new (int id, int lvl, bool ranged, string nameEn, string nameRu, string dmg, string uki, string shi, string hp, string extraEn, string extraRu)[]
{
    ( 5, 5,  false, "Scimitar",          "Скимитар",          "24 + (40 * uuu)",  "15 + (4 * uuu)", null, null, null, null),
    (15, 8,  false, "Stiletto",          "Стилет",            "14 + (30 * uuu)",  "40 + (6 * uuu)", null, null, null, null),
    (25, 9,  false, "Trident",           "Трезубец",          "30 + (50 * uuu)",  null, null, null,
        "\"#\" + string(20 + (3 * uuu)) + \"% chance to hold the monster at bay#(counts as a dodge)\"",
        "\"#\" + string(20 + (3 * uuu)) + \"% шанс удержать монстра на расстоянии#(считается как уклонение)\""),
    (31, 9,  true,  "Boomerang",         "Бумеранг",          "24 + (40 * uuu)",  "45 + (8 * uuu)", null, null, null, null),
    (13, 10, false, "Scepter of Faith",  "Скипетр веры",      "30 + (40 * uuu)",  null, null, null,
        "\"#+\" + string(1 + uuu) + \" holiness per kill\"",
        "\"#+\" + string(1 + uuu) + \" святости за каждое убийство\""),
    (10, 12, false, "Warhammer",         "Боевой молот",      "40 + (60 * uuu)",  null, null, null,
        "\"#\" + string(20 + (2 * uuu)) + \"% chance of a crushing blow (x3 damage)\"",
        "\"#\" + string(20 + (2 * uuu)) + \"% шанс сокрушительного удара (x3 урон)\""),
    (12, 13, false, "Mace",              "Булава",            "38 + (50 * uuu)",  null, null, "40 + (15 * uuu)", null, null),
    (30, 13, true,  "Longbow",           "Длинный лук",       "62 + (50 * uuu)",  "30 + (6 * uuu)", null, null, null, null),
    (14, 15, false, "Thorn club",        "Шипастая дубина",   "45 + (50 * uuu)",  null, null, null,
        "\"#Thorns: the monster takes \" + string(25 + (5 * uuu)) + \"% of your damage when it hits you\"",
        "\"#Шипы: монстр получает \" + string(25 + (5 * uuu)) + \"% вашего урона, когда бьет вас\""),
    (16, 18, false, "Morning star",      "Моргенштерн",       "75 + (50 * uuu)",  null, "20 + (10 * uuu)", null, null, null),
    (32, 18, true,  "Crossbow",          "Арбалет",           "95 + (60 * uuu)",  "25 + (5 * uuu)", null, null, null, null),
    (18, 21, false, "Pike",              "Пика",              "110 + (60 * uuu)", null, null, null,
        "\"#+\" + string(2 + uuu) + \" damage after each kill\"",
        "\"#+\" + string(2 + uuu) + \" к урону после каждого убийства\""),
    ( 7, 23, false, "Kris",              "Крис",              "150 + (60 * uuu)", "35 + (8 * uuu)", null, null, null, null),
};

// 1) Setting, read from the "opt" ini ([QOL] hero, default ON) at the very top of main Create.
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    @"VERSION = 167;
if (!variable_global_exists(""qol_hero""))
{
    ini_open(""opt"");
    global.qol_hero = ini_read_real(""QOL"", ""hero"", 1);
    ini_close();
}");

// 2) Two new variables on every tile (used by the Knight's armor repair and the Cleric's kill counter).
importGroup.QueueAppend("gml_Object_Land_Create_0", "\nhero_SHI0 = 0;\nhero_kc = 0;\n");

// 3) hg_wap: the new weapons, inserted before the last case. hlp: 0 = apply the stats, 1 = Russian text, 2 = English text.
var sbW = new System.Text.StringBuilder();
foreach (var w in W)
{
    string en = S(w.nameEn + "#Damage: +") + " + bers_bon_str(" + w.dmg + ", 1.5, beb_dbl)";
    string ru = S(w.nameRu + "#Урон: +") + " + bers_bon_str(" + w.dmg + ", 1.5, beb_dbl)";
    if (w.hp != null)  { en += " + " + S("#HP: +") + " + string(" + w.hp + ")";  ru += " + " + S("#HP: +") + " + string(" + w.hp + ")"; }
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
    if (w.hp != null)  sbW.Append("        hero.hero_HP += " + w.hp + ";\n");
    sbW.Append("        break;\n");
}
importGroup.QueueFindReplace("gml_Script_hg_wap", "case 29:", sbW.ToString() + "    case 29:");

// 4) Both pickers (the normal one and the forge's) unlock weapons by hero level in the same way. Our lines go in front of
//    the "free for Ronin" check that follows the unlocks; the "New!" tag lines go at the start of the new-item block.
var sbAllow = new System.Text.StringBuilder();
var sbNew = new System.Text.StringBuilder();
foreach (var w in W)
{
    sbAllow.Append("if (global.qol_hero && O.hero_lvl >= " + w.lvl + ")\n{\n    allow[" + w.id + "] = 1;\n}\n");
    sbNew.Append("    if (global.qol_hero && O.hero_lvl == " + w.lvl + ")\n    {\n        new[" + w.id + "] = 1;\n    }\n");
}
foreach (string entry in new[] { "gml_Object_wap_choose_Alarm_1", "gml_Object_forge_wap_choose_Alarm_1" })
{
    importGroup.QueueFindReplace(entry, "if (O.hero_CLASS == 3 || O.hero_super == 1)", sbAllow.ToString() + "if (O.hero_CLASS == 3 || O.hero_super == 1)");
    importGroup.QueueFindReplace(entry, "var nw = O.hero_new - 1;", "var nw = O.hero_new - 1;\n" + sbNew.ToString());
}

// 5) Weapon 20 ("Chaos") turns into a random weapon when the hero gets tired: let it pick the new melee ones as well.
importGroup.QueueFindReplace("gml_Script_random_wap",
    "choose(0, 1, 2, 3, 4, 6, 8, 9, 11, 17, 19, 20, 21, 22, 23, 24)",
    "choose(0, 1, 2, 3, 4, 6, 8, 9, 11, 17, 19, 20, 21, 22, 23, 24, 5, 10, 12, 14, 25)");

// 6) Battle tick (LandBattle Other_10): the special effects. self = the battle tile, O = the hero's academy tile.
//    a) Trident: an extra way to avoid the counter-attack, counted as a dodge.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "if (random(100) < ((O.hero_UKL - UST) * uklon_2))",
    "if (random(100) < ((O.hero_UKL - UST) * uklon_2) || (O.hero_wp == 25 && random(100) < (20 + (3 * main.wap_upgrade[25]))))");
//    b) Warhammer: a crushing blow = two more hits' worth of damage. Warlock: life drain on every hit.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "vrag_HP -= O.hero_DAM;",
    @"vrag_HP -= O.hero_DAM;
if (O.hero_CLASS == 10)
{
    O.hero_HP += max(1, ceil(O.hero_DAM * 0.04));
}
if (O.hero_wp == 10 && random(100) < (20 + (2 * main.wap_upgrade[10])))
{
    vrag_HP -= (2 * O.hero_DAM);
}");
//    b2) Duelist: every dodge is answered with a riposte.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "uklon = 1;",
    @"uklon = 1;
if (O.hero_CLASS == 9)
{
    vrag_HP -= ceil(O.hero_DAM * 0.6);
}");
//    b3) Martyr: one more revive in every battle (the game's revive counter is shared with the relic and weapon 24).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "obor + (global.fs_lv[12] * global.qol_fame)) > revive",
    "obor + (global.fs_lv[12] * global.qol_fame) + (O.hero_CLASS == 11)) > revive");
//    c) Thorn club: when the monster's hit lands (shield or HP), it takes a share of the hero's damage.
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "if (O.hero_wp == 28)",
    @"if (O.hero_wp == 14)
{
    vrag_HP -= ceil(O.hero_DAM * (0.25 + (0.05 * main.wap_upgrade[14])));
}
if (O.hero_wp == 28)");
//    d) On every kill: Scepter of Faith (holiness), Pike (damage), Knight (armor repair), Cleric (holiness + AAP).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "if (O.hero_wp == 29 && !UST)",
    @"if (O.hero_wp == 13)
{
    main.holy += 1 + main.wap_upgrade[13];
}
if (O.hero_wp == 18)
{
    O.hero_DAM += 2 + main.wap_upgrade[18];
}
if (O.hero_CLASS == 6 && O.hero_SHI < O.hero_SHI0)
{
    O.hero_SHI = min(O.hero_SHI0, O.hero_SHI + 1 + (O.hero_lvl div 4));
}
if (O.hero_CLASS == 8)
{
    main.holy += 1;
    O.hero_kc += 1;
    if (O.hero_kc >= 20)
    {
        O.hero_kc = 0;
        O.hero_AAP += 1;
    }
}
if (O.hero_wp == 29 && !UST)");

// 7) AfterShlem (the stat pass after the helmet is chosen): the Knight and the Ranger. They go in front of the one-battle
//    boost, like the game's own class changes; the Knight's starting armor is remembered at the very end.
importGroup.QueueFindReplace(
    "gml_Script_AfterShlem",
    "if (global.D_BTL > 0 && global.qol_ab[8])",
    @"if (O.hero_CLASS == 6)
{
    O.hero_UKL = 0;
    O.hero_SHI = ceil((O.hero_SHI + (2 * O.hero_lvl)) * 2.5);
    O.hero_HP = ceil(O.hero_HP * 1.15);
}
else if (O.hero_CLASS == 7)
{
    if (O.hero_wp >= 26)
    {
        O.hero_DAM = ceil(O.hero_DAM * 2.2);
        O.hero_UKL = ceil(O.hero_UKL * 1.5);
    }
    else
    {
        O.hero_DAM = ceil(O.hero_DAM * 0.6);
    }
}
else if (O.hero_CLASS == 9)
{
    O.hero_UKL = ceil(O.hero_UKL * 1.5);
    O.hero_SHI = ceil(O.hero_SHI * 0.5);
}
else if (O.hero_CLASS == 10)
{
    O.hero_DAM = ceil(O.hero_DAM * 1.2);
    O.hero_HP = ceil(O.hero_HP * 0.8);
}
if (global.D_BTL > 0 && global.qol_ab[8])");
importGroup.QueueFindReplace("gml_Script_AfterShlem", "O.hren_wp = O.hero_wp;", "O.hero_SHI0 = O.hero_SHI;\nO.hren_wp = O.hero_wp;");

// 8) AAP with every level up for the passive classes (the Ronin gets 30, next to these checks on the academy tile):
//    Cleric 15, Martyr 10, Scholar 5.
importGroup.QueueFindReplace(
    "gml_Object_Land_Step_0",
    "if (hero_CLASS == 3)",
    @"if (hero_CLASS == 8)
{
    hero_AAP += 15;
}
if (hero_CLASS == 11)
{
    hero_AAP += 10;
}
if (hero_CLASS == 12)
{
    hero_AAP += 5;
}
if (hero_CLASS == 3)");
//    Scholar: three times the experience of every kill (the Adventurer gets two times).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "var class_k = 1;",
    @"var class_k = 1;
if (O.hero_CLASS == 12)
{
    class_k = 3;
}");

// 9) The class dialog. The class levels needed for the unlock are read from the save ([CLS] c1..c8).
importGroup.QueueFindReplace("gml_Object_class_choose_Create_0", "for (var i = 1; i <= 5; i++)", "for (var i = 1; i <= 12; i++)");
importGroup.QueueFindReplace("gml_Object_class_choose_Create_0", "bc5 = 0;", "bc5 = 0;\nbc6 = 0;\nbc7 = 0;\nbc8 = 0;\nbc9 = 0;\nbc10 = 0;\nbc11 = 0;\nbc12 = 0;");
// The dialog needs room for 11 classes: it gets 60 px taller (the room is 800 px high) and the buttons 36 px apart
// (they are 30 px high). Layout, from the top of the dialog: Adventurer 90 | "Active" 140 | 165 Berserker, 201 Rogue,
// 237 Knight, 273 Ranger, 309 Duelist, 345 Warlock | "Passive" 390 | 415 Ronin, 451 Paladin, 487 Cleric, 523 Martyr, 559 Scholar.
importGroup.QueueFindReplace("gml_Object_class_choose_Create_0", "y2 = 690;", "y2 = 750;");

//    Draw: the active classes, the "passive" section lower, the new passive classes under the Paladin.
importGroup.QueueFindReplace(
    "gml_Object_class_choose_Draw_64",
    "draw_text(cX, y1 + 155, g(\"Классы активной игры\", \"Active play classes\"));",
    "draw_text(cX, y1 + 140, g(\"Классы активной игры\", \"Active play classes\"));");
importGroup.QueueFindReplace(
    "gml_Object_class_choose_Draw_64",
    "bc2 = DrawBtnRel(cX, y1 + 190, \"Berserker\"",
    "bc2 = DrawBtnRel(cX, y1 + 165, \"Berserker\"");
importGroup.QueueFindReplace(
    "gml_Object_class_choose_Draw_64",
    "bc4 = DrawBtnRel(cX, y1 + 190 + 45, \"Rogue\", 1, merge_color(c_fuchsia, c_blue, 0.5), 1);",
    @"bc4 = DrawBtnRel(cX, y1 + 201, ""Rogue"", 1, merge_color(c_fuchsia, c_blue, 0.5), 1);
bc6 = 0;
bc7 = 0;
bc9 = 0;
bc10 = 0;
if (global.qol_hero)
{
    bc6 = DrawBtnRel(cX, y1 + 237, ""Knight"", 1, merge_color(c_ltgray, c_blue, 0.4), 1);
    bc7 = DrawBtnRel(cX, y1 + 273, ""Ranger"", 1, merge_color(c_lime, c_green, 0.4), 1);
    bc9 = DrawBtnRel(cX, y1 + 309, ""Duelist"", 1, merge_color(c_white, c_red, 0.45), 1);
    bc10 = DrawBtnRel(cX, y1 + 345, ""Warlock"", 1, merge_color(c_purple, c_fuchsia, 0.5), 1);
}");
importGroup.QueueFindReplace(
    "gml_Object_class_choose_Draw_64",
    "draw_text(cX, y1 + 355, g(\"Классы пассивной игры\", \"Passive play classes\"));",
    "draw_text(cX, y1 + 390, g(\"Классы пассивной игры\", \"Passive play classes\"));");
importGroup.QueueFindReplace(
    "gml_Object_class_choose_Draw_64",
    "bc3 = DrawBtnRel(cX, y1 + 390, \"Ronin\"",
    "bc3 = DrawBtnRel(cX, y1 + 415, \"Ronin\"");
importGroup.QueueFindReplace(
    "gml_Object_class_choose_Draw_64",
    "bc5 = DrawBtnRel(cX, y1 + 390 + 45, c_txt, 1, merge_color(c_blue, c_ltgray, 0.5), 1);",
    @"bc5 = DrawBtnRel(cX, y1 + 451, c_txt, 1, merge_color(c_blue, c_ltgray, 0.5), 1);
bc8 = 0;
bc11 = 0;
bc12 = 0;
if (global.qol_hero)
{
    bc8 = DrawBtnRel(cX, y1 + 487, ""Cleric"", 1, merge_color(c_yellow, c_white, 0.3), 1);
    bc11 = DrawBtnRel(cX, y1 + 523, ""Martyr"", 1, merge_color(c_aqua, c_white, 0.4), 1);
    bc12 = DrawBtnRel(cX, y1 + 559, ""Scholar"", 1, merge_color(c_orange, c_yellow, 0.5), 1);
}");
//    (The game's own tooltips keep their positions: they open to the right of the dialog's centre and the screen clamps them.)

// { button flag, class id, best-level slot, needed level, "[x/N]" requirement texts, description texts, tooltip y }
var C = new (string flag, int cls, int slot, int need, string reqEn, string reqRu, string descEn, string descRu, string y)[]
{
    ("bc6", 6, 1, 12, "an adventurer", "авантюристом",
        "Heavy armor class##Armor x2.5 (and +2 per hero level)#+15% HP#-----#Evasion chance is always 0#-----#Every kill repairs 1 armor per 4 levels (+1)#up to the starting armor",
        "Класс тяжелой брони##Броня x2.5 (и +2 за уровень героя)#+15% HP#-----#Шанс уклонения всегда 0#-----#Каждое убийство чинит броню:#1 + (уровень / 4), до стартовой брони",
        "y1 + 205"),
    ("bc7", 7, 4, 15, "a rogue", "плутом",
        "Ranged specialist##Ranged weapons: damage x2.2, evasion x1.5#-----#Other weapons: damage x0.6#-----#(Shields can't be used with ranged weapons)",
        "Мастер дальнего боя##Стрелковое оружие: урон x2.2, уклонение x1.5#-----#Другое оружие: урон x0.6#-----#(Со стрелковым оружием нельзя щит)",
        "y1 + 250"),
    ("bc8", 8, 3, 15, "a ronin", "ронином",
        "Gets 15 AAP with each level up#-----#+1 AAP for every 20 kills#-----#+1 holiness for every kill##AAP is spent on auto-attacks# and give auto-healing",
        "Получает 15 AAP с каждым уровнем#-----#+1 AAP за каждые 20 убийств#-----#+1 святости за каждое убийство##AAP тратятся на авто-атаки# и дают авто-лечение",
        "y1 + 400"),
    ("bc9", 9, 6, 15, "a knight", "рыцарем",
        "Fencer##Evasion chance x1.5#Armor x0.5#-----#Every dodge is answered with a riposte:#the monster takes 60% of your damage",
        "Фехтовальщик##Шанс уклонения x1.5#Броня x0.5#-----#Каждое уклонение - ответный удар:#монстр получает 60% вашего урона",
        "y1 + 290"),
    ("bc10", 10, 7, 15, "a ranger", "следопытом",
        "Glass cannon##Damage x1.2#HP x0.8#-----#Life drain: every hit heals the hero#for 4% of the damage dealt",
        "Стеклянная пушка##Урон x1.2#HP x0.8#-----#Похищение жизни: каждый удар лечит героя#на 4% нанесенного урона",
        "y1 + 330"),
    ("bc11", 11, 8, 15, "a cleric", "клириком",
        "Gets 10 AAP with each level up#-----#Revives once in every battle#(on top of other revives)##AAP is spent on auto-attacks# and give auto-healing",
        "Получает 10 AAP с каждым уровнем#-----#Воскресает один раз в каждой битве#(в дополнение к другим воскрешениям)##AAP тратятся на авто-атаки# и дают авто-лечение",
        "y1 + 440"),
    ("bc12", 12, 1, 12, "an adventurer", "авантюристом",
        "Gets 5 AAP with each level up#-----#Experience from kills x3#(the adventurer gets x2)##AAP is spent on auto-attacks# and give auto-healing",
        "Получает 5 AAP с каждым уровнем#-----#Опыт за убийства x3#(у авантюриста x2)##AAP тратятся на авто-атаки# и дают авто-лечение",
        "y1 + 480"),
};
var sbC = new System.Text.StringBuilder("\n");
foreach (var c in C)
{
    sbC.Append("if (" + c.flag + ")\n{\n");
    sbC.Append("    var ut = \"\";\n    var _u = 0;\n");
    sbC.Append("    if (_class[" + c.slot + "] < " + c.need + ")\n    {\n        _u = 128;\n");
    sbC.Append("        ut = g(" + S("** Требование разблокировки **#Получить уровень " + c.need + "#" + c.reqRu + " [") + ", " + S("* Unlock Requirement *#Reach hero level " + c.need + "#with " + c.reqEn + " [") + ") + string(_class[" + c.slot + "]) + " + S("/" + c.need + "]#*********************##") + ";\n    }\n");
    sbC.Append("    mHelp(ut + g(" + S(c.descRu) + ", " + S(c.descEn) + "), cX + (_V_ div 2) + 8, " + c.y + ", _u);\n");
    sbC.Append("    if (mouse_check_button_pressed(mb_left) && _class[" + c.slot + "] >= " + c.need + ")\n    {\n        O.hero_CLASS = " + c.cls + ";\n        alarm[0] = 1;\n    }\n");
    sbC.Append("}\n");
}
importGroup.QueueAppend("gml_Object_class_choose_Step_0", sbC.ToString());

// 10) Class tooltip on the hero. Hover the hero (on the academy tile, or in a battle) to read what its class does, so the
//     description does not have to be remembered after the class was chosen. The names and descriptions of all twelve
//     classes live in two global arrays, global.cls_name / global.cls_desc, set in main Create. The first five are the
//     game's own texts (copied from the class dialog); the other seven come from the table above.
var clsName = new Dictionary<int, string> { {1, "Adventurer"}, {2, "Berserker"}, {3, "Ronin"}, {4, "Rogue"}, {5, "Paladin"},
    {6, "Knight"}, {7, "Ranger"}, {8, "Cleric"}, {9, "Duelist"}, {10, "Warlock"}, {11, "Martyr"}, {12, "Scholar"} };
var clsDesc = new Dictionary<int, (string ru, string en)>
{
    { 1, ("Базовый класс##+100% к получаемому опыту", "Basic class##+100% to the experience gained") },
    { 2, ("+200% к урону топоров#+50% к урону другого оружия#-----#Может убить двух монстров за один удар#(если хватает урона)#-----#Шанс уклонения всегда 0",
          "+200% damage to axes#+50% damage to other weapons#-----#Can kill two monsters with one hit#(if there is enough damage)#-----#Evasion chance is always 0") },
    { 3, ("Предметы за золото становятся бесплатными#-----#Получает 30 AAP с каждым уровнем##AAP тратятся на авто-атаки# и дают авто-лечение",
          "Gold items can be used for free#-----#Gets 30 AAP with each level up##AAP is spent on auto-attacks# and give auto-healing") },
    { 4, ("2x шанс уклонения#-----#Герой устает в 4 раза медленнее#-----#-1 к усталости после того как герой получает повреждение#-----#Броня всегда 0",
          "2x evasion chance#-----#The hero gets tired four times slower#-----#-1 to fatigue after the hero gets hit#-----#Armor is always 0") },
    { 5, ("С самого начала имеет святой щит#-----#Не может брать кинжалы#-----#Способен изучать особые апгрейды#На эти апгрейды нужно будет тратить святость",
          "Holy shield is available from the start#-----#Can't use daggers#-----#Can spend holiness to get#special holy upgrades!") },
};
foreach (var c in C) clsDesc[c.cls] = (c.descRu, c.descEn);
var sbCls = new System.Text.StringBuilder();
foreach (var kv in clsDesc.OrderBy(k => k.Key))
{
    sbCls.Append("global.cls_name[" + kv.Key + "] = " + S(clsName[kv.Key]) + ";\n");
    sbCls.Append("global.cls_desc[" + kv.Key + "] = " + G(kv.Value.ru, kv.Value.en) + ";\n");
}
// main Create: in front of a line that the game executes after its texts are ready (the same anchor as in patch 13).
importGroup.QueueFindReplace("gml_Object_main_Create_0", "emp_max = 9;", sbCls.ToString() + "emp_max = 9;");

// The hover areas: the academy tile draws the hero at (x+18, y+48) with its origin in the middle of a 32 x 48 picture; the
// battle tile at (x + hero_x, y + hero_y). Same checks as the game's own hover tooltips on a tile (no window on top).
string clsTip = "g(\"Класс: \", \"Class: \") + global.cls_name[CLS] + \"#\" + global.cls_desc[CLS]";
importGroup.QueueFindReplace(
    "gml_Object_Land_Draw_0",
    "hero_ia += 0.05;",
    "hero_ia += 0.05;\nif (hero_CLASS > 0 && hero_CLASS <= 12 && !upper() && !modal() && point_in_rectangle(mouse_x, mouse_y, x + 2, y + 24, x + 34, y + 72))\n{\n    mHelp(" + clsTip.Replace("CLS", "hero_CLASS") + ");\n}");
// 11) Unlocked classes stay unlocked when you sail away. The best level reached with each class ([CLS] c1..c12, which
//     decides the unlocks) lives in the save file "game2", and sailing to a new continent deletes that file and writes a new
//     one with only the relics and Empire data. The values are read just before the file is deleted and written into the new one.
importGroup.QueueFindReplace(
    "gml_Object_main_Alarm_11",
    "CONT++;",
    @"ini_open(""game2"");
for (var _ci = 1; _ci <= 12; _ci++)
{
    global.cls_keep[_ci] = ini_read_real(""CLS"", ""c"" + string(_ci), 0);
}
ini_close();
CONT++;");
importGroup.QueueFindReplace(
    "gml_Object_main_Alarm_11",
    "ini_write_string(\"EMP\", \"empire_ppp\", string(main.empire_ppp));",
    @"ini_write_string(""EMP"", ""empire_ppp"", string(main.empire_ppp));
for (var _cj = 1; _cj <= 12; _cj++)
{
    if (global.cls_keep[_cj] > 0)
    {
        ini_write_string(""CLS"", ""c"" + string(_cj), string(global.cls_keep[_cj]));
    }
}");

importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Draw_0",
    "hero_ia += (0.05 + (0.025 * global.S_BTL * global.qol_ab[7]));",
    "if (!ranen && O.hero_CLASS > 0 && O.hero_CLASS <= 12 && !upper() && !modal() && point_in_rectangle(mouse_x, mouse_y, (x + hero_x) - 16, (y + hero_y) - 24, x + hero_x + 16, y + hero_y + 24))\n{\n    mHelp(" + clsTip.Replace("CLS", "O.hero_CLASS") + ");\n}\nhero_ia += (0.05 + (0.025 * global.S_BTL * global.qol_ab[7]));");

importGroup.Import();
