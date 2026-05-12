namespace CasaLog.Api.Agents;

public class AgentException(string message, Exception? inner = null)
    : Exception(message, inner);
