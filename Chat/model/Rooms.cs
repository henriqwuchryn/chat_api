namespace Chat.model;

public class Room : BaseModel
{
    public string Name { get; set; }
    public virtual List<User> Users { get; set; }
    public virtual List<Message> Messages { get; set; }
}