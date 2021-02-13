using NUnit.Framework;
using QuickRestClient.Test.Models.Posts;
using QuickRestClient.Test.Models.Users;
using System;
using System.Net.Http;

namespace QuickRestClient.Test
{
    [TestFixture]
    public class ParametersTest
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
        public void GetUser_IntParam()
        {
            var client = clientsFactory.CreateClient<IUserService_IntParam>();
            const int id = 1;
            User user = client.GetUser(id);
            Assert.NotNull(user);
            Assert.AreEqual("Bret", user.UserName);
        }

        public interface IUserService_IntParam
        {
            [Endpoint("users/{id}")]
            public User GetUser(int id);
        }

        [Test]
        public void GetUser_StringParam()
        {
            var client = clientsFactory.CreateClient<IUserService_StringParam>();
            const string id = "1";
            User user = client.GetUser(id);
            Assert.NotNull(user);
            Assert.AreEqual("Bret", user.UserName);
        }

        public interface IUserService_StringParam
        {
            [Endpoint("users/{id}")]
            public User GetUser(string id);
        }

        [Test]
        public void GetUser_ByIdButNoIdGiven()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var client = clientsFactory.CreateClient<IUserService_ByIdButNoIdGiven>();
            });
        }

        public interface IUserService_ByIdButNoIdGiven
        {
            [Endpoint("users/{id}")]
            public User GetUser();
        }

        [Test]
        public void MakeCommentForPost()
        {
            var client = clientsFactory.CreateClient<ICommentService_NamedParams>();
            var newComment = new CommentCreate
            {
                PostId = 1,
                Body = "foo",
                Name = "bar",
                Email = "baz@mail.com"
            };
            var createdComment = client.LeaveComment(1, newComment);
            Assert.NotNull(createdComment);
            Assert.AreEqual(newComment.PostId, createdComment.PostId);
            Assert.AreEqual(newComment.Body, createdComment.Body);
            Assert.AreEqual(newComment.Name, createdComment.Name);
            Assert.AreEqual(newComment.Email, createdComment.Email);
        }

        public interface ICommentService_NamedParams
        {
            [Endpoint("posts/{postId}/comments", HttpMethod = EndpointHttpMethod.Post)]
            public Comment LeaveComment(int postId, CommentCreate newComment);
        }

        [Test]
        public void MakeCommentForPost_CheckNamedParamsInvariance()
        {
            var client = clientsFactory.CreateClient<ICommentService_CheckNamedParamsInvariance>();
            var newComment = new CommentCreate
            {
                PostId = 1,
                Body = "foo",
                Name = "bar",
                Email = "baz@mail.com"
            };
            var result1 = client.LeaveCommentDirectOrderOfParams(1, newComment);
            var result2 = client.LeaveCommentInversedOrderOfParams(newComment, 1);
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.AreEqual(result1, result2);
        }

        public interface ICommentService_CheckNamedParamsInvariance
        {
            [Endpoint("posts/{postId}/comments", HttpMethod = EndpointHttpMethod.Post)]
            public Comment LeaveCommentDirectOrderOfParams(int postId, CommentCreate newComment);

            [Endpoint("posts/{postId}/comments", HttpMethod = EndpointHttpMethod.Post)]
            public Comment LeaveCommentInversedOrderOfParams(CommentCreate newComment, int postId);
        }

        [Test]
        public void MakeCommentForPost_UnnamedParams()
        {
            var client = clientsFactory.CreateClient<ICommentService_UnnamedParams>();
            var newComment = new CommentCreate
            {
                PostId = 1,
                Body = "foo",
                Name = "bar",
                Email = "baz@mail.com"
            };
            var createdComment = client.LeaveComment(1, newComment);
            Assert.NotNull(createdComment);
            Assert.AreEqual(newComment.PostId, createdComment.PostId);
            Assert.AreEqual(newComment.Body, createdComment.Body);
            Assert.AreEqual(newComment.Name, createdComment.Name);
            Assert.AreEqual(newComment.Email, createdComment.Email);
        }

        public interface ICommentService_UnnamedParams
        {
            [Endpoint("posts/{postId}/comments", HttpMethod = EndpointHttpMethod.Post)]
            public Comment LeaveComment(int justRandomlyNamedParameter, CommentCreate newComment);
        }

        [Test]
        public void MakeCommentForPost_TooManyParams()
        {
            Assert.Throws<InvalidOperationException>(
                () => clientsFactory.CreateClient<ICommentService_TooManyParams>());
        }

        public interface ICommentService_TooManyParams
        {
            [Endpoint("posts/{postId}/comments", HttpMethod = EndpointHttpMethod.Post)]
            public Comment LeaveComment(int postId, CommentCreate newComment, int wtfParam);
        }

        [Test]
        public void MakeCommentForPost_NotEnoughParams()
        {
            Assert.Throws<InvalidOperationException>(
                () => clientsFactory.CreateClient<ICommentService_NotEnoughParams>());
        }

        public interface ICommentService_NotEnoughParams
        {
            [Endpoint("posts/{postId}/comments", HttpMethod = EndpointHttpMethod.Post)]
            public Comment LeaveComment();
        }

        [Test]
        public void MakeCommentForPost_ValueTypeContentParam()
        {
            Assert.Throws<InvalidOperationException>(
                () => clientsFactory.CreateClient<ICommentService_ValueTypeContentParam>());
        }

        public interface ICommentService_ValueTypeContentParam
        {
            [Endpoint("posts/{postId}/comments", HttpMethod = EndpointHttpMethod.Post)]
            public Comment LeaveComment(int postId, int newComment);
        }
    }
}
