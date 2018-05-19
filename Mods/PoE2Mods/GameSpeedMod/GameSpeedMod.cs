﻿/** 
 * GameSpeedMod.cs
 * Dylan Bailey
 * 11/14/16
 * License - Do whatever you want with this code as long as no puppies are set aflame
*/

using Game;
using Game.UI;
using IniParser;
using IniParser.Model;
using Onyx;
using Patchwork;
using System.IO;
using UnityEngine;

//using SDK;

namespace PoE2Mods
{

    /// <summary>
    /// Modifies the game speed toggles to add a new 6x speed.
    /// </summary>
    [ModifiesType]
    public class mod_TimeController : Game.TimeController
    {
        [NewType]
        enum GameSpeedState
        {
            SLOW,
            NORMAL,
            DOUBLE,
            SIX,
            TEN
        }

        [NewMember]
        GameSpeedState gameSpeed = GameSpeedState.NORMAL;

        [NewMember]
        string Config;

        [NewMember]
        bool UseMod;

        [NewMember]
        float ToggleMaxGameSpeed;

        [NewMember] 
        float MaxGameSpeedSetting;


        [ModifiesMember("OnyxStart")]
        public void OnyxStartNew()
        {
            UseMod = UserConfig.GetValueAsBool("GameSpeedMod", "enableMod");
            ToggleMaxGameSpeed = UserConfig.GetValueAsFloat("GameSpeedMod", "toggleGameSpeed");
            MaxGameSpeedSetting = UserConfig.GetValueAsFloat("GameSpeedMod", "maxGameSpeed");
            Config = MaxGameSpeedSetting.ToString();

            this.m_TimeScale = this.NormalTime;
        }

        [NewMember]
        public void orig_ToggleFast()
        {
            if (this.TimeScale == this.FastTime) {
                this.TimeScale = this.NormalTime;
            }
            else if (this.CanUseFastMode) {
                this.TimeScale = this.FastTime;
            }
            this.UpdateTimeScale();
        }

        [ModifiesMember("ToggleFast")]
        public void ToggleFastNew()
        {
            if (!UseMod) {
                orig_ToggleFast();
                return;
            }

            if (TimeScale < 0.9f) {
                TimeScale = 1.0f;
            } else if (TimeScale < 1.9f) {
                TimeScale = 2.0f;
            } else if (TimeScale < 5.9f) {
                TimeScale = 6.0f;
            } else if (TimeScale < 6.1f) {
                TimeScale = MaxGameSpeedSetting;
            }

            this.UpdateTimeScale();
        }

        [NewMember]
        [DuplicatesBody("ToggleSlow")]
        public void orig_ToggleSlow()
        {
            if (this.TimeScale == this.SlowTime) {
                this.TimeScale = this.NormalTime;
            }
            else if (this.CanUseSlowMode) {
                this.TimeScale = this.SlowTime;
            }
            this.UpdateTimeScale();
        }

        [ModifiesMember("ToggleSlow")]
        public void ToggleSlowNew()
        {
            if (!UseMod) {
                orig_ToggleSlow();
                return;
            }

            if (TimeScale > 6.1f) {
                TimeScale = 6.0f;
            } else if (TimeScale > 3.0f) {
                TimeScale = 2.0f;
            } else if (TimeScale > 1.1f) {
                TimeScale = 1.0f;
            }
            else {
                TimeScale = 0.2f;
            }
            
            this.UpdateTimeScale();
        }

        [NewMember]
        public void orig_UpdateTimeScale()
        {
            if ((this.m_PlayerPaused || this.m_UiPaused) && !this.ProhibitPause) {
                Time.timeScale = 0f;
            }
            else if (Cutscene.CutsceneActive || SingletonBehavior<ConversationManager>.Instance.IsConversationOrSIRunning()) {
                Time.timeScale = 1f;
            }
            else if (GameState.InCombat) {
                Time.timeScale = GameState.Option.CombatSpeed;
            }
            else {
                Time.timeScale = this.TimeScale;
            }
        }

