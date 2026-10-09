// QoL: more hero SHIELDS (5) and HELMETS (8), in the slots the game has art for but no item behind (the pickers draw them as
// dimmed, locked icons). Switch: global.qol_hero (same switch as the new weapons and classes, see 10_hero_classes_weapons.csx).
//
// Shields (picker slots 0..10 are shields, 11..N2-1 are off-hand daggers that replace the shield; the game's Paladin may not
// use the daggers). Two more frames exist: dagger 14 has no item, and dagger 15 is outside N2 (we raise N2 to 16).
//   id  name              unlocks at   effect
//   6   Kite shield       level 5      +25 armor, +25 HP
//   7   Crescent shield   level 11     +20 armor, +25 evasion
//   8   Spiked rhomb      level 17     +50 armor; every time the hero is hit it gains +4 damage for the rest of the fight
//   14  Parrying dagger   level 16     +25 damage, +20 evasion (doubled/etc. by weapon 4 like the other daggers)
//   15  Fang dagger       level 20     +50 damage, +8 evasion (same)
//
// Helmets (the picker draws 8 per row; N2 goes from 16 to 17, which makes a third row). Evasion on helmets is multiplied by
// the same factor as for the game's own helmets (weapon 4 and shield 4 double and quintuple it).
//   6   Cap               level 3      +35 HP, +10 evasion
//   7   Hood              level 6      +25 HP, +8 damage
//   8   Crusader helm     level 9      +60 HP, +8 armor
//   9   Horned helm       level 11     +40 HP, +20 damage
//   11  Winged helm       level 14     +60 HP, +25 evasion
//   14  Great helm        level 16     +100 HP, +25 armor
//   15  Battle mask       level 19     +60 damage, +10 evasion
//   16  Crown             level 21     +300 HP, +30 damage, +15 armor
//
// Plug-in points: hg_shi / hg_shlem (stats + tooltip in both languages), the two pickers (unlock by hero level + the yellow
// "New!" tag), the picker size N2, and the battle tick for the Spiked rhomb.

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

string S(string text) => "\"" + text.Replace("\"", "\\\"") + "\"";

// { id, unlock level, name EN, name RU, damage, HP, armor, evasion (multiplied by dbl when evaDbl), evaDbl, extra EN, extra RU }
var items = new (bool shield, int id, int lvl, string nameEn, string nameRu, int dmg, int hp, int shi, int uki, bool evaDbl, string extraEn, string extraRu)[]
{
    (true,   6,  5, "Kite shield",     "Каплевидный щит",   0, 25, 25,  0, false, null, null),
    (true,   7, 11, "Crescent shield", "Полулунный щит",    0,  0, 20, 25, false, null, null),
    (true,   8, 17, "Spiked rhomb",    "Ромб с шипами",     0,  0, 50,  0, false, "#+4 damage every time you are hit", "#+4 к урону при каждом полученном ударе"),
    (true,  14, 16, "Parrying dagger", "Парирующий кинжал", 25, 0,  0, 20, true,  null, null),
    (true,  15, 20, "Fang dagger",     "Кинжал-клык",       50, 0,  0,  8, true,  null, null),
    (false,  6,  3, "Cap",             "Колпак",            0, 35,  0, 10, true,  null, null),
    (false,  7,  6, "Hood",            "Капюшон",           8, 25,  0,  0, false, null, null),
    (false,  8,  9, "Crusader helm",   "Шлем крестоносца",  0, 60,  8,  0, false, null, null),
    (false,  9, 11, "Horned helm",     "Рогатый шлем",     20, 40,  0,  0, false, null, null),
    (false, 11, 14, "Winged helm",     "Крылатый шлем",     0, 60,  0, 25, true,  null, null),
    (false, 14, 16, "Great helm",      "Большой шлем",      0, 100, 25, 0, false, null, null),
    (false, 15, 19, "Battle mask",     "Боевая маска",     60,  0,  0, 10, true,  null, null),
    (false, 16, 21, "Crown",           "Корона",           30, 300, 15, 0, false, null, null),
};

