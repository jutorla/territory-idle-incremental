// Fame Shop window - Create.
// 32 perks in 4 branches of 8 (index = branch * 8 + slot). Each branch is a "diamond":
//   slot 0 root (row 0)
//   left path  slots 1, 2, 3  (rows 1-3)     slot 1 needs slot 0, slot 2 needs 1, slot 3 needs 2
//   right path slots 4, 5, 6  (rows 1-3)     slot 4 needs slot 0, slot 5 needs 4, slot 6 needs 5
//   slot 7 KEYSTONE (row 4) needs BOTH slot 3 and slot 6
// Per-perk arrays: nm name, ds description, vv value per level, sg sign, sx unit.
// Levels live in global.fs_lv[], max levels in global.fs_mx[], costs in global.fs_bs[] (level L costs bs * L).
x1 = 15;
x2 = 985;
y1 = 50;
y2 = 782;
cX = (x1 + x2) div 2;
NN = 32;
bc = 0;
brs = 0;
rcf = 0;
hov = -1;
av = 0;
tot = 0;

// price of restoring the buildings after abdication, by Blueprints level (perk 26)
bpc[0] = 0;
bpc[1] = 5000000;
bpc[2] = 2000000;
bpc[3] = 500000;
bpc[4] = 100000;
bpc[5] = 0;

bcol[0] = merge_color(c_orange, c_yellow, 0.4);
bcol[1] = merge_color(c_red, c_white, 0.3);
bcol[2] = merge_color(c_aqua, c_white, 0.15);
bcol[3] = merge_color(c_green, c_lime, 0.55);
bnm[0] = g("Производство", "Industry");
bnm[1] = g("Завоевание", "Conquest");
bnm[2] = g("Благочестие", "Devotion");
bnm[3] = g("Наследие", "Legacy");

// ================= INDUSTRY =================
nm[0] = g("Ловкие руки", "Quick Hands");
ds[0] = g("+25% к скорости найма рабочих#за уровень.", "+25% worker hiring speed#per level.");
vv[0] = 25;
sg[0] = "+";
sx[0] = "%";

nm[1] = g("Честная оплата", "Fair Wages");
ds[1] = g("-10% к стоимости найма рабочих#за уровень.", "-10% cost of hiring workers#per level.");
vv[1] = 10;
sg[1] = "-";
sx[1] = "%";

nm[2] = g("Эффект масштаба", "Economies of Scale");
ds[2] = g("-0.6% к росту цены каждого#следующего рабочего за уровень.", "-0.6% growth of the cost of each#next worker, per level.");
vv[2] = 0.6;
sg[2] = "-";
sx[2] = "%";

nm[3] = g("Массовый набор", "Open Enrollment");
ds[3] = g("+1 дополнительный рабочий за каждого#нанятого рабочего за уровень#(не герои).", "+1 extra worker for every worker#you hire, per level#(not heroes).");
vv[3] = 1;
sg[3] = "+";
sx[3] = g(" раб.", " workers");

nm[4] = g("Мастера-ремесленники", "Master Craftsmen");
ds[4] = g("+3% к выработке здания за каждые#20 рабочих в нём (до 200) за уровень.", "+3% output of a building for every#20 workers in it (up to 200), per level.");
vv[4] = 3;
sg[4] = "+";
sx[4] = "%";

nm[5] = g("Имперские амбиции", "Imperial Ambition");
ds[5] = g("+0.4% к общему производству за каждую#вашу клетку за уровень.", "+0.4% total production for every#tile you own, per level.");
vv[5] = 0.4;
sg[5] = "+";
sx[5] = "%";

nm[6] = g("Знаменитые мастерские", "Renowned Workshops");
ds[6] = g("+0.8% к общему производству за уровень,#умноженные на корень из вашей славы.", "+0.8% total production per level,#multiplied by the square root of your Fame.");
vv[6] = 0.8;
sg[6] = "+";
sx[6] = "%";

nm[7] = g("Надсмотрщики", "Foremen");
ds[7] = g("1% вашего дохода зерна, дерева, камня и#веры добавляется к доходу золота#за уровень.", "1% of your wheat, wood, stone and faith#income is added to your gold income,#per level.");
vv[7] = 1;
sg[7] = "+";
sx[7] = "%";

