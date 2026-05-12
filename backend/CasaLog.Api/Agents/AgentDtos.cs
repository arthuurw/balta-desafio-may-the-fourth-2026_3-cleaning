namespace CasaLog.Api.Agents;

public record GenerateScheduleResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("reply")] string Reply,
    [property: JsonPropertyName("tasks")] AgentTaskItem[] Tasks
);

public record AgentTaskItem(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("scheduledDate")] string ScheduledDate,
    [property: JsonPropertyName("priority")] string Priority,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("equipmentType")] string? EquipmentType
);

public record EvaluateAlertResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("shouldAlert")] bool ShouldAlert,
    [property: JsonPropertyName("alertType")] string? AlertType,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("reply")] string Reply
);

public record RescheduleTaskResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("newDate")] string NewDate,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("reply")] string Reply
);

public record AnswerMaintenanceResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("reply")] string Reply
);

public record SeasonalTipsResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("tips")] SeasonalTip[] Tips,
    [property: JsonPropertyName("reply")] string Reply
);

public record SeasonalTip(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("tip")] string Tip
);

public record ChatResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("reply")] string Reply,
    [property: JsonPropertyName("tips")] SeasonalTip[]? Tips
);

public record SuggestEquipmentResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("suggestions")] EquipmentSuggestion[] Suggestions,
    [property: JsonPropertyName("reply")] string Reply
);

public record EquipmentSuggestion(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("reason")] string Reason
);

public record HomeReportResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("recommendations")] ReportRecommendation[] Recommendations,
    [property: JsonPropertyName("reply")] string Reply
);

public record ReportRecommendation(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("priority")] string Priority
);

public record NormalizeEquipmentResponse(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("normalizedType")] string? NormalizedType,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("reply")] string Reply
);
