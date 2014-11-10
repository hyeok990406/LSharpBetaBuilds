﻿using System;
using LeagueSharp;
using LeagueSharp.Common;
using OC = Oracle.Program;

namespace Oracle
{
    internal static class Consumables
    {
        private static Menu Main;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public static void Initialize(Menu Root)
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Main = new Menu("Consumables", "imenu");

            CreateMenuItem("Biscuit", "Biscuit", 40, 25, true, true);
            CreateMenuItem("Mana Potion", "Mana", 40, 0);
            CreateMenuItem("Health Potion", "Health", 40, 25, false, true);
            CreateMenuItem("Crystaline Flask", "Flask", 40, 35, true, true);

            Root.AddSubMenu(Main);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            UseItem("ItemMiniRegenPotion", "Biscuit", OC.MinionDamage, OC.IncomeDamage);
            UseItem("ItemCrystalFlask", "Flask", OC.MinionDamage, OC.IncomeDamage);
            UseItem("FlaskofCrystalWater", "Mana", 0, OC.IncomeDamage, false);
            UseItem("RegenerationPotion", "Health", OC.MinionDamage, OC.IncomeDamage, true, false);
        }

        private static void UseItem(string name, string menuvar, float incdmg = 0, float mindmg = 0, bool usehealth = true, bool usemana = true)
        {
            if (Me.HasBuff(name) || Me.HasBuff("Recall") || !Items.HasItem(name))
                return;

            if (!Main.Item("use" + menuvar).GetValue<bool>())
                return;

            SpellSlot consumable = Me.GetSpellSlot(name);
            if (consumable == SpellSlot.Unknown)
                return;

            var consumableslot = new Spell(consumable);
            if (!consumableslot.IsReady())
                return;

            var aHealthPercent = (int) ((Me.Health/Me.MaxHealth)*100);
            var aManaPercent = (int) ((Me.Mana/Me.MaxMana)*100);
            var iDamagePercent = (int) ((incdmg/Me.MaxHealth)*100);
            var mDamagePercent = (int) ((mindmg/Me.MaxHealth)*100);

            if (usehealth && aHealthPercent <= Main.Item("use" + menuvar + "Pct").GetValue<Slider>().Value)
            {
                if (iDamagePercent >= 1 || incdmg >= Me.Health || Me.HasBuff("summonerdot") ||
                    mDamagePercent >= 1 || mindmg >= Me.Health)
                {
                    if (OC.AggroTarget.NetworkId == Me.NetworkId)
                        consumableslot.Cast();
                }
                else if (iDamagePercent >= Main.Item("use" + menuvar + "Dmg").GetValue<Slider>().Value)
                {
                    if (OC.AggroTarget.NetworkId == Me.NetworkId)
                        consumableslot.Cast();
                }
            }
            else if (usemana && aManaPercent <= Main.Item("use" + menuvar + "Mana").GetValue<Slider>().Value)
            {
                // check if we use mana
                if (Me.Mana != 0)
                {
                    consumableslot.Cast();
                }
            }
        }

        private static void CreateMenuItem(string name, string menuvar, int dvalue, int dmgvalue, bool usemana = true, bool usehealth = false)
        {
            var menuName = new Menu(name, "m" + menuvar);
            menuName.AddItem(new MenuItem("use" + menuvar, "Use " + name)).SetValue(true);
            if (usehealth)
            {
                menuName.AddItem(new MenuItem("use" + menuvar + "Pct", "Use on HP &")).SetValue(new Slider(dvalue));
                menuName.AddItem(new MenuItem("use" + menuvar + "Dmg", "Use on Dmg %")).SetValue(new Slider(dmgvalue));
            }
            if (usemana)
                menuName.AddItem(new MenuItem("use" + menuvar + "Mana", "Use on Mana %")).SetValue(new Slider(40));
            Main.AddSubMenu(menuName);
        }
    }
}