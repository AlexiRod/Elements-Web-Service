using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementsClassLibrary
{
    public class Unit
    {
        private string unit1, unit2;
        [JsonProperty]
        public string Unit1 { get => unit1; private set => unit1 = value.Trim(); }
        [JsonProperty]
        public string Unit2 { get => unit2; private set => unit2 = value.Trim(); }
        [JsonProperty]
        public string Equation { get; private set; }


        /// <summary>
        /// Перестановка единиц измерения.
        /// </summary>
        public void ChangeUnits() 
        {
            string temp = Unit1;
            Unit1 = Unit2;
            Unit2 = temp;
        }

        // From Uint1 to Unit2
        /// <summary>
        /// Конвертация значения value из ЕИ1 в ЕИ2 по заданному условию.
        /// </summary>
        /// <param name="value">Значение для конвертации.</param>
        /// <returns>Значение после конвертации.</returns>
        public decimal Convert(decimal value) {
            decimal[] vals = Equation.Replace('.', ',').Split(new char[] { '*', '/', '+', '-' })
                .Where(x => decimal.TryParse(x, out decimal res)).Select(decimal.Parse).ToArray();

            int i = 0;
            foreach (var symb in Equation)
                switch (symb) 
                {
                    case '+':
                        value += vals[i++];
                        break;
                    case '-':
                        value -= vals[i++];
                        break;
                    case '*':
                        value *= vals[i++];
                        break;
                    case '/':
                        value /= vals[i++];
                        break;
                }
            return value;
        }
    }
}