// hg_shi / hg_shlem: new cases in front of the last one. hlp: 0 = apply, 1 = Russian text, 2 = English text.
foreach (bool shield in new[] { true, false })
{
    var sb = new System.Text.StringBuilder();
    foreach (var it in items)
    {
        if (it.shield != shield) continue;
        string uki = it.evaDbl ? it.uki + " * dbl" : it.uki.ToString();
        string en = S(it.nameEn);
        string ru = S(it.nameRu);
        if (it.dmg > 0) { en += " + " + S("#Damage: +" + it.dmg); ru += " + " + S("#Урон: +" + it.dmg); }
        if (it.hp > 0)  { en += " + " + S("#HP: +" + it.hp);      ru += " + " + S("#HP: +" + it.hp); }
        if (it.shi > 0) { en += " + " + S("#Armor: +" + it.shi);  ru += " + " + S("#Защита: +" + it.shi); }
        if (it.uki > 0) { en += " + " + S("#Evasion chance: +") + " + string(" + uki + ")"; ru += " + " + S("#Шанс уклонения: +") + " + string(" + uki + ")"; }
        if (it.extraEn != null) { en += " + " + S(it.extraEn); ru += " + " + S(it.extraRu); }
        sb.Append("    case " + it.id + ":\n");
        sb.Append("        if (hlp > 1)\n        {\n            return " + en + ";\n        }\n");
        sb.Append("        if (hlp)\n        {\n            return " + ru + ";\n        }\n");
        if (it.dmg > 0) sb.Append("        hero.hero_DAM += " + it.dmg + ";\n");
        if (it.hp > 0)  sb.Append("        hero.hero_HP += " + it.hp + ";\n");
        if (it.shi > 0) sb.Append("        hero.hero_SHI += " + it.shi + ";\n");
        if (it.uki > 0) sb.Append("        hero.hero_UKL += " + uki + ";\n");
        sb.Append("        break;\n");
    }
    importGroup.QueueFindReplace(shield ? "gml_Script_hg_shi" : "gml_Script_hg_shlem", "case 13:", sb.ToString() + "    case 13:");
}

// Pickers: unlock lines (in front of the "free for Ronin" check, which both Alarm_1 events have exactly once), the "New!" tag,
// and the room for one more dagger / helmet.
foreach (bool shield in new[] { true, false })
{
    var sbAllow = new System.Text.StringBuilder();
    var sbNew = new System.Text.StringBuilder("if (O.hero_new)\n{\n");
    foreach (var it in items)
    {
        if (it.shield != shield) continue;
        sbAllow.Append("if (global.qol_hero && O.hero_lvl >= " + it.lvl + ")\n{\n    allow[" + it.id + "] = 1;\n}\n");
        sbNew.Append("    if (global.qol_hero && O.hero_lvl == " + it.lvl + ")\n    {\n        new[" + it.id + "] = 1;\n    }\n");
    }
    sbNew.Append("}\n");
    string alarm = shield ? "gml_Object_shi_choose_Alarm_1" : "gml_Object_shlem_choose_Alarm_1";
    importGroup.QueueFindReplace(alarm, "if (O.hero_CLASS == 3)", sbAllow.ToString() + "if (O.hero_CLASS == 3)");
    importGroup.QueueFindReplace(alarm, "if (O.hero_new)", sbNew.ToString() + "if (O.hero_new)");
}
importGroup.QueueFindReplace("gml_Object_shi_choose_Create_0", "N2 = 15;", "N2 = 16;");
importGroup.QueueFindReplace("gml_Object_shlem_choose_Create_0", "N2 = 16;", "N2 = 17;");

// Spiked rhomb: +4 damage every time the monster's hit lands (in front of the Thorn club's check, which is inside the same
// "the hero was hit" branch of the battle tick).
importGroup.QueueFindReplace(
    "gml_Object_LandBattle_Other_10",
    "if (O.hero_wp == 14)",
    @"if (O.hero_ss == 8)
{
    O.hero_DAM += 4;
}
if (O.hero_wp == 14)");

importGroup.Import();
