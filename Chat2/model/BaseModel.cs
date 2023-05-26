namespace Chat2.model;

public class BaseModel
{
    public BaseModel()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; set; }
}