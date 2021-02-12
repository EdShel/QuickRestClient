using NUnit.Framework;
using QuickRestClient.Test.Models;
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
    }

    //public interface IUsersService
    //{
    //    [Endpoint("users")]
    //    public string GetUsersString();

    //    [Endpoint("users")]
    //    public HttpResponseMessage GetUsersResponse();

    //    [Endpoint("users")]
    //    public IEnumerable<User> GetUsers();

    //    [Endpoint("users/{id}")]
    //    public User GetUser(int id);

    //    [Endpoint("users/{id}")]
    //    public Task<User> GetUserAsync(int id);

    //    [Endpoint("users/{id}")]
    //    public void DeleteUser(int id);

    //    [Endpoint("users")]
    //    public void CreateUser(User user);
    //}
}
