// Fame Shop window - Create.
// 28 perks in 4 branches of 7 (index = branch * 7 + slot):
//   slot 0 root (row 0)            slot 1 needs slot 0 (row 1)
//   slot 2 / slot 3 need slot 1    (row 2, left / right)
//   slot 4 needs slot 2, slot 5 needs slot 3   (row 3, left / right)
//   slot 6 capstone needs BOTH slot 4 and slot 5 (row 4)
// Per-perk arrays: nm name, ds description, vv value per level, sg sign, sx unit.
// Levels live in global.fs_lv[], max levels in global.fs_mx[], costs in global.fs_bs[] (level L costs bs * L).
x1 = 15;
x2 = 985;
y1 = 50;
y2 = 782;
cX = (x1 + x2) div 2;
NN = 28;
bc = 0;
brs = 0;
rcf = 0;
hov = -1;
av = 0;
tot = 0;

bcol[0] = merge_color(c_orange, c_yellow, 0.4);
bcol[1] = merge_color(c_green, c_lime, 0.55);
bcol[2] = merge_color(c_red, c_white, 0.3);
bcol[3] = merge_color(c_aqua, c_white, 0.15);
bnm[0] = g("Процветание", "Prosperity");
bnm[1] = g("Производство", "Industry");
bnm[2] = g("Доблесть", "Valor");
bnm[3] = g("Благочестие", "Devotion");

// ---------------- Prosperity ----------------
nm[0] = g("Плодородные поля", "Fertile Fields");
ds[0] = g("+25% к производству зерна#за уровень.", "+25% wheat production#per level.");
vv[0] = 25;
sg[0] = "+";
sx[0] = "%";

nm[1] = g("Лесной бум", "Lumber Boom");
ds[1] = g("+25% к производству дерева#за уровень.", "+25% wood production#per level.");
vv[1] = 25;
sg[1] = "+";
sx[1] = "%";

nm[2] = g("Мастера каменоломен", "Quarry Masters");
ds[2] = g("+25% к производству камня#за уровень.", "+25% stone production#per level.");
vv[2] = 25;
sg[2] = "+";
sx[2] = "%";

nm[3] = g("Золотое прикосновение", "Golden Touch");
ds[3] = g("+30% к доходу золота#за уровень.", "+30% gold income#per level.");
vv[3] = 30;
sg[3] = "+";
sx[3] = "%";

nm[4] = g("Королевская казна", "Royal Treasury");
ds[4] = g("+25% к золоту за отречение#за уровень.", "+25% gold from abdication#per level.");
vv[4] = 25;
sg[4] = "+";
sx[4] = "%";

nm[5] = g("Земельные дары", "Land Grants");
ds[5] = g("-12% к золотой цене новых клеток#за уровень.", "-12% gold cost of new tiles#per level.");
vv[5] = 12;
sg[5] = "-";
sx[5] = "%";

nm[6] = g("Золотой век", "Golden Age");
ds[6] = g("+12% к производству ВСЕХ ресурсов#(зерно, дерево, камень, вера)#за уровень.", "+12% production of ALL resources#(wheat, wood, stone, faith)#per level.");
vv[6] = 12;
sg[6] = "+";
sx[6] = "%";

// ---------------- Industry ----------------
nm[7] = g("Ловкие руки", "Quick Hands");
ds[7] = g("+30% к скорости найма рабочих#за уровень.", "+30% worker hiring speed#per level.");
vv[7] = 30;
sg[7] = "+";
sx[7] = "%";

nm[8] = g("Честная оплата", "Fair Wages");
ds[8] = g("-10% к стоимости найма рабочих#за уровень.", "-10% cost of hiring workers#per level.");
vv[8] = 10;
sg[8] = "-";
sx[8] = "%";

nm[9] = g("Запас семян", "Seed Stock");
ds[9] = g("+1000 зерна, дерева и камня в начале#каждой новой игры за уровень.", "+1000 wheat, wood and stone at the start#of every new game, per level.");
vv[9] = 1000;
sg[9] = "+";
sx[9] = "";

nm[10] = g("Запасы паломника", "Pilgrim's Provisions");
ds[10] = g("+1000 веры в начале каждой#новой игры за уровень.", "+1000 faith at the start of every#new game, per level.");
vv[10] = 1000;
sg[10] = "+";
sx[10] = "";

nm[11] = g("Массовый набор", "Open Enrollment");
ds[11] = g("+1 дополнительный рабочий за каждого#нанятого рабочего за уровень#(не герои).", "+1 extra worker for every worker#you hire, per level#(not heroes).");
vv[11] = 1;
sg[11] = "+";
sx[11] = g(" раб.", " workers");

nm[12] = g("Эффект масштаба", "Economies of Scale");
ds[12] = g("-0.6% к росту цены каждого#следующего рабочего за уровень.", "-0.6% growth of the cost of each#next worker, per level.");
vv[12] = 0.6;
sg[12] = "-";
sx[12] = "%";

