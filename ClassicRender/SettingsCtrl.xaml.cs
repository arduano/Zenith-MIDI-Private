﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace ClassicRender
{
    /// <summary>
    /// Interaction logic for SettingsCtrl.xaml
    /// </summary>
    public partial class SettingsCtrl : UserControl
    {
        Settings settings;

        public void SetValues()
        {
            firstNote.Value = settings.firstNote;
            lastNote.Value = settings.lastNote - 1;
            pianoHeight.Value = (int)(settings.pianoHeight * 100);
            noteBrightness.Value = (decimal)settings.noteBrightness;
            noteDeltaScreenTime.Value = settings.deltaTimeOnScreen;
            screenTime.Content = settings.deltaTimeOnScreen;
        }

        public SettingsCtrl(Settings settings) : base()
        {
            InitializeComponent();
            this.settings = settings;
            SetValues();
        }

        private void Nud_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (sender == firstNote) settings.firstNote = (int)firstNote.Value;
                if (sender == lastNote) settings.lastNote = (int)lastNote.Value + 1;
                if (sender == pianoHeight) settings.pianoHeight = (double)pianoHeight.Value / 100;
                if (sender == noteBrightness) settings.noteBrightness = (float)noteBrightness.Value;
                if (sender == noteDeltaScreenTime) settings.deltaTimeOnScreen = (int)noteDeltaScreenTime.Value;
            }
            catch (NullReferenceException)
            {

            }
        }

        private void NoteDeltaScreenTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                settings.deltaTimeOnScreen = (int)noteDeltaScreenTime.Value;
                screenTime.Content = settings.deltaTimeOnScreen;
            }
            catch (NullReferenceException)
            {

            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string s = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("Plugins/ClassicRender.json", s);
                Console.WriteLine("Saved settings to ClassicRender.json");
            }
            catch
            {
                Console.WriteLine("Could not save settings");
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string s = File.ReadAllText("Plugins/ClassicRender.json");
                var sett = JsonConvert.DeserializeObject<Settings>(s);
                injectSettings(sett);
                Console.WriteLine("Loaded settings from ClassicRender.json");
            }
            catch
            {
                Console.WriteLine("Could not load settings");
            }
        }

        void injectSettings(Settings sett)
        {
            var sourceProps = typeof(Settings).GetFields().ToList();
            var destProps = typeof(Settings).GetFields().ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    p.SetValue(settings, sourceProp.GetValue(sett));
                }
            }
            SetValues();
        }

        private void DefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            injectSettings(new Settings());
        }
    }
}