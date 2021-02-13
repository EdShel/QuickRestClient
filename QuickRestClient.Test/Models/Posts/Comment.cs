using System;

namespace QuickRestClient.Test.Models.Posts
{
    public class Comment
    {
        public int PostId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Body { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Comment comment &&
                   PostId == comment.PostId &&
                   Id == comment.Id &&
                   Name == comment.Name &&
                   Email == comment.Email &&
                   Body == comment.Body;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PostId, Id, Name, Email, Body);
        }
    }
}
