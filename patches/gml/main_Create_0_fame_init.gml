// Fame Shop globals, initialised once at the very top of main Create.
// fs_lv[]  perk levels, saved in the "never" ini ([FAME3] section: kept through abdication, sailing away,
//          exports/imports; deleted only by a hard reset; Divine Trials start without it).
// fs_mx[]  max level of each perk (5, except Open Enrollment 3, Foremen 3, Phoenix 3, Cleave 2,
//          Timeless Rites 4, Famous Voyage 3, Legend of Fame 3).
// fs_bs[]  base cost by slot of the branch: root 5, first step 8, second 12, third 16, keystone 40.
//          Level L of a perk costs base * L perk points.
// qol_fame on/off switch from Options > QoL Features ([QOL] fame in the "opt" ini, default ON).
// d_fgold  gold per second from the Foremen perk (set every step by main Step; Forw adds it for timelapses).
d_fgold = 0;
if (!variable_global_exists("fs_lv"))
{
    ini_open("opt");
    global.qol_fame = ini_read_real("QOL", "fame", 1);
    ini_close();
    ini_open("never");
    for (var _fi = 0; _fi < 32; _fi++)
    {
        global.fs_mx[_fi] = 5;
        if (_fi == 3 || _fi == 7 || _fi == 12 || _fi == 29 || _fi == 31)
        {
            global.fs_mx[_fi] = 3;
        }
        if (_fi == 15)
        {
            global.fs_mx[_fi] = 2;
        }
        if (_fi == 23)
        {
            global.fs_mx[_fi] = 4;
        }
        global.fs_lv[_fi] = min(global.fs_mx[_fi], ini_read_real("FAME3", "q" + string(_fi), 0));
        var _fk = _fi mod 8;
        global.fs_bs[_fi] = 5;
        if (_fk == 1 || _fk == 4)
        {
            global.fs_bs[_fi] = 8;
        }
        if (_fk == 2 || _fk == 5)
        {
            global.fs_bs[_fi] = 12;
        }
        if (_fk == 3 || _fk == 6)
        {
            global.fs_bs[_fi] = 16;
        }
        if (_fk == 7)
        {
            global.fs_bs[_fi] = 40;
        }
    }
    ini_close();
}
