using QuickRestClient.Test.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuickRestClient.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var client = new HttpClient();
            const string host = "https://jsonplaceholder.typicode.com/";
            RestClientsFactory clientsFactory = new RestClientsFactory(client, new Uri(host));
            IUsersService usersService = clientsFactory.CreateClient<IUsersService>();
            string response = usersService.GetUsersString();
            Console.Write(response);
            //var users = usersService.GetUsers();
            
            //foreach(var user in users)
            //{
            //    Console.WriteLine(user.UserName);
            //}
        }
    }

    public interface IUsersService
    {
        [Endpoint("users")]
        public string GetUsersString();

        [Endpoint("users")]
        public HttpResponseMessage GetUsersResponse();

        [Endpoint("users")]
        public IEnumerable<User> GetUsers();

        [Endpoint("users/{id}")]
        public User GetUser(int id);

        [Endpoint("users/{id}")]
        public Task<User> GetUserAsync(int id);

        [Endpoint("users/{id}")]
        public void DeleteUser(int id);

        [Endpoint("users")]
        public void CreateUser(User user);
    }
}
