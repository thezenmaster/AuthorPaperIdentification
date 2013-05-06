using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Entity.Validation;

namespace AuthorPaperIdentification
{
    class InputParser
    {
        static void Main(string[] args)
        {
            var startTime = DateTime.Now;
            //Be careful with the order of reading the documents. Otherwise database constraints will be violated.
            //string[] files = { "Author", "Conference", "Journal", "Paper", "PaperAuthor", "Train", "Valid" };
            string[] files = { "Paper" };
            PopulateTables(files);
            var endTime = DateTime.Now;
            Console.WriteLine(endTime - startTime);
            Console.ReadLine();
        }

        private static void PopulateTables(string[] files)
        {
            var context = new AuthorPaperEntities();
            context.Configuration.AutoDetectChangesEnabled = false;
            foreach (var item in files)
            {
                var filePath = @"..\..\..\Input\" + item + ".csv";
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        //Skip the first line that contains column names
                        var line = reader.ReadLine();
                        int i = 0;
                        while (reader.Peek() >= 0)
                        {
                            line = reader.ReadLine();
                            var values = ParseCSVLine(line);
                            if (values == null)
                            {
                                Console.WriteLine("Could not parse line {0}", line);
                                continue;
                            }
                            switch (item)
                            {
                                case "Author":
                                    PopulateAuthors(ref context, reader, ref i, values);
                                    break;
                                case "Paper":
                                    PopulatePapers(ref context, reader, ref i, values);
                                    break;
                                case "Conference":
                                    PopulateConferences(ref context, reader, ref i, values);
                                    break;
                                case "Journal":
                                    PopulateJournals(ref context, reader, ref i, values);
                                    break;
                            }
                            if (i == 1000)
                            {
                                i = 0;
                                try
                                {
                                    context.SaveChanges();
                                    context.Dispose();
                                    context = new AuthorPaperEntities();
                                    context.Configuration.AutoDetectChangesEnabled = false;
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        //After while loop is over, commit any remaining inserts. Those will be < 1000.
                        context.SaveChanges();
                        context.Dispose();
                        context = new AuthorPaperEntities();
                        context.Configuration.AutoDetectChangesEnabled = false;
                    }
                }
            }
        }

        private static List<string> ParseCSVLine(string line)
        {
            List<string> values = new List<string>();
            int quotesIndex;
            do
            {
                quotesIndex = String.IsNullOrEmpty(line) ? -1 : line.IndexOf('"');
                if (quotesIndex >= line.Length - 1)
                    return null;
                string leftSubstring = quotesIndex > -1 ? line.Substring(0, quotesIndex) : line;
                //Add all comma-separated values on the left ot first quotes.
                foreach (var item in leftSubstring.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    values.Add(item);
                }
                //Add the quoted text as 1 item.
                if (quotesIndex > -1)
                {
                    string rightSubstring = line.Substring(quotesIndex + 1);
                    quotesIndex = String.IsNullOrEmpty(rightSubstring) ? -1 : rightSubstring.IndexOf('"');
                    if (quotesIndex == -1)
                        //This means we have incorrect data. Quotes are not closed.
                        return null;
                    string quotedText = rightSubstring.Substring(0, quotesIndex);
                    values.Add(quotedText);
                    rightSubstring = rightSubstring.Substring(quotesIndex + 1);
                    line = rightSubstring;
                }
                else
                {
                    //There are no quotes, so everything should be parsed.
                    line = String.Empty;
                }
            } while (line.Length > 0);

            return values;
        }

        private static void PopulateConferences(ref AuthorPaperEntities context, StreamReader reader, ref int i, List<string> values)
        {
            if (values.Count < 3)
                return;
            try
            {
                var id = int.Parse(values[0]);
                if (!context.Conferences.Any(c => c.Id == id))
                {
                    i++;
                    var conference = new Conference();
                    conference.Id = id;
                    conference.ShortName = values[1].Trim(new char[] { '"' });
                    conference.FullName = values[2].Trim(new char[] { '"' });
                    if (values.Count == 4)
                    {
                        var homePage = values[3].Trim(new char[] { '"' });
                        if (!String.IsNullOrEmpty(homePage))
                            conference.HomePage = homePage;
                    }
                    context.Conferences.Add(conference);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {

                    }
                }
            }
            catch
            {
                return;
            }
        }

        private static void PopulateJournals(ref AuthorPaperEntities context, StreamReader reader, ref int i, List<string> values)
        {
            if (values.Count < 3)
                return;
            try
            {
                var id = int.Parse(values[0]);
                if (!context.Journals.Any(j => j.Id == id))
                {
                    i++;
                    var journal = new Journal();
                    journal.Id = id;
                    journal.ShortName = values[1].Trim(new char[] { '"' });
                    journal.FullName = values[2].Trim(new char[] { '"' });
                    if (values.Count == 4)
                    {
                        var homePage = values[3].Trim(new char[] { '"' });
                        if (!String.IsNullOrEmpty(homePage))
                            journal.HomePage = homePage;
                    }
                    if (journal.ShortName == null)
                    {

                    }
                    context.Journals.Add(journal);
                    if (i == 1000)
                    {
                        i = 0;
                        context.SaveChanges();
                        context.Dispose();
                        context = new AuthorPaperEntities();
                        context.Configuration.AutoDetectChangesEnabled = false;
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {

                    }
                }
            }
            catch
            {
                return;
            }
        }

        private static void PopulateAuthors(ref AuthorPaperEntities context, StreamReader reader, ref int i, List<string> values)
        {
            if (values.Count < 2)
                return;
            try
            {
                var id = int.Parse(values[0]);
                if (!context.Authors.Any(a => a.Id == id))
                {
                    i++;
                    var author = new Author();
                    author.Id = id;
                    author.Name = values[1];
                    if (values.Count == 3)
                        author.Affiliation = values[2];
                    context.Authors.Add(author);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {

                    }
                }
            }
            catch
            {
                return;
            }
        }

        private static void PopulatePapers(ref AuthorPaperEntities context, StreamReader reader, ref int i, List<string> values)
        {
            if (values.Count < 5)
                return;
            try
            {
                var id = int.Parse(values[0]);
                if (!context.Papers.Any(p => p.Id == id))
                {
                    i++;
                    var paper = new Paper();
                    paper.Id = id;
                    paper.Title = values[1];
                    paper.Year = int.Parse(values[2]);
                    var conferenceId = int.Parse(values[3]);
                    if ((conferenceId > 0) && (context.Conferences.Any(c => c.Id == conferenceId)))
                    {
                        paper.ConferenceId = conferenceId;
                    }
                    else
                    {

                    }
                    var journalId = int.Parse(values[4]);
                    if ((journalId > 0) && (context.Journals.Any(j => j.Id == journalId)))
                    {
                        paper.JournalId = journalId;
                    }
                    else
                    {
                    }
                    if (values.Count == 6)
                        paper.Keywords = values[5];
                    context.Papers.Add(paper);
                }

            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {

                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}
