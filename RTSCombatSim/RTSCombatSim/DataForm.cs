﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using XColor = Microsoft.Xna.Framework.Color;
using RTSEngine.Interfaces;
using RTSEngine.Data.Parsers;

namespace RTSCS {
    public partial class DataForm : Form, IDataForm {
        // Should Only Be One Form Ever Made
        public static DataForm Instance { get; private set; }

        public delegate void CloseDelegate();
        public CloseDelegate Closer;

        public event Action<RTSUnitInstance, XColor> OnUnitSpawn;

        // This Is The Unit Data That Must Be Modified By The Form
        private RTSUnit[] units;

        // This Is Team Data That Must Be Modified In A Different Tab
        private RTSTeam[] teams;
        private Vector3[] teamSpawnPositions;
        private Vector2[] teamWaypoints;
        private XColor[] teamColors;
        private Dictionary<string, ReflectedEntityController> controllers;

        // This Should Be Where You Figure Out Which Team You Are Operating On
        int selectedIndex;
        int spawn1SelectedIndex;
        int spawn2SelectedIndex;
        int spawn3SelectedIndex;

        public DataForm(RTSUnit[] ud, RTSTeam[] t, Dictionary<string, ReflectedEntityController> c) {
            InitializeComponent();
            Closer = () => { Close(); };

            // Set Up Data
            units = ud;
            teams = t;
            teamSpawnPositions = new Vector3[teams.Length];
            teamWaypoints = new Vector2[teams.Length];
            teamColors = new XColor[teams.Length];
            controllers = c;

            // Populate Combo Boxes
            unitTypeComboBox.Items.Add("Unit Type 1");
            unitTypeComboBox.Items.Add("Unit Type 2");
            unitTypeComboBox.Items.Add("Unit Type 3");

            spawn1ComboBox.Items.Add("Unit Type 1");
            spawn1ComboBox.Items.Add("Unit Type 2");
            spawn1ComboBox.Items.Add("Unit Type 3");

            spawn2ComboBox.Items.Add("Unit Type 1");
            spawn2ComboBox.Items.Add("Unit Type 2");
            spawn2ComboBox.Items.Add("Unit Type 3");

            spawn3ComboBox.Items.Add("Unit Type 1");
            spawn3ComboBox.Items.Add("Unit Type 2");
            spawn3ComboBox.Items.Add("Unit Type 3");
        }

        private void DataForm_Load(object sender, EventArgs e) {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(!e.Cancel) Instance = null;
        }

        private IActionController GetActionController(string name) {
            return controllers[name] as IActionController;
        }
        private IMovementController GetMovementController(string name) {
            return controllers[name] as IMovementController;
        }
        private ITargettingController GetTargettingController(string name) {
            return controllers[name] as ITargettingController;
        }
        private ICombatController GetCombatController(string name) {
            return controllers[name] as ICombatController;
        }
        private void SpawnUnit(RTSUnit ud, int teamIndex) {
            RTSUnitInstance u = teams[teamIndex].AddUnit(ud, teamSpawnPositions[teamIndex]);
            u.ActionController = GetActionController(App.DEFAULT_ACTION_CONTROLLER);
            u.MovementController = GetMovementController(App.DEFAULT_MOVEMENT_CONTROLLER);
            u.MovementController.SetWaypoints(new Vector2[] { teamWaypoints[teamIndex] });
            u.CombatController = GetCombatController(App.DEFAULT_COMBAT_CONTROLLER);
            u.TargettingController = GetTargettingController(App.DEFAULT_TARGETTING_CONTROLLER);
            if(OnUnitSpawn != null)
                OnUnitSpawn(u, teamColors[teamIndex]);
        }


        private void unitTypeComboBox_Change(object sender, EventArgs e) {
            selectedIndex = unitTypeComboBox.SelectedIndex;
            minRangeTextBox.Text = units[selectedIndex].BaseCombatData.MinRange.ToString();
            maxRangeTextBox.Text = units[selectedIndex].BaseCombatData.MaxRange.ToString();
            attackDamageTextBox.Text = units[selectedIndex].BaseCombatData.AttackDamage.ToString();
            attackTimerTextBox.Text = units[selectedIndex].BaseCombatData.AttackTimer.ToString();
            armorTextBox.Text = units[selectedIndex].BaseCombatData.Armor.ToString();
            criticalDamageTextBox.Text = units[selectedIndex].BaseCombatData.CriticalDamage.ToString();
            criticalChanceTextBox.Text = units[selectedIndex].BaseCombatData.CriticalChance.ToString();
            healthTextBox.Text = units[selectedIndex].Health.ToString();
            movementSpeedTextBox.Text = units[selectedIndex].MovementSpeed.ToString();
        }

