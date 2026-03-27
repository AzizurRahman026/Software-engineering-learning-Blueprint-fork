using Application.Common.Interfaces.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Domain.Enums;

namespace Application.Tools;

[McpServerToolType]
public class TutorialTools
{
    private readonly ILlmFactory _llmFactory;

    public TutorialTools(ILlmFactory llmFactory)
    {
        _llmFactory = llmFactory;
    }

    [McpServerTool]
    [Description("Generate a professional tutorial blog from beginner to expert level. Use when user asks to learn or explain any topic.")]
    public async Task<string> GenerateTutorialBlog(
        [Description("Topic or subject name (e.g., Redis, ASP.NET Core, Angular Signals)")]
        string topic,
        [Description("Target level: 10-year-old, beginner, intermediate, advanced, professional")]
        string level,
        [Description("Optional technology context (e.g., ASP.NET Core, Angular)")]
        string? techContext = null
    )
    {
        var prompt = BuildPrompt(topic, level, techContext);

        var llmChatClient = _llmFactory.Create(LlmProvider.Gemini);

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "Never announce actions. Return only the final result immediately."),
            new ChatMessage(ChatRole.User, prompt)
        };
        try
        {
            var response = await llmChatClient.GetResponseAsync(messages);
            return response.Text;
        }
        catch (Exception ex)
        {
            return "Failed to generate blog content.";
        }
    }

    private string BuildPrompt(string topic, string level, string? techContext)
    {
        return $@"
You are a world-class technical educator and content writer.
Create a PROFESSIONAL BLOG POST.
---
Topic: {topic}
Level: {level}
Tech Context: {techContext ?? "General"}
---
Structure:
1. Title (SEO optimized)
2. Introduction (why important + simple analogy)
3. Explanation based on level
4. Step-by-step guide
5. Code examples (if applicable)
6. Real-world use cases
7. Common mistakes
8. Best practices
9. Summary
---
Rules:
- Markdown format
- Clear and structured
- Short paragraphs
- Blog-ready (Medium/Dev.to)
Return ONLY blog content.
";
    }
}