# PDF Chat Assistant (WPF + Gemini AI)

A WPF desktop application that lets users upload a PDF and chat with its contents using Google Gemini AI.

## ✨ Features

📂 Upload and process PDF files

📖 Extract text from multi-page PDFs

🤖 Ask questions about the PDF using Gemini AI

💬 Context-aware chat (conversation memory)

⚡ Async processing for a responsive UI

🧰 Tech Stack

.NET / WPF

Google Gemini AI SDK

PdfPig (PDF text extraction)

C# async/await

🧠 How It Works

Upload PDF

User selects a PDF file.

Text is extracted page-by-page using PdfPig.

Extracted text is stored in memory.

Ask Questions

User types a question in the chat.

On the first question, the full PDF text is sent to Gemini.

Follow-up questions reuse conversation history.

AI Response

Gemini generates a contextual answer.

Response is added to the chat window.

Conversation history is preserved for continuity.

🔑 API Key Setup

Replace the API key in MainWindow.xaml.cs:

string apikey = "Your_API_KEY";


⚠️ Do not hardcode API keys in production.
Use environment variables or secure configuration.

🚧 Limitations

No OCR support (scanned/image PDFs won’t work)

Large PDFs may exceed token limits

API key stored in code (demo only)

Chat history is session-based (not saved)

🚀 Possible Improvements

Add OCR for scanned PDFs

Chunk large PDFs for token safety

Implement MVVM architecture

Stream AI responses

Persist chat history

Secure API key handling
