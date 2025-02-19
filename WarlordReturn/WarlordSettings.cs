using Monocle;
using FortRise;
using TowerFall;

namespace Warlord;

public class WarlordSettings : ModuleSettings 
{
    [SettingsName("Time to score!")]
    [SettingsNumber(5, 30, 1)]
    public int TimeToScore = 10;
}