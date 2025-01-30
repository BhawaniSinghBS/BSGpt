using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BSGpt.Components.Pages
{
    public partial class Home
    {
        private string UserInput { get; set; } = string.Empty;
        private List<ChatMessage> Messages { get; set; } = new();

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
                return;

            // Add user message to chat
            Messages.Add(new ChatMessage(UserInput, true));

            try
            {
                // Hugging Face API URL (You can change this to another model as needed)
                var apiUrl = "https://api-inference.huggingface.co/models/Qwen/Qwen2.5-72B-Instruct"; // Replace with the appropriate model

                // Your Hugging Face API Key
                var apiKey = "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"; // Replace with your actual API key

                // Create HTTP request to Hugging Face API
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(new
                {
                    inputs = UserInput
                }), Encoding.UTF8, "application/json");

                // Send request and get the response
                var response = await Http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Read and process the response
                var aiResponse = await response.Content.ReadFromJsonAsync<List<HuggingFaceResponse>>();

                // Hugging Face API might return a list of choices, so we take the first one
                var aiMessage = aiResponse?.FirstOrDefault()?.generated_text;
                aiMessage = aiMessage.Replace("**", "\r\n");
                if (!string.IsNullOrWhiteSpace(aiMessage))
                {
                    // Add AI response to chat
                    Messages.Add(new ChatMessage(aiMessage, false));
                }
                else
                {
                    Messages.Add(new ChatMessage("Error: No response from the AI.", false));
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Messages.Add(new ChatMessage($"Error: {ex.Message}", false));
            }

            // Clear input field
            UserInput = string.Empty;
        }

        private void HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                SendMessage();
            }
        }

        // ChatMessage class
        public class ChatMessage
        {
            public string Text { get; set; }
            public bool IsUser { get; set; }

            public ChatMessage(string text, bool isUser)
            {
                Text = text;
                IsUser = isUser;
            }
        }

        // HuggingFace response structure
        public class HuggingFaceResponse
        {
            public string generated_text { get; set; }
        }
    }
}
