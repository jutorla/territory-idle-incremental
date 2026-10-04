// "Fame Shop" button on the bottom bar, right of the Amber Shop button (inserted into main Draw GUI).
// The Amber Shop button is at x = 25 and as wide as its text + 36; the next bottom button (Abdicate) is at
// x = 250, so this one uses the slimmer DrawBtnRel style. A badge shows the unspent perk points.
// Like the game's own bottom buttons (DrawBtn), it is not drawn while a window is open (modal()).
if (global.qol_fame && !modal())
{
    draw_set_font(font1);
    draw_set_halign(fa_left);
    draw_set_valign(fa_top);
    var _fspent = 0;
    for (var _fi = 0; _fi < 32; _fi++)
    {
        _fspent += global.fs_bs[_fi] * ((global.fs_lv[_fi] * (global.fs_lv[_fi] + 1)) / 2);
    }
    var _fav = empire_ppp - _fspent;
    var _fx = 25 + string_width(g("Магазин янтаря", "Amber Shop")) + 36 + 8;
    var _fcol = merge_color(c_fuchsia, c_white, 0.55);
    if (_fav > 0)
    {
        _fcol = merge_color(c_aqua, c_white, abs(sin(current_time / 400)));
    }
    var _fb = DrawBtnRel(_fx, 750, g("Слава", "Fame Shop"), 0, _fcol);
    if (_fav > 0)
    {
        var _fbt = string(_fav);
        if (_fav > 99)
        {
            _fbt = "99+";
        }
        var _fbx = _fx + _w_;
        draw_set_color(c_red);
        draw_circle(_fbx, 750, 12, false);
        draw_set_color(c_white);
        draw_set_halign(fa_center);
        draw_set_valign(fa_middle);
        draw_text(_fbx, 750, _fbt);
        draw_set_halign(fa_left);
        draw_set_valign(fa_top);
    }
    if (_fb)
    {
        mHelp(g("Магазин славы#Тратьте очки перков (1 слава = 1 очко)#на постоянные улучшения.#Свободных очков: ", "Fame Shop#Spend perk points (1 Fame = 1 point)#on permanent upgrades.#Available points: ") + string(_fav));
        if (mouse_check_button_pressed(mb_left) && !instance_exists(FameShop))
        {
            instance_create(0, 0, FameShop);
        }
    }
}
