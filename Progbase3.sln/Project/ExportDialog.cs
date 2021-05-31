//Можна відфільтрувати пости по співпадінню тексту і експортувати всі знайдені пости з усіма їх коментарями у форматі XML
using System;
using Terminal.Gui;
using System.Collections.Generic;
public class ExportWindow: Dialog
{
    public bool canceled;
    private TextField searchText;
    private TextField filename;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    public ExportWindow(PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        int rightColumnX = 20;

        Label fileNameLbl = new Label(2,4,"File name:");
        filename = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(fileNameLbl),
            Width = 40,
        };
        this.Add(fileNameLbl, filename);

        Label textSearchLbl = new Label(2,2,"Text:"); //TextField
        searchText = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(textSearchLbl),
            Width = 40,
        };
        this.Add(textSearchLbl, searchText);

        Button exportBtn = new Button("Export");
        exportBtn.Clicked += OnExportDialog;
        this.AddButton(exportBtn);

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += OnCreateDialogCanceled;
        this.AddButton(cancelButton);
    }
    private void OnExportDialog()
    {
        string searchValue = this.searchText.Text.ToString();
        //перетворити у ліст по співпадінню
        //перевірити чи коректно введено назву файлу
        List<Post> posts = postReposytory.GetSearchValue_1(searchValue);
        foreach (Post item in posts)
        {
            item.user = this.postReposytory.User(item.id);
            item.comments = this.postReposytory.CommentsOfPost(item.id, this.commentReposytory);
        }
        Export_Import.Export(posts, filename.Text.ToString());
        Application.RequestStop();
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
}

// using System;
// using System.IO;
// using System.Text;
// using System.Collections.Generic;
// using System.Linq;
// using System.Xml;
// using System.Xml.Serialization;
// using System.Drawing;
// using ScottPlot;
// [Serializable]
// public class Root
// {
//     public List<Review> reviews;
// }
// class DataProcces
// {
//     public void ExportAllReview(int id, List<Review> reviewsList, string file_path)
//     {
//         Root root = new Root();
//         root.reviews = reviewsList;
//         this.Serialize(file_path, root);
//     }
//     private void Serialize(string file_path, Root root)
//     {
//         //
//         XmlSerializer ser = new XmlSerializer(typeof(Root));
//         System.IO.StreamWriter writer = new System.IO.StreamWriter(file_path);
//         ser.Serialize(writer, root);
//         writer.Close();
//     }
//     public void ImportAllReview(int id, string file_path, ReviewRepository reviews)
//     {
//         List<Review> list = ReadDataForImport(file_path);
//         Review review = new Review();
//         bool isHere;
//         foreach(Review item in list)
//         {
//             if(item.id == id)
//             {
//                 isHere = reviews.CheckReviewInDb(item.id);
//                 if(isHere == false)
//                 {
//                     item.imported = "imported";
//                     reviews.CreateImported(item);
//                 }
//             }
//         }
//     }
//     private List<Review> ReadDataForImport(string file_path)
//     {
//         int counter = 0;
//         List<Review> list = new List<Review>();
//         StreamReader sr = new StreamReader(file_path);
//         string text = "";
//         while (true)
//         {
//             text = sr.ReadLine();
//             if (text == null)
//             {
//                 break;
//             }
//             if(counter != 0)
//             {
//                 Review d = CsvToReview(text);
//                 list.Add(d);
//             }
//             counter++;
//         }
//         sr.Close();
//         return list;
//     }
//     private Review CsvToReview(string text)
//     {
//         Review review = new Review();
//         string [] arr = new string[5];
//         for(int i = 0; i < 5; i++)
//         {
//             arr = text.Split(',');
//         }
//         review.id = int.Parse(arr[0]);
//         review.rating = int.Parse(arr[1]);
//         review.text = arr[2];
//         review.userId = int.Parse(arr[3]);
//         review.titleId = int.Parse(arr[4]);
//         return review;
//     }
//     public  void GenerateImage(List<Review> reviews, string filePath)
//     {
//         var plt = new ScottPlot.Plot(600, 400);
//         int pointCount = 5;
//         double[] xs = DataGen.Consecutive(pointCount);
//         double[] ys = { reviews[0].rating,reviews[1].rating,reviews[2].rating, reviews[3].rating, reviews[4].rating};
//         plt.PlotBar(xs, ys, horizontal: true);
//         plt.Grid(enableHorizontal: false, lineStyle: LineStyle.Dot);
//         try 
//         {  
//             plt.SaveFig(filePath);
//         }
//         catch
//         {
//             throw new ArgumentException("Extension not supported. Supported extension: .png | .jpg | .jpeg | .bmp");
//         }
//     }
// }