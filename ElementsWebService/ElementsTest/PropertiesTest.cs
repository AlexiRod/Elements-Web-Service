using ElementsClassLibrary;
using ElementsWebAPI.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ElementsTest
{
    [TestClass]
    public class PropertiesTest
    {
        private static readonly PropertyRepository rep = new PropertyRepository(
            new DapperContext("server=.; database=phases; Integrated Security=true; TrustServerCertificate=True"));
        
       
        [TestMethod]
        public void TestGetLiteratureReference()
        {
            string exp = @"Bakshi B. Berndt M. Brandenburg K. Chen P. Igelnik B. Iwata S. Jackson A. LeClair S. Oxley M. Pao Y.-H. Villars P.";
            string res = rep.GetLiteratureReference("13823").Result;
            Assert.AreEqual(exp, res.Split('\n')[0]);
        }


        [TestMethod]
        public void TestGetUnitsAndPairs()
        {
            Tuple<Dictionary<string, Unit>, Dictionary<string, string>> res = rep.GetUnitsAndPairs().Result;
            Dictionary<string, Unit> units = res.Item1;

            Assert.AreEqual(units.Count, 7);
            Assert.AreEqual(units["(K)"].Unit1, "(K)");
            Assert.AreEqual(units["(K)"].Unit2, "(C)");
        }


        [TestMethod]
        public void TesGetAllElements()
        {
            List<Element> elems = rep.GetAllElements().Result.ToList(); 

            Assert.AreEqual(elems.Count, 100);
            Assert.AreEqual(elems[0].Symbol, "H ");
            Assert.AreEqual(elems[54].Symbol, "Cs");
        }


        [TestMethod]
        public void TestGetAllPropertiesNames()
        {
            List<Property> props = rep.GetAllPropertiesNames().Result.ToList();

            Assert.AreEqual(props.Count, 90);
            Assert.AreEqual(props[0].Id, "I26");
            Assert.AreEqual(props[54].Id, "M2");
        }


        [TestMethod]
        public void TestGetAllPropertiesOfElementById()
        {
            List<IProperty> props = rep.GetAllPropertiesOfElementById(1).Result.ToList();

            Assert.AreEqual(props.Count, 95);
            Assert.AreEqual(props[0].Value, 0);
            Assert.AreEqual(props[54].Value, 97);
        }


        [TestMethod]
        public void TestGetGivenPropertyOfElementByIds()
        {
            List<IProperty> props = rep.GetGivenPropertyOfElementByIds(1, "C2").Result.ToList();

            Assert.AreEqual(props.Count, 2);
            Assert.AreEqual(props[0].Value, 21.15m);
            Assert.AreEqual(props[1].Value, 20.28m);

            props = rep.GetGivenPropertyOfElementByIds(1, "S11").Result.ToList();

            Assert.AreEqual(props.Count, 2);
            Assert.AreEqual(props[0].Value, 1.36m);
        }


        [TestMethod]
        public void TestGetGivenPropertiesOfElementById()
        {
            List<IProperty> props = rep.GetGivenPropertiesOfElementById(1, new List<string> { "C1", "C2", "I25", "S14"}).Result.ToList();

            Assert.AreEqual(props.Count, 4);
            Assert.AreEqual(props[0].Value, 14);
            Assert.AreEqual(props[1].Value, 21.15m);
            Assert.AreEqual(props[2].Value, 20.28m);
            Assert.AreEqual(props[3].Value, 0);
        }


        [TestMethod]
        public void TestGetGivenPropertiesValues()
        {
            Dictionary<string, IEnumerable<IProperty>> props = 
                rep.GetGivenPropertiesValues(new List<string> { "C1", "S14" }, false).Result;

            Assert.AreEqual(props.Count, 2);
            Assert.AreEqual(props["C1"].Count(), 163);
            Assert.AreEqual(props["S14"].Count(), 167);
        }


        [TestMethod]
        public void TestGetGivenPropertiesValuesRec()
        {
            Dictionary<string, IEnumerable<IProperty>> props =
                rep.GetGivenPropertiesValues(new List<string> { "C1", "S14" }, true).Result;

            Assert.AreEqual(props.Count, 2);
            Assert.AreEqual(props["C1"].Count(), 100);
            Assert.AreEqual(props["S14"].Count(), 167);
        }
        

        [TestMethod]
        public void TestGetGivenPropertiesValuesQuery()
        {
            List<IProperty> props = rep.GetGivenPropertyValuesById("C2", true, -1000.31m, 2000.54m).Result.ToList();

            Assert.AreEqual(props.Count, 40);
            Assert.AreEqual(props[0].Value, 20.28m);
        }


        [TestMethod]
        public void TestGetGivenPropertiesValuesWithQuery()
        {
            Dictionary<string, IEnumerable<IProperty>> props =
                rep.GetGivenPropertiesValuesWithQuery(new List<string> { "C2", "S15" }, true,
                new List<decimal> { 10.2m, 0.4m }, new List<decimal> { 100, 1.5m }).Result;

            Assert.AreEqual(props.Keys.Count, 3);
            Assert.AreEqual(string.Join(" ", props.Keys), "N O F");
            Assert.AreEqual(props["N"].Count(), 2);
            Assert.AreEqual(props["O"].Count(), 6);
            Assert.AreEqual(props["F"].Count(), 5);
        }
    }
}