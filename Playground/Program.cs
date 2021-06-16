using Raptorious.SharpMt940Lib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MT940
{
    class Program
    {
        static void Main(string[] args)
        {
            Auszug a = new Auszug();
            a.readCustMsg();
        }
    }
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

                using (var fileStream = new FileStream(@"K:\user\barbara.borgards\Entwicklung\mt940_Testdaten.txt", FileMode.Open, FileAccess.Read))
                {
                    TextReader textreader = new StreamReader(fileStream);
                    List<CustomerStatementMessage> customerStatementMessages = (List<CustomerStatementMessage>)Mt940Parser.Parse(genericFormat, textreader, CultureInfo.CurrentCulture);

                    //XML anlegen
                    XmlWriterSettings setting = new XmlWriterSettings();
                    setting.Indent = true;
                    setting.IndentChars = "  ";
                    setting.ConformanceLevel = ConformanceLevel.Fragment;

                    XmlWriter writer = XmlWriter.Create(@"K:\user\barbara.borgards\Entwicklung\Test.xml");
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

                    int creditCounter = 0;
                    int debitCounter = 0;
                    int transactionCounter = 0;

                    List<Transaction> credit = new List<Transaction>();

                    //Kontobewegungen werden an Transaktionsliste übergeben
                    for (int i = 0; i < count; i++)
                    {
                        transaction = customerStatementMessages[i];
                        transactions = (List<Transaction>)transaction.Transactions;

                        //Betrachtet jede Transaktion der Transaktionsliste
                        foreach (Transaction transact in transactions)
                        {
                            transactionCounter++;
                            //liest den Betrag mit Währung aus
                            var transactionAmount = transact.Amount;

                            // prüft auf Debit(Abbuchung) oder Credit(Gutschrift)
                            var creditDebit = transact.DebitCredit;

                            if (creditDebit.Equals(DebitCredit.Credit))
                            {
                                Console.WriteLine(creditDebit);
                                credit.Add(transact);
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

                                creditCounter++;
                            }
                            else
                            {
                                debitCounter++;
                                continue;
                            }
                        }
                    }
                    Console.WriteLine("******************************");
                    Console.WriteLine(credit[1].Amount);
                    Console.WriteLine("Gutschriften: " + creditCounter + "\n Abbuchungen: " + debitCounter);
                    Console.WriteLine(transactionCounter);
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }
        }
    }