﻿using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using SharpDX;

namespace Template
{
    public static class SpellManager
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot QE;
        public static Spell.Targeted R;
        public static Spell.Targeted Ignite, Smite;
        public static Spell.Skillshot Flash;
        public static float Q_LastCastTime, W_LastCastTime, W_LastSentTime, E_LastCastTime;
        public static Vector3 Q_EndPosition, W_EndPosition = Vector3.Zero;
        public static int Q_CastDelay2 = 600;
        public static int Q_Width1 = 180;
        public static int Q_Width2 = 120;
        public static int W_CastDelay2 = 70;
        public static int W_Speed2 = 1100;
        public static int W_Width1 = 210;
        public static int W_Width2 = 160;
        public static int E_ExtraWidth = 40;
        public static int E_CastDelay1 = 300;
        public static int E_CastDelay2 = 250;
        public static int QE_Speed = 2000;
        public static float Combo_QE, Combo_WE;
        private static Obj_AI_Base _W_Object;
        private static Obj_AI_Base _WE_Object;

        public static Obj_AI_Base W_Object
        {
            get
            {
                if (IsW2)
                {
                    if (_W_Object != null)
                    {
                        if (_W_Object.IsValid && !_W_Object.IsDead)
                        {
                            return _W_Object;
                        }
                    }
                    var ball = BallManager.Balls.Where(m => m.IsWObject).FirstOrDefault();
                    if (ball != null)
                    {
                        _WE_Object = _W_Object;
                        return ball.Object;
                    }
                }
                return null;
            }
        }

        public static bool IsW2
        {
            get
            {
                return SpellSlot.W.GetSpellDataInst().SData.Name.ToLower().Equals("syndrawcast") ||
                       Util.MyHero.HasBuff("syndrawtooltip");
            }
        }

        public static bool SmiteIsReady
        {
            get { return Smite != null && Smite.IsReady(); }
        }

