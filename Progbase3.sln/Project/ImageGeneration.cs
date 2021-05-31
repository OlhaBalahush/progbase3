using System;
using System.Drawing;
using ScottPlot;
static class ImageGeneration
{
    public static void Graphic(int numberOfPosts, int numberOfcomments, int timeInterval) // timeInterval in day
    {
        var plt = new ScottPlot.Plot(600, 400);

        // create a series of dates
        // int numberOfPosts = 5;
        // int numberOfcomment = 5;
        DateTime firstDay = new DateTime(2020, 1, 22);
        DateTime firstDay_2 = new DateTime(2020, 1, 29);
        DateTime firstDay_3 = new DateTime(2020, 2, 7);
        DateTime firstDay_4 = new DateTime(2020, 2, 14);
        DateTime firstDay_5 = new DateTime(2020, 2, 21);


        double[] datesOfPosts = new double[numberOfPosts];
        datesOfPosts[0] = firstDay.ToOADate();
        datesOfPosts[1] = firstDay_2.ToOADate();
        datesOfPosts[2] = firstDay_3.ToOADate();
        datesOfPosts[3] = firstDay_4.ToOADate();
        datesOfPosts[4] = firstDay_5.ToOADate();
        // simulate data for each date
        double[] valuesOfposts = new double[numberOfPosts];
        Random rand = new Random(0);
        for (int i = 1; i < numberOfPosts; i++)
            valuesOfposts[i] = valuesOfposts[i - 1] + rand.NextDouble();


        double[] datesOfComments = new double[numberOfPosts];
        datesOfComments[0] = firstDay.AddDays(1).ToOADate();
        datesOfComments[1] = firstDay_2.AddDays(1).ToOADate();
        datesOfComments[2] = firstDay_3.AddDays(1).ToOADate();
        datesOfComments[3] = firstDay_4.AddDays(1).ToOADate();
        datesOfComments[4] = firstDay_5.AddDays(1).ToOADate();
        double[] valuesOfComments = new double[numberOfcomments];
        for (int i = 1; i < numberOfcomments; i++)
            valuesOfComments[i] = valuesOfComments[i - 1] + rand.NextDouble();

        plt.AddScatter(datesOfPosts, valuesOfposts);
        plt.AddScatter(datesOfComments, valuesOfComments);
        plt.XAxis.DateTimeFormat(true);

        // define tick spacing as 1 day (every day will be shown)
        plt.XAxis.ManualTickSpacing(7, ScottPlot.Ticks.DateTimeUnit.Day);
        plt.XAxis.TickLabelStyle(rotation: 45);

        // add some extra space for rotated ticks
        plt.XAxis.SetSizeLimit(min: 50);

        plt.SaveFig("ticks_definedDateTimeSpace.png");
    }
}