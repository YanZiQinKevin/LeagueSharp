﻿#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Health.cs is part of SFXUtility.
 
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
    using System.Drawing;
    using System.Globalization;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class Health : Base
    {
        #region Constructors

        public Health(Container container)
            : base(container)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get { return Menu != null && Menu.Item("Enabled").GetValue<bool>(); }
        }

        public override string Name
        {
            get { return "Health"; }
        }

        #endregion

        #region Methods

        private void InhibitorHealth()
        {
            if (!Menu.Item("InhibitorEnabled").GetValue<bool>())
                return;
            foreach (var inhibitor in ObjectManager.Get<Obj_BarracksDampener>())
            {
                if (inhibitor.IsValid && !inhibitor.IsDead && inhibitor.Health > 0.1f)
                {
                    var percent = ((int) (inhibitor.Health/inhibitor.MaxHealth)*100);
                    Utilities.DrawTextCentered(Drawing.WorldToMinimap(inhibitor.Position),
                        Menu.Item("InhibitorColor").GetValue<Color>(), Menu.Item("InhibitorPercentage").GetValue<bool>()
                            ? (percent == 0 ? 1 : percent).ToString(CultureInfo.InvariantCulture)
                            : ((int) inhibitor.Health).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;
                InhibitorHealth();
                TurretHealth();
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnGameLoad(EventArgs args)
        {
            Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);
            try
            {
                Menu = new Menu(Name, Name);

                var inhibitorMenu = new Menu("Inhibitor", "Inhibitor");
                inhibitorMenu.AddItem(new MenuItem("InhibitorColor", "Color").SetValue(Color.Yellow));
                inhibitorMenu.AddItem(new MenuItem("InhibitorEnabled", "Enabled").SetValue(true));
                inhibitorMenu.AddItem(new MenuItem("InhibitorPercentage", "Percentage").SetValue(true));

                var turretMenu = new Menu("Turret", "Turret");
                turretMenu.AddItem(new MenuItem("TurretColor", "Color").SetValue(Color.Yellow));
                turretMenu.AddItem(new MenuItem("TurretEnabled", "Enabled").SetValue(true));
                turretMenu.AddItem(new MenuItem("TurretPercentage", "Percentage").SetValue(true));

                Menu.AddSubMenu(inhibitorMenu);
                Menu.AddSubMenu(turretMenu);

                Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(false));

                BaseMenu.AddSubMenu(Menu);

                Drawing.OnDraw += OnDraw;
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void TurretHealth()
        {
            if (!Menu.Item("TurretEnabled").GetValue<bool>())
                return;
            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (turret.IsValid && !turret.IsDead && turret.Health > 0f && turret.Health < 9999f)
                {
                    var percent = ((int) (turret.Health/turret.MaxHealth)*100);
                    Utilities.DrawTextCentered(Drawing.WorldToMinimap(turret.Position),
                        Menu.Item("TurretColor").GetValue<Color>(), Menu.Item("TurretPercentage").GetValue<bool>()
                            ? (percent == 0 ? 1 : percent).ToString(CultureInfo.InvariantCulture)
                            : ((int) turret.Health).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        #endregion
    }
}