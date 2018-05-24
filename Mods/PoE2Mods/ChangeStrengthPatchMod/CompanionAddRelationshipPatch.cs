using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.GameData;
using Onyx;
using Patchwork;
using UnityEngine;

namespace PoE2Mods
{
    [ModifiesType("Game.Scripts")]
    class CompanionAddRelationshipPatch
    {
        [NewMember]
        private static readonly List<KeyValuePair<Guid, Guid>> CompanionStrengthOverrides = new List<KeyValuePair<Guid, Guid>>
        {
            new KeyValuePair<Guid, Guid>(new Guid("54772c0d-cf3f-4589-8cab-9f3601d575c2"), new Guid("ca8d096d-ec25-42f8-b637-52b266ef8f38")),
            new KeyValuePair<Guid, Guid>(new Guid("71c858fe-7c4b-432a-a105-c518319eaed7"), new Guid("b80dd400-d917-4401-9cce-92a8a9bc98ea")),
            new KeyValuePair<Guid, Guid>(new Guid("e19a6f92-2165-4e34-be10-c65e8de970eb"), new Guid("e7a313a6-447a-4400-b76c-d3b2a3aadf65")),
        };

        [ModifiesMember("CompanionAddRelationship")]
        public static void CompanionAddRelationshipNew(Guid sourceGuid, Guid targetGuid, Axis axis, Guid strengthGuid, bool onlyInParty)
        {
            Guid strengthOverrideGuid = CompanionStrengthOverrides.First(x => x.Key == strengthGuid).Value;
            ChangeStrengthGameData strengthOverride = Scripts.GetGameDataByGuid<ChangeStrengthGameData>(strengthOverrideGuid);

            sourceGuid = InstanceID.GetObjectID(sourceGuid);
            targetGuid = InstanceID.GetObjectID(targetGuid);

            Guid guid = GameState.PlayerCharacter.GetComponent<InstanceID>().Guid;

            if (onlyInParty && (!SingletonBehavior<PartyManager>.Instance.IsActivePartyMember(sourceGuid) || !SingletonBehavior<PartyManager>.Instance.IsActivePartyMember(targetGuid)))
            {
                return;
            }

            if (sourceGuid == SpecialCharacterInstanceID.PlayerGuid || sourceGuid == guid)
            {
                Debug.LogError("Cannot change the player's relationship towards a companion");
                UIDebug.LogOnScreenWarning("Script is trying to use Player's guid as SourceGuid for CompanionAddRelationship", UIDebug.Department.Design, 10f);
            }
            else if (targetGuid == SpecialCharacterInstanceID.PlayerGuid || targetGuid == guid)
            {
                CompanionGameData sourceCompanion = CompanionManager.GetCompanionData(sourceGuid);
                SingletonBehavior<PartyManager>.Instance.AddPlayerRelationship(sourceCompanion, strengthOverride.ChangeValue, axis);
            }
            else
            {
                CompanionGameData sourceCompanion = CompanionManager.GetCompanionData(sourceGuid);
                CompanionGameData targetCompanion = CompanionManager.GetCompanionData(targetGuid);

                if (sourceCompanion == null || targetCompanion == null)
                {
                    Debug.LogError($"Null companion data being used for guid: {((!(sourceCompanion == null)) ? targetGuid : sourceGuid)}");
                }
                else
                {
                    SingletonBehavior<PartyManager>.Instance.AddRelationship(sourceCompanion, targetCompanion, strengthOverride.ChangeValue, axis);
                }
            }
        }
    }
}
