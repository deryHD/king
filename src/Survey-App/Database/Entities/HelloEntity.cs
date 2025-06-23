using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Database.Entities
{
	[Table("TestTable")]
	public class HelloEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required, StringLength(100)]
		public string TestString { get; set; }
	}
}