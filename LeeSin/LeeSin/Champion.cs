﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace LeeSin
{
    public static class Champion
    {
        public static string Author = "iCreative";
        public static string AddonName = "Master the enemy";
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Util.myHero.Hero != EloBuddy.Champion.LeeSin) { return; }
            Chat.Print(AddonName + " made by " + Author + " loaded, have fun!.");
            SpellManager.Init();
            MenuManager.Init();
            ModeManager.Init();
            WardManager.Init();
            Q_Spell.Init();
            TargetSelector.Init(SpellManager.Q2.Range + 200, DamageType.Physical);
            LoadCallbacks();
        }
        private static void LoadCallbacks()
        {
            Game.OnTick += Game_OnTick;
            TargetSelector.Range = 1300f;
            if (!SpellSlot.Q.IsFirstSpell() && Q_Spell.Target != null)
            {
                TargetSelector.Range = 1500f;
            }

            Drawing.OnDraw += Drawing_OnDraw;

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
        }

        public static Obj_AI_Base GetBestObjectFarFrom(Vector3 Position)
        {
            var minion = AllyMinionManager.GetFurthestTo(Position);
            var ally = AllyHeroManager.GetFurthestTo(Position);
            var ward = WardManager.GetFurthestTo(Position);
            var miniondistance = minion != null ? Extensions.Distance(Position, minion, true) : 0;
            var allydistance = ally != null ? Extensions.Distance(Position, ally, true) : 0;
            var warddistance = ward != null ? Extensions.Distance(Position, ward, true) : 0;
            var best = Math.Max(miniondistance, Math.Max(allydistance, warddistance));
            if (best > 0f)
            {
                if (best == allydistance)
                {
                    return ally;
                }
                else if (best == miniondistance)
                {
                    return minion;
                }
                else if (best == warddistance)
                {
                    return ward;
                }
            }
            return null;
        }
        public static Obj_AI_Base GetBestObjectNearTo(Vector3 Position)
        {
            var minion = AllyMinionManager.GetNearestTo(Position);
            var ally = AllyHeroManager.GetNearestTo(Position);
            var ward = WardManager.GetNearestTo(Position);
            var miniondistance = minion != null ? Extensions.Distance(Position, minion, true) : 999999999f;
            var allydistance = ally != null ? Extensions.Distance(Position, ally, true) : 999999999f;
            var warddistance = ward != null ? Extensions.Distance(Position, ward, true) : 999999999f;
            var best = Math.Min(miniondistance, Math.Min(allydistance, warddistance));
            if (best <= Math.Pow(250f, 2))
            {
                if (best == allydistance)
                {
                    return ally;
                }
                else if (best == miniondistance)
                {
                    return minion;
                }
                else if (best == warddistance)
                {
                    return ward;
                }
            }
            return null;
        }
        public static void JumpTo(Obj_AI_Base target)
        {
            if (SpellManager.CanCastW1)
            {
                Util.myHero.Spellbook.CastSpell(SpellSlot.W, target);
            }
        }


        private static void Game_OnTick(EventArgs args)
        {
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Util.myHero.IsDead) { return; }
            //Draw current combo mode;
        }


    }
}
