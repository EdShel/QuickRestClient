using NUnit.Framework;
using QuickRestClient.Test.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace QuickRestClient.Test
{
    [TestFixture]
    public class ReturnTypesTest
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

        [Test]
        public void GetUsers_ReturnVoid()
        {
            var client = clientsFactory.CreateClient<IUsersService_ReturnVoid>();
            client.GetUsers();
        }

        public interface IUsersService_ReturnVoid
        {
            [Endpoint("users")]
            public void GetUsers();
        }

        [Test]
        public void GetUsers_HttpMessage()
        {
            var client = clientsFactory.CreateClient<IUsersService_HttpMessage>();
            var response = client.GetUsers();
            Assert.NotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        public interface IUsersService_HttpMessage
        {
            [Endpoint("users")]
            public HttpResponseMessage GetUsers();
        }

        [Test]
        public void GetUsers_ParsedUsers()
        {
            var client = clientsFactory.CreateClient<IUsersService_ParsedUsers>();
            var users = client.GetUsers();
            Assert.NotNull(users);
            Assert.AreEqual(10, users.Count());
            Assert.AreEqual("Bret", users.First().UserName);
            Assert.AreEqual("Robel-Corkery", users.First(u => u.Id == 4).Company.Name);
        }

        public interface IUsersService_ParsedUsers
        {
            [Endpoint("users")]
            public IEnumerable<User> GetUsers();
        }

        [Test]
        public void GetUsers_WrongReturnType()
        {
            var client = clientsFactory.CreateClient<IUsersService_WrongReturnType>();
            Assert.Throws<InvalidOperationException>(() => client.GetUsers());
        }

        public interface IUsersService_WrongReturnType
        {
            [Endpoint("users")]
            public User GetUsers();
        }
    }
}
