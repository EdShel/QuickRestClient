using NUnit.Framework;
using System;
using System.Net.Http;

namespace QuickRestClient.Test
{
    [TestFixture]
    public class UserEndpointsTest
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
        public void GetUsers_NoEndpointAttributeException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                clientsFactory.CreateClient<IUsersService_NoEndpointAttributeException>());
        }

        interface IUsersService_NoEndpointAttributeException
        {
            public string GetUsersString();
        }

        [Test]
        public void GetUsers_RawJson()
        {
            var client = clientsFactory.CreateClient<IUsersService_RawJson>();
            var response = client.GetUsersString();
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
        }

        public interface IUsersService_RawJson
        {
            [Endpoint("users")]
            public string GetUsersString();
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
