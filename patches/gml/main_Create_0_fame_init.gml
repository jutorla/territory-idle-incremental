// Fame Shop globals, initialised once at the very top of main Create.
// fs_lv[]  perk levels, saved in the "never" ini ([FAME2] section: kept through abdication, sailing away,
//          exports/imports; deleted only by a hard reset; Divine Trials start without it).
// fs_mx[]  max level of each perk (5, except perk 11 = 3 and perk 13 = 2).
// fs_bs[]  base cost by row of the tree: row 0 = 4, row 1 = 7, row 2 = 10, row 3 = 15, row 4 (capstone) = 25.
//          Level L of a perk costs base * L perk points.
// qol_fame on/off switch from Options > QoL Features ([QOL] fame in the "opt" ini, default ON).
if (!variable_global_exists("fs_lv"))
{
    ini_open("opt");
    global.qol_fame = ini_read_real("QOL", "fame", 1);
    ini_close();
    ini_open("never");
    for (var _fi = 0; _fi < 28; _fi++)
    {
        global.fs_mx[_fi] = 5;
        if (_fi == 11)
        {
            global.fs_mx[_fi] = 3;
        }
        if (_fi == 13)
        {
            global.fs_mx[_fi] = 2;
        }
        global.fs_lv[_fi] = min(global.fs_mx[_fi], ini_read_real("FAME2", "q" + string(_fi), 0));
        var _fk = _fi mod 7;
        global.fs_bs[_fi] = 4;
        if (_fk == 1)
        {
            global.fs_bs[_fi] = 7;
        }
        if (_fk == 2 || _fk == 3)
        {
            global.fs_bs[_fi] = 10;
        }
        if (_fk == 4 || _fk == 5)
        {
            global.fs_bs[_fi] = 15;
        }
        if (_fk == 6)
        {
            global.fs_bs[_fi] = 25;
        }
    }
    ini_close();
}
