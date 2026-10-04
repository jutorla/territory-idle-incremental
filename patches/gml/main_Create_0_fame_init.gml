// Fame Shop globals, initialised once at the very top of main Create.
// fs_lv[]  perk levels, saved in the "never" ini ([FAME] section: kept through abdication, sailing away,
//          exports/imports; deleted only by a hard reset; Divine Trials start without it).
// fs_bs[]  base cost of each perk: tier 1 = 4, tier 2 = 8, tier 3 = 14 (level L costs base * L).
// qol_fame on/off switch from Options > QoL Features ([QOL] fame in the "opt" ini, default ON).
if (!variable_global_exists("fs_lv"))
{
    ini_open("opt");
    global.qol_fame = ini_read_real("QOL", "fame", 1);
    ini_close();
    ini_open("never");
    for (var _fi = 0; _fi < 12; _fi++)
    {
        global.fs_lv[_fi] = ini_read_real("FAME", "p" + string(_fi), 0);
        global.fs_bs[_fi] = 4;
        if ((_fi mod 4) == 1)
        {
            global.fs_bs[_fi] = 8;
        }
        if ((_fi mod 4) >= 2)
        {
            global.fs_bs[_fi] = 14;
        }
    }
    ini_close();
}
