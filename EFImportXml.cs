using FIAS;
using ShellProgressBar;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace importFias
{
    public class EFImportXml
    {
        public static async Task Import(string xmlPath)
        {

/*
            var schema = XmlSchema.Read(XmlReader.Create(new FileStream(schemaPath, FileMode.Open, FileAccess.Read)), (object sender, ValidationEventArgs args) => { });


            var xmlSchema = await XDocument.LoadAsync(XmlReader.Create(new FileStream(schemaPath, FileMode.Open, FileAccess.Read), new XmlReaderSettings { Async = true }), LoadOptions.PreserveWhitespace, CancellationToken.None);
            var prefix = xmlSchema.Root.GetNamespaceOfPrefix("xs");

            var addrobject = xmlSchema.Root.Element(prefix + "element");
            var rowTypes = addrobject.Element(prefix + "complexType");

            var names = rowTypes.Descendants().Select(item => item.Name.LocalName );
*/

//            var xmlDocument = await XDocument.LoadAsync(XmlReader.Create(new FileStream(xmlPath, FileMode.Open, FileAccess.Read), new XmlReaderSettings { Async = true }), LoadOptions.PreserveWhitespace, CancellationToken.None);

//            var elements = xmlDocument.Root.Elements().ToList();

//            const string delimiter = "\t";


//            string tableName = xmlDocument.Root.Name.LocalName;
            //string fields = elements.First().Attributes().Aggregate("", (fs, f) => fs + (fs.Length == 0 ? "" : ",") + $"{f.Name.LocalName.ToLower()}");
//            var fieldsArray = elements.First().Attributes().Select(item => item.Name.LocalName.ToLower()).ToList();
//            string fields = string.Join(",", fieldsArray);

            try
            {


                await using var context = new fiasContext();
                using var bar = new ProgressBar(500000, "import file to database",
                    new ProgressBarOptions {ProgressCharacter = '─', ProgressBarOnBottom = true});
                var updated = 0;
                var inserted = 0;
                var skipped = 0;

                using var reader = XmlReader.Create(xmlPath, new XmlReaderSettings {Async = true});

                await reader.MoveToContentAsync();


                while (await reader.ReadAsync())
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                        break;

//                    var addressObjectXml = await XNode.ReadFromAsync(reader, CancellationToken.None) as XElement;

                    Addrobj addressObject = new Addrobj();
                    try
                    {
                        addressObject = (Addrobj) new XmlSerializer(typeof(Addrobj)).Deserialize(reader);
                    }
                    catch (Exception)
                    {
//                        Console.WriteLine($"incorrect object {addressObjectXml}, field {reader.LocalName}");
                        skipped++;
                        continue;
                    }

//                    var currentKey = addressObjectXml?.Attributes().First(field => string.Compare(field.Name.LocalName, "aoid", StringComparison.CurrentCultureIgnoreCase) == 0)?.Value;
                    if (context.Addrobj.Any(obj => obj.Aoid == addressObject.Aoid))
                    {
                        // UPDATE
                        context.Addrobj.Update(addressObject);
                        updated++;
                    }
                    else
                    {
                        // INSERT
                        context.Addrobj.Add(addressObject);
                        inserted++;
                    }

                    if ((inserted + updated) % 10000 == 0)
                        await context.SaveChangesAsync();
                    bar.Tick();
                }

                bar.Tick(500000);
                Console.WriteLine(
                    $"{inserted} records inserted, {updated} records updated, {skipped} skipped because of errors, from file {xmlPath}");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
