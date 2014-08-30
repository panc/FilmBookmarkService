using System;
using System.Configuration;
using System.Linq;

namespace FilmBookmarkService.Core
{
    public class UserStore :IDisposable
    {
        private readonly Lazy<User[]> _users; 

        public UserStore()
        {
            _users = new Lazy<User[]>(_ParseConfigFile);
        }

        public User[] Users
        {
            get { return _users.Value; }
        }

        public static UserStore Create()
        {
            return new UserStore();
        }

        public void Dispose()
        {    
        }

        private User[] _ParseConfigFile()
        {
            return UserConfigurationSection.UserConfigurations.Cast<User>().ToArray();
        }
    }


    public class UserConfigurationSection : ConfigurationSection
    {
        private static readonly UserConfigurationSection _instance = 
            ConfigurationManager.GetSection("usersSection") as UserConfigurationSection;

        public static UserConfigurationCollection UserConfigurations
        {
            get { return _instance.Users; }
        }

        [ConfigurationProperty("users")]
        public UserConfigurationCollection Users
        {
            get { return this["users"] as UserConfigurationCollection; }
        }
    }

    public class UserConfigurationCollection : ConfigurationElementCollection
    {
        public User this[object key]
        {
            get { return BaseGet(key) as User; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "user"; }
        }

        protected override bool IsElementName(string elementName)
        {
            return !string.IsNullOrEmpty(elementName) && elementName.Equals("user");
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new User();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((User)element).UserName;
        }
    }
}