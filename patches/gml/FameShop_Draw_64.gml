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
tot = round(main.empire_ppp * 100) / 100;
av = tot - spent;

draw_set_font(font0);
draw_set_halign(fa_center);
draw_set_valign(fa_top);
draw_set_color(uU);
draw_text(cX, y1 + 8, g("Магазин славы", "Fame Shop"));
draw_set_font(font1);
draw_set_color(c_white);
draw_text(cX, y1 + 34, g("Очки перков: ", "Perk points: ") + string(av) + " / " + string(tot) + "#" + g("1 слава = 1 очко перка. Улучшения постоянны.", "1 Fame = 1 perk point. Upgrades are permanent."));
if (tot < 1)
{
    draw_set_color(c_yellow);
    draw_text(cX, y1 + 70, g("Получите славу, уплыв на новый континент.", "Earn Fame by sailing away to a new continent."));
}
draw_set_font(font0);
for (var b = 0; b < 4; b++)
{
    draw_set_color(bcol[b]);
    draw_text(x1 + (((x2 - x1) / 4) * (b + 0.5)), y1 + 92, bnm[b]);
}
draw_set_font(font1);

// links between perks (a keystone has two)
for (var i = 0; i < NN; i++)
{
    for (var q = 0; q < 2; q++)
    {
        var pr = req[i];
        if (q == 1)
        {
            pr = req2[i];
        }
        if (pr >= 0)
        {
            var lc = merge_color(c_black, bcol[brn[i]], 0.25);
            if (global.fs_lv[pr] > 0)
            {
                lc = bcol[brn[i]];
            }
            draw_set_color(lc);
            draw_line_width(px[pr], py[pr], px[i], py[i], 3);
        }
    }
}

// perks
hov = -1;
for (var i = 0; i < NN; i++)
{
    var lv = global.fs_lv[i];
    var mxl = global.fs_mx[i];
    var rr = rdi[i];
    var opn = 1;
    if (req[i] >= 0)
    {
        if (global.fs_lv[req[i]] < 1)
        {
            opn = 0;
        }
    }
    if (req2[i] >= 0)
    {
        if (global.fs_lv[req2[i]] < 1)
        {
            opn = 0;
        }
    }
    var cc = bcol[brn[i]];
    var hv = point_in_circle(mouse.x, mouse.y, px[i], py[i], rr);
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
    else if (lv >= mxl)
    {
        body = merge_color(c_black, cc, 0.8);
        edge = c_white;
    }
    else if (lv > 0)
    {
        body = merge_color(c_black, cc, 0.5);
    }
    // keystones get a gold outer ring
    if (slt[i] == 7)
    {
        var gc = c_dkgray;
        if (opn)
        {
            gc = merge_color(c_yellow, c_orange, 0.4);
        }
        draw_set_color(gc);
        draw_circle(px[i], py[i], rr + 5, true);
        draw_circle(px[i], py[i], rr + 4, true);
    }
    draw_set_color(body);
    draw_circle(px[i], py[i], rr, false);
    if (hv)
    {
        edge = c_yellow;
    }
    draw_set_color(edge);
    draw_circle(px[i], py[i], rr, true);
    draw_circle(px[i], py[i], rr - 1, true);
    draw_set_color(c_white);
    if (!opn)
    {
        draw_set_color(c_gray);
    }
    draw_set_valign(fa_middle);
    draw_text(px[i], py[i], string(lv) + "/" + string(mxl));
    draw_set_valign(fa_top);
    draw_set_color(tcol);
    if (opn)
    {
        draw_set_color(cc);
    }
    draw_text_ext(px[i], py[i] + rr + 4, nm[i], 13, 112);
    if (hv)
    {
        hov = i;
        var t = nm[i] + "#" + g("Уровень ", "Level ") + string(lv) + "/" + string(mxl) + "#" + ds[i] + "##";
        var nw = sg[i] + string(vv[i] * lv) + sx[i];
        var nx = sg[i] + string(vv[i] * (lv + 1)) + sx[i];
        if (i == 26)
        {
            nw = g("не открыто", "locked");
            if (lv > 0)
            {
                nw = kstr(bpc[lv]) + g(" золота", " gold");
            }
            if (lv < mxl)
            {
                nx = kstr(bpc[lv + 1]) + g(" золота", " gold");
            }
        }
        t += g("Сейчас: ", "Now: ") + nw;
        if (lv < mxl)
        {
            t += "  ->  " + nx + "#" + g("Стоимость: ", "Cost: ") + string(global.fs_bs[i] * (lv + 1)) + g(" очк. перков", " perk points");
        }
        else
        {
            t += "#" + g("Максимальный уровень", "Max level");
        }
        // live value for the perks that scale with your progress
        var lt = "";
        if (i == 5)
        {
            lt = "+" + string(floor(0.4 * lv * main.tile_num * 10) / 10) + g("% к производству", "% total production");
        }
        if (i == 6)
        {
            lt = "+" + string(floor(0.8 * lv * sqrt(main.empire_ppp) * 10) / 10) + g("% к производству", "% total production");
        }
        if (i == 22)
        {
            lt = "+" + string(floor(0.1 * lv * min(main.o_cnt, 200) * 10) / 10) + g("% к производству", "% total production");
        }
        if (i == 27)
        {
            lt = "+" + string(3 * lv * main.CONT) + g("% к производству", "% total production");
        }
        if (i == 30)
        {
            lt = "+" + string(floor(0.01 * lv * Var.rcnt[8] * 10) / 10) + g("% к производству", "% total production");
        }
        if (i == 7)
        {
            lt = "+" + kstr(0.01 * lv * (main.d_zern + main.d_les + main.d_kam + main.d_ver)) + g(" золота в секунду", " gold per second");
        }
        if (lt != "")
        {
            t += "#" + g("Прямо сейчас: ", "Right now: ") + lt;
        }
        if (!opn)
        {
            var rq = "";
            if (req[i] >= 0)
            {
                if (global.fs_lv[req[i]] < 1)
                {
                    rq += nm[req[i]];
                }
            }
            if (req2[i] >= 0)
            {
                if (global.fs_lv[req2[i]] < 1)
                {
                    if (rq != "")
                    {
                        rq += ", ";
                    }
                    rq += nm[req2[i]];
                }
            }
            t += "#" + g("Нужно: ", "Requires: ") + rq + g(" (ур. 1)", " (level 1)");
        }
        mHelp(t);
    }
}

// bottom bar
draw_set_halign(fa_left);
draw_set_valign(fa_top);
draw_set_font(font1);
draw_set_color(merge_color(c_white, c_black, 0.4));
draw_text(x1 + 20, y2 - 62, g("Улучшения сохраняются при отречении и при переезде на новый континент. Перки с золотым кольцом - ключевые.", "Upgrades are kept through abdications and new continents. Perks with a gold ring are keystones."));
bc = DrawBtnRel(x2 - 100, y2 - 36, g("Закрыть", "Close"), 0, uU);
var rt = g("Сбросить перки (бесплатно)", "Reset perks (free)");
if (current_time < rcf)
{
    rt = g("Нажмите ещё раз для подтверждения", "Click again to confirm");
}
brs = DrawBtnRel(x1 + 20, y2 - 36, rt, 0, uU);
mouse_norm();