// ================= CONQUEST =================
nm[8] = g("Гильдия землемеров", "Surveyors' Guild");
ds[8] = g("Цена новой клетки растёт в 10 раз за клетку;#каждый уровень снижает этот множитель#на 0.5.", "A new tile's price grows x10 per tile;#every level lowers that multiplier#by 0.5.");
vv[8] = 0.5;
sg[8] = "-";
sx[8] = "";

nm[9] = g("Стойкость ветерана", "Veteran's Resolve");
ds[9] = g("-8% монстров, нужных для победы#в битве за клетку, за уровень.", "-8% monsters to defeat in each#tile battle, per level.");
vv[9] = 8;
sg[9] = "-";
sx[9] = "%";

nm[10] = g("Боевая мудрость", "Battle Wisdom");
ds[10] = g("+25% к опыту героя#за уровень.", "+25% hero experience#per level.");
vv[10] = 25;
sg[10] = "+";
sx[10] = "%";

nm[11] = g("Добыча", "Plunder");
ds[11] = g("Каждый убитый монстр даёт 2% секундного#дохода зерна, дерева, камня и веры#за уровень.", "Every monster killed gives 2% of one#second of your wheat, wood, stone and#faith income, per level.");
vv[11] = 2;
sg[11] = "+";
sx[11] = "%";

nm[12] = g("Феникс", "Phoenix");
ds[12] = g("+1 возрождение героя за бой#за уровень.", "+1 hero revive per battle#per level.");
vv[12] = 1;
sg[12] = "+";
sx[12] = "";

nm[13] = g("Трофеи войны", "Spoils of War");
ds[13] = g("Захват клетки приносит 20% золотой цены#следующей клетки за уровень.", "Conquering a tile pays 20% of the#gold price of the next tile, per level.");
vv[13] = 20;
sg[13] = "+";
sx[13] = "%";

nm[14] = g("Земельные акты", "Land Deeds");
ds[14] = g("Покупка клетки возвращает 10% её#золотой цены за уровень.", "Buying a tile refunds 10% of its#gold price, per level.");
vv[14] = 10;
sg[14] = "+";
sx[14] = "%";

nm[15] = g("Размах", "Cleave");
ds[15] = g("Каждое убийство считается как +1#дополнительное убийство для захвата#клетки за уровень.", "Each kill counts as +1 extra kill#toward capturing the tile,#per level.");
vv[15] = 1;
sg[15] = "+";
sx[15] = g(" убийств", " kills");

// ================= DEVOTION =================
nm[16] = g("Долгие обряды", "Long Rituals");
ds[16] = g("+10 секунд к длительности обряда#за уровень.", "+10 seconds ritual duration#per level.");
vv[16] = 10;
sg[16] = "+";
sx[16] = g(" с", " s");

nm[17] = g("Бережливые обряды", "Frugal Rites");
ds[17] = g("-10% к стоимости обряда#за уровень.", "-10% ritual cost#per level.");
vv[17] = 10;
sg[17] = "-";
sx[17] = "%";

nm[18] = g("Сила обряда", "Ritual Power");
ds[18] = g("+2 к силе обряда за уровень#(бонусы обряда сильнее).", "+2 ritual power per level#(stronger ritual bonuses).");
vv[18] = 2;
sg[18] = "+";
sx[18] = "";

nm[19] = g("Апофеоз", "Apotheosis");
ds[19] = g("+20% к производству ВСЕХ ресурсов#во время обряда за уровень.", "+20% production of ALL resources#while a ritual is running, per level.");
vv[19] = 20;
sg[19] = "+";
sx[19] = "%";

nm[20] = g("Благочестивые учёные", "Devout Scholars");
ds[20] = g("-10% к цене религиозных перков#в святости за уровень.", "-10% holiness cost of religion#perks, per level.");
vv[20] = 10;
sg[20] = "-";
sx[20] = "%";

nm[21] = g("Божья милость", "Divine Favor");
ds[21] = g("+25% к приросту святости#за уровень.", "+25% holiness gain#per level.");
vv[21] = 25;
sg[21] = "+";
sx[21] = "%";

