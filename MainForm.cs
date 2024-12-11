using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace ITextDemo
{
    public partial class MainForm : Form
    {
        private const string ConnectionString = "Data Source=clients.db";
        private List<Client> clients;
        private SplitContainer docsSplitContainer;
        private ListView methodListView;
        private FastColoredTextBox codeBox;

        public MainForm()
        {
            InitializeComponent();
            ConfigureDataGridView();
            dataGridView1.CellMouseDoubleClick += DataGridView1_CellMouseDoubleClick;
            dataGridView1.ReadOnly = true;
            LoadClients();
            PopulateDocumentation();
            SetupDocumentationButton();
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void LoadClients()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                clients = connection.Query<Client>("SELECT Name, Address, AccountNumber, CAST(Balance as REAL) as Balance, EmailAddress, PhoneNumber, LastTransactionDate FROM Clients").ToList();
            }
            dataGridView1.DataSource = clients;
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            var selectedClients = clients;  // For now, using all clients. You can modify this to use selected rows
            var pdfPath = Program.GenerateClientReport(selectedClients);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            });
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadClients();
        }

        private void DataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Check if it's a valid row and the Name column (index 0)
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                var selectedClient = clients[e.RowIndex];
                var pdfPath = Program.GenerateClientReport(new List<Client> { selectedClient }, selectedClient.Name);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });
            }
        }

        private void PopulateDocumentation()
        {
            // Create a split container for docs and code
            docsSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            tabDocs.Controls.Add(docsSplitContainer);

            // Left panel for method list
            methodListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 10)
            };
            
            // Add columns - we'll resize them after form load
            methodListView.Columns.Add("Section/Method", 200);
            methodListView.Columns.Add("Description", 300);
            
            docsSplitContainer.Panel1.Controls.Add(methodListView);

            // Right panel for code
            codeBox = new FastColoredTextBox
            {
                Dock = DockStyle.Fill,
                Language = FastColoredTextBoxNS.Language.CSharp,
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = Color.Black,
                LineNumberColor = Color.DarkGray,
                IndentBackColor = Color.White,
                Font = new Font("Consolas", 10)
            };
            docsSplitContainer.Panel2.Controls.Add(codeBox);

            // Handle form load to set correct proportions
            this.Load += MainForm_Load;

            LoadDocumentationContent();
        }

        private void LoadDocumentationContent()
        {
            string programPath = Path.Combine(Application.StartupPath, "Program.cs");
            if (!File.Exists(programPath))
            {
                programPath = Path.Combine(Directory.GetCurrentDirectory(), "Program.cs");
            }

            if (File.Exists(programPath))
            {
                string programCs = File.ReadAllText(programPath);
                codeBox.Text = programCs;

                // Extract both function documentation and section comments
                var sectionMatches = Regex.Matches(programCs, @"/\*\s*\n\s*\*\s*(.*?)\s*\n\s*\*\s*-+\s*\n\s*(.*?)\s*\*/",
                    RegexOptions.Singleline);

                Dictionary<string, int> codePositions = new Dictionary<string, int>();

                foreach (Match match in sectionMatches)
                {
                    string sectionTitle = match.Groups[1].Value.Trim();
                    string description = match.Groups[2].Value
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim('*', ' '))
                        .First();
                    int position = match.Index;

                    var item = new ListViewItem(sectionTitle) { BackColor = Color.LightGray };
                    item.SubItems.Add(description);
                    methodListView.Items.Add(item);
                    codePositions[sectionTitle] = position;

                    // Look for subsections within this section
                    var subsectionMatches = Regex.Matches(match.Value, @"/\*\s*\n\s*\*\s*(.*?)\s*\n\s*\*\s*-+\s*\n\s*(.*?)\s*\*/",
                        RegexOptions.Singleline);

                    foreach (Match subsection in subsectionMatches)
                    {
                        if (subsection.Index > 0) // Skip if it's the section itself
                        {
                            string subsectionTitle = "  → " + subsection.Groups[1].Value.Trim();
                            string subsectionDesc = subsection.Groups[2].Value
                                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => line.Trim('*', ' '))
                                .First();
                            
                            var subItem = new ListViewItem(subsectionTitle);
                            subItem.SubItems.Add(subsectionDesc);
                            methodListView.Items.Add(subItem);
                            codePositions[subsectionTitle] = position + subsection.Index;
                        }
                    }
                }

                // Handle click events for navigation
                methodListView.ItemSelectionChanged += (sender, e) =>
                {
                    if (methodListView.SelectedItems.Count > 0)
                    {
                        string selectedItem = methodListView.SelectedItems[0].Text;
                        if (codePositions.ContainsKey(selectedItem))
                        {
                            int position = codePositions[selectedItem];
                            
                            // Find the end of the code block
                            int commentEnd = programCs.IndexOf("*/", position) + 2;
                            
                            // Look for the next comment block or the end of the file
                            int nextCommentStart = programCs.IndexOf("/*", commentEnd);
                            int nextSectionStart = nextCommentStart != -1 ? nextCommentStart : programCs.Length;
                            
                            // Look for potential earlier ending based on closing braces
                            int closingBrace = programCs.IndexOf("\n}", commentEnd);
                            if (closingBrace != -1 && closingBrace < nextSectionStart)
                            {
                                nextSectionStart = closingBrace + 2; // Include the brace and newline
                            }
                            
                            codeBox.Selection = new FastColoredTextBoxNS.Range(codeBox)
                            {
                                Start = codeBox.PositionToPlace(position),
                                End = codeBox.PositionToPlace(nextSectionStart)
                            };
                            codeBox.DoSelectionVisible();
                            codeBox.Focus();
                        }
                    }
                };
            }
            else
            {
                methodListView.Items.Add(new ListViewItem("Error: Could not find Program.cs file."));
            }
        }

        private void SetupDocumentationButton()
        {
            Button btnViewDocumentation = new Button
            {
                Location = new System.Drawing.Point(10, 10),
                Name = "btnViewDocumentation",
                Size = new System.Drawing.Size(150, 30),
                TabIndex = 2,
                Text = "View Documentation"
            };
            btnViewDocumentation.Click += btnViewDocumentation_Click;
            this.tabDocs.Controls.Add(btnViewDocumentation);
        }

        private void btnViewDocumentation_Click(object sender, EventArgs e)
        {
            var docViewer = new DocumentationViewer();
            docViewer.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set the form to full screen
            this.WindowState = FormWindowState.Maximized;

            // Set the splitter distance to 1/4 of the tab width
            if (docsSplitContainer != null)
            {
                docsSplitContainer.SplitterDistance = tabDocs.Width / 4;
            }

            // Load client data
            LoadClientData();
        }

        private void LoadClientData()
        {
            using (var connection = new SqliteConnection(Program.ConnectionString))
            {
                var clients = connection.Query<Client>("SELECT * FROM Clients ORDER BY Name").ToList();
                dataGridView1.DataSource = clients;
            }
        }

        private void btnHtmlReport_Click(object sender, EventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    FileName = $"ClientReport_HTML_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Program.GenerateHtmlReport(saveFileDialog.FileName);
                    if (MessageBox.Show("Report generated successfully! Would you like to open it?",
                        "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