nm[13] = g("Гильдейская хартия", "Guild Charter");
ds[13] = g("+1 ещё один дополнительный рабочий#за каждого нанятого, за уровень#(не герои).", "+1 more extra worker for every worker#you hire, per level#(not heroes).");
vv[13] = 1;
sg[13] = "+";
sx[13] = g(" раб.", " workers");

// ---------------- Valor ----------------
nm[14] = g("Стойкость ветерана", "Veteran's Resolve");
ds[14] = g("-8% монстров, нужных для победы#в битве за клетку, за уровень.", "-8% monsters to defeat in each#tile battle, per level.");
vv[14] = 8;
sg[14] = "-";
sx[14] = "%";

nm[15] = g("Мощные удары", "Mighty Blows");
ds[15] = g("+25% к урону героя#за уровень.", "+25% hero damage#per level.");
vv[15] = 25;
sg[15] = "+";
sx[15] = "%";

nm[16] = g("Боевая мудрость", "Battle Wisdom");
ds[16] = g("+25% к опыту героя#за уровень.", "+25% hero experience#per level.");
vv[16] = 25;
sg[16] = "+";
sx[16] = "%";

nm[17] = g("Толстая кожа", "Thick Skin");
ds[17] = g("-10% к получаемому героем урону#за уровень.", "-10% damage taken by the hero#per level.");
vv[17] = 10;
sg[17] = "-";
sx[17] = "%";

nm[18] = g("Быстрые удары", "Swift Strikes");
ds[18] = g("+20% к скорости атаки героя#за уровень.", "+20% hero attack speed#per level.");
vv[18] = 20;
sg[18] = "+";
sx[18] = "%";

nm[19] = g("Проворные ноги", "Nimble Feet");
ds[19] = g("+4% к шансу уклониться от атаки#монстра за уровень.", "+4% chance to dodge a monster's#attack, per level.");
vv[19] = 4;
sg[19] = "+";
sx[19] = "%";

nm[20] = g("Знамя полководца", "Warlord's Banner");
ds[20] = g("+15% к урону и опыту героя#за уровень.", "+15% hero damage and experience#per level.");
vv[20] = 15;
sg[20] = "+";
sx[20] = "%";

// ---------------- Devotion ----------------
nm[21] = g("Долгие обряды", "Long Rituals");
ds[21] = g("+10 секунд к длительности обряда#за уровень.", "+10 seconds ritual duration#per level.");
vv[21] = 10;
sg[21] = "+";
sx[21] = g(" с", " s");

nm[22] = g("Бережливые обряды", "Frugal Rites");
ds[22] = g("-10% к стоимости обряда#за уровень.", "-10% ritual cost#per level.");
vv[22] = 10;
sg[22] = "-";
sx[22] = "%";

nm[23] = g("Божья милость", "Divine Favor");
ds[23] = g("+25% к приросту святости#за уровень.", "+25% holiness gain#per level.");
vv[23] = 25;
sg[23] = "+";
sx[23] = "%";

nm[24] = g("Верные сердца", "Faithful Hearts");
ds[24] = g("+25% к производству веры#за уровень.", "+25% faith production#per level.");
vv[24] = 25;
sg[24] = "+";
sx[24] = "%";

nm[25] = g("Сила обряда", "Ritual Power");
ds[25] = g("+2 к силе обряда за уровень#(бонусы обряда сильнее).", "+2 ritual power per level#(stronger ritual bonuses).");
vv[25] = 2;
sg[25] = "+";
sx[25] = "";

nm[26] = g("Эхо обряда", "Ritual Echo");
ds[26] = g("+2 к автоматизации обрядов за уровень#(больше обрядов подряд).", "+2 ritual automation per level#(more rituals in a row).");
vv[26] = 2;
sg[26] = "+";
sx[26] = "";

nm[27] = g("Апофеоз", "Apotheosis");
ds[27] = g("+20% к производству ВСЕХ ресурсов#во время обряда за уровень.", "+20% production of ALL resources#while a ritual is running, per level.");
vv[27] = 20;
sg[27] = "+";
sx[27] = "%";

// ---------------- layout / links ----------------
for (var i = 0; i < NN; i++)
{
    var b = i div 7;
    var k = i mod 7;
    brn[i] = b;
    var cxx = x1 + (((x2 - x1) / 4) * (b + 0.5));
    px[i] = cxx;
    py[i] = y1 + 150;
    req[i] = -1;
    req2[i] = -1;
    if (k == 1)
    {
        py[i] = y1 + 252;
        req[i] = i - 1;
    }
    if (k == 2)
    {
        px[i] = cxx - 60;
        py[i] = y1 + 354;
        req[i] = i - 1;
    }
    if (k == 3)
    {
        px[i] = cxx + 60;
        py[i] = y1 + 354;
        req[i] = i - 2;
    }
    if (k == 4)
    {
        px[i] = cxx - 60;
        py[i] = y1 + 456;
        req[i] = i - 2;
    }
    if (k == 5)
    {
        px[i] = cxx + 60;
        py[i] = y1 + 456;
        req[i] = i - 2;
    }
    if (k == 6)
    {
        py[i] = y1 + 558;
        req[i] = i - 2;
        req2[i] = i - 1;
    }
}
