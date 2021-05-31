using Raptorious.SharpMt940Lib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Playground
{
    class Auszug
    {

        public void readCustMsg()
        {
            // File Header
            var header = new Raptorious.SharpMt940Lib.Mt940Format.Separator("STARTUMSE");

            //Kontoinformationen
            var trailer = new Raptorious.SharpMt940Lib.Mt940Format.Separator("-");

            // Erstellt ein neues MT940 Format auf Grundlage der mitgegebenen Werte
            var genericFormat = new Raptorious.SharpMt940Lib.Mt940Format.GenericFormat(header, trailer);


            using (var fileStream = new FileStream(@"U:\Projekte\mt940_Testdaten.txt", FileMode.Open, FileAccess.Read))
            {
                TextReader textreader = new StreamReader(fileStream);
                List<CustomerStatementMessage> customerStatementMessages = (List<CustomerStatementMessage>)Mt940Parser.Parse(genericFormat, textreader, CultureInfo.CurrentCulture);

                //XML anlegen
                XmlWriterSettings setting = new XmlWriterSettings();

                setting.Indent = true;
                setting.IndentChars = "  ";
                
                XmlWriter writer = XmlWriter.Create(@"U:\Projekte\Test.xml");
                writer.WriteStartDocument();

                List<Transaction> transactions;
                int count = customerStatementMessages.Count;
                int accounId;

                setting.ConformanceLevel = ConformanceLevel.Fragment;


                writer.WriteStartElement("Kontobezeichnung");
                
                foreach (CustomerStatementMessage customerStatementMessage in customerStatementMessages)
                {
                    
                    var transaction = customerStatementMessage;
                    transactions = (List<Transaction>)transaction.Transactions;
                    
                    foreach(Transaction transact in transactions)
                    {
                        var transactionAmount = transact.Amount;
                        var transactionRef = transact.Description;
                        writer.WriteStartElement("Umsatz");
                        writer.WriteAttributeString("Currency", transactionAmount.Currency.Code);
                        writer.WriteString(transactionAmount.ToString());
                        writer.WriteStartElement("Verwendungszweck");
                        writer.WriteString(transactionRef);
               
                        Console.WriteLine(transactionAmount);
                        Console.WriteLine(transactionRef);
                        writer.WriteEndElement();
                        writer.WriteEndElement();

                    }                
                }
                writer.WriteEndDocument();
                writer.Close();
            }

        }
    }
}
