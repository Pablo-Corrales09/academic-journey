using System.Reflection.Metadata.Ecma335;

namespace Smart_Gym.Models
{
    public class UserRolesViewModel
    {
        public string UserId  { get; set; }

        public string UserName { get; set; }

        public string RoleName { get; set; }

        public IEnumerable<string> Roles{ get; set; } = new List<string>();
    }
}
