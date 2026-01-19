namespace CustomAutoComplet.Data;

public class TimeEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeId { get; set; } = "";      // 员工唯一标识（可以是UserId、工号等）
    public string EmployeeName { get; set; } = "";    // 冗余方便显示
    public string ProjectCode { get; set; } = "";       // 项目编号或短名称
    public string ProjectName { get; set; } = "";
    public DateOnly WorkDate { get; set; }          // 工作日期（只关心日期，不关心时间）
    public decimal Hours { get; set; }              // 工时（支持小数，例如 3.5）
    public string? Remark { get; set; }             // 备注，可选
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class TrackedProject
{
    public string ProjectCode { get; set; } = "";
    public string ProjectName { get; set; } = "";
}