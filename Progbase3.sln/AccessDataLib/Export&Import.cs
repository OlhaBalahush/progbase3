/*
a.	експорту - з параметрами, що визначають дані експорту і файли експорту. 
Сформувати файл(и) експорту за варіантом.
b.	імпорту - з параметром для файлу (директорії) з даними для імпорту. 
Імпортовані дані додаються до БД за вимогами до імпорту.
Експорт: Можна відфільтрувати пости по співпадінню тексту і експортувати всі знайдені пости з усіма їх коментарями у форматі XML
Імпорт: Можна імпортувати експортовані пости і коментарі.
*/
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Xml;
namespace AccessDataLib
{
    public static class Export_Import
    {
        private static XmlSerializer sr = new XmlSerializer(typeof(List<Post>));
        private static StreamReader reader;
        public static void Export(List<Post> posts, string filename)
        {
            StreamWriter output = new StreamWriter(filename);
            XmlWriterSettings set = new XmlWriterSettings();
            set.Indent = true;
            set.NewLineHandling = NewLineHandling.Entitize;
            XmlWriter writer = XmlWriter.Create(output, set);
            sr.Serialize(writer, posts);
        }
        public static List<Post> Import(string filename)
        {
            reader = new StreamReader(filename);
            List<Post> posts = (List<Post>)sr.Deserialize(reader);
            reader.Close();
            return posts;
        }
    }
}
/*using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Xml;
static class De_Serialize
{
    private static XmlSerializer sr = new XmlSerializer(typeof(ListOfCustomers));
    private static ListOfCustomers customers;
    private static StreamReader reader;
    public static ListOfCustomers _Deserialize(string filename)
    {
        reader = new StreamReader(filename);
        customers = (ListOfCustomers)sr.Deserialize(reader);
        reader.Close();
        return customers;
    }
}
public class Customer
{
    public int C_CUSTKEY;
    public string C_NAME;
    public string C_ADDRESS;
    public int C_NATIONKEY;
    public string C_PHONE;
    public double C_ACCTBAL;
    public string C_MKTSEGMENT;
    public string C_COMMENT;
    public override string ToString()
    {
        return string.Format($"\n{C_CUSTKEY}. {C_NAME}\n\tadress: {C_ADDRESS}\n\tnation key: {C_NATIONKEY}\n\tphone: {C_PHONE}\n\tacctbal: {C_ACCTBAL}\n\tmktsegment: {C_MKTSEGMENT}\n\tcomment: {C_COMMENT}");
    }
}
[XmlType(TypeName = "table")]
public class ListOfCustomers
{
    [XmlElement("T")]
    public List<Customer> customers;
}*/