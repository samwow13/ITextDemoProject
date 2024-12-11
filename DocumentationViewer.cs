using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using FastColoredTextBoxNS;
using System.IO;
using System.Reflection;
using System.Text;

namespace ITextDemo
{
    public partial class DocumentationViewer : Form
    {
        private FlowLayoutPanel flowLayoutPanel;
        private Dictionary<string, (string Documentation, string Code)> documentationItems;

        public DocumentationViewer()
        {
            InitializeComponent();
            InitializeUI();
            LoadDocumentation();
        }

        private void InitializeComponent()
        {
            this.Text = "Documentation Viewer";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeUI()
        {
            flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            this.Controls.Add(flowLayoutPanel);
        }

        private void LoadDocumentation()
        {
            documentationItems = new Dictionary<string, (string, string)>();

            // Get the content of Program.cs
            string programPath = Path.Combine(Application.StartupPath, "Program.cs");
            if (!File.Exists(programPath))
            {
                programPath = Path.Combine(Directory.GetCurrentDirectory(), "Program.cs");
            }
            
            if (!File.Exists(programPath))
            {
                AddDocumentationItem(
                    "Error",
                    "Could not find Program.cs file. Please ensure you're running the application from the correct directory.",
                    "// No code available"
                );
                return;
            }

            string programCs = File.ReadAllText(programPath);

            // Extract function documentation and code
            var functionMatches = Regex.Matches(programCs, @"/\*\s*(.*?)\s*\*/\s*public\s+static\s+.*?\s+(\w+)\s*\((.*?)\).*?{(.*?)(?=public|private|\}[\r\n\s]*$)", 
                RegexOptions.Singleline);

            foreach (Match match in functionMatches)
            {
                string documentation = match.Groups[1].Value.Trim();
                string functionName = match.Groups[2].Value;
                string code = match.Groups[0].Value;

                // Format documentation by removing * at start of lines
                documentation = Regex.Replace(documentation, @"^\s*\*\s*", "", RegexOptions.Multiline);
                documentation = documentation.Trim();

                AddDocumentationItem(functionName, documentation, code);
            }

            if (documentationItems.Count == 0)
            {
                AddDocumentationItem(
                    "No Documentation Found",
                    "No documented functions were found in the codebase.",
                    "// No code available"
                );
            }
        }

        private void AddDocumentationItem(string title, string documentation, string code)
        {
            // Create card panel with shadow effect
            Panel cardPanel = new Panel
            {
                Width = flowLayoutPanel.Width - 50,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(15)
            };

            // Add shadow effect
            cardPanel.Paint += (s, e) =>
            {
                var graphics = e.Graphics;
                var shadow = Color.FromArgb(20, 0, 0, 0);
                var rect = cardPanel.ClientRectangle;
                for (int i = 1; i <= 3; i++)
                    graphics.DrawRectangle(new Pen(shadow, i), rect);
            };

            // Title
            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            cardPanel.Controls.Add(titleLabel);

            // Documentation
            RichTextBox docTextBox = new RichTextBox
            {
                Text = documentation,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(245, 245, 245),
                Location = new Point(15, titleLabel.Bottom + 10),
                Width = cardPanel.Width - 30,
                Height = 150,
                Font = new Font("Segoe UI", 10)
            };
            cardPanel.Controls.Add(docTextBox);

            // View Code button
            Button viewCodeButton = new Button
            {
                Text = "View Code",
                Location = new Point(15, docTextBox.Bottom + 10),
                AutoSize = true,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };
            viewCodeButton.FlatAppearance.BorderSize = 0;
            viewCodeButton.Click += (s, e) => ShowCodeDialog(title, code);
            cardPanel.Controls.Add(viewCodeButton);

            // Calculate total height
            cardPanel.Height = viewCodeButton.Bottom + 15;

            flowLayoutPanel.Controls.Add(cardPanel);
        }

        private void ShowCodeDialog(string title, string code)
        {
            Form codeForm = new Form
            {
                Text = $"Code: {title}",
                Size = new Size(1000, 700),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };

            FastColoredTextBox codeBox = new FastColoredTextBox
            {
                Dock = DockStyle.Fill,
                Language = FastColoredTextBoxNS.Language.CSharp,
                Text = code,
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = Color.Black,
                LineNumberColor = Color.DarkGray,
                IndentBackColor = Color.White,
                Font = new Font("Consolas", 11)
            };

            codeBox.ClearStylesBuffer();
            codeBox.Range.ClearStyle(StyleIndex.All);
            codeForm.Controls.Add(codeBox);
            codeForm.ShowDialog();
        }
    }
}
