using System;
namespace KinMai.Logic.Models
{
	public class GetUserProfileModel
	{
		public Guid UserId { get; set; }
		public string Username { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public bool IsLoginWithGoogle { get; set; }
	}

	public class UpdateUserProfileModel
	{
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}