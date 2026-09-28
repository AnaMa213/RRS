using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Garde de partition de la selection de validation (profils Fast / Geometry, voir
    /// scripts/validate.ps1 et docs/setup/build-workflow-rules.md) : chaque test EditMode doit
    /// porter au moins une categorie Core ou Geometry, posee sur sa fixture ou sur sa methode.
    /// Un test sans categorie serait exclu des DEUX profils partiels sans que rien ne le signale ;
    /// validate.ps1 echoue deja ferme dans ce cas, et cette garde le montre dans la suite
    /// elle-meme, y compris en profil Full. Poser une categorie est un acte conscient : une
    /// nouvelle fixture lourde reste en Core tant que personne ne la classe Geometry, et la
    /// premiere execution de Fast rend la garde rouge.
    /// </summary>
    [Category("Core")]
    public sealed class TestSuiteCategoryPartitionTests
    {
        private const string CoreCategory = "Core";
        private const string GeometryCategory = "Geometry";

        [Test]
        public void EveryEditModeTestCarriesCoreOrGeometry()
        {
            var uncovered = new List<string>();
            var assembly = typeof(TestSuiteCategoryPartitionTests).Assembly;

            foreach (var type in assembly.GetTypes())
            {
                var fixtureCategories = Categories(type.GetCustomAttributes(true));
                foreach (var method in type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (!IsTest(method))
                    {
                        continue;
                    }

                    var methodCategories = Categories(method.GetCustomAttributes(true));
                    bool covered =
                        fixtureCategories.Contains(CoreCategory) || fixtureCategories.Contains(GeometryCategory) ||
                        methodCategories.Contains(CoreCategory) || methodCategories.Contains(GeometryCategory);
                    if (!covered)
                    {
                        uncovered.Add(type.FullName + "." + method.Name);
                    }
                }
            }

            Assert.That(uncovered, Is.Empty,
                "Test(s) EditMode sans categorie Core/Geometry : la selection de profil ne peut pas certifier sa couverture. Tagger la fixture (ou la methode) puis relancer. Liste : "
                + string.Join("; ", uncovered.ToArray()));
        }

        [Test]
        public void BothSelectionCategoriesExistAndAreDistinct()
        {
            var assembly = typeof(TestSuiteCategoryPartitionTests).Assembly;
            int coreFixtures = 0;
            int geometryFixtures = 0;

            foreach (var type in assembly.GetTypes())
            {
                var categories = Categories(type.GetCustomAttributes(true));
                if (categories.Contains(CoreCategory)) { coreFixtures++; }
                if (categories.Contains(GeometryCategory)) { geometryFixtures++; }
            }

            Assert.That(coreFixtures, Is.GreaterThan(0), "Aucune fixture Core : le profil Fast ne selectionnerait rien.");
            Assert.That(geometryFixtures, Is.GreaterThan(0), "Aucune fixture Geometry : le profil Geometry ne selectionnerait rien.");
            Assert.That(CoreCategory, Is.Not.EqualTo(GeometryCategory), "Les deux categories de selection doivent rester distinctes.");
        }

        private static bool IsTest(MethodInfo method)
        {
            var attributes = method.GetCustomAttributes(true);
            return attributes.Any(a => a is TestAttribute
                || a is TestCaseAttribute
                || a is UnityEngine.TestTools.UnityTestAttribute);
        }

        private static List<string> Categories(object[] attributes)
        {
            var result = new List<string>();
            foreach (var attribute in attributes)
            {
                var category = attribute as CategoryAttribute;
                if (category != null)
                {
                    foreach (var name in category.Name.Split(','))
                    {
                        var trimmed = name.Trim();
                        if (trimmed.Length > 0)
                        {
                            result.Add(trimmed);
                        }
                    }
                }
            }

            return result;
        }
    }
}
