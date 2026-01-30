using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Win32;
using UglyToad.PdfPig;

namespace WpfApp1
{
    /// <summary>
    /// Main window of the WPF application.
    /// Handles PDF upload, text extraction, and chat interaction with Gemini AI.
    /// </summary>
    public partial class MainWindow : Window
    {
        // API key for accessing Google Gemini
        // ⚠️ In production, do NOT hardcode this. Use environment variables or secure storage.
        string apikey = "Your_API_KEY";

        // Observable collection bound to the UI to display chat messages
        public ObservableCollection<string> Messages { get; } = new();

        // Stores the full conversation history (user + AI)
        // This is sent on every request to preserve context
        private List<Content> _chatHistory = new List<Content>();

        // Holds extracted text from the uploaded PDF
        private string _pdfText = "";

        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext for data binding (Messages collection)
            DataContext = this;
        }

        /// <summary>
        /// Handles PDF upload button click.
        /// Opens file dialog, extracts text from PDF, and stores it for later AI queries.
        /// </summary>
        private async void UploadPdf_Click(object sender, RoutedEventArgs e)
        {
            // Configure file picker to allow only PDF files
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                Messages.Add("⏳ Processing PDF...");

                try
                {
                    // Run PDF processing on a background thread
                    var result = await Task.Run(() =>
                    {
                        StringBuilder sb = new StringBuilder();
                        int pageCount = 0;
                        int textCharsFound = 0;

                        // Open and read the PDF file
                        using (var document = PdfDocument.Open(filePath))
                        {
                            pageCount = document.NumberOfPages;

                            // Extract text from each page
                            foreach (var page in document.GetPages())
                            {
                                string pageText = page.Text;
                                textCharsFound += pageText.Length;
                                sb.AppendLine(pageText);
                            }
                        }

                        // Return extracted information
                        return new
                        {
                            Text = sb.ToString(),
                            Pages = pageCount,
                            CharCount = textCharsFound
                        };
                    });

                    // If no text was extracted, likely a scanned or encrypted PDF
                    if (result.CharCount == 0)
                    {
                        Messages.Add("⚠️ Warning: No text found. This PDF might be a scanned image or encrypted.");
                    }
                    else
                    {
                        _pdfText = result.Text;
                        Messages.Add($"✅ Processed {result.Pages} pages ({result.CharCount} characters found).");
                    }
                }
                catch (Exception ex)
                {
                    // Handle any PDF processing errors
                    Messages.Add($"❌ Error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sends a user question along with PDF context to Gemini AI.
        /// Maintains conversation history for contextual responses.
        /// </summary>
        private async Task<string> AskGeminiAsync(string userQuestion, string pdfContext)
        {
            // Create Gemini API client
            var client = new Client(apiKey: apikey);

            // If this is the first message, include the full PDF content
            if (_chatHistory.Count == 0)
            {
                string distinctPrompt = $"""
                Use the following PDF content to answer questions.

                PDF CONTENT:
                {pdfContext}

                QUESTION:
                {userQuestion}
                """;

                _chatHistory.Add(new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        new Part { Text = distinctPrompt }
                    }
                });
            }
            else
            {
                // For follow-up questions, send only the question
                _chatHistory.Add(new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        new Part { Text = userQuestion }
                    }
                });
            }

            // Call Gemini model with full conversation history
            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                contents: _chatHistory
            );

            // Extract AI-generated text response
            string responseText = response.Candidates[0].Content.Parts[0].Text;

            // Store AI response in history for future context
            _chatHistory.Add(new Content
            {
                Role = "model",
                Parts = new List<Part>
                {
                    new Part { Text = responseText }
                }
            });

            return responseText;
        }

        /// <summary>
        /// Handles Send button click.
        /// Sends user input to Gemini and displays AI response.
        /// </summary>
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            string userText = UserInput.Text;

            // Prevent sending empty messages
            if (string.IsNullOrWhiteSpace(userText))
                return;

            UserInput.Clear();
            Messages.Add("🧑 You: " + userText);

            // Ensure PDF has been uploaded before querying AI
            if (string.IsNullOrEmpty(_pdfText))
            {
                Messages.Add("⚠️ Please upload a PDF first.");
                return;
            }

            try
            {
                // Ask Gemini using the PDF context
                string aiReply = await AskGeminiAsync(userText, _pdfText);
                Messages.Add("🤖 AI: " + aiReply);
            }
            catch (Exception ex)
            {
                // Handle API or runtime errors
                Messages.Add("❌ Error: " + ex.Message);
            }
        }
    }
}