nm[22] = g("Преданность обрядам", "Ritual Devotion");
ds[22] = g("+0.1% к общему производству за каждый#проведённый обряд (до 200) за уровень.", "+0.1% total production for every#ritual you performed (up to 200),#per level.");
vv[22] = 0.1;
sg[22] = "+";
sx[22] = "%";

nm[23] = g("Вневременные обряды", "Timeless Rites");
ds[23] = g("-25% к росту цены обряда после каждого#проведённого обряда за уровень#(4 уровень = цена не растёт).", "-25% of the cost increase that every#ritual adds, per level#(level 4 = the cost never rises).");
vv[23] = 25;
sg[23] = "-";
sx[23] = "%";

// ================= LEGACY =================
nm[24] = g("Королевская казна", "Royal Treasury");
ds[24] = g("+25% к золоту за отречение#за уровень.", "+25% gold when you abdicate#per level.");
vv[24] = 25;
sg[24] = "+";
sx[24] = "%";

nm[25] = g("Наследство", "Heirloom");
ds[25] = g("При отречении сохраняется 8% зерна,#дерева, камня и веры за уровень.", "Keep 8% of your wheat, wood, stone#and faith when you abdicate,#per level.");
vv[25] = 8;
sg[25] = "+";
sx[25] = "%";

nm[26] = g("Чертежи", "Blueprints");
ds[26] = g("Восстановление зданий после отречения#(без рабочих). Цена: 5 млн золота,#2 млн, 500 тыс., 100 тыс., бесплатно.", "Restore your buildings after abdication#(without workers). Price: 5M gold,#2M, 500K, 100K, then free.");
vv[26] = 0;
sg[26] = "";
sx[26] = "";

nm[27] = g("Дух континентов", "Continental Spirit");
ds[27] = g("+3% к общему производству за каждый#посещённый континент за уровень.", "+3% total production for every#continent you visited, per level.");
vv[27] = 3;
sg[27] = "+";
sx[27] = "%";

nm[28] = g("Гильдия корабелов", "Shipwrights' Guild");
ds[28] = g("-10% кораблей, нужных для плавания#на другой континент, за уровень.", "-10% ships needed to sail away#to another continent, per level.");
vv[28] = 10;
sg[28] = "-";
sx[28] = "%";

nm[29] = g("Славное плавание", "Famous Voyage");
ds[29] = g("+10% славы при плавании#на другой континент за уровень.", "+10% Fame when you sail away#to another continent, per level.");
vv[29] = 10;
sg[29] = "+";
sx[29] = "%";

nm[30] = g("Торговый флот", "Merchant Fleet");
ds[30] = g("+0.01% к общему производству за каждый#построенный корабль за уровень.", "+0.01% total production for every#ship you built, per level.");
vv[30] = 0.01;
sg[30] = "+";
sx[30] = "%";

nm[31] = g("Легенда славы", "Legend of Fame");
ds[31] = g("Каждое очко славы даёт +2% к золоту#за отречение за уровень#(базовый бонус: +3% за очко).", "Each Fame point gives +2% more gold#when you abdicate, per level#(the base bonus is +3% per point).");
vv[31] = 2;
sg[31] = "+";
sx[31] = "%";

// ---------------- layout / links ----------------
for (var i = 0; i < NN; i++)
{
    var b = i div 8;
    var k = i mod 8;
    brn[i] = b;
    slt[i] = k;
    var cxx = x1 + (((x2 - x1) / 4) * (b + 0.5));
    px[i] = cxx;
    py[i] = y1 + 150;
    req[i] = -1;
    req2[i] = -1;
    rdi[i] = 27;
    if (k == 1)
    {
        px[i] = cxx - 60;
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
        px[i] = cxx - 60;
        py[i] = y1 + 456;
        req[i] = i - 1;
    }
    if (k == 4)
    {
        px[i] = cxx + 60;
        py[i] = y1 + 252;
        req[i] = i - 4;
    }
    if (k == 5)
    {
        px[i] = cxx + 60;
        py[i] = y1 + 354;
        req[i] = i - 1;
    }
    if (k == 6)
    {
        px[i] = cxx + 60;
        py[i] = y1 + 456;
        req[i] = i - 1;
    }
    if (k == 7)
    {
        py[i] = y1 + 558;
        req[i] = i - 4;
        req2[i] = i - 1;
        rdi[i] = 31;
    }
}