        [ModifiesMember("UpdateTimeScale")]
        public void UpdateTimeScaleNew()
        {
            if (!UseMod) {
                orig_UpdateTimeScale();
                return;
            }
            if ((this.m_PlayerPaused || this.m_UiPaused) && !this.ProhibitPause) {
                Time.timeScale = 0f;
            }
            else if (Cutscene.CutsceneActive || Onyx.SingletonBehavior<ConversationManager>.Instance.IsConversationOrSIRunning()) {
                Time.timeScale = 1f;
            }
            else if (GameState.InCombat && this.TimeScale > 2.1f) {
                //Time.timeScale = GameState.Option.CombatSpeed;
                //In combat, need to cache the old and rescale
                this.TimeScale = 1.0f;
            }
            else {
                Time.timeScale = this.TimeScale;
            }

            Config = TimeScale.ToString();
        }

        [NewMember]
        bool UltraFastModeEngaged = false;


        [NewMember]
        public void orig_OnyxUpdate()
        {
            this.RealtimeSinceStartupThisFrame = Time.realtimeSinceStartup;
            this.GameTimeSinceStartup += Time.deltaTime;
            float num = 0.2f;
            TimeController.m_smoothUnscaledDeltaTime = num * TimeController.UnscaledDeltaTime + (1f - num) * TimeController.m_previousSmoothUnscaledDeltaTime;
            TimeController.m_previousSmoothUnscaledDeltaTime = TimeController.m_smoothUnscaledDeltaTime;
            if (!this.CanUseSlowMode && this.TimeScale == this.SlowTime) {
                this.TimeScale = 1f;
            }
            if (!this.CanUseFastMode && this.TimeScale == this.FastTime) {
                this.TimeScale = 1f;
            }
            if (!GameState.IsLoading) {
                this.UpdateTimeScale();
            }
            if (UIWindowManager.KeyInputAvailable) {
                if (GameInput.GetControlDown(MappedControl.RESTORE_SPEED, true)) {
                    this.TimeScale = this.NormalTime;
                }
                else if (GameInput.GetControlDown(MappedControl.SLOW_TOGGLE, true)) {
                    this.ToggleSlow();
                }
                else if (GameInput.GetControlDown(MappedControl.FAST_TOGGLE, true)) {
                    this.ToggleFast();
                }
                else if (GameInput.GetControlDown(MappedControl.GAME_SPEED_CYCLE, true)) {
                    if (this.Fast) {
                        this.Slow = true;
                    }
                    else if (this.Slow) {
                        this.Slow = false;
                    }
                    else if (this.CanUseFastMode) {
                        this.Fast = true;
                    }
                    else if (this.CanUseSlowMode) {
                        this.Slow = true;
                    }
                }
            }
        }


        [ModifiesMember("OnyxUpdate")]
        public void OnyxUpdateNew()
        {
            if (!UseMod) {
                orig_OnyxUpdate();
                return;
            }
            //Game.Console.AddMessage("CurrentSpeed: " + Config);
            this.RealtimeSinceStartupThisFrame = Time.realtimeSinceStartup;
            this.GameTimeSinceStartup += Time.deltaTime;
            float num = 0.2f;
            TimeController.m_smoothUnscaledDeltaTime = num * TimeController.UnscaledDeltaTime + (1f - num) * TimeController.m_previousSmoothUnscaledDeltaTime;
            TimeController.m_previousSmoothUnscaledDeltaTime = TimeController.m_smoothUnscaledDeltaTime;

            if (!GameState.IsLoading) {
                this.UpdateTimeScale();
            }
            if (UIWindowManager.KeyInputAvailable) {
                if (GameInput.GetControlDown(MappedControl.RESTORE_SPEED, true)) {
                    this.TimeScale = this.NormalTime;
                }
                else if (GameInput.GetControlDown(MappedControl.COMBAT_SPEED_DOWN, true)) {
                    this.ToggleSlow();
                    //Debug.LogError("TOGGLE SLOW");
                }
                else if (GameInput.GetControlDown(MappedControl.COMBAT_SPEED_UP, true)) {
                    this.ToggleFast();
                    //Debug.LogError("TOGGLE SLOW");
                }
                else if (GameInput.GetControlDown(MappedControl.FAST_TOGGLE, true)) {
                    if (!UltraFastModeEngaged) {
                        this.TimeScale = ToggleMaxGameSpeed;
                        //gameSpeed = GameSpeedState.SIX;
                        UltraFastModeEngaged = true;
                    }
                    else {
                        this.TimeScale = 1.0f;
                        //gameSpeed = GameSpeedState.NORMAL;
                        UltraFastModeEngaged = false;
                    }
                }
            }
        }
    }
}













