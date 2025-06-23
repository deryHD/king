using System.Text;
using System.Text.Json;
using SurveyApp.Models;

namespace SurveyApp.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiService> _logger;

        public OpenAiService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<string>> GenerateFollowUpQuestionsAsync(string ticketDescription, int npsScore)
        {
            try
            {
                var endpoint = _configuration["ConnectionStrings:OpenAi"];
                var apiKey = _configuration["ApiKeys:OpenAi"];

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenAI configuration is missing");
                }

                var prompt = CreateFollowUpQuestionsPrompt(ticketDescription, npsScore);
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "Du bist ein Experte für Kundenfeedback und NPS-Umfragen. Generiere präzise, relevante Folgefragen basierend auf der Ticketbeschreibung und dem NPS-Score." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);

                var questionsText = openAiResponse?.choices?.FirstOrDefault()?.message?.content ?? "";
                return ParseQuestions(questionsText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating follow-up questions");
                return GetFallbackQuestions(npsScore);
            }
        }

        public async Task<SentimentResult> AnalyzeSentimentAsync(string responseText)
        {
            try
            {
                var endpoint = _configuration["ConnectionStrings:OpenAi"];
                var apiKey = _configuration["ApiKeys:OpenAi"];

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenAI configuration is missing");
                }

                var prompt = CreateSentimentAnalysisPrompt(responseText);
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "Du bist ein Experte für Sentiment-Analyse. Analysiere den gegebenen Text und gib das Ergebnis im JSON-Format zurück: {\"label\": \"[Sehr zufrieden|Zufrieden|Neutral|Unzufrieden|Sehr unzufrieden]\", \"confidence\": [0.0-1.0]}" },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 100,
                    temperature = 0.1
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);

                var sentimentText = openAiResponse?.choices?.FirstOrDefault()?.message?.content ?? "";
                return ParseSentimentResult(sentimentText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sentiment");
                return new SentimentResult { Label = "Neutral", Confidence = 0.5 };
            }
        }

        private string CreateFollowUpQuestionsPrompt(string ticketDescription, int npsScore)
        {
            var scoreContext = npsScore switch
            {
                >= 9 => "sehr positiven",
                >= 7 => "positiven",
                >= 5 => "neutralen",
                _ => "negativen"
            };

            return $@"
Basierend auf dieser Ticketbeschreibung: ""{ticketDescription}""
Und dem {scoreContext} NPS-Score von {npsScore}/10:

Generiere genau 3 relevante Folgefragen, die sich auf:
1. Die Umsetzung/Qualität der Lösung
2. Die Kommunikation/den Prozess
3. Verbesserungsvorschläge oder positive Aspekte

Formatiere die Antwort als nummerierte Liste (1., 2., 3.).
Jede Frage sollte spezifisch auf das Ticket und den Score eingehen.
";
        }

        private string CreateSentimentAnalysisPrompt(string responseText)
        {
            return $@"
Analysiere das Sentiment dieser Antwort: ""{responseText}""

Bewerte die Stimmung und gib das Ergebnis im folgenden JSON-Format zurück:
{{""label"": ""[Sehr zufrieden|Zufrieden|Neutral|Unzufrieden|Sehr unzufrieden]"", ""confidence"": [0.0-1.0]}}
";
        }

        private List<string> ParseQuestions(string questionsText)
        {
            var questions = new List<string>();
            var lines = questionsText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("1.") || trimmed.StartsWith("2.") || trimmed.StartsWith("3."))
                {
                    var question = trimmed.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(question))
                    {
                        questions.Add(question);
                    }
                }
            }

            return questions.Count == 3 ? questions : GetFallbackQuestions(0);
        }

        private SentimentResult ParseSentimentResult(string sentimentText)
        {
            try
            {
                var cleanJson = sentimentText.Trim();
                if (cleanJson.StartsWith("```json"))
                {
                    cleanJson = cleanJson.Substring(7);
                }
                if (cleanJson.EndsWith("```"))
                {
                    cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
                }

                var result = JsonSerializer.Deserialize<SentimentResult>(cleanJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new SentimentResult { Label = "Neutral", Confidence = 0.5 };
            }
            catch
            {
                return new SentimentResult { Label = "Neutral", Confidence = 0.5 };
            }
        }

        private List<string> GetFallbackQuestions(int npsScore)
        {
            return npsScore >= 7
                ? new List<string>
                {
                    "Was hat Ihnen bei der Bearbeitung Ihres Tickets besonders gut gefallen?",
                    "Wie bewerten Sie die Kommunikation während des Bearbeitungsprozesses?",
                    "Welche Aspekte könnten wir noch weiter verbessern?"
                }
                : new List<string>
                {
                    "Was hätte bei der Bearbeitung Ihres Tickets besser laufen können?",
                    "Wie bewerten Sie die Kommunikation während des Bearbeitungsprozesses?",
                    "Was würden Sie sich für zukünftige Tickets wünschen?"
                };
        }
    }

    public class OpenAiResponse
    {
        public Choice[]? choices { get; set; }
    }

    public class Choice
    {
        public Message? message { get; set; }
    }

    public class Message
    {
        public string? content { get; set; }
    }
}