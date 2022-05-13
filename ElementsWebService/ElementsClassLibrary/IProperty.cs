using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementsClassLibrary
{
    public interface IProperty
    {
        public string Id { get; set; }
        public string ElementSymbol { get; set; }
        public string Name { get; set; }        
       
        public decimal Value { get; set; }
        public string Unit { get; }
        public string? Comments { get; }
        public string? Reference { get; }

        public string GetFormattedValue();
    }
}
