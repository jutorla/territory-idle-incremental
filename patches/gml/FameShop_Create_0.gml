// Fame Shop window - Create.
// 12 perks in 3 branches of 4 (index = branch * 4 + k):
//   k = 0 root, k = 1 needs the root, k = 2 and k = 3 need k = 1.
// Per-perk arrays: nm name, ds description, vv value per level, sg sign, sx unit.
// Levels live in global.fs_lv[], costs in global.fs_bs[] (cost of level L = bs * L).
x1 = 100;
x2 = 900;
y1 = 70;
y2 = 730;
cX = (x1 + x2) div 2;
NN = 12;
RM = 5;
bc = 0;
brs = 0;
rcf = 0;
hov = -1;
av = 0;
tot = 0;

bcol[0] = merge_color(c_orange, c_yellow, 0.4);
bcol[1] = merge_color(c_green, c_lime, 0.55);
bcol[2] = merge_color(c_red, c_white, 0.3);
bnm[0] = g("Процветание", "Prosperity");
bnm[1] = g("Производство", "Industry");
bnm[2] = g("Доблесть", "Valor");

// ---- Prosperity ----
nm[0] = g("Щедрый урожай", "Bountiful Harvest");
ds[0] = g("+10% к производству зерна,#дерева и камня за уровень.", "+10% wheat, wood and stone#production per level.");
vv[0] = 10;
sg[0] = "+";
sx[0] = "%";

nm[1] = g("Верные сердца", "Faithful Hearts");
ds[1] = g("+10% к производству веры за уровень.", "+10% faith production per level.");
vv[1] = 10;
sg[1] = "+";
sx[1] = "%";

nm[2] = g("Королевская казна", "Royal Treasury");
ds[2] = g("+10% к золоту за отречение за уровень.", "+10% gold from abdication per level.");
vv[2] = 10;
sg[2] = "+";
sx[2] = "%";

nm[3] = g("Земельные дары", "Land Grants");
ds[3] = g("-8% к золотой цене новых клеток#за уровень.", "-8% gold cost of new tiles#per level.");
vv[3] = 8;
sg[3] = "-";
sx[3] = "%";

// ---- Industry ----
nm[4] = g("Ловкие руки", "Quick Hands");
ds[4] = g("+15% к скорости найма рабочих#за уровень.", "+15% worker hiring speed#per level.");
vv[4] = 15;
sg[4] = "+";
sx[4] = "%";

nm[5] = g("Честная оплата", "Fair Wages");
ds[5] = g("-6% к стоимости найма рабочих#за уровень.", "-6% cost of hiring workers#per level.");
vv[5] = 6;
sg[5] = "-";
sx[5] = "%";

nm[6] = g("Запас семян", "Seed Stock");
ds[6] = g("+300 зерна, дерева и камня в начале#каждой новой игры за уровень.", "+300 wheat, wood and stone at the start#of every new game, per level.");
vv[6] = 300;
sg[6] = "+";
sx[6] = "";

nm[7] = g("Запасы паломника", "Pilgrim's Provisions");
ds[7] = g("+300 веры в начале каждой#новой игры за уровень.", "+300 faith at the start of every#new game, per level.");
vv[7] = 300;
sg[7] = "+";
sx[7] = "";

// ---- Valor ----
nm[8] = g("Стойкость ветерана", "Veteran's Resolve");
ds[8] = g("-6% монстров, нужных для победы#в битве за клетку, за уровень.", "-6% monsters to defeat in each#tile battle, per level.");
vv[8] = 6;
sg[8] = "-";
sx[8] = "%";

nm[9] = g("Боевая мудрость", "Battle Wisdom");
ds[9] = g("+12% к опыту героя за уровень.", "+12% hero experience per level.");
vv[9] = 12;
sg[9] = "+";
sx[9] = "%";

nm[10] = g("Долгие обряды", "Long Rituals");
ds[10] = g("+6 секунд к длительности обряда#за уровень.", "+6 seconds ritual duration#per level.");
vv[10] = 6;
sg[10] = "+";
sx[10] = g(" с", " s");

nm[11] = g("Бережливые обряды", "Frugal Rites");
ds[11] = g("-6% к стоимости обряда за уровень.", "-6% ritual cost per level.");
vv[11] = 6;
sg[11] = "-";
sx[11] = "%";

// ---- layout / links ----
for (var i = 0; i < NN; i++)
{
    var b = i div 4;
    var k = i mod 4;
    brn[i] = b;
    var cxx = x1 + (((x2 - x1) / 3) * (b + 0.5));
    px[i] = cxx;
    py[i] = y1 + 190;
    req[i] = -1;
    if (k == 1)
    {
        py[i] = y1 + 330;
        req[i] = i - 1;
    }
    if (k == 2)
    {
        px[i] = cxx - 70;
        py[i] = y1 + 470;
        req[i] = i - 1;
    }
    if (k == 3)
    {
        px[i] = cxx + 70;
        py[i] = y1 + 470;
        req[i] = i - 2;
    }
}
