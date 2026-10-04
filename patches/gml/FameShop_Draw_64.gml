// Fame Shop window - Draw GUI.
mouse_gui();
draw_set_color(c_black);
draw_set_alpha(0.22);
draw_rectangle(-1, -1, room_width + 1, room_height + 1, false);
draw_set_alpha(1);
draw_set_color(merge_color(c_purple, c_black, 0.92));
draw_rectangle(x1, y1, x2, y2, false);
var uU = merge_color(c_red, c_yellow, 0.88);
draw_set_color(uU);
draw_rectangle(x1, y1, x2, y2, true);

// perk points: every Fame point earned is one perk point
var spent = 0;
for (var i = 0; i < NN; i++)
{
    spent += global.fs_bs[i] * ((global.fs_lv[i] * (global.fs_lv[i] + 1)) / 2);
}
tot = main.empire_ppp;
av = tot - spent;

draw_set_font(font0);
draw_set_halign(fa_center);
draw_set_valign(fa_top);
draw_set_color(uU);
draw_text(cX, y1 + 10, g("Магазин славы", "Fame Shop"));
draw_set_font(font1);
draw_set_color(c_white);
draw_text(cX, y1 + 38, g("Очки перков: ", "Perk points: ") + string(av) + " / " + string(tot) + "#" + g("1 слава = 1 очко перка. Улучшения постоянны.", "1 Fame = 1 perk point. Upgrades are permanent."));
if (tot < 1)
{
    draw_set_color(c_yellow);
    draw_text(cX, y1 + 78, g("Получите славу, уплыв на новый континент.", "Earn Fame by sailing away to a new continent."));
}
draw_set_font(font0);
for (var b = 0; b < 3; b++)
{
    draw_set_color(bcol[b]);
    draw_text(x1 + (((x2 - x1) / 3) * (b + 0.5)), y1 + 110, bnm[b]);
}
draw_set_font(font1);

// links between perks
for (var i = 0; i < NN; i++)
{
    if (req[i] >= 0)
    {
        var lc = merge_color(c_black, bcol[brn[i]], 0.25);
        if (global.fs_lv[req[i]] > 0)
        {
            lc = bcol[brn[i]];
        }
        draw_set_color(lc);
        draw_line_width(px[req[i]], py[req[i]], px[i], py[i], 4);
    }
}

// perks
hov = -1;
for (var i = 0; i < NN; i++)
{
    var lv = global.fs_lv[i];
    var opn = 1;
    if (req[i] >= 0)
    {
        if (global.fs_lv[req[i]] < 1)
        {
            opn = 0;
        }
    }
    var cc = bcol[brn[i]];
    var hv = point_in_circle(mouse.x, mouse.y, px[i], py[i], 32);
    if (instance_exists(Message))
    {
        hv = 0;
    }
    var body = merge_color(c_black, cc, 0.18);
    var edge = cc;
    var tcol = c_white;
    if (!opn)
    {
        body = merge_color(c_black, c_dkgray, 0.5);
        edge = c_dkgray;
        tcol = c_gray;
    }
    else if (lv >= RM)
    {
        body = merge_color(c_black, cc, 0.8);
        edge = c_white;
    }
    else if (lv > 0)
    {
        body = merge_color(c_black, cc, 0.5);
    }
    draw_set_color(body);
    draw_circle(px[i], py[i], 32, false);
    if (hv)
    {
        edge = c_yellow;
    }
    draw_set_color(edge);
    draw_circle(px[i], py[i], 32, true);
    draw_circle(px[i], py[i], 31, true);
    draw_set_color(c_white);
    if (!opn)
    {
        draw_set_color(c_gray);
    }
    draw_set_valign(fa_middle);
    draw_text(px[i], py[i], string(lv) + "/" + string(RM));
    draw_set_valign(fa_top);
    draw_set_color(tcol);
    if (opn)
    {
        draw_set_color(cc);
    }
    draw_text(px[i], py[i] + 38, nm[i]);
    if (hv)
    {
        hov = i;
        var t = nm[i] + "#" + g("Уровень ", "Level ") + string(lv) + "/" + string(RM) + "#" + ds[i] + "##";
        t += g("Сейчас: ", "Now: ") + sg[i] + string(vv[i] * lv) + sx[i];
        if (lv < RM)
        {
            t += "  ->  " + sg[i] + string(vv[i] * (lv + 1)) + sx[i] + "#" + g("Стоимость: ", "Cost: ") + string(global.fs_bs[i] * (lv + 1)) + g(" очк. перков", " perk points");
        }
        else
        {
            t += "#" + g("Максимальный уровень", "Max level");
        }
        if (!opn)
        {
            t += "#" + g("Нужно: ", "Requires: ") + nm[req[i]] + g(" (ур. 1)", " (level 1)");
        }
        mHelp(t);
    }
}

// bottom bar
draw_set_halign(fa_left);
draw_set_valign(fa_top);
draw_set_font(font1);
draw_set_color(merge_color(c_white, c_black, 0.4));
draw_text(x1 + 20, y2 - 72, g("Улучшения сохраняются при отречении и при переезде на новый континент.", "Upgrades are kept through abdications and new continents."));
bc = DrawBtnRel(x2 - 100, y2 - 40, g("Закрыть", "Close"), 0, uU);
var rt = g("Сбросить перки (бесплатно)", "Reset perks (free)");
if (current_time < rcf)
{
    rt = g("Нажмите ещё раз для подтверждения", "Click again to confirm");
}
brs = DrawBtnRel(x1 + 20, y2 - 40, rt, 0, uU);
mouse_norm();
