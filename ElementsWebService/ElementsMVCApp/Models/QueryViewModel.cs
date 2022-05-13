using System.ComponentModel.DataAnnotations;

namespace ElementsMVCApp.Models
{
    public class QueryViewModel
    {
        public string PropId { get; set; }
        public string Property { get; set; }

        [RegularExpression(@"^-?\d+([,]\d{1,})?$",
             ErrorMessage = "Left border should be a numver separated by comma")]
        public decimal Left { get; set; }

        [RegularExpression(@"-?\d+([,]\d{1,})?",
             ErrorMessage = "Right border should be a numver separated by comma")]
        [Range(int.MinValue, int.MaxValue, ErrorMessage = "")]
        public decimal Right { get; set; }

        public QueryViewModel(string propId, string prop)
        {
            Property = prop;
            PropId = propId;
            Left = decimal.MinValue;
            Right = decimal.MaxValue;
        }
        public QueryViewModel()
        {
            Left = decimal.MinValue;
            Right = decimal.MaxValue;
        }
    }
}
