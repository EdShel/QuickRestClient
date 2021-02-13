namespace QuickRestClient.Test.Models.Posts
{
    public class CommentCreate
    {
        public int PostId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Body { get; set; }
    }
}
