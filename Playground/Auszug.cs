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
                setting.ConformanceLevel = ConformanceLevel.Fragment;

                XmlWriter writer = XmlWriter.Create(@"U:\Projekte\Test.xml");
                writer.WriteStartDocument();
                writer.WriteStartElement("Konto");

                //Legt Liste mit Transaktionen(Kontobewegungen) an
                List<Transaction> transactions;
                int count = customerStatementMessages.Count;

                // Kontonummer für des Auszugs ermitteln
                var transaction = customerStatementMessages[0];
                var accountID = transaction.Account;
                Console.WriteLine(accountID);

                // account ID an XML geben
                writer.WriteAttributeString("Kontonummer", accountID);

                //Kontobewegungen werden an Transaktionsliste übergeben
                for (int i = 0; i < count; i++)
                {
                    transaction = customerStatementMessages[i];
                    transactions = (List<Transaction>)transaction.Transactions;

                    //Betrachtet jede Transaktion der Transaktionsliste
                    foreach (Transaction transact in transactions)
                    {
                        //liest den Betrag mit Währung aus
                        var transactionAmount = transact.Amount;

                        //liest den Verwendungszweck aus
                        var transactionRef = transact.Description;

                        //legt XML Element an
                        writer.WriteStartElement("Umsatz");
                        writer.WriteAttributeString("Currency", transactionAmount.Currency.Code);
                        writer.WriteString(transactionAmount.ToString());
                        writer.WriteStartElement("Verwendungszweck");
                        writer.WriteString(transactionRef);

                        Console.WriteLine(transactionAmount);
                        Console.WriteLine(transactionRef);

                        //schliesst XML Element
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
