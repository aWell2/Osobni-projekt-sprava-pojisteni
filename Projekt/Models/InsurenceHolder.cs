using System.ComponentModel.DataAnnotations;

namespace Projekt.Models
{
	public class InsurenceHolder
	{

		public int Id { get; set; }

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Křestní jméno")]
		[StringLength(50)]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Příjmení")]
		[StringLength(50)]
		public string LastName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Telefonní číslo")]
		[RegularExpression(@"^(?:\+\d{1,3}\s*)?\d{3}\s*\d{3}\s*\d{3}$", ErrorMessage = "Neplatné telefonní číslo")]
		public string PhoneNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Neplatná e-mailová adresa")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Ulice a číslo popisné")]
		[StringLength(100)]
		public string Street { get; set; } = string.Empty;

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Město ")]
		[StringLength(50)]
		public string City { get; set; } = string.Empty;

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Poštovní směrovací číslo")]
		[RegularExpression(@"^\d{3}\s?\d{2}$", ErrorMessage = "Neplatné poštovní směrovací číslo")]
		public string PostalCode { get; set; } = string.Empty;

		public virtual ICollection<Insurence>? Insurences { get; set; }

		[Display(Name = "Počet pojištění ")]
		public int InsurenceCount { get; set; } = 0;


	}
}
