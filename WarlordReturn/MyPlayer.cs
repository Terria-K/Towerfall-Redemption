using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;
using FortRise;
using Warlord;

internal class MyPlayer : Player
{
    public static Dictionary<int, int> HasWarlordHelm = new Dictionary<int, int>(16);
    public static Dictionary<int, float> HoldWarlordHelm = new Dictionary<int, float>(16);

    public MyPlayer(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, HatStates hatState, bool frozen, bool flash, bool indicator)
        : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator)
    {
   
    }


    public static void ctor(On.TowerFall.Player.orig_Added orig, TowerFall.Player self) { 
        orig(self);
        HasWarlordHelm[self.PlayerIndex] = 0;
        HoldWarlordHelm[self.PlayerIndex] = 0;
    }
    public static PlayerCorpse Die(On.TowerFall.Player.orig_Die_DeathCause_int_bool_bool orig, global::TowerFall.Player self, DeathCause deathCause, int killerIndex, bool brambled, bool laser)
    {
       
        Level level = self.Level;
        while (HasWarlordHelm[self.PlayerIndex] > 0)
        {
            if (deathCause == DeathCause.JumpedOn && level.GetPlayer(killerIndex) != null)
            {
                HasWarlordHelm[killerIndex]++;
            }
            else
            {
                ((Warlord.WarlordRoundLogic)level.Session.RoundLogic).DropHelm(self, self.Position + Player.ArrowOffset, self.Facing);
            }
            HasWarlordHelm[self.PlayerIndex]--;
        }
        return orig(self, deathCause, killerIndex, brambled, laser);
    }


    public static void Update(On.TowerFall.Player.orig_Update orig, global::TowerFall.Player self)
    {
        Level level = self.Level;
        Entity entity = self.CollideFirst(GameTags.Hat);
        if (entity != null)
        {
            if (entity is WarlordHelm)
            { 
                HasWarlordHelm[self.PlayerIndex]++;
                var headSprite = self.DynGetData<Sprite<string>>("headSprite");
                self.Remove(headSprite);
                headSprite = TFGame.SpriteData.GetSpriteString("PlayerHeadWarlord0");
                self.DynSetData("headSprite", headSprite);
                headSprite.Play("idle", 0);
                self.Add(headSprite);
                entity.RemoveSelf();
            }
        }
        
        if(HasWarlordHelm[self.PlayerIndex] > 0)
        {
            HoldWarlordHelm[self.PlayerIndex] += Engine.DeltaTime;

            if (HoldWarlordHelm[self.PlayerIndex] >= ExampleModModule.Settings.TimeToScore)
            {
                ((Warlord.WarlordRoundLogic)level.Session.RoundLogic).IncreaseScore(self);
                HoldWarlordHelm[self.PlayerIndex] = 0;

                self.Level.Add<FloatText>(
                    new FloatText(self.Position + new Vector2(0f, -8f), "+1 POINT", 
                    ArcherData.GetColorA(self.PlayerIndex), Color.Yellow, 1f, 1f, true));
            }
        }
        orig(self);
    }
    public static void Load()
    {
        On.TowerFall.Player.Added += ctor;
        On.TowerFall.Player.Die_DeathCause_int_bool_bool += Die;
        On.TowerFall.Player.Update += Update;
    }
    public static void Unload()
    {
        On.TowerFall.Player.Added -= ctor;
        On.TowerFall.Player.Die_DeathCause_int_bool_bool -= Die;
        On.TowerFall.Player.Update -= Update;
    }
}
