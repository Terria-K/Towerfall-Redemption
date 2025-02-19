using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;
using FortRise;

namespace Warlord;

public class Warlord : CustomGameMode
{
    public override RoundLogic CreateRoundLogic(Session session)
    {
        return new WarlordRoundLogic(session);
    }

    public override void Initialize()
    {
        Icon = TFGame.MenuAtlas["gameModes/warlord"];
        NameColor = Color.Red;
    }

    public override void InitializeSounds() {}
}

public class WarlordRoundLogic : RoundLogic
{
	public Vector2 helmPos;

	public Alarm spawnAlarm;

    private Counter endDelay;

	public List<Arrow> arrowQueue;

	public int lastThrower;

    public WarlordRoundLogic(Session session)
		: base(session, false)
	{
		CanMiasma = false;
		arrowQueue = new List<Arrow>();
        this.endDelay = new Counter();
        this.endDelay.Set(90);
    }

    public override void OnLevelLoadFinish()
	{
		base.OnLevelLoadFinish();
        base.Session.CurrentLevel.Add(new VersusStart(base.Session));
        base.Players = TFGame.PlayerAmount;
		List<Vector2> xMLPositions = Session.CurrentLevel.GetXMLPositions("BigTreasureChest");
        foreach (Vector2 pos in xMLPositions)
        {
            helmPos = pos;
        }
        DropHelm(null, helmPos, Facing.Left);
        SpawnPlayersFFA();
        this.endDelay.Set(90);
        Session.CurrentLevel.Ending = false;
    }

	public override void OnUpdate()
	{
		for (int i = 0; i < arrowQueue.Count; i++)
		{
			base.Session.CurrentLevel.Add(arrowQueue[i]);
		}
		arrowQueue.Clear();
        SessionStats.TimePlayed += Engine.DeltaTicks;
        base.OnUpdate();
        if (!base.RoundStarted || !base.Session.CurrentLevel.Ending || !base.Session.CurrentLevel.CanEnd)
        {
            return;
        }
        
        if ((bool)spawnAlarm)
        {
            spawnAlarm.Update();
        }
        if (base.RoundStarted && base.Session.CurrentLevel.Ending)
        {
            if (this.endDelay)
            {
                this.endDelay.Update();
                return;
            }
            base.Session.EndRound();
            InsertCrownEvent();
        }
	}
    public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
    {
        base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);
        if (FFACheckForAllButOneDead())
        {
            Session.CurrentLevel.Ending = true;
            int WinPlayerIndex = 0;
            foreach (Player item in base.Session.CurrentLevel[GameTags.Player])
            {
                if (!item.Dead)
                {
                    WinPlayerIndex = item.PlayerIndex;
                }
            }
            if (MyPlayer.HasWarlordHelm[playerIndex] > 0 && killerIndex != playerIndex && killerIndex >= 0)
            { 
                AddScore(killerIndex, 1);
                this.Session.CurrentLevel.Add<FloatText>(
                    new FloatText(player.Position + new Vector2(0f, -8f), "+1 POINT", 
                    ArcherData.GetColorA(killerIndex), Color.Yellow, 1f, 1f, true));

                int winnerIndex = Session.GetWinner();
                if (winnerIndex != -1)
                {
                    FinalKill(corpse, winnerIndex);
                }
                else 
                {
                    FinalKillNoSpotlightOrMusicStop();
                }
            }
            else if (MyPlayer.HasWarlordHelm[WinPlayerIndex] > 0)
            {
                AddScore(WinPlayerIndex, 1);
                this.Session.CurrentLevel.Add<FloatText>(
                    new FloatText(player.Position + new Vector2(0f, -8f), "+1 POINT", 
                    ArcherData.GetColorA(WinPlayerIndex), Color.Yellow, 1f, 1f, true));

                int winnerIndex = Session.GetWinner();
                if (winnerIndex != -1)
                {
                    Session.CurrentLevel.Ending = true;
                    FinalKill(corpse, winnerIndex);
                }
                else 
                {
                    FinalKillNoSpotlightOrMusicStop();
                }
            }
        }
    }

    public void DropHelm(Player player, Vector2 position, Facing Facing)
	{
        Entity helm = new WarlordHelm(position, Facing == Facing.Left, null, 0);
        Session.CurrentLevel.Add(helm);
	}
    public override void OnRoundStart()
    {
        base.OnRoundStart();
        SpawnTreasureChestsVersus();
    }
    public void IncreaseScore(Player player)
    {
        if (Session.CurrentLevel.Ending == false)
        {
            AddScore(player.PlayerIndex, 1);
            int winnerIndex = Session.GetWinner();
            if (winnerIndex != -1)
            {
                FinalKillNoCorpse(winnerIndex);
                Session.CurrentLevel.Ending = true;
            }
        }
    }

    private void FinalKillNoCorpse(int otherSpotlightIndex)
    {
        Session.MatchStats[otherSpotlightIndex].GotWin = true;
        if (Session.CurrentLevel.CanEnd)
        {
            LevelEntity playerOrCorpse = Session.CurrentLevel.GetPlayerOrCorpse(otherSpotlightIndex);
            Session.CurrentLevel.LightingLayer.SetSpotlight(playerOrCorpse);
        }

        FinalKillNoSpotlight();
    }
}
