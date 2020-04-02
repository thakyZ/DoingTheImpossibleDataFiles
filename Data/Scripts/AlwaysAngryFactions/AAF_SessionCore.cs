using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Weapons;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace AlwaysAngryFactions
{
  [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
  public class AAF_SessionCore : MySessionComponentBase
  {
    public List<IMyIdentity> IdList = new List<IMyIdentity>();
    public List<long> ReduceRep = new List<long>();
    public List<IMyFaction> AngryFaction = new List<IMyFaction>();
    public List<string> BlockedFactions = new List<string>() { "ASSERT", "FORMIC", "Junk", "MMEC", "MILT", "ORKS", "Rust", "REAVER", "SPRT", "SPID", "CORRUPT" };
    public int Counter = 0;

    public override void UpdateBeforeSimulation()
    {
      Counter++;

      if (Counter < 300)
      {
        return;
      }

      Counter = 0;

      if (!MyAPIGateway.Multiplayer.IsServer)
      {
        return;
      }

      MyAPIGateway.Parallel.Start(() =>
      {
        foreach (string faction in BlockedFactions)
        {
          IMyFaction factionObject = MyAPIGateway.Session.Factions.TryGetFactionByTag(faction);
          if (factionObject == null)
          {
            return;
          }
          AngryFaction.Add(factionObject);
        }

        IdList.Clear();
        MyAPIGateway.Players.GetAllIdentites(IdList);

        foreach (IMyIdentity id in IdList)
        {

          if (MyAPIGateway.Players.TryGetSteamId(id.IdentityId) > 0)
          {
            foreach (IMyFaction faction in AngryFaction)
            {
              int playerRep = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(id.IdentityId, faction.FactionId);

              if (playerRep <= -600)
              {
                continue;
              }

              ReduceRep.Add(id.IdentityId);
            }
          }
        }
      }, () =>
      {
        foreach (IMyFaction faction in AngryFaction)
        {
          if (faction == null)
          {
            return;
          }

          foreach (long id in ReduceRep)
          {
            MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(id, faction.FactionId, -600);
          }

          ReduceRep.Clear();
        }
      });
    }
  }
}