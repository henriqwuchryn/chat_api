﻿namespace Chat2.model;

public class Room : BaseModel
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public virtual List<User> Users { get; set; }
    public virtual List<Message> Messages { get; set; }
    public virtual string AuthorId { get; set; }
}