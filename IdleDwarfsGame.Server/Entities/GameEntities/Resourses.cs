public class Resourses
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ResourseType ResourseType { get; set; }
}

public enum ResourseType
{
    Experience,
    Gold
}