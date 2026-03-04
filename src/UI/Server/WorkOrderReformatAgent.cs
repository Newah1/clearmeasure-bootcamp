using ClearMeasure.Bootcamp.Core.Model;
using ClearMeasure.Bootcamp.LlmGateway;
using Microsoft.Extensions.AI;

namespace ClearMeasure.Bootcamp.UI.Server;

/// <summary>
///     AI agent responsible for reformatting work order title and description fields
/// </summary>
public class WorkOrderReformatAgent(
    ChatClientFactory chatClientFactory,
    ILogger<WorkOrderReformatAgent> logger)
{
    /// <summary>
    ///     Reformats a work order's description for grammar and punctuation,
    ///     and ensures the title starts with a capital letter.
    ///     Returns the updated title and description, or null if no changes are needed.
    /// </summary>
    public async Task<ReformatResult?> ReformatWorkOrderAsync(WorkOrder workOrder)
    {
        try
        {
            var assigneePreferredLanguage = workOrder.Assignee?.PreferredLanguage ?? "en-US";
            var chatClient = await chatClientFactory.GetChatClient();

            var systemPrompt = $"""
                               You are an AI agent responsible for reformatting and translating work order fields.
                               You will receive a work order title and description.

                               Your tasks:
                               1. Translate the description into the assignee's preferred language, which in this case is {assigneePreferredLanguage}.
                               2. Correct the description for grammar and punctuation. Do not change the meaning.
                               3. Ensure the title starts with a capital letter. Do not change anything else about the title.

                               If no changes are needed, respond with exactly: NO_CHANGES

                               Otherwise respond in this exact format (two lines only):
                               TITLE: <corrected title>
                               DESCRIPTION: <corrected description>
                               """;

            var workOrderInfo = $"""
                                 Title: {workOrder.Title}
                                 Description: {workOrder.Description},
                                 PreferredLanguage: {assigneePreferredLanguage}
                                 """;

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, workOrderInfo)
            };

            var response = await chatClient.GetResponseAsync(messages);
            var responseText = response.Text?.Trim();

            if (string.IsNullOrWhiteSpace(responseText) ||
                responseText.Equals("NO_CHANGES", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("No reformatting needed for WorkOrder {WorkOrderNumber}",
                    workOrder.Number);
                return null;
            }

            var result = ParseResponse(responseText, workOrder);

            if (result != null)
            {
                logger.LogInformation(
                    "Reformatted WorkOrder {WorkOrderNumber}: Title changed={TitleChanged}, Description changed={DescriptionChanged}",
                    workOrder.Number,
                    result.Title != workOrder.Title,
                    result.Description != workOrder.Description);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reformatting WorkOrder {WorkOrderNumber}",
                workOrder.Number);
            return null;
        }
    }

    internal static ReformatResult? ParseResponse(string responseText, WorkOrder workOrder)
    {
        var lines = responseText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        string? title = null;
        string? description = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
            {
                title = line["TITLE:".Length..].Trim();
            }
            else if (line.StartsWith("DESCRIPTION:", StringComparison.OrdinalIgnoreCase))
            {
                description = line["DESCRIPTION:".Length..].Trim();
            }
        }

        title ??= workOrder.Title;
        description ??= workOrder.Description;

        if (string.Equals(title, workOrder.Title, StringComparison.OrdinalIgnoreCase) &&
            description == workOrder.Description)
        {
            return null;
        }

        return new ReformatResult(title!, description!);
    }
}

public record ReformatResult(string Title, string Description);
