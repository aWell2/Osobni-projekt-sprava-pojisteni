using Microsoft.AspNetCore.Identity;

namespace Projekt.Resources
{
	public class CustomIdentityErrorDescriber : IdentityErrorDescriber
	{
		public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Heslo musí mít alespoň jeden nealfanumerický znak." }; }
		public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Heslo musí mít alespoň jedno číslo ('0'-'9')." }; }
		public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Heslo musí mít alespoň jedno malé písmeno ('a'-'z')." }; }
		public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Heslo musí mít alespoň jedno velké písmeno ('A'-'Z')." }; }
	}
}
