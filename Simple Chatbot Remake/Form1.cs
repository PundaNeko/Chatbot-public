using Microsoft.VisualBasic.ApplicationServices;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static Simple_Chatbot_Remake.Form1;

namespace Simple_Chatbot_Remake
{
    public partial class Form1 : Form
    {
        private string connectionString = "Server=localhost;Database=ChatBotHistory;Uid=root;Pwd=OneTwoThree45;Port=3306;";

        private string LLMModel = "upstage/solar-pro-3:free";

        string apiKey = "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";
        private List<object> conversationHistory = new();
        private readonly HttpClient client = new();
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClearTable();
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

                //DebugText.AppendText("data is null? " + (data == null) + "\n");
                //DebugText.AppendText("data.rules is null? " + (data?.rules == null) + "\n");
                //DebugText.AppendText("language count: " + (data?.rules?.Count ?? 0) + "\n");

                if (data?.rules == null || data.rules.Count == 0)
                {
                    MessageBox.Show("No rules found in JSON!");
                    return;
                }

                //DebugText.AppendText("=== RAW JSON LOADED ===" + "\n");

                foreach (var langGroup in data.rules)
                {
                    //DebugText.AppendText($"--- {langGroup.language} rules ---\n");

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

        private async void button1_Click(object sender, EventArgs e)
        {
            string userInput = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(userInput)) return;

            AddMessage($"You:\n {userInput}\n", Color.Blue);
            textBox1.Clear();

            conversationHistory.Add(new { role = "user", content = userInput });

            InsertIntoSQL("NekoPunda", "user", userInput);

            client.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);

            var request = new
            {
                model = LLMModel,
                messages = conversationHistory.ToArray(),
                max_tokens = 1000
            };

            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                client.DefaultRequestHeaders.Add("X-Title", "ChatBot");

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                DebugText.AppendText("Waiting on AI" + "\n");
                var response = await
                    client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    DebugText.AppendText($"API Error: {response.StatusCode} - {errorBody} \n");
                    return;
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                File.WriteAllText($"response_{DateTime.Now:yyyyMMdd_HHmmss}.json", resultJson);
                var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
                var botChoices = result.GetProperty("choices")[0].GetProperty("message");
                string botReply = botChoices.GetProperty("content").GetString();
                string botRole = botChoices.GetProperty("role").GetString();
                int tokenCount = result.GetProperty("usage").GetProperty("completion_tokens").GetInt32();

                await InsertIntoSQL("LLM", botRole, botReply, LLMModel, tokenCount);

                AddMessage($"Bot:\n {botReply}\n", Color.White);
                conversationHistory.Add(new { role = botRole, content = botReply });
            }
            catch (Exception ex)
            {
                DebugText.AppendText("Error: " + ex.Message);
            }
        }

        private void AddMessage(string message, Color backColor)
        {
            // Add user message
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionColor = System.Drawing.Color.Blue;
            richTextBox1.AppendText(message + "\r\n");
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
            textBox1.Clear();
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
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

        private async Task InsertIntoSQL(string userId, string role, string content, string modelUsed = "Human", int tokensUsed = 0)
        {
            try
            {
                DateTime thisTime = DateTime.Now;
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                DebugText.AppendText("MySQL Connected!\n");

                string sql = @"INSERT INTO ChatHistory (Username, Role, Content, ModelUsed, TokensUsed) 
                   VALUES (@userId, @role, @content, @modelUsed, @tokensUsed)";


                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@role", role);
                cmd.Parameters.AddWithValue("@content", content);
                cmd.Parameters.AddWithValue("@modelUsed", modelUsed);
                cmd.Parameters.AddWithValue("@tokensUsed", tokensUsed);
                await cmd.ExecuteNonQueryAsync();
                DebugText.AppendText($" Inserted: {userId}\n");
            }
            catch (Exception ex)
            {
                DebugText.AppendText($" DB ERROR: {ex.Message}\n");
            }
        }
        private async void ClearTable()
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                string sql = @"TRUNCATE TABLE ChatHistory";
                using var cmd = new MySqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
                DebugText.AppendText("Table cleared!\n");
            }
            catch (Exception ex)
            {
                DebugText.AppendText($" DB ERROR: {ex.Message}\n");
            }
        }
    }
}
