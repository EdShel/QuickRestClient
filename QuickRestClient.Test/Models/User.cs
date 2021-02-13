namespace QuickRestClient.Test.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UserName { get; set; }

        public Address Address { get; set; }

        public string Phone { get; set; }

        public string Website { get; set; }

        public Company Company { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string Suite { get; set; }

        public string City { get; set; }

        public string Zipcode { get; set; }

        public Geolocation Geo { get; set; }
    }

    public class Geolocation
    {
        public float Lat { get; set; }

        public float Lng { get; set; }
    }

    public class Company
    {
        public string Name { get; set; }

        public string CatchPhrase { get; set; }

        public string Bs { get; set; }
    }
}
