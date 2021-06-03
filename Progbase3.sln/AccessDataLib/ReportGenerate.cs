using System;
using System.Drawing;
using ScottPlot;
using System.Xml;
using System.Xml.Linq;
using System.IO.Compression;
using System.IO;
namespace AccessDataLib
{
    public class ImageGeneration
    {
        private int year;
        private string filenameimage = "graphic" + ".png";
        private string filenamereport;
        private User user;
        private UserRepository userReposytory;
        private PostRepository postReposytory;
        private CommentRepository commentReposytory;
        private string [] months = new string[]{
                    "Jan", "Feb",
                    "Mar", "Apr",
                    "May", "Jun",
                    "Jul", "Aug",
                    "Sep", "Oct",
                    "Nov", "Dec",
                };
        public ImageGeneration(User user, UserRepository userReposytory, PostRepository postReposytory, CommentRepository commentReposytory)
        {
            this.user = user;
            this.userReposytory = userReposytory;
            this.postReposytory = postReposytory;
            this.commentReposytory = commentReposytory;
        }
        public void GraphicAndReport(int year, string filenamereport)
        {
            string zipPath = @"./template.docx";
            string extractPath = @"./template";
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            this.filenamereport = filenamereport;
            this.year = year;
            var plt = new ScottPlot.Plot(600, 400);

            int count = 12;
            double[] xs = DataGen.Consecutive(count);
            string[] currentmonths = months;
            if(this.year < user.createdAt.Year || this.year > DateTime.Now.Year)
            {
                return;
            }
            double[] posts = new double[count];
            double[] comments = new double[count];
            for(int i = 0; i < count; i++)
            {
                DateTime start = new DateTime(year, i + 1 ,1);
                DateTime end;
                if(i == 11)
                {
                    end = new DateTime(year + 1, 1 , 1);
                }
                else
                {
                    end = new DateTime(year, i + 2 ,1);
                }
                int numberOfPosts = userReposytory.NumberOfPostsFromTo(user.id, start, end).Count;
                posts[i] = numberOfPosts;
                int numberOfComments = userReposytory.NumberOfCommentsFromTo(user.id, start, end).Count;
                comments[i] = numberOfComments;
            }
            plt.PlotScatter(xs, posts, label: "posts");
            plt.PlotScatter(xs, comments, label: "comments");
            plt.Legend();
            plt.XTicks(xs, months);

            plt.Title($"Statistics for {year.ToString()}");

            plt.SaveFig(filenameimage);
            GenerationReport();

            //?????
            DeleteFiles(@"./template/_rels");
            DeleteFiles(@"./template/docProps");
            DeleteFiles(@"./template/word/_rels");
            DeleteFiles(@"./template/word/media");
            DeleteFiles(@"./template/word/theme");
            DeleteFiles(@"./template/word");
            DeleteFiles(@"./template");
        }
        static void DeleteFiles(string extractPath)
        {
            string[] files = Directory.GetFiles(extractPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(extractPath);  
        }
        public void GenerationReport()
        {
            XElement root = XElement.Load("./template/word/document.xml");
            
            FindAndReplace(root);

            Bitmap bmp = new Bitmap("./" + this.filenameimage);
            bmp.Save("./template/word/media/image1.png");

            root.Save("./template/word/document.xml", SaveOptions.DisableFormatting);
            ZipFile.CreateFromDirectory("./template", this.filenamereport + ".docx");
        }
        private void FindAndReplace(XElement node)
        {
            DateTime startDate = new DateTime(this.year, 1, 1);
            DateTime endDate = new DateTime(this.year + 1, 1, 1);
            int numberOfAllPosts = userReposytory.NumberOfPostsFromTo(user.id, startDate, endDate).Count;
            int numberOfAllComments = userReposytory.NumberOfCommentsFromTo(user.id, startDate, endDate).Count;
            Post post = postReposytory.PostWithTheMostCommentsDuringThisPeriod(userReposytory.NumberOfPostsFromTo(user.id, startDate, endDate), this.commentReposytory);
            if (node.FirstNode != null
                && node.FirstNode.NodeType == XmlNodeType.Text)
            {
                switch (node.Value)
                {
                    case "start": node.Value = startDate.ToString(); break;
                    case "end": node.Value = endDate.ToString(); break;
                    case "posts": node.Value = numberOfAllPosts.ToString(); break;
                    case "comments": node.Value = numberOfAllComments.ToString(); break;
                    case "post": node.Value = post.ToString(); break;
                }
            }
        
            foreach (XElement el in node.Elements())
            {
                FindAndReplace(el);
            }
        }
    }
}