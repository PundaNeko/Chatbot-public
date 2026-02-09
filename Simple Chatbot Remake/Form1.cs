using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using NAudio.Wave;

using static Simple_Chatbot_Remake.Form1;

namespace Simple_Chatbot_Remake
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine _recognizer;
        private SpeechSynthesizer _speaker = new SpeechSynthesizer();

        private List<ChatRule> chatRules = new List<ChatRule>();
        private Random rand = new Random();

        public class Root
        {
            public List<LanguageGroup>? rules { get; set; }
        }

        public class LanguageGroup
        {
            public string? language { get; set; }
            public List<ChatRule>? rules { get; set; }
        }

        public class ChatRule
        {
            public Regex? regex { get; set; }

            public string? pattern { get; set; } = "";
            public string[]? responses { get; set; }
            public string options { get; set; } = "Compiled";
        }

        public Form1()
        {
            InitializeComponent();
            InitializeRules();
            SetupVoice();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.AppendText("Bot: Hi! Type something...\n");

        }
        private void InitializeRules()
        {
            DebugText.AppendText("Execution started\n");
            //try to open json file
            try
            {
                string jsonPath = Path.Combine(Application.StartupPath, "rules.json");
                string json = File.ReadAllText(jsonPath);
                var data = JsonSerializer.Deserialize<Root>(json);

                DebugText.AppendText("data is null? " + (data == null) + "\n");
                DebugText.AppendText("data.rules is null? " + (data?.rules == null) + "\n");
                DebugText.AppendText("language count: " + (data?.rules?.Count ?? 0) + "\n");

                if (data?.rules == null || data.rules.Count == 0)
                {
                    MessageBox.Show("No rules found in JSON!");
                    return;
                }

                DebugText.AppendText("=== RAW JSON LOADED ===" + "\n");

                foreach (var langGroup in data.rules)
                {
                    DebugText.AppendText($"--- {langGroup.language} rules ---\n");

                    foreach (var rule in langGroup.rules ?? new List<ChatRule>())
                    {
                        //turn Json into a faster format
                        RegexOptions options = RegexOptions.Compiled;
                        if (!string.IsNullOrEmpty(rule.options) && rule.options.Contains("IgnoreCase"))
                            options |= RegexOptions.IgnoreCase;

                        //adds stuff into ChatRule
                        chatRules.Add(new ChatRule
                        {
                            pattern = rule.pattern ?? "",
                            responses = rule.responses ?? Array.Empty<string>(),
                            regex = new Regex(rule.pattern ?? "", options)
                        });
                        Console.WriteLine($"Pattern: '{rule.pattern}'" + "\n");

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rules: {ex.Message}");
            }
        }
        private string GetResponse(string input)
        {
            input = input.ToLower();
            foreach (var rule in chatRules)
            {
                if (rule.regex.IsMatch(input))
                    return rule.responses[rand.Next(rule.responses.Length)];
            }
            return "This bot is still stupid";
        }

        private void sendButton_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string userInput = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(userInput)) return;

            // Add user message
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionColor = System.Drawing.Color.Blue;
            richTextBox1.AppendText($"You:\n {userInput}\n");
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
            textBox1.Clear();

            // Scroll to bottom
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();

            // Bot response
            string response = GetResponse(userInput);
            richTextBox1.AppendText($"Bot:\n {response}\n");
            richTextBox1.ScrollToCaret();
        }

        #region Voice Recognition and Synthesis
        private void SetupVoice()
        {
            try
            {
                _recognizer = new SpeechRecognitionEngine();

                _recognizer.SetInputToDefaultAudioDevice();
                _recognizer.LoadGrammar(new DictationGrammar());
                _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                DebugText.AppendText($"Voice setup failed: {ex.Message}\n");
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string answer = e.Result.Text.ToLower();
            DebugText.AppendText($"Heard: '{answer}'\n");

            string response = GetResponse(answer);
        }
        #endregion
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Start recording on hold, stop recording on release
            if (_recognizer != null)
            {
                _speaker.SpeakAsync("Listening for you");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        //TRY 0: add in a profile json add a snarky ass comment from the bot 
        //TRY 1: next, connect to a web based application (probably should make myself) sql server?
        //TRY 2: branch it and add in a free AI API (not sure if this will work)
        //TRY 3: add in a 2 way comminucation methods
        //TRY 4: try to make every word have a vector value for personal use (probably seperate branch

    }
}
