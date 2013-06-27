using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AuthorPaper
{
    public class GenerateScripts
    {

        #region Generate Files
        public static void GenerateFiles()
        {
            //"Author", "Conference", "Journal", 
            var files = new[] { "Paper" };//, "PaperAuthor" };// "Train", "Valid" };
            var stringStorage = new StringBuilder();

            foreach (var file in files)
            {
                var context = new AuthorPaperEntities();
                switch (file)
                {
                    case "Author":
                        InsertAuthors(context, stringStorage);
                        break;
                    case "Conference":
                        InsertConferences(context, stringStorage);
                        break;
                    case "Journal":
                        InsertJournals(context, stringStorage);
                        break;
                    case "Paper":
                        InsertPapers(context, stringStorage);
                        break;
                    case "PaperAuthor":
                        InsertPaperAuthors(context, stringStorage);
                        break;
                }
            }
        }

        public static void WriteFile(string file, StringBuilder storage)
        {
            using (var outfile = new StreamWriter(file + ".sql", true))
            {
                outfile.Write(storage.ToString());
            }
            storage.Clear();
        }
        public static string GetEscapedString(string value)
        {
            return value != null ? value.Replace("'", "''") : string.Empty;
        }

        public static void InsertAuthors(AuthorPaperEntities context, StringBuilder stringStorage)
        {
            foreach (var author in context.authors)
            {
                stringStorage.AppendLine(string.Format("insert into Author(Id,Name,Affiliation) values ({0},N'{1}',N'{2}')",
                    author.id, GetEscapedString(author.name), GetEscapedString(author.affiliation)));
            }
            WriteFile("Authors", stringStorage);
        }

        public static void InsertConferences(AuthorPaperEntities context, StringBuilder stringStorage)
        {
            foreach (var conference in context.conferences)
            {
                stringStorage.AppendLine(string.Format("insert into Conference(Id,ShortName,FullName,HomePage) values ({0},N'{1}',N'{2}',N'{3}')",
                    conference.id, GetEscapedString(conference.shortname), GetEscapedString(conference.fullname),
                    GetEscapedString(conference.homepage)));
            }
            WriteFile("Conferences", stringStorage);
        }

        public static void InsertJournals(AuthorPaperEntities context, StringBuilder stringStorage)
        {
            foreach (var journal in context.journals)
            {
                stringStorage.AppendLine(string.Format("insert into Journal(Id,ShortName,FullName,HomePage) values ({0},N'{1}',N'{2}',N'{3}')",
                      journal.id, GetEscapedString(journal.shortname), GetEscapedString(journal.fullname),
                    GetEscapedString(journal.homepage)));
            }
            WriteFile("Journals", stringStorage);
        }

        public static void InsertPapers(AuthorPaperEntities context, StringBuilder stringStorage)
        {
            var iteration = 0;
            var getItems = 1000000;
            var totalCount = context.papers.Count();

            while (totalCount > getItems * iteration)
            {
                var count = 0;
                var myContext = new AuthorPaperEntities();
                var orderedPapers = myContext.papers.OrderBy(p => p.id);
                var papers = iteration == 0 ? orderedPapers.Take(getItems) :
                    orderedPapers.Skip(iteration * getItems)
                        .Take(totalCount < getItems * (iteration + 1) ? (totalCount - getItems * iteration) : getItems);
                foreach (var paper in papers)
                {
                    stringStorage.AppendLine(string.Format("insert into Paper(Id,Title,Year,ConferenceId,JournalId,Keywords) values " +
                        "({0},N'{1}',{2},{3},{4},N'{5}')",
                        paper.id, GetEscapedString(paper.title), paper.year, paper.conferenceid, paper.journalid, GetEscapedString(paper.keyword)));
                    count++;
                    if (count % 1000 == 0)
                    {
                        WriteFile("Papers", stringStorage);
                    }
                }
                stringStorage.AppendLine("go");
                WriteFile("Papers", stringStorage);
                iteration++;
            }
        }

        public static void InsertPaperAuthors(AuthorPaperEntities context, StringBuilder stringStorage)
        {
            //foreach (var paperauthor in context.paperauthors)
            //{
            //    stringStorage.AppendLine(string.Format("insert into PaperAuthor(PaperId,AuthorId,Name,Affiliation) values "
            //        + "({0},{1},N'{1}',N'{2}')",
            //        paperauthor.paperid, paperauthor.authorid, paperauthor.name.Replace("'", "''"), paperauthor.affiliation.Replace("'", "''")));
            //}
        }
        #endregion
    }
}
