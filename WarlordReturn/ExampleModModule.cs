using FortRise;
using Monocle;
using System.Diagnostics;
using TowerFall;

namespace Warlord;


[Fort("com.CoolModder.Warlord", "Warlord")]
public class ExampleModModule : FortModule
{
    public static ExampleModModule Instance;
 
    public ExampleModModule() 
    {
        Instance = this;
    }
    public override Type SettingsType => typeof(WarlordSettings);
    public static WarlordSettings Settings => (WarlordSettings)Instance.InternalSettings;

    public override void Load()
    {
        MyPlayer.Load();
    }

    public override void Unload()
    {
        MyPlayer.Unload();
    }
}