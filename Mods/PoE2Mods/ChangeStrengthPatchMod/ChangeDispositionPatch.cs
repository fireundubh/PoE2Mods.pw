using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.GameData;
using Onyx;
using Patchwork;

#pragma warning disable 108,114

namespace PoE2Mods
{
    [ModifiesType]
    class ChangeDispositionPatch : Disposition
    {
        [NewMember]
        private readonly List<KeyValuePair<Guid, Guid>> _dispositionStrengthOverrides = new List<KeyValuePair<Guid, Guid>>
        {
            new KeyValuePair<Guid, Guid>(new Guid("54772c0d-cf3f-4589-8cab-9f3601d575c2"), new Guid("d41180a5-fb16-4b07-996c-dc73ea45ca2c")),
            new KeyValuePair<Guid, Guid>(new Guid("71c858fe-7c4b-432a-a105-c518319eaed7"), new Guid("122f590f-e442-4919-a247-454158c99032")),
            new KeyValuePair<Guid, Guid>(new Guid("e19a6f92-2165-4e34-be10-c65e8de970eb"), new Guid("933f3c6c-813a-45f3-9d05-0e3137a8380a")),
        };

        [ModifiesMember("ChangeDisposition")]
        public void ChangeDispositionNew(DispositionGameData disposition, ChangeStrengthGameData strength)
        {
            if (!this.m_dispositions.ContainsKey(disposition))
            {
                return;
            }

            int preChangeRank = this.GetRank(disposition);

            Guid strengthOverrideGuid = this._dispositionStrengthOverrides.First(x => x.Key == strength.ID).Value;
            ChangeStrengthGameData strengthOverride = Scripts.GetGameDataByGuid<ChangeStrengthGameData>(strengthOverrideGuid);

            Dictionary<DispositionGameData, int> dispositions;
            (dispositions = this.m_dispositions)[disposition] = dispositions[disposition] + strengthOverride.ChangeValue;

            int postChangeRank = this.GetRank(disposition);

            if (preChangeRank < 3 && postChangeRank >= 3 && SingletonBehavior<AchievementTracker>.Instance)
            {
                SingletonBehavior<AchievementTracker>.Instance.IncrementTrackedStat(TrackedAchievementStat.NumDispositionsAtLevel);
            }

            TutorialManager.STriggerTutorialsOfType(TutorialEventType.DispositionGain);
        }
    }
}
