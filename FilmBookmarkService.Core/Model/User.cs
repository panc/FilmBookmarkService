using System.Configuration;

namespace FilmBookmarkService.Core
{
    public class User : ConfigurationElement
    {
        [ConfigurationProperty("username", IsRequired = true, IsKey = true)]
        public string UserName
        {
            get { return this["username"] as string; }
            set { this["username"] = value; }
        }
        
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return this["password"] as string; }
            set { this["password"] = value; }
        }
    }
}