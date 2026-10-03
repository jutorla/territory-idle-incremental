// QoL: click the "Ritual" button while a ritual is running to finish it instantly.
// Reuses the game's own timelapse code (FPrep + Forw), so all production for the
// remaining ritual time is credited, exactly like offline progress.
// Toggle: global.qol_rit (Options > QoL Features).

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// 1) Tooltip: add a hint line to the "Ritual is being held!" help text (only while enabled;
//    string_copy with a count of 0 yields an empty string).
string ruHint = "##Нажмите, чтобы мгновенно завершить обряд";
string enHint = "##Click to finish the ritual instantly";

importGroup.QueueFindReplace(
    "gml_Object_main_Step_0",
    "\" seconds\"));",
    "\" seconds\") + string_copy(g(\"" + ruHint + "\", \"" + enHint + "\"), 1, global.qol_rit * 200));");

// 2) End Step: perform the finish. Done here (not in Step_0) because Forw() re-runs
//    the Step event internally; b_rel1 is cleared first so that nested pass does
//    not see the same click and open the "perform a ritual" dialog.
importGroup.QueueAppend(
    "gml_Object_main_Step_2",
    @"
if (global.qol_rit && b_rel1 && o_time > 0 && mouse_check_button_pressed(mb_left))
{
    var _fd = o_time;
    var _fc = o_cast;
    b_rel1 = 0;
    FPrep();
    Forw(_fd);
    if (_fc > 1 && o_time <= 0 && o_cast == (_fc - 1))
    {
        o_cast = _fc;
    }
}");

importGroup.Import();
