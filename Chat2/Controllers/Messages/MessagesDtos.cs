namespace Chat2.Controllers;


public class MessageDto
{
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDetailsDto Author { get; set; }
    public RoomDetailsDto Room { get; set; }
}

public class CreateMessageDto
{
    public string Body { get; set; }
}

public class MessageDetailsDto
{
    public string Body { get; set; }
    public string AuthorName { get; set; }
    public string RoomName { get; set; }
}

public class EditMessageDto
{
    public string Id { get; set; }
    public string NewBody { get; set; }
}