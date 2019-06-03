﻿using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Hl7.Fhir.Serialization.Tests
{
    [TestClass]
    public class DeserializationTests
    {
        [TestMethod]
        public void RoundtripValueQuantityXml() => RoundtripValueQuantity(true);

        [TestMethod]
        public void RoundtripValueQuantityJson() => RoundtripValueQuantity(false);

        static void RoundtripValueQuantity(bool xml)
        {
            var resource = new StructureDefinition()
            {
                Differential = new StructureDefinition.DifferentialComponent()
                {
                    Element = new System.Collections.Generic.List<ElementDefinition>()
                    {
                        new ElementDefinition("Observation.valueQuantity")
                        {
                            Type = new System.Collections.Generic.List<ElementDefinition.TypeRefComponent>()
                            {
                                new ElementDefinition.TypeRefComponent()
                                {
                                    Code = FHIRAllTypes.Quantity.GetLiteral()
                                }
                            },
                            Example = new System.Collections.Generic.List<ElementDefinition.ExampleComponent>()
                            {
                                new ElementDefinition.ExampleComponent()
                                {
                                    Label = "Example Quantity",
                                    Value = new Quantity(42.0M, "kg", "http://ucom.org/example")
                                }
                            }
                        }
                    }
                }
            };

            var orgExample = resource.Differential.Element[0].Example[0];
            Assert.AreEqual("Quantity", orgExample.Value.TypeName);

            var parsed = xml ? XmlRoundTrip(resource) : JsonRoundTrip(resource);

            Assert.IsNotNull(parsed);
            Assert.IsNotNull(parsed.Differential?.Element);
            Assert.AreEqual(1, parsed.Differential.Element.Count);
            var examples = parsed.Differential.Element[0].Example;
            Assert.IsNotNull(examples);
            Assert.AreEqual(1, examples.Count);
            var example = examples[0];
            Assert.IsNotNull(example);
            Assert.AreEqual(orgExample.Label, example.Label);
            Assert.IsNotNull(example.Value);
            Assert.AreEqual("Quantity", example.Value.TypeName);
        }

        static T XmlRoundTrip<T>(T resource) where T : Resource
        {
            var baseTestPath = createEmptyDir();

            var xmlFile = Path.Combine(baseTestPath, "ObservationWithValueQuantityExample.xml");
            var xml = new FhirXmlSerializer().SerializeToString(resource);
            File.WriteAllText(xmlFile, xml);

            xml = File.ReadAllText(xmlFile);
            var parsed = new FhirXmlParser(new ParserSettings { PermissiveParsing = true }).Parse<T>(xml);

            return parsed;
        }

        static T JsonRoundTrip<T>(T resource) where T : Resource
        {
            var baseTestPath = createEmptyDir();

            var jsonFile = Path.Combine(baseTestPath, "ObservationWithValueQuantityExample.json");
            var json = new FhirJsonSerializer().SerializeToString(resource);
            File.WriteAllText(jsonFile, json);

            json = File.ReadAllText(jsonFile);
            var parsed = new FhirJsonParser(new ParserSettings { PermissiveParsing = true }).Parse<T>(json);

            return parsed;
        }

        static string createEmptyDir()
        {
            string baseTestPath = Path.Combine(Path.GetTempPath(), "FHIRRoundTripQuantity");
            if (Directory.Exists(baseTestPath)) Directory.Delete(baseTestPath, true);
            Directory.CreateDirectory(baseTestPath);
            return baseTestPath;
        }

    }
}
