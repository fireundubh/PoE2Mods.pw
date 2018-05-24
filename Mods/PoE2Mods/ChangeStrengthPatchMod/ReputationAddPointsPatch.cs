using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.GameData;
using Onyx;
using Patchwork;
using UnityEngine;

namespace PoE2Mods
{
    [ModifiesType("Game.Scripts")]
    class ReputationAddPointsPatch
    {
        [NewMember]
        private static readonly List<KeyValuePair<Guid, Guid>> ReputationStrengthOverrides = new List<KeyValuePair<Guid, Guid>>
        {
            new KeyValuePair<Guid, Guid>(new Guid("54772c0d-cf3f-4589-8cab-9f3601d575c2"), new Guid("9b641b25-2e96-42d1-8548-3fd854a35703")),
            new KeyValuePair<Guid, Guid>(new Guid("71c858fe-7c4b-432a-a105-c518319eaed7"), new Guid("897c1a89-8704-4545-9eaf-4d41fddd9181")),
            new KeyValuePair<Guid, Guid>(new Guid("e19a6f92-2165-4e34-be10-c65e8de970eb"), new Guid("174da658-7788-4bc5-b762-1ca619ccb898")),
        };

        [ModifiesMember("ReputationAddPoints")]
        public static void ReputationAddPointsNew(Guid factionGuid, Axis axis, Guid strengthGuid)
        {
            Guid strengthOverrideGuid = ReputationStrengthOverrides.First(x => x.Key == strengthGuid).Value;
            ChangeStrengthGameData strengthOverride = Scripts.GetGameDataByGuid<ChangeStrengthGameData>(strengthOverrideGuid);

            FactionGameData faction = Scripts.GetGameDataByGuid<FactionGameData>(factionGuid);
            Reputation reputation = SingletonBehavior<ReputationManager>.Instance.GetReputation(faction);

            if (reputation != null)
            {
                reputation.AddReputation(axis, strengthOverride);
                Debug.Log($"{faction} reputation changed on the {axis} axis ({strengthOverride}).");
            }
            else
            {
                Debug.LogError($"Faction {faction} is not set up!");
            }
        }
    }
}
