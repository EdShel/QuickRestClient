namespace QuickRestClient.Test.Models
{
    public class Post
    {
        public int UserId { get; set; }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }
    }

    public class PostCreate
    {
        public int UserId { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }
    }
}
