// QoL: Fame Shop. A "Fame Shop" button next to the Amber Shop opens a skill tree of permanent upgrades.
// Every Fame point you earn (Fame comes from sailing away to a new continent) is also one perk point.
// Toggle: global.qol_fame (Options > QoL Features). The perk effects themselves are in 06_fame_perks.csx.
//
// What this script adds:
//   * a new game object FameShop (the window): Create / Draw GUI / Mouse (global left pressed),
//     code in patches\gml\FameShop_*.gml, depth -10 like the game's other shops (tooltips are at -99);
//   * FameShop in modal(), so the rest of the UI ignores clicks while the window is open;
//   * the globals fs_lv[] / fs_bs[] / qol_fame at the top of main Create;
//   * the button on the bottom bar (main Draw GUI).

string gmlDir = Path.Combine(Path.GetDirectoryName(ScriptPath), "gml");
string Gml(string file) => File.ReadAllText(Path.Combine(gmlDir, file));

GlobalDecompileContext globalDecompileContext = new(Data);
Underanalyzer.Decompiler.IDecompileSettings decompilerSettings = Data.ToolInfo.DecompilerSettings;
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings)
{
    ThrowOnNoOpFindReplace = true
};

// 1) The FameShop object (must exist before any code that mentions it is compiled).
if (Data.GameObjects.ByName("FameShop") != null)
    throw new Exception("FameShop already exists - is this patch being applied twice?");
var fameShop = new UndertaleGameObject();
fameShop.Name = Data.Strings.MakeString("FameShop");
fameShop.Depth = -10;
fameShop.Visible = true;
fameShop.Persistent = false;
fameShop.Solid = false;
Data.GameObjects.Add(fameShop);

importGroup.QueueReplace(fameShop.EventHandlerFor(EventType.Create, Data), Gml("FameShop_Create_0.gml"));
importGroup.QueueReplace(fameShop.EventHandlerFor(EventType.Draw, EventSubtypeDraw.DrawGUI, Data), Gml("FameShop_Draw_64.gml"));
importGroup.QueueReplace(fameShop.EventHandlerFor(EventType.Mouse, EventSubtypeMouse.GlobLeftPressed, Data), Gml("FameShop_Mouse_53.gml"));

// 2) modal(): the window blocks the UI underneath, like the Amber Shop does.
importGroup.QueueFindReplace(
    "gml_Script_modal",
    "if (instance_exists(AmShop))",
    "if (instance_exists(FameShop))\n{\n    return 1;\n}\nif (instance_exists(AmShop))");

// 3) Globals at the very top of main Create.
importGroup.QueueFindReplace(
    "gml_Object_main_Create_0",
    "VERSION = 167;",
    "VERSION = 167;\n" + Gml("main_Create_0_fame_init.gml"));

// 4) The button (before the final mouse_norm() of main Draw GUI).
importGroup.QueueFindReplace(
    "gml_Object_main_Draw_64",
    "mouse_norm();",
    Gml("main_Draw_64_fame_button.gml") + "mouse_norm();");

importGroup.Import();
