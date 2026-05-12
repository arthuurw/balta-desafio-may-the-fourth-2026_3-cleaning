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
