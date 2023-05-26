namespace Chat.model;

public class Message : BaseModel
{
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Edited { get; set; }
    public int AuthorId { get; set; }
    public virtual User Author { get; set; }
    public int RoomId { get; set; }
    public virtual Room Room { get; set; }
}