        public static bool CanUseSmiteOnHeroes
        {
            get
            {
                if (SmiteIsReady)
                {
                    var name = Smite.Slot.GetSpellDataInst().SData.Name.ToLower();
                    if (name.Contains("smiteduel") || name.Contains("smiteplayerganker"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static bool IgniteIsReady
        {
            get { return Ignite != null && Ignite.IsReady(); }
        }

        public static bool FlashIsReady
        {
            get { return Flash != null && Flash.IsReady(); }
        }

        public static void Init(EventArgs args)
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Circular, 620, int.MaxValue, 180)
            {
                AllowedCollisionCount = int.MaxValue
            };
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 0, 1450, 210)
            {
                AllowedCollisionCount = int.MaxValue
            };
            E = new Spell.Skillshot(SpellSlot.E, 700, SkillShotType.Cone, 300, 2500, (int)(45 * 0.5))
            {
                AllowedCollisionCount = int.MaxValue
            };
            QE = new Spell.Skillshot(SpellSlot.E, 1200, SkillShotType.Circular, 0, 2000, 60)
            {
                AllowedCollisionCount = int.MaxValue
            };
            R = new Spell.Targeted(SpellSlot.R, 675);
            var slot = Util.MyHero.SpellSlotFromName("summonerdot");
            if (slot != SpellSlot.Unknown)
            {
                Ignite = new Spell.Targeted(slot, 600);
            }
            slot = Util.MyHero.SpellSlotFromName("smite");
            if (slot != SpellSlot.Unknown)
            {
                Smite = new Spell.Targeted(slot, 500);
            }
            slot = Util.MyHero.SpellSlotFromName("flash");
            if (slot != SpellSlot.Unknown)
            {
                Flash = new Spell.Skillshot(slot, 400, SkillShotType.Circular);
            }
            Game.OnTick += Game_OnUpdate;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
            Obj_AI_Base.OnBuffLose += Obj_AI_Base_OnBuffLose;
        }


        private static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Buff.Name.Equals("syndrawbuff"))
                {
                    _W_Object = sender;
                }
            }
        }

        private static void Obj_AI_Base_OnBuffLose(Obj_AI_Base sender, Obj_AI_BaseBuffLoseEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Buff.Name.Equals("syndrawbuff"))
                {
                    _W_Object = null;
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            W.Width = W_Width1;
            Q.Width = Q_Width1;
            QE = new Spell.Skillshot(SpellSlot.E, 1200 - (uint)MenuManager.MiscMenu.GetSliderValue("QE.Range"),
                SkillShotType.Circular, 0, 2000, 60)
            { AllowedCollisionCount = int.MaxValue };
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (args.Slot)
                {
                    case SpellSlot.Q:
                        Q_EndPosition = args.End;
                        Q_LastCastTime = Game.Time;
                        break;
                    case SpellSlot.W:
                        if (args.SData.Name.ToLower().Equals("syndrawcast"))
                        {
                            W_EndPosition = args.End;
                            W_LastCastTime = Game.Time;
                            Core.DelayAction(delegate { _WE_Object = null; },
                                W.CastDelay + 1500 * (int)(Util.MyHero.Distance(args.End) / W.Speed));
                        }
                        break;
                    case SpellSlot.E:
                        E_LastCastTime = Game.Time;
                        break;
                }
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.W)
            {
                W_LastSentTime = Game.Time;
            }
            if (Harass.IsActive && !Orbwalker.CanMove && Orbwalker.LastTarget.Type != Util.MyHero.Type)
            {
                args.Process = false;
            }
        }

        public static SpellSlot SpellSlotFromName(this AIHeroClient hero, string name)
        {
            foreach (var s in hero.Spellbook.Spells)
            {
                if (s.Name.ToLower().Contains(name.ToLower()))
                {
                    return s.Slot;
                }
            }
            return SpellSlot.Unknown;
        }

        public static void CastQ(Obj_AI_Base target)
        {
            if (SpellSlot.Q.IsReady() && target.IsValidTarget() && target.IsEnemy)
            {
                if (target is AIHeroClient)
                {
                    Q.Width = Q_Width2;
                }
                var pred = Q.GetPrediction(target);
                if (pred.HitChancePercent >= Q.Slot.HitChancePercent())
                {
                    Q.Cast(pred.CastPosition);
                }
            }
        }

        public static void CastW(Obj_AI_Base target)
        {
            if (SpellSlot.W.IsReady() && target.IsValidTarget(W.Range + W.Width) && target.IsEnemy &&
                Game.Time - W_LastSentTime > 0.25f)
            {
                var pred = W.GetPrediction(target);
                if (IsW2)
                {
                    if (target is AIHeroClient)
                    {
                        W.Width = W_Width2;
                    }
                    if (pred.HitChancePercent >= W.Slot.HitChancePercent())
                    {
                        Util.MyHero.Spellbook.CastSpell(W.Slot, pred.CastPosition);
                    }
                }
                else if (pred.HitChancePercent >= W.Slot.HitChancePercent()/2)
                {
                    Obj_AI_Base best = null;
                    if (best == null)
                    {
                        var minion =
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                                m => m.IsValidTarget(W.Range) && m.NetworkId != target.NetworkId)
                                .OrderBy(m => Util.MyHero.Distance(m, true))
                                .FirstOrDefault();
                        if (minion.IsValidTarget())
                        {
                            best = minion;
                        }
                    }
                    if (best == null)
                    {
                        var minion =
                            EntityManager.MinionsAndMonsters.Monsters.Where(
                                m => m.IsValidTarget(W.Range) && m.NetworkId != target.NetworkId)
                                .OrderBy(m => Util.MyHero.Distance(m, true))
                                .FirstOrDefault();
                        if (minion.IsValidTarget())
                        {
                            best = minion;
                        }
                    }
                    if (best == null)
                    {
                        foreach (
                            var b in
                                BallManager.Balls.Where(
                                    m =>
                                        m.IsIdle && Util.MyHero.Distance(m.Position, true) <= Math.Pow(W.Range, 2) &&
                                        !m.E_IsOnTime && m.Object.NetworkId != target.NetworkId)
                                    .OrderBy(m => Util.MyHero.Distance(m.Position, true)))
                        {
                            best = b.Object;
                            break;
                        }
                    }
                    if (best != null)
                    {
                        Util.MyHero.Spellbook.CastSpell(W.Slot, best.Position);
                    }
                }
            }
        }

        public static void CastE(Obj_AI_Base target)
        {
            if (SpellSlot.E.IsReady() && target.IsValidTarget() && target.IsEnemy)
            {
                foreach (var b in BallManager.Balls.Where(m => m.IsIdle))
                {
                    CastE2(target, b.Position);
                }
            }
        }

        public static void CastE2(Obj_AI_Base target, Vector3 position)
        {
            if (SpellSlot.E.IsReady() && target.IsValidTarget() && target.IsEnemy)
            {
                if (Util.MyHero.Distance(position, true) <= Math.Pow(E.Range + E_ExtraWidth, 2))
                {
                    var startPosition = Util.MyHero.Position.To2D() +
                                        (position - Util.MyHero.Position).To2D().Normalized() *
                                        Math.Min(Util.MyHero.Distance(position), E.Range / 2);
                    var endPosition = Util.MyHero.Position.To2D() +
                                      (position - Util.MyHero.Position).To2D().Normalized() *
                                      ((Util.MyHero.Distance(position, true) >= Math.Pow(200, 2)) ? QE.Range : 1000);
                    var info = target.ServerPosition.To2D().ProjectOn(startPosition, endPosition);
                    if (info.IsOnSegment &&
                        target.ServerPosition.To2D().Distance(info.SegmentPoint, true) <=
                        Math.Pow(1.8f * (QE.Width + target.BoundingRadius), 2))
                    {
                        QE.Speed = Util.MyHero.Distance(target, true) >= Util.MyHero.Distance(target, true)
                            ? QE_Speed
                            : int.MaxValue;
                        QE.CastDelay = E.CastDelay +
                                       1000 *
                                       (int)
                                           (Math.Min(Util.MyHero.Distance(position), Util.MyHero.Distance(target)) /
                                            E.Speed);
                        QE.SourcePosition = position;
                        var pred = QE.GetPrediction(target);
                        if (pred.HitChancePercent >= QE.Slot.HitChancePercent())
                        {
                            var info2 = pred.CastPosition.To2D().ProjectOn(startPosition, endPosition);
                            if (info2.IsOnSegment &&
                                info2.SegmentPoint.Distance(pred.CastPosition.To2D(), true) <=
                                Math.Pow((QE.Width + target.BoundingRadius) * 4 / 4, 2))
                            {
                                Util.MyHero.Spellbook.CastSpell(QE.Slot, pred.CastPosition);
                            }
                        }
                    }
                }
            }
        }

        public static void CastQE(Obj_AI_Base target)
        {
            if (SpellSlot.E.IsReady() && target.IsValidTarget(QE.Range + QE.Width) && target.IsEnemy)
            {
                if (SpellSlot.Q.IsReady())
                {
                    if (Util.MyHero.Mana >= SpellSlot.Q.Mana() + SpellSlot.E.Mana())
                    {
                        if (!target.IsValidTarget(Q.Range))
                        {
                            QE.CastDelay = Q.CastDelay + E.CastDelay;
                            QE.Speed = int.MaxValue;
                            QE.SourcePosition = Util.MyHero.Position;
                            var pred1 = QE.GetPrediction(target);
                            if (pred1.HitChancePercent >= 0)
                            {
                                QE.Speed = QE_Speed;
                                QE.SourcePosition = Util.MyHero.Position +
                                                    (pred1.CastPosition - Util.MyHero.Position).Normalized() *
                                                    (E.Range + E_ExtraWidth);
                                QE.CastDelay = Q_CastDelay2 + E.CastDelay +
                                               1000 * (int)(Util.MyHero.Distance(QE.SourcePosition.Value) / E.Speed);
                                var pred2 = QE.GetPrediction(target);
                                if (pred2.HitChancePercent >= QE.Slot.HitChancePercent())
                                {
                                    var startPosition = Util.MyHero.Position +
                                                        (pred2.CastPosition - Util.MyHero.Position).Normalized() *
                                                        (E.Range + E_ExtraWidth);
                                    Util.MyHero.Spellbook.CastSpell(Q.Slot, startPosition);
                                    Combo_QE = Game.Time;
                                }
                            }
                        }
                        else
                        {
                            if (target is AIHeroClient)
                            {
                                Q.Width = Q_Width2;
                            }
                            var pred = Q.GetPrediction(target);
                            if (pred.HitChancePercent >= SpellSlot.Q.HitChancePercent() / 2)
                            {
                                Q.Cast(pred.CastPosition);
                                Combo_QE = Game.Time;
                            }
                        }
                    }
                }
                else if (Game.Time - Combo_QE <= 1.5f * Q.CastDelay / 1000)
                {
                    var timeToArriveQ = Q_CastDelay2 / 1000 - (Game.Time - Q_LastCastTime);
                    if (timeToArriveQ >= 0)
                    {
                        if (timeToArriveQ <= Util.MyHero.Distance(Q_EndPosition) / E.Speed + E_CastDelay2 / 1000)
                        {
                            CastE2(target, Q_EndPosition);
                        }
                    }
                }
            }
        }

        public static void CastWE(Obj_AI_Base target)
        {
            if (SpellSlot.E.IsReady() && BallManager.Balls.Count > 0 && target.IsValidTarget(QE.Range + QE.Width) &&
                target.IsEnemy)
            {
                if (SpellSlot.W.IsReady() && Game.Time - W_LastSentTime > 0.25f)
                {
                    if (!IsW2 && W.Slot.Mana() + QE.Slot.Mana() <= Util.MyHero.Mana)
                    {
                        var best =
                            BallManager.Balls.Where(m => m.IsIdle && !m.E_IsOnTime)
                                .OrderBy(
                                    m =>
                                        target.Position.To2D()
                                            .ProjectOn(m.Position.To2D(), m.E_EndPosition.To2D())
                                            .SegmentPoint.Distance(target.Position.To2D(), true))
                                .LastOrDefault();
                        if (best != null)
                        {
                            Util.MyHero.Spellbook.CastSpell(W.Slot, best.Position);
                        }
                    }
                    else if (IsW2 && QE.Slot.Mana() <= Util.MyHero.Mana && W_Object != null && W_Object.IsBall())
                    {
                        if (!target.IsValidTarget(W.Range))
                        {
                            QE.CastDelay = W.CastDelay + E.CastDelay;
                            QE.Speed = W.Speed;
                            QE.SourcePosition = Util.MyHero.Position;
                            var pred = QE.GetPrediction(target);
                            if (pred.HitChancePercent >= 0)
                            {
                                QE.SourcePosition = Util.MyHero.Position +
                                                    (pred.CastPosition - Util.MyHero.Position).Normalized() *
                                                    (E.Range + E_ExtraWidth);
                                QE.CastDelay = W.CastDelay + E.CastDelay +
                                               1000 *
                                               (int)
                                                   (Util.MyHero.Distance(pred.CastPosition) / W.Speed +
                                                    Util.MyHero.Distance(QE.SourcePosition.Value) / E.Speed);
                                var pred2 = QE.GetPrediction(target);
                                if (pred2.HitChancePercent >= QE.Slot.HitChancePercent())
                                {
                                    var startPos = Util.MyHero.Position +
                                                   (pred2.CastPosition - Util.MyHero.Position).Normalized() *
                                                   (E.Range + E_ExtraWidth);
                                    Util.MyHero.Spellbook.CastSpell(W.Slot, startPos);
                                    Combo_WE = Game.Time;
                                }
                            }
                        }
                        else
                        {
                            if (target is AIHeroClient)
                            {
                                W.Width = W_Width2;
                            }
                            var pred = W.GetPrediction(target);
                            if (pred.HitChancePercent >= SpellSlot.W.HitChancePercent() / 2)
                            {
                                Util.MyHero.Spellbook.CastSpell(W.Slot, pred.CastPosition);
                                Combo_WE = Game.Time;
                            }
                        }
                    }
                }
                else if (_WE_Object.IsValidAlly() && _WE_Object.IsBall() &&
                         Game.Time - Combo_WE <= 1.5f * (W_CastDelay2 / 1000 + _WE_Object.Distance(W_EndPosition) / W_Speed2))
                {
                    var timeToArriveW = W_CastDelay2 / 1000 + _WE_Object.Distance(W_EndPosition) / W_Speed2 -
                                        (Game.Time - W_LastCastTime);
                    if (timeToArriveW >= 0)
                    {
                        if (timeToArriveW <= W_EndPosition.Distance(Util.MyHero) / E.Speed + E_CastDelay2 / 1000)
                        {
                            CastE2(target, W_EndPosition);
                        }
                    }
                }
            }
        }

        public static void CastR(AIHeroClient target)
        {
            if (SpellSlot.R.IsReady() && target.IsValidTarget(R.Range) &&
                !MenuManager.MiscMenu.GetCheckBoxValue("Dont.R." + target.ChampionName))
            {
                R.Cast(target);
            }
        }

        public static float HitChancePercent(this SpellSlot s)
        {
            var slot = s.ToString().Trim();
            if (Harass.IsActive)
            {
                return MenuManager.PredictionMenu.GetSliderValue(slot + "Harass");
            }
            return MenuManager.PredictionMenu.GetSliderValue(slot + "Combo");
        }

        public static bool IsReady(this SpellSlot slot)
        {
            return slot.GetSpellDataInst().IsReady;
        }

        public static SpellDataInst GetSpellDataInst(this SpellSlot slot)
        {
            return Util.MyHero.Spellbook.GetSpell(slot);
        }

        public static float Mana(this SpellSlot slot)
        {
            return slot.GetSpellDataInst().SData.ManaCostArray[slot.GetSpellDataInst().Level - 1];
        }

        public static bool IsInSmiteRange(this Obj_AI_Base target)
        {
            return target.IsValidTarget(Smite.Range + Util.MyHero.BoundingRadius + target.BoundingRadius);
        }

        public static float SmiteDamage(this Obj_AI_Base target)
        {
            if (target.IsValidTarget() && SmiteIsReady)
            {
                if (target is AIHeroClient)
                {
                    if (CanUseSmiteOnHeroes)
                    {
                        return Util.MyHero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Smite);
                    }
                }
                else
                {
                    var level = Util.MyHero.Level;
                    return (new[] { 20 * level + 370, 30 * level + 330, 40 * level + 240, 50 * level + 100 }).Max();
                }
            }
            return 0;
        }
    }
}