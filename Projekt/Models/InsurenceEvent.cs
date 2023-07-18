using System.ComponentModel.DataAnnotations;

namespace Projekt.Models
{
	public class InsurenceEvent
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Popis události")]
		public string Description { get; set; } = string.Empty;
		[Display(Name = "Čás kdy událost proběhla")]
		public DateTime TimeOfEvent { get; set; } = DateTime.Today;
		[Required(ErrorMessage = "Pole musí být vyplněno")]
		[Display(Name = "Místo kde událost proběhla")]
		[StringLength(100)]
		public string PlaceOfEvent { get; set; } = string.Empty;

		[Display(Name = "Pojištění")]
		public int InsurenceId { get; set; }
		[Display(Name = "Pojištění")]
		public virtual Insurence? Insurence { get; set; }

	}
}
