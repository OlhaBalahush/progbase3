
using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;
using System;
public class GenerationImageWindow: Dialog
{
    public bool canceled;
    private User currentuser;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private TextField year;
    private TextField filename;
    public GenerationImageWindow(User currentuser, UserReposytory userReposytory, PostReposytory postReposytory)
    {
        this.currentuser = currentuser;
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;

        this.Title = "Statistics";

        Button okBtn = new Button("Ok");
        okBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(okBtn);

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += OnCreateDialogCanceled;
        this.AddButton(cancelButton);

        int rightColumnX = 20;

        Label yearLbl = new Label(2,2,"Year:");
        year = new TextField("2021")
        {
            X = rightColumnX,
            Y = Pos.Top(yearLbl),
            Width = 40,
        };
        this.Add(yearLbl, year);
        
        Label filenameLbl = new Label(2,4,"File name:");
        filename = new TextField("statistics")
        {
            X = rightColumnX,
            Y = Pos.Top(filenameLbl),
            Width = 40,
        };
        this.Add(filenameLbl, filename);
    }
    private void OnCreateDialogSubmit()
    {
        if(!IsNumber(this.year.Text.ToString()))
        {
            MessageBox.ErrorQuery("Incorrect input year","year must be a number","ok");
            return;
        }
        int year = int.Parse(this.year.Text.ToString());
        string filename = this.filename.Text.ToString();
        if(year < this.currentuser.createdAt.Year || year > DateTime.Now.Year)
        {
            MessageBox.ErrorQuery("Incorrect input year","there are no statistics for this year","ok");
            return; //doesn't have statistic
        }
        ImageGeneration imageGeneration = new ImageGeneration(this.currentuser, this.userReposytory, this.postReposytory);
        imageGeneration.GraphicAndReport(year, filename);
        Application.RequestStop();
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
    private bool IsNumber(string value)
    {
        char[] charArr = value.ToCharArray();
        int i = 0;
        if(charArr[0] == 45)
        {
            i = 1;
        }
        while (i < charArr.Length)
        {
            if(char.IsDigit(charArr[i]) == false)
            {
                return false;
            }
            i++;
        }
        return true;
    }
}