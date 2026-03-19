using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PromptEditor
{
    public partial class Form1 : Form
    {
        private readonly string _sourcePromptsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Prompts");
        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://your-api-base-url/AI/prompts"; // Replace with actual base URL

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("ApiKey", "your-api-key"); // Replace with actual API key
            LoadFilesAsync();
        }

        private void InitializeCustomComponents()
        {
            // File Selector Dropdown
            ComboBox fileSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 300,
                Top = 20,
                Left = 20
            };
            fileSelector.SelectedIndexChanged += FileSelector_SelectedIndexChanged;
            this.Controls.Add(fileSelector);

            // Text Editor
            TextBox contentEditor = new TextBox
            {
                Multiline = true,
                Width = 500,
                Height = 300,
                Top = 60,
                Left = 20,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(contentEditor);

            // Save Button
            Button saveButton = new Button
            {
                Text = "Save",
                Width = 100,
                Top = 380,
                Left = 20
            };
            saveButton.Click += (sender, e) => SaveFile(fileSelector, contentEditor);
            this.Controls.Add(saveButton);

            // Reload Button
            Button reloadButton = new Button
            {
                Text = "Reload",
                Width = 100,
                Top = 380,
                Left = 140
            };
            reloadButton.Click += (sender, e) => ReloadFile(fileSelector, contentEditor);
            this.Controls.Add(reloadButton);

            // Load files into the file selector
            LoadFiles(fileSelector);
        }

        private async Task LoadFilesAsync()
        {
            try
            {
                var files = await _httpClient.GetFromJsonAsync<List<string>>(_baseUri);
                if (files != null)
                {
                    comboBoxFiles.DataSource = files;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}");
            }
        }

        private void LoadFiles(ComboBox fileSelector)
        {
            try
            {
                Console.WriteLine($"Using Prompts directory: {_sourcePromptsDir}");

                if (Directory.Exists(_sourcePromptsDir))
                {
                    var files = Directory.GetFiles(_sourcePromptsDir, "*.txt").Concat(Directory.GetFiles(_sourcePromptsDir, "*.md"));
                    if (files.Any())
                    {
                        fileSelector.Items.AddRange(files.Select(Path.GetFileName).ToArray());
                    }
                    else
                    {
                        MessageBox.Show("No .txt or .md files found in the Prompts folder.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Prompts folder not found. Please ensure the folder exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadFileContentAsync(string fileName)
        {
            try
            {
                var content = await _httpClient.GetStringAsync($"{_baseUri}/{fileName}");
                textBoxContent.Text = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file content: {ex.Message}");
            }
        }

        private async Task SaveFileContentAsync(string fileName, string content)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUri}/{fileName}", content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("File saved successfully.");
                }
                else
                {
                    MessageBox.Show($"Error saving file: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}");
            }
        }

        private void FileSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox fileSelector = sender as ComboBox;
            if (fileSelector != null && fileSelector.SelectedItem != null)
            {
                string selectedFile = fileSelector.SelectedItem.ToString();
                string filePath = Path.Combine(_sourcePromptsDir, selectedFile);
                if (File.Exists(filePath))
                {
                    TextBox contentEditor = this.Controls.OfType<TextBox>().FirstOrDefault();
                    if (contentEditor != null)
                    {
                        contentEditor.Text = File.ReadAllText(filePath);
                    }
                }
            }
        }

        private void SaveFile(ComboBox fileSelector, TextBox contentEditor)
        {
            if (fileSelector.SelectedItem != null && !string.IsNullOrWhiteSpace(contentEditor.Text))
            {
                string selectedFile = fileSelector.SelectedItem.ToString();
                string filePath = Path.Combine(_sourcePromptsDir, selectedFile);
                File.WriteAllText(filePath, contentEditor.Text);
                MessageBox.Show("File saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ReloadFile(ComboBox fileSelector, TextBox contentEditor)
        {
            if (fileSelector.SelectedItem != null)
            {
                string selectedFile = fileSelector.SelectedItem.ToString();
                string filePath = Path.Combine(_sourcePromptsDir, selectedFile);
                if (File.Exists(filePath))
                {
                    contentEditor.Text = File.ReadAllText(filePath);
                    MessageBox.Show("File reloaded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
