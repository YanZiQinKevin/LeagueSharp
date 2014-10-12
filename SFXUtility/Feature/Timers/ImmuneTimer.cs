﻿#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 ImmuneTimer.cs is part of SFXUtility.
 
 SFXUtility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 SFXUtility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with SFXUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace SFXUtility.Feature
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using Color = System.Drawing.Color;
    using Utilities = global::SFXUtility.Class.Utilities;

    #endregion

    internal class ImmuneTimer : Base
    {
        #region Fields

        private readonly List<ImmuneHero> _immuneHero = new List<ImmuneHero>();
        private readonly List<ImmuneStruct> _immuneStructs = new List<ImmuneStruct>();
        private Timers _timers;

        #endregion

        #region Constructors

        public ImmuneTimer(IContainer container)
            : base(container)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get
            {
                return _timers != null && _timers.Menu != null &&
                       _timers.Menu.Item(_timers.Name + "Enabled").GetValue<bool>() && Menu != null &&
                       Menu.Item(Name + "Enabled").GetValue<bool>();
            }
        }

        public override string Name
        {
            get { return "Immune"; }
        }

        #endregion

        #region Methods

        private void ObjectOnCreate(GameObject sender, EventArgs args)
        {
            try
            {
                if (!Enabled || !sender.IsValid)
                    return;

                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (hero.IsValid && (hero.IsAlly && Menu.Item(Name + "ShowAlly").GetValue<bool>() ||
                                         hero.IsEnemy && Menu.Item(Name + "ShowEnemy").GetValue<bool>()))
                    {
                        foreach (ImmuneStruct iStruct in _immuneStructs)
                        {
                            if (iStruct.SpellName == sender.Name &&
                                Vector3.Distance(sender.Position, hero.Position) <= 100)
                            {
                                _immuneHero.Add(new ImmuneHero((int) Game.Time, hero, iStruct));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                foreach (ImmuneHero iHero in _immuneHero.ToList().Where(iHero => iHero.TimeUsed != -1))
                {
                    float endTime = iHero.TimeUsed - (int) Game.Time + iHero.Struct.Delay;
                    var m = (float) Math.Floor(endTime/60);
                    var s = (float) Math.Ceiling(endTime%60);
                    Utilities.DrawTextCentered(iHero.Hero.Position.To2D(),
                        Menu.Item(Name + "Drawing" + (iHero.Hero.IsAlly ? "Ally" : "Enemy") + "Color").GetValue<Color>(),
                        (s < 10 ? m + ":0" + s : m + ":" + s));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);

                if (IoC.IsRegistered<Timers>() && IoC.Resolve<Timers>().Initialized)
                {
                    TimersLoaded(IoC.Resolve<Timers>());
                }
                else
                {
                    if (IoC.IsRegistered<Mediator>())
                    {
                        IoC.Resolve<Mediator>().Register("Timers_initialized", TimersLoaded);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                _immuneHero.RemoveAll(iHero => iHero.TimeUsed + iHero.Struct.Delay < Game.Time);
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void TimersLoaded(object o)
        {
            try
            {
                if (o is Timers && (o as Timers).Menu != null)
                {
                    _timers = (o as Timers);

                    Menu = new Menu(Name, Name);

                    var drawingMenu = new Menu("Drawing", Name + "Drawing");
                    drawingMenu.AddItem(new MenuItem(Name + "DrawingAllyColor", "Ally Color").SetValue(Color.DarkRed));
                    drawingMenu.AddItem(new MenuItem(Name + "DrawingEnemyColor", "Enemy Color").SetValue(Color.DarkRed));

                    Menu.AddSubMenu(drawingMenu);

                    Menu.AddItem(new MenuItem(Name + "ShowAlly", "Show Ally").SetValue(true));
                    Menu.AddItem(new MenuItem(Name + "ShowEnemy", "Show Enemy").SetValue(true));
                    Menu.AddItem(new MenuItem(Name + "Enabled", "Enabled").SetValue(false));

                    _timers.Menu.AddSubMenu(Menu);

                    _immuneStructs.Add(new ImmuneStruct("eyeforaneye_self.troy", 2f));
                    _immuneStructs.Add(new ImmuneStruct("zhonyas_ring_activate.troy", 2.5f));
                    _immuneStructs.Add(new ImmuneStruct("Aatrox_Passive_Death_Activate.troy", 3f));
                    _immuneStructs.Add(new ImmuneStruct("LifeAura.troy", 4f));
                    _immuneStructs.Add(new ImmuneStruct("UndyingRage_buf.troy", 5f));
                    _immuneStructs.Add(new ImmuneStruct("EggTimer.troy", 6f));
                    _immuneStructs.Add(new ImmuneStruct("nickoftime_tar.troy", 7f));

                    Game.OnGameUpdate += OnGameUpdate;
                    GameObject.OnCreate += ObjectOnCreate;
                    Drawing.OnDraw += OnDraw;

                    Initialized = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        #endregion

        #region Nested Types

        private class ImmuneHero
        {
            #region Fields

            public readonly Obj_AI_Hero Hero;
            public readonly ImmuneStruct Struct;
            public readonly int TimeUsed;

            #endregion

            #region Constructors

            public ImmuneHero(int timeUsed, Obj_AI_Hero hero, ImmuneStruct iStruct)
            {
                TimeUsed = timeUsed;
                Hero = hero;
                Struct = iStruct;
            }

            #endregion
        }

        private class ImmuneStruct
        {
            #region Fields

            public readonly float Delay;
            public readonly string SpellName;

            #endregion

            #region Constructors

            public ImmuneStruct(string spellName, float delay)
            {
                SpellName = spellName;
                Delay = delay;
            }

            #endregion
        }

        #endregion
    }
}