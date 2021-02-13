using NUnit.Framework;
using QuickRestClient.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace QuickRestClient.Test
{
    [TestFixture]
    public class HttpMethodsTest
    {
        private RestClientsFactory clientsFactory;

        [OneTimeSetUp]
        public void SetUp()
        {
            var client = new HttpClient();
            const string host = "https://jsonplaceholder.typicode.com/";
            client.BaseAddress = new Uri(host);
            this.clientsFactory = new RestClientsFactory(client);
        }

        [Test]
        public void GetPost_GetHttpMethod()
        {
            var client = clientsFactory.CreateClient<IPostService_GetHttpMethod>();
            var posts = client.GetAllPosts();
            Assert.NotNull(posts);
            Assert.AreEqual(100, posts.Count());
            Assert.AreEqual(3, posts.ElementAt(2).Id);
        }

        public interface IPostService_GetHttpMethod
        {
            [Endpoint("posts", HttpMethod = EndpointHttpMethod.Get)]
            public IEnumerable<Post> GetAllPosts();
        }

        [Test]
        public void CreatePost_PostHttpMethod()
        {
            var client = clientsFactory.CreateClient<IPostService_PostHttpMethod>();
            var newPost = new PostCreate
            {
                Title = "foo",
                UserId = 1,
                Body = "bar"
            };
            var result = client.CreatePost(newPost);
            Assert.NotNull(result);
            Assert.AreEqual(newPost.Title, result.Title);
            Assert.AreEqual(newPost.UserId, result.UserId);
            Assert.AreEqual(newPost.Body, result.Body);
        }

        public interface IPostService_PostHttpMethod
        {
            [Endpoint("posts", HttpMethod = EndpointHttpMethod.Post)]
            public Post CreatePost(PostCreate newPost);
        }
    }
}
