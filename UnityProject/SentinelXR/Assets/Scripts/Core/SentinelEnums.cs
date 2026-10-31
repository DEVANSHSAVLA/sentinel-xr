namespace SentinelXR.Core
{
    public enum NodeStatus
    {
        Healthy,
        Suspicious,
        Critical,
        Isolated
    }

    public enum SeverityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum IncidentLifecycleStage
    {
        Detected,
        Investigating,
        AssessingRisk,
        ResponseRecommended,
        PendingApproval,
        Contained,
        Verifying,
        Resolved
    }

    public enum AttackScenarioType
    {
        DDoS,
        BruteForce,
        PortScan,
        LateralMovement
    }

    public enum ApplicationMode
    {
        DesktopSoc,
        MobileAR,
        ImmersiveVR
    }
}
