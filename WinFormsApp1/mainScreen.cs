using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using static WinFormsApp1.mainScreen;

namespace WinFormsApp1
{
    // A static repository to hold the list of pin definitions.
    

    public partial class mainScreen : Form
    {
        List<Mnb> motherboards;
        List<Cab> cables;
        List<Lcd> lcd;
        List<Label> pinLabels;

        public mainScreen()
        {
            InitializeComponent();
            mnbBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            lcdBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            button1.Click += GeneratePinout;

            // Initialize the pinLabels list and add the labels to it.
            pinLabels = new List<Label>();
            for (int i = 1; i <= 40; i++)
            {
                var label = this.Controls.Find($"pin{i}", true).FirstOrDefault() as Label;
                if (label != null)
                {
                    pinLabels.Add(label);
                }
            }
        }

        private void mainScreen_Load(object sender, EventArgs e)
        {
            // Load MNB data
            string json = File.ReadAllText(@"..\..\..\Resources\mnbData.json");
            motherboards = JsonSerializer.Deserialize<List<Mnb>>(json) ?? new List<Mnb>();

            // Load Cab data
            json = File.ReadAllText(@"..\..\..\Resources\cabData.json");
            cables = JsonSerializer.Deserialize<List<Cab>>(json) ?? new List<Cab>();

            // Load LCD data
            json = File.ReadAllText(@"..\..\..\Resources\lcdData.json");
            lcd = JsonSerializer.Deserialize<List<Lcd>>(json) ?? new List<Lcd>();

            // Load Pin Family Data into the repository.
            json = File.ReadAllText(@"..\..\..\Resources\pinData.json");
            PinRepository.Pinouts = JsonSerializer.Deserialize<List<Pin>>(json) ?? new List<Pin>();

            mnbBox.DataSource = motherboards;
            mnbBox.DisplayMember = "PN"; // which property to display
            mnbBox.SelectedIndex = -1;

            lcdBox.DataSource = lcd;
            lcdBox.DisplayMember = "PN"; // which property to display
            lcdBox.SelectedIndex = -1;

            cabBox.Enabled = false; // Initially disable the cable combo box
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckComboBoxSelections();
        }

        private void CheckComboBoxSelections()
        {
            if (mnbBox.SelectedIndex != -1 && lcdBox.SelectedIndex != -1)
            {
                // Filter cables based on whether the LCD's connector array contains the cable's connector
                var selectedLcd = (Lcd)lcdBox.SelectedItem;
                var filteredCables = cables.Where(c => selectedLcd.connector.Contains(c.connector)).ToList();

                cabBox.Enabled = true;
                cabBox.DataSource = filteredCables;
                cabBox.DisplayMember = "PN"; // which property to display
                cabBox.SelectedIndex = -1;
            }
        }

        private void GeneratePinout(object sender, EventArgs e)
        {
            if (mnbBox.SelectedIndex != -1 && cabBox.SelectedIndex != -1 && lcdBox.SelectedIndex != -1)
            {
                // Create the Generation instance. No need to pass in the pinouts list.
                var generation = new Generation(
                    cabBox.SelectedItem as Cab,
                    lcdBox.SelectedItem as Lcd,
                    mnbBox.SelectedItem as Mnb
                );

                // Update the pin labels with the generated pinout.
                for (int i = 0; i < pinLabels.Count; i++)
                {
                    if (i < generation.pinout.Length)
                        pinLabels[i].Text = generation.pinout[i];
                    else
                        pinLabels[i].Text = string.Empty;
                }
            }
            else
            {
                MessageBox.Show("Please make a selection in all three combo boxes.");
            }
        }

        public void saveJson()
        {
            File.WriteAllText(
                @"..\..\..\Resources\cabData.json",
                JsonSerializer.Serialize(cables, new JsonSerializerOptions { WriteIndented = true })
            );

            File.WriteAllText(
                @"..\..\..\Resources\lcdData.json",
                JsonSerializer.Serialize(lcd, new JsonSerializerOptions { WriteIndented = true })
            );

            File.WriteAllText(
                @"..\..\..\Resources\pinData.json",
                JsonSerializer.Serialize(PinRepository.Pinouts, new JsonSerializerOptions { WriteIndented = true })
            );

            File.WriteAllText(
                @"..\..\..\Resources\mnbData.json",
                JsonSerializer.Serialize(motherboards, new JsonSerializerOptions { WriteIndented = true })
            );
        }

        
    }
    public class Generation
    {
        public Cab selectedCab;
        public Mnb selectedMnb;
        public Lcd selectedLcd;
        public string[] mnbPinout;
        public string[] lcdPinout;
        public string[] pinout;

        public Generation(Cab c, Lcd l, Mnb m)
        {
            selectedCab = c;
            selectedMnb = m;
            selectedLcd = l;
            // Call the pinout() method that now uses the static repository.
            mnbPinout = selectedMnb.pinout();
            lcdPinout = selectedLcd.pinout();
            pinout = selectedMnb.pinout();
        }
    }

    public class Mnb
    {
        public required string PN { get; set; }
        public required string pinFamily { get; set; }

        // This method now directly references the static repository.
        public string[] pinout()
        {
            var pinData = PinRepository.Pinouts.FirstOrDefault(p => p.pinFamily == this.pinFamily);
            return pinData != null ? pinData.pinout : Array.Empty<string>();
        }
    }

    public class Cab
    {
        public required string PN { get; set; }
        public required string[] colors { get; set; }
        public required string connector { get; set; }
    }

    public class Lcd
    {
        public required string PN { get; set; }
        public required string pinFamily { get; set; }
        public required string[] connector { get; set; }

        public string[] pinout()
        {
            var pinData = PinRepository.Pinouts.FirstOrDefault(p => p.pinFamily == this.pinFamily);
            return pinData != null ? pinData.pinout : Array.Empty<string>();
        }
    }

    public class Pin
    {
        public required string pinFamily { get; set; }
        public required string[] pinout { get; set; }
    }
    public static class PinRepository
    {
        public static List<Pin> Pinouts { get; set; } = new List<Pin>();
    }
}
