using System.ComponentModel.DataAnnotations;

namespace Projekt.Models
{
	public class Insurence
	{

		public int Id { get; set; }

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Pojištění")]
		[StringLength(50)]
		public string TypeOfInsurence { get; set; } = string.Empty;

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Částka")]
		public decimal Amount { get; set; }

		[Display(Name = "Platnost od")]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime ValidFrom { get; set; } = DateTime.Today;

		[Display(Name = "Platnost do")]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime ValidUntil { get; set; } = DateTime.Today.AddDays(1);

		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Předmět pojištění")]
		[StringLength(50)]
		public string SubjectOfInsurence { get; set; } = string.Empty;

		[Display(Name = "Pojištěnec")]
		public int InsurenceHolderId { get; set; }

		[Display(Name = "Pojištěnec")]
		public virtual InsurenceHolder? InsurenceHolder { get; set; }
		public virtual ICollection<InsurenceEvent>? InsurenceEvents { get; set; }

		[Display(Name = "Počet pojistných událostí")]
		public int InsurenceEventsCount { get; set; } = 0;


	}
}
