// Fame Shop window - Mouse (global left pressed).
// bc / brs / hov are set by the Draw GUI event of the previous frame, like the game's other windows.
// Levels are saved in the "never" ini, section [FAME2] (keys q0..q27).
if (bc)
{
    instance_destroy();
    exit;
}
if (brs)
{
    if (current_time < rcf)
    {
        for (var i = 0; i < NN; i++)
        {
            global.fs_lv[i] = 0;
        }
        ini_open("never");
        for (var j = 0; j < NN; j++)
        {
            ini_write_real("FAME2", "q" + string(j), global.fs_lv[j]);
        }
        ini_close();
        rcf = 0;
    }
    else
    {
        rcf = current_time + 3000;
    }
    exit;
}
if (hov >= 0)
{
    var p = hov;
    var lv = global.fs_lv[p];
    var opn = 1;
    if (req[p] >= 0)
    {
        if (global.fs_lv[req[p]] < 1)
        {
            opn = 0;
        }
    }
    if (req2[p] >= 0)
    {
        if (global.fs_lv[req2[p]] < 1)
        {
            opn = 0;
        }
    }
    var cost = global.fs_bs[p] * (lv + 1);
    if (lv < global.fs_mx[p] && opn && av >= cost)
    {
        global.fs_lv[p] = lv + 1;
        ini_open("never");
        for (var j = 0; j < NN; j++)
        {
            ini_write_real("FAME2", "q" + string(j), global.fs_lv[j]);
        }
        ini_close();
    }
}