        private void saveButton_Click(object sender, EventArgs e) {
            units[selectedIndex].BaseCombatData.MinRange = int.Parse(minRangeTextBox.Text);
            units[selectedIndex].BaseCombatData.MaxRange = int.Parse(maxRangeTextBox.Text);
            units[selectedIndex].BaseCombatData.AttackTimer = int.Parse(attackTimerTextBox.Text);
            units[selectedIndex].BaseCombatData.AttackDamage = int.Parse(attackDamageTextBox.Text);
            units[selectedIndex].BaseCombatData.Armor = int.Parse(armorTextBox.Text);
            units[selectedIndex].BaseCombatData.CriticalDamage = int.Parse(criticalDamageTextBox.Text);
            units[selectedIndex].BaseCombatData.CriticalChance = int.Parse(criticalChanceTextBox.Text);
            units[selectedIndex].Health = int.Parse(healthTextBox.Text);
            units[selectedIndex].MovementSpeed = int.Parse(movementSpeedTextBox.Text);
        }

        // Assumes Data Is Input As (x,y,z)
        private Vector3 StringToVector3(String s) {
            String[] splitString = s.Split(',');
            if(splitString.Length != 3) return Vector3.Zero;
            return new Vector3(float.Parse(splitString[0]), float.Parse(splitString[1]), float.Parse(splitString[2]));
        }

        // Assumes Data Is Input As (x,y)
        private Vector2 StringToVector2(String s) {
            String[] splitString = s.Split(',');
            if(splitString.Length != 2) return Vector2.Zero;
            return new Vector2(float.Parse(splitString[0]), float.Parse(splitString[1]));
        }
      
        private void spawnButton_Click(object sender, EventArgs e) {          
            teamSpawnPositions[0] = StringToVector3(team1SpawnPositionTextBox.Text);
            teamSpawnPositions[1] = StringToVector3(team2SpawnPositionTextBox.Text);
            teamSpawnPositions[2] = StringToVector3(team3SpawnPositionTextBox.Text);

            teamWaypoints[0] = StringToVector2(team1WaypointTextBox.Text);
            teamWaypoints[1] = StringToVector2(team2WaypointTextBox.Text);
            teamWaypoints[2] = StringToVector2(team3WaypointTextBox.Text);

            System.Drawing.Color systemColor = System.Drawing.Color.FromName(team1ColorTextBox.Text);
            XColor color1 = new XColor(systemColor.R, systemColor.G, systemColor.B, systemColor.A); //Here Color is Microsoft.Xna.Framework.Graphics.Color
            teamColors[0] = color1;
            System.Drawing.Color systemColor2 = System.Drawing.Color.FromName(team2ColorTextBox.Text);
            XColor color2 = new XColor(systemColor2.R, systemColor2.G, systemColor2.B, systemColor2.A); 
            teamColors[1] = color2;
            System.Drawing.Color systemColor3 = System.Drawing.Color.FromName(team3ColorTextBox.Text);
            XColor color3 = new XColor(systemColor3.R, systemColor3.G, systemColor3.B, systemColor3.A); 
            teamColors[2] = color3;

            for(int t = 0; t < teams.Length; t++) {
                for(int u = 0; u < units.Length; u++) {
                    int spawnCount = int.Parse(PickTextBox(t, u).Text);
                    for(int count = 0; count < spawnCount; count++) {
                        teams[t].AddUnit(units[u], teamSpawnPositions[t]);
                    }
                }
            }
        }

        // There Has To Be A Cleaner Way To Do This... I Wish I Had OCaml's Match...
        private TextBox PickTextBox(int team, int unit) {
            if(team == 0 && unit == 0) return team1Unit1TextBox;
            else if(team == 0 && unit == 1) return team1Unit2TextBox;
            else if(team == 0 && unit == 2) return team1Unit3TextBox;
            else if(team == 1 && unit == 0) return team2Unit1TextBox;
            else if(team == 1 && unit == 1) return team2Unit2TextBox;
            else if(team == 1 && unit == 2) return team2Unit3TextBox;
            else if(team == 2 && unit == 0) return team3Unit1TextBox;
            else if(team == 2 && unit == 1) return team3Unit2TextBox;
            else return team3Unit3TextBox;
        }

        private void Spawn1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            spawn1SelectedIndex = spawn1ComboBox.SelectedIndex;
        }

        private void Spawn2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            spawn2SelectedIndex = spawn2ComboBox.SelectedIndex;
        }

        private void Spawn3ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            spawn3SelectedIndex = spawn3ComboBox.SelectedIndex;
        }

        private void spawn1Button_Click(object sender, EventArgs e)
        {
            teams[1].AddUnit(units[spawn1SelectedIndex], teamSpawnPositions[1]);
        }

        private void spawn2Button_Click(object sender, EventArgs e)
        {
            teams[2].AddUnit(units[spawn2SelectedIndex], teamSpawnPositions[2]);
        }

        private void spawn3Button_Click(object sender, EventArgs e)
        {
            teams[3].AddUnit(units[spawn3SelectedIndex], teamSpawnPositions[3]);
        }
    }
}