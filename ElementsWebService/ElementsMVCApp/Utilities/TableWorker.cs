using ElementsClassLibrary;

namespace ElementsMVCApp.Utilities
{
    public class TableWorker
    {
        // Ac Ku ?
        private static string[][] table = new string[][] {
            new string[] { "H", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "He" },
            new string[] { "Li","Be", "", "", "", "", "", "", "", "", "", "", "B", "C","N","O","F","Ne" },
            new string[] { "Na", "Mg", "", "", "", "", "", "", "", "", "", "", "Al","Si","P","S","Cl","Ar" },
            new string[] { "K","Ca","Sc","Ti","V","Cr","Mn","Fe","Co","Ni", "Cu","Zn","Ga","Ge","As","Se","Br","Kr" },
            new string[] { "Rb","Sr","Y","Zr","Nb","Mo","Tc","Ru","Rh","Pd", "Ag","Cd","In","Sn","Sb","Te","I","Xe" },
            new string[] { "Cs", "Ba", "", "Hf", "Ta", "W", "Re", "Os", "Ir", "Pt", "Au", "Hg", "Tl", "Pb", "Bi", "Po", "At", "Rn" },
            new string[] { "Fr", "Ra", "", "Rf", "Db", "Sg", "Bh", "Hs", "Mt", "Ds", "Rg", "Cn", "Uut", "Fl", "Uup", "Lv", "Uus", "Uuo"},
            new string[] { },
            new string[] { "", "", "", "La", "Ce","Pr","Nd","Pm","Sm","Eu","Gd","Tb","Dy","Ho","Er","Tm","Yb", "Lu" },
            new string[] { "", "", "", "Ac","Th","Pa","U","Np","Pu","Am","Cm","Bk","Cf","Es","Fm","Md","No","Lr" }
        };

        public static List<List<Element>> MakeTable(List<Element> list)
        {
            List<List<Element>> elements = new List<List<Element>>();
            for (int i = 0; i < table.Length; i++)
            {
                List<Element> row = new List<Element>();
                for (int j = 0; j < table[i].Length; j++)
                {
                    if (string.IsNullOrEmpty(table[i][j]))
                    {
                        row.Add(null);
                        continue;
                    }
                    var el = list.FirstOrDefault(x => x.Symbol.Trim() == table[i][j]);
                    if (el == null)
                        el = new Element() { Symbol = table[i][j] };
                    row.Add(el);
                }
                elements.Add(row);
            }

            return elements;
        }
    }
}